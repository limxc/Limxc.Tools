using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Limxc.Tools.Extensions.Communication
{
    public static class TemplateParserExtension
    {
        /// <summary>
        ///     在指定时间内获取首个匹配模板
        /// </summary>
        /// <param name="source"></param>
        /// <param name="template"></param>
        /// <param name="timeoutMs"></param>
        /// <param name="sepBegin"></param>
        /// <param name="sepEnd"></param>
        /// <returns></returns>
        public static async Task<string> TryGetTemplateMatchResult(
            this IObservable<string> source,
            string template,
            int timeoutMs,
            char sepBegin = '[',
            char sepEnd = ']'
        )
        {
            var tms = new TemplateMatchState(template, timeoutMs, sepBegin, sepEnd);
            var rst = string.Empty;
            try
            {
                var interval = timeoutMs / 100;
                interval = interval < 10 ? 10 : interval;

                await source
                    .Merge(
                        Observable
                            .Interval(TimeSpan.FromMilliseconds(interval))
                            .Select(_ => string.Empty)
                    )
                    .Scan(
                        tms,
                        (acc, v) =>
                        {
                            acc.Add(v);
                            return acc;
                        }
                    )
                    .ToTask();
            }
            //catch (TimeoutException)
            //{
            //}
            catch (Complete)
            {
                rst = tms.MatchResult;
            }

            return rst;
        }

        #region Helpers

        private class TemplateMatchState
        {
            private readonly char _sepBegin;
            private readonly char _sepEnd;
            private readonly string _template;
            private readonly DateTimeOffset _until;

            private string _received;

            public TemplateMatchState(
                string template,
                int timeoutMs,
                char sepBegin = '[',
                char sepEnd = ']'
            )
            {
                _template = template;
                _sepBegin = sepBegin;
                _sepEnd = sepEnd;
                _until = DateTimeOffset.Now.AddMilliseconds(timeoutMs);
            }

            public string MatchResult { get; private set; }

            public void Add(string value)
            {
                if (DateTimeOffset.Now > _until)
                    throw new TimeoutException(
                        $"模板解析超时: Template=({_template}) Received=({_received})"
                    );

                if (value == string.Empty)
                    return;

                _received += value;

                GetMatchResult();

                if (!string.IsNullOrWhiteSpace(MatchResult))
                    throw new Complete();
            }

            private void GetMatchResult()
            {
                var pattern = _template;
                foreach (Match m in Regex.Matches(_template, $@"\{_sepBegin}[0-9]+\{_sepEnd}"))
                {
                    var o = m.Value;
                    var n =
                        $"[0-9a-fA-F]{{{Convert.ToInt32(o.Replace("[", "").Replace("]", ""))}}}";
                    pattern = pattern.Replace(o, n);
                }

                MatchResult = Regex.Match(_received, pattern, RegexOptions.IgnoreCase).Value;
            }
        }

        private class Complete : Exception
        {
        }

        #endregion
    }
}