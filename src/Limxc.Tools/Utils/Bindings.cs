//
//  Copyright 2013-2014 Frank A. Krueger
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

// ReSharper disable UnusedMember.Local

namespace Limxc.Tools.Utils
{
    /// <summary>
    ///     Abstract class that represents bindings between values in an applications.
    ///     Binding are created using Create and removed by calling Unbind.
    /// </summary>
    public abstract class Bindings
    {
        /// <summary>
        ///     Unbind this instance. This cannot be undone.
        /// </summary>
        public virtual void Unbind()
        {
        }

        /// <summary>
        ///     Uses the lambda expression to create data bindings.
        ///     Equality expression (==) become data bindings.
        ///     And expressions (&&) can be used to group the data bindings.
        /// </summary>
        /// <param name="specifications">The binding specifications.</param>
        public static Bindings Create<T>(Expression<Func<T>> specifications)
        {
            return BindExpression(specifications.Body);
        }

        private static Bindings BindExpression(Expression expr)
        {
            //
            // Is this a group of bindings
            //
            if (expr.NodeType == ExpressionType.AndAlso)
            {
                var b = (BinaryExpression)expr;

                var parts = new List<Expression>();

                while (b != null)
                {
                    var l = b.Left;
                    parts.Add(b.Right);
                    if (l.NodeType == ExpressionType.AndAlso)
                    {
                        b = (BinaryExpression)l;
                    }
                    else
                    {
                        parts.Add(l);
                        b = null;
                    }
                }

                parts.Reverse();

                return new MultipleBindingsContext(parts.Select(BindExpression));
            }


            //
            // Are we binding two values?
            //
            if (expr.NodeType == ExpressionType.Equal)
            {
                var b = (BinaryExpression)expr;

                return new EqualityBindings(b.Left, b.Right);
            }

            //
            // This must be a new object binding (a template)
            //
            throw new NotSupportedException("Only equality bindings are supported.");
        }

        protected static bool SetValue(Expression expr, object value, int changeId)
        {
            if (expr.NodeType == ExpressionType.MemberAccess)
            {
                var m = (MemberExpression)expr;
                var mem = m.Member;

                var target = Evaluator.EvalExpression(m.Expression);

                var f = mem as FieldInfo;
                var p = mem as PropertyInfo;

                if (f != null)
                {
                    f.SetValue(target, value);
                }
                else if (p != null)
                {
                    p.SetValue(target, value, null);
                }
                else
                {
                    ReportError("Trying to SetValue on " + mem.GetType() + " member");
                    return false;
                }

                InvalidateMember(target, mem, changeId);
                return true;
            }

            ReportError("Trying to SetValue on " + expr.NodeType + " expression");
            return false;
        }

        public static event Action<string> Error = delegate { };

        private static void ReportError(string message)
        {
            Debug.WriteLine(message);
            Error(message);
        }

        private static void ReportError(object errorObject)
        {
            ReportError(errorObject.ToString());
        }

        #region Change Notification

        private class MemberActions
        {
            private readonly List<MemberChangeAction> _actions = new List<MemberChangeAction>();
            private readonly MemberInfo _member;
            private readonly object _target;
            private Delegate _eventHandler;

            private EventInfo _eventInfo;

            public MemberActions(object target, MemberInfo mem)
            {
                _target = target;
                _member = mem;
            }

            private void AddChangeNotificationEventHandler()
            {
                if (_target != null)
                {
                    if (_target is INotifyPropertyChanged npc && _member is PropertyInfo)
                        npc.PropertyChanged += HandleNotifyPropertyChanged;
                    else
                        AddHandlerForFirstExistingEvent(_member.Name + "Changed", "EditingDidEnd", "ValueChanged",
                            "Changed");
                }
            }

            // ReSharper disable once UnusedMethodReturnValue.Local
            private bool AddHandlerForFirstExistingEvent(params string[] names)
            {
                var type = _target.GetType();
                foreach (var name in names)
                {
                    var ev = GetEvent(type, name);

                    if (ev != null)
                    {
                        _eventInfo = ev;
                        var isClassicHandler = typeof(EventHandler).GetTypeInfo()
                            .IsAssignableFrom(ev.EventHandlerType.GetTypeInfo());

                        _eventHandler = isClassicHandler
                            ? (EventHandler)HandleAnyEvent
                            : CreateGenericEventHandler(ev, () => HandleAnyEvent(null, EventArgs.Empty));

                        ev.AddEventHandler(_target, _eventHandler);
                        Debug.WriteLine("BIND: Added handler for {0} on {1}", _eventInfo.Name, _target);
                        return true;
                    }
                }

                return false;
            }

