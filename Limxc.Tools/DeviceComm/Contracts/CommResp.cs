using Limxc.Tools.DeviceComm.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Limxc.Tools.DeviceComm.Contracts
{
    /// <summary>
    /// 自定义版返回值
    /// </summary>
    public class CommResp
    {
        #region 初始化

        /// <summary>
        /// 初始化响应模板,占位符 $n n=1-9
        /// </summary>
        /// <param name="template"></param>
        /// <param name="desc"></param>
        public CommResp(string template = "", string desc = "")
        {
            Template = template.Replace(" ", "").ToUpper();
            Desc = desc;

            if (string.IsNullOrWhiteSpace(Template))
                IsReceived = true;
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
        /// 标明是否已处理返回值
        /// </summary>
        public bool IsReceived { get; set; }

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
            if (string.IsNullOrWhiteSpace(Template))
                return new List<string>();

            resp = resp.Replace(" ", "");

            //校验长度
            if (Length != resp.Length)
                throw new FormatException($"返回值与响应模板不匹配! Template:[{Template}] Value:{resp}");

            var values = new List<string>();

            //根据模板获取返回值列表
            var tmplateArray = Template.ToStrArray(2);

            int skipLen = 0;
            //解析数据
            for (int i = 0; i < tmplateArray.Count(); i++)
            {
                if (tmplateArray[i].StartsWith("$"))
                {
                    //取值位数
                    var len = Convert.ToInt32(tmplateArray[i].Skip(1).FirstOrDefault().ToString());
                    var tfv = string.Join("", resp.Skip((i + skipLen) * 2).Take(len * 2));
                    skipLen += len > 1 ? len - 1 : 0;
                    values.Add(tfv);
                }
                else if(checkPattern)//校验每一位固定数值
                {
                    if (!resp.Skip((i + skipLen) * 2).Take(2).SequenceEqual(tmplateArray[i]))
                        throw new FormatException($"返回值与响应模板不匹配! Template:[{Template}] Value:{resp}");
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
            return $"Resp:[ 描述:{Desc} 响应:{Template} 响应值:{Value?.HexStrFormat()} ]";
        }
    }
}