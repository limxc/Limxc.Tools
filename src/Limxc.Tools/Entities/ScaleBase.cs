using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Limxc.Tools.Extensions;

// ReSharper disable VirtualMemberCallInConstructor

namespace Limxc.Tools.Entities
{
    public abstract class ScaleBase
    {
        protected ScaleBase()
        {
            Init();

            var count = Topics.Count;
            count = HasStartPage ? count - 1 : count;
            if (count == 0)
                throw new Exception("量表初始化错误.无题目.");

            if (Topics.Count(p => p.Index > 0 && p.Answers.Count == 0) > 0)
                throw new Exception("量表初始化错误.无答案.");
        }

        /// <summary>当前题号</summary>
        public int CurrentTopicNum { set; get; }

        /// <summary>
        ///     量表题目,编号1~N
        /// </summary>
        public List<Topic> Topics { set; get; } = new List<Topic>();

        public bool HasNext => CurrentTopicNum < Topics.Max(p => p.Index);
        public bool HasPrev => CurrentTopicNum > 1;
        public bool HasStartPage => Topics.Min(p => p.Index) == 0;

        /// <summary>量表名</summary>
        public abstract string Name { get; }

        /// <summary>
        ///     指导评价
        /// </summary>
        public string Assessment { get; set; }

        /// <summary>
        ///     初始化
        /// </summary>
        protected abstract void Init();

        /// <summary>
        ///     结果
        /// </summary>
        /// <returns></returns>
        public abstract object Result();

        #region Methods

        /// <summary>当前题目</summary>
        public Topic CurrentTopic()
        {
            return Topics.FirstOrDefault(p => p.Index == CurrentTopicNum) ?? Topics[0];
        }

        /// <summary>
        ///     计算得分
        /// </summary>
        /// <param name="indexes">题号</param>
        /// <returns></returns>
        public double CalcScore(IEnumerable<int> indexes)
        {
            double totalScore = 0;

            var topics = Topics.Where(p => indexes.Contains(p.Index)).ToList();

            topics.ForEach(t => totalScore += t.Answers.Where(p => p.Checked).Sum(p => p.Value));
            return totalScore;
        }

        /// <summary>
        ///     计算得分
        /// </summary>
        /// <returns></returns>
        public virtual double CalcScore()
        {
            double totalScore = 0;

            var topics = Topics.Where(p => p.Index > 0).ToList();

            topics.ForEach(t => totalScore += t.Answers.Where(p => p.Checked).Sum(p => p.Value));
            return totalScore;
        }

        /// <summary>
        ///     添加题目,自动编号(1~N, 第一个且无答案的编号为0,视为欢迎页)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="answers"></param>
        protected void AddTopic(string text, params (string Text, double Score)[] answers)
        {
            AddTopic(text, "", answers);
        }

        /// <summary>
        ///     添加题目,自动编号(1~N, 第一个且无答案的编号为0,视为欢迎页)
        /// </summary>
        /// <param name="text"></param>
        /// <param name="res"></param>
        /// <param name="answers"></param>
        protected void AddTopic(string text, string res, params (string Text, double Score)[] answers)
        {
            var list = new List<Answer>();

            foreach (var ans in answers)
                list.Add(new Answer(list.Count() + 1, ans.Text, ans.Score));

            var topic = new Topic
            {
                Text = text,
                Answers = list,
                Res = res
            };

            if (Topics.Count == 0)
                topic.Index = !answers.Any() ? 0 : 1;
            else
                topic.Index = Topics.Max(p => p.Index) + 1;

            Topics.Add(topic);

            CurrentTopicNum = Topics.Min(p => p.Index);
        }

        /// <summary>停留在最后一个</summary>
        public Topic Next()
        {
            if (HasNext)
                CurrentTopicNum++;

            var t = Topics.First(p => p.Index == CurrentTopicNum);

            return t;
        }

        /// <summary>停留在第一个</summary>
        public Topic Prev()
        {
            if (HasPrev)
                CurrentTopicNum--;

            var t = Topics.First(p => p.Index == CurrentTopicNum);

            return t;
        }

        /// <summary>
        ///     清空选项并重置当前题号为0
        /// </summary>
        public virtual void Clear()
        {
            CurrentTopicNum = 0;
            Topics.ForEach(t => t.Answers.ForEach(p => p.Checked = false));
        }

        /// <summary>保存到文件</summary>
        public void SaveToFile(string fileName)
        {
            var path = Path.Combine(Environment.CurrentDirectory, "Scales", fileName);
            this.Save(path);
        }

        /// <summary>从文件加载</summary>
        public static T LoadFromFile<T>(string fileName) where T : ScaleBase
        {
            var path = Path.Combine(Environment.CurrentDirectory, "Scales", fileName);
            return path.Load<T>();
        }

        #endregion
    }

    public class Topic
    {
        /// <summary>1~N</summary>
        public int Index { get; set; }

        /// <summary>
        ///     资源/文件/路径/etc..
        /// </summary>
        public object Res { get; set; }

        public string Text { get; set; }

        public List<Answer> Answers { get; set; } = new List<Answer>();

        public double CheckedScore => Answers.Where(p => p.Checked).Sum(p => p.Value);
        public double TotalScore => Answers.Sum(p => p.Value);

        public override string ToString()
        {
            return $"{Index}.{Text} [{string.Join(", ", Answers.Where(p => p.Checked))}] ";
        }
    }

    public class Answer
    {
        public Answer()
        {
        }

        public Answer(int index, string text, double value)
        {
            Index = index;
            Text = text;
            Checked = false;
            Value = value;
        }

        public Answer(string text, double value)
        {
            Text = text;
            Checked = false;
            Value = value;
        }

        /// <summary>1~N</summary>
        public int Index { get; set; }

        public string Text { get; set; }

        public bool Checked { get; set; }

        public double Value { get; set; }

        public override string ToString()
        {
            return
                $"{Index}.{Text}{(Checked ? $"({(Value > 0 ? Value.ToString(CultureInfo.InvariantCulture) : "Y")})" : "")}";
        }
    }
}