            private static EventInfo GetEvent(Type type, string eventName)
            {
                var t = type;
                while (t != null && t != typeof(object))
                {
                    var ti = t.GetTypeInfo();
                    var ev = t.GetTypeInfo().GetDeclaredEvent(eventName);
                    if (ev != null)
                        return ev;
                    t = ti.BaseType;
                }

                return null;
            }

            private static Delegate CreateGenericEventHandler(EventInfo evt, Action d)
            {
                var handlerType = evt.EventHandlerType;
                var handlerTypeInfo = handlerType.GetTypeInfo();
                var handlerInvokeInfo = handlerTypeInfo.GetDeclaredMethod("Invoke");
                var eventParams = handlerInvokeInfo.GetParameters();

                //lambda: (object x0, EventArgs x1) => d()
                var parameters = eventParams.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();
                var body = Expression.Call(Expression.Constant(d),
                    d.GetType().GetTypeInfo().GetDeclaredMethod("Invoke"));
                var lambda = Expression.Lambda(body, parameters);

                var delegateInvokeInfo = lambda.Compile().GetMethodInfo();
                return delegateInvokeInfo.CreateDelegate(handlerType, null);
            }

            private void UnsubscribeFromChangeNotificationEvent()
            {
                if (_target is INotifyPropertyChanged npc && _member is PropertyInfo)
                {
                    npc.PropertyChanged -= HandleNotifyPropertyChanged;
                    return;
                }

                if (_eventInfo == null)
                    return;

                _eventInfo.RemoveEventHandler(_target, _eventHandler);

                Debug.WriteLine("BIND: Removed handler for {0} on {1}", _eventInfo.Name, _target);

                _eventInfo = null;
                _eventHandler = null;
            }

            private void HandleNotifyPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == _member.Name)
                    InvalidateMember(_target, _member);
            }

            private void HandleAnyEvent(object sender, EventArgs e)
            {
                InvalidateMember(_target, _member);
            }

            /// <summary>
            ///     Add the specified action to be executed when Notify() is called.
            /// </summary>
            /// <param name="action">Action.</param>
            public void AddAction(MemberChangeAction action)
            {
                if (_actions.Count == 0) AddChangeNotificationEventHandler();

                _actions.Add(action);
            }

            public void RemoveAction(MemberChangeAction action)
            {
                _actions.Remove(action);

                if (_actions.Count == 0) UnsubscribeFromChangeNotificationEvent();
            }

