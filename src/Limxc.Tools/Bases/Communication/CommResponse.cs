using System;
using System.Collections.Generic;
using Limxc.Tools.Extensions.Communication;

namespace Limxc.Tools.Bases.Communication
{
    /// <summary>
    ///     Communication Protocol Response
    /// </summary>
    public class CommResponse
    {
        public List<string> GetStrValues()
        {
            return Value.GetValues(Template);
        }

        public List<int> GetIntValues()
        {
            return Value.GetValues(Template).ConvertAll(p => p.HexToInt());
        }

        public override string ToString()
        {
            string strValue, intValue;
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

            return $"Response:[{Template}=({Value?.HexFormat()})]  hex=({strValue})  int=({intValue})";
        }

        #region 初始化

        /// <summary>
        ///     初始化响应模板,占位符 $n n=1-9
        /// </summary>
        /// <param name="template"></param>
        public CommResponse(string template = "")
        {
            Template = template.Replace(" ", "").ToUpper();
        }

        /// <summary>
        ///     响应模板,占位符 $n n=1-9位 length=n*2
        /// </summary>
        public string Template { get; }

        /// <summary>
        ///     原始响应值
        /// </summary>
        public string Value { get; set; }

        public int Length => Template.TemplateLength();

        #endregion 初始化
    }
}