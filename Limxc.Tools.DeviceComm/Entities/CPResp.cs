using Limxc.Tools.DeviceComm.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Limxc.Tools.DeviceComm.Entities
{
    /// <summary>
    /// 自定义版返回值
    /// </summary>
    public class CPResp
    {
        #region 初始化

        /// <summary>
        /// 初始化响应模板,占位符 $n n=1-9
        /// </summary>
        /// <param name="template"></param>
        /// <param name="desc"></param>
        public CPResp(string template = "", string desc = "")
        {
            Template = template.Replace(" ", "").ToUpper();
            Desc = desc;
        }

        /// <summary>
        /// 返回值格式描述
        /// </summary>
        public string Desc { get; }

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
                throw new FormatException($"返回值与响应模板不匹配! Template:[{Template}] Value:{resp}");

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
                strValue = string.Join(",", GetStrValues());
            }
            catch (Exception e)
            {
                strValue = e.Message;
            }
            try
            {
                intValue = string.Join(",", GetIntValues());
            }
            catch (Exception e)
            {
                intValue = e.Message;
            }
            return $"Resp:[ 描述:{Desc} 响应:{Template} 响应值:{Value?.HexStrFormat()} 解析值: hex=({strValue});int=({intValue}) ]";
        }
    }
}