            /// <summary>
            ///     Execute all the actions.
            /// </summary>
            /// <param name="changeId">Change identifier.</param>
            public void Notify(int changeId)
            {
                foreach (var s in _actions) s.Notify(changeId);
            }
        }

        private static readonly Dictionary<Tuple<object, MemberInfo>, MemberActions> ObjectSubs =
            new Dictionary<Tuple<object, MemberInfo>, MemberActions>();

        internal static MemberChangeAction AddMemberChangeAction(object target, MemberInfo member, Action<int> k)
        {
            var key = Tuple.Create(target, member);
            if (!ObjectSubs.TryGetValue(key, out var subs))
            {
                subs = new MemberActions(target, member);
                ObjectSubs.Add(key, subs);
            }

            var sub = new MemberChangeAction(target, member, k);
            subs.AddAction(sub);
            return sub;
        }

        internal static void RemoveMemberChangeAction(MemberChangeAction sub)
        {
            var key = Tuple.Create(sub.Target, sub.Member);
            if (ObjectSubs.TryGetValue(key, out var subs))
                subs.RemoveAction(sub);
        }

        /// <summary>
        ///     Invalidate the specified object member. This will cause all actions
        ///     associated with that member to be executed.
        ///     This is the main mechanism by which binding values are distributed.
        /// </summary>
        /// <param name="target">Target object</param>
        /// <param name="member">Member of the object that changed</param>
        /// <param name="changeId">Change identifier</param>
        public static void InvalidateMember(object target, MemberInfo member, int changeId = 0)
        {
            var key = Tuple.Create(target, member);
            if (ObjectSubs.TryGetValue(key, out var subs))
                subs.Notify(changeId);
        }

        /// <summary>
        ///     A nice expression based way to invalidate the specified object member.
        ///     This will cause all actions associated with that member to be executed.
        ///     This is the main mechanism by which binding values are distributed.
        /// </summary>
        /// <param name="lambdaExpr">Lambda expr of the Member of the object that changed</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void InvalidateMember<T>(Expression<Func<T>> lambdaExpr)
        {
            var body = lambdaExpr.Body;
            if (body.NodeType == ExpressionType.MemberAccess)
            {
                var m = (MemberExpression)body;
                var obj = Evaluator.EvalExpression(m.Expression);
                InvalidateMember(obj, m.Member);
            }
        }

        #endregion
    }


    /// <summary>
    ///     An action tied to a particular member of an object.
    ///     When Notify is called, the action is executed.
    /// </summary>
    internal class MemberChangeAction
    {
        private readonly Action<int> _action;

        public MemberChangeAction(object target, MemberInfo member, Action<int> action)
        {
            Target = target;
            if (member == null)
                throw new ArgumentNullException(nameof(member));
            Member = member;
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public object Target { get; }
        public MemberInfo Member { get; }

        public void Notify(int changeId)
        {
            _action(changeId);
        }
    }


    /// <summary>
    ///     Methods that can evaluate Linq expressions.
    /// </summary>
    internal static class Evaluator
    {
        /// <summary>
        ///     Gets the value of a Linq expression.
        /// </summary>
        /// <param name="expr">The expression.</param>
        public static object EvalExpression(Expression expr)
        {
            if (expr.NodeType == ExpressionType.Constant) return ((ConstantExpression)expr).Value;

            var lambda = Expression.Lambda(expr, Enumerable.Empty<ParameterExpression>());
            return lambda.Compile().DynamicInvoke();
        }
    }

    /// <summary>
    ///     Binding between two values. When one changes, the other
    ///     is set.
    /// </summary>
    internal class EqualityBindings : Bindings
    {
        private readonly HashSet<int> _activeChangeIds = new HashSet<int>();

        private readonly List<Trigger> _leftTriggers = new List<Trigger>();
        private readonly List<Trigger> _rightTriggers = new List<Trigger>();

        private int _nextChangeId = 1;
        private object _value;

        public EqualityBindings(Expression left, Expression right)
        {
            // Try eval the right and assigning left
            _value = Evaluator.EvalExpression(right);
            var leftSet = SetValue(left, _value, _nextChangeId);

            // If that didn't work, then try the other direction
            if (!leftSet)
            {
                _value = Evaluator.EvalExpression(left);
                SetValue(right, _value, _nextChangeId);
            }

            _nextChangeId++;

            CollectTriggers(left, _leftTriggers);
            CollectTriggers(right, _rightTriggers);

            Resubscribe(_leftTriggers, left, right);
            Resubscribe(_rightTriggers, right, left);
        }

        public override void Unbind()
        {
            Unsubscribe(_leftTriggers);
            Unsubscribe(_rightTriggers);
            base.Unbind();
        }

        private void Resubscribe(List<Trigger> triggers, Expression expr, Expression dependentExpr)
        {
            Unsubscribe(triggers);
            Subscribe(triggers, changeId => OnSideChanged(expr, dependentExpr, changeId));
        }

        private void OnSideChanged(Expression expr, Expression dependentExpr, int causeChangeId)
        {
            if (_activeChangeIds.Contains(causeChangeId))
                return;

            var v = Evaluator.EvalExpression(expr);

            if (v == null && _value == null)
                return;

            if (v == null && _value != null ||
                v != null && _value == null ||
                v is IComparable && ((IComparable)v).CompareTo(_value) != 0)
            {
                _value = v;

                var changeId = _nextChangeId++;
                _activeChangeIds.Add(changeId);
                SetValue(dependentExpr, v, changeId);
                _activeChangeIds.Remove(changeId);
            }
        }

        private static void Unsubscribe(List<Trigger> triggers)
        {
            foreach (var t in triggers)
                if (t.ChangeAction != null)
                    RemoveMemberChangeAction(t.ChangeAction);
        }

        private static void Subscribe(List<Trigger> triggers, Action<int> action)
        {
            foreach (var t in triggers)
                t.ChangeAction = AddMemberChangeAction(Evaluator.EvalExpression(t.Expression), t.Member, action);
        }

        private void CollectTriggers(Expression s, List<Trigger> triggers)
        {
            if (s.NodeType == ExpressionType.MemberAccess)
            {
                var m = (MemberExpression)s;
                CollectTriggers(m.Expression, triggers);
                var t = new Trigger { Expression = m.Expression, Member = m.Member };
                triggers.Add(t);
            }
            else
            {
                if (s is BinaryExpression b)
                {
                    CollectTriggers(b.Left, triggers);
                    CollectTriggers(b.Right, triggers);
                }
            }
        }

        private class Trigger
        {
            public MemberChangeAction ChangeAction;
            public Expression Expression;
            public MemberInfo Member;
        }
    }


    /// <summary>
    ///     Multiple bindings grouped under a single binding to make adding and removing easier.
    /// </summary>
    internal class MultipleBindingsContext : Bindings
    {
        private readonly List<Bindings> _bindings;

        public MultipleBindingsContext(IEnumerable<Bindings> bindings)
        {
            _bindings = bindings.Where(x => x != null).ToList();
        }

        public override void Unbind()
        {
            base.Unbind();
            foreach (var b in _bindings) b.Unbind();
            _bindings.Clear();
        }
    }
}