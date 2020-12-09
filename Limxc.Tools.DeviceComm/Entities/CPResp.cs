using Limxc.Tools.DeviceComm.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Limxc.Tools.DeviceComm.Entities
{
    /// <summary>
    /// Communication Protocol Response
    /// </summary>
    public class CPResp
    {
        #region 初始化

        /// <summary>
        /// 初始化响应模板,占位符 $n n=1-9
        /// </summary>
        /// <param name="template"></param>
        public CPResp(string template = "")
        {
            Template = template.Replace(" ", "").ToUpper();
        }

        /// <summary>
        /// 响应模板,占位符 $n n=1-9位 length=n*2
        /// </summary>
        public string Template { get; }

        /// <summary>
        /// 原始响应值
        /// </summary>
        public string Value { get; set; }

        public int Length => Template.TemplateLength('$');

        #endregion 初始化

        /// <summary>
        /// 无返回值 返回new List<string>();
        /// </summary>
        /// <returns></returns>
        public List<string> GetStrValues(bool checkPattern = true) => GetStrValues(Value, checkPattern);

        /// <summary>
        /// 无返回值 返回new List<string>();
        /// </summary>
        /// <returns></returns>
        public List<string> GetStrValues(string resp, bool checkPattern = true)
        {
            var values = new List<string>();

            if (string.IsNullOrWhiteSpace(Template))
                return values;

            if (checkPattern && !Template.IsTemplateMatch(resp))
                throw new FormatException($"Response Parse Error. Template:[{Template}] Value:{resp}");
            else if (string.IsNullOrWhiteSpace(resp))
                return values;

            resp = resp.Replace(" ", "");

            var arr = Template.ToCharArray();
            int skipLen = 0;

            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == '$' && i < arr.Length - 1)
                {
                    var len = arr[i + 1].ToString().ToInt();
                    var tfv = new string(resp.Skip(i + skipLen * 2).Take(len * 2).ToArray());
                    skipLen += len > 1 ? len - 1 : 0;
                    values.Add(tfv);
                }
            }

            return values;
        }

        /// <summary>
        /// 无返回值 返回new List<int>();
        /// </summary>
        /// <returns></returns>
        public List<int> GetIntValues(bool checkPattern = true) => GetIntValues(Value, checkPattern);

        /// <summary>
        /// 无返回值 返回new List<int>();
        /// </summary>
        /// <returns></returns>
        public List<int> GetIntValues(string resp, bool checkPattern = true) => GetStrValues(resp, checkPattern).Select(p => p.ToInt()).ToList();

        public override string ToString()
        {
            string strValue = string.Empty;
            string intValue = string.Empty;
            try
            {
                strValue = string.Join(",", GetStrValues(false));
            }
            catch (Exception e)
            {
                strValue = e.Message;
            }
            try
            {
                intValue = string.Join(",", GetIntValues(false));
            }
            catch (Exception e)
            {
                intValue = e.Message;
            }
            return $"Response:[{Template}={Value?.HexStrFormat()}]  hex=({strValue})  int=({intValue})";
        }
    }
}