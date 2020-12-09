using Limxc.Tools.DeviceComm.Extensions;
using System;
using System.Linq;

namespace Limxc.Tools.DeviceComm.Entities
{
    /// <summary>
    /// Communication Protocol Command
    /// </summary>
    public class CPCmd
    {
        /// <summary>
        /// 初始化指令模板,占位符 $n n=1-9位 length=n*2
        /// </summary>
        /// <param name="cmdTemplate"></param>
        /// <param name="respTemplate"></param>
        /// <param name="desc"></param>
        public CPCmd(string cmdTemplate, string respTemplate, string desc = "")
        {
            Desc = desc;
            Template = cmdTemplate.Replace(" ", "").ToUpper();

            //校验
            if (Template.Length <= 0 || Template.Length % 2 != 0) throw new FormatException($"Command Format Error.{Template}");

            Response = new CPResp(respTemplate);
        }

        /// <summary>
        /// 返回值
        /// </summary>
        public CPResp Response { get; }

        /// <summary>
        /// 命令描述
        /// </summary>
        public string Desc { get; }

        /// <summary>
        /// 指令模板,占位符 $n n=1-9位 length=n*2
        /// </summary>
        public string Template { get; }

        public int Length => Template.TemplateLength('$');

        /// <summary>
        /// 输出命令
        /// </summary>
        /// <param name="pars"></param>
        /// <returns></returns>
        public string ToCommand(params int[] pars)
        {
            try
            {
                //分割
                var array = Template.ToStrArray(2);

                //替换
                int parsIndex = 0;
                for (int i = 0; i < array.Count(); i++)
                {
                    if (array[i].StartsWith("$"))
                    {
                        var bit = Convert.ToInt32(array[i].Skip(1).FirstOrDefault().ToString());
                        array[i] = pars[parsIndex].ToHexStr(bit * 2);
                        parsIndex++;
                    }
                }

                //合并
                var command = "";
                array.ToList().ForEach(p => command += p);

                //校验
                if (Length != command.Length)
                    throw new Exception();

                return command.ToUpper();
            }
            catch (Exception ex)
            {
                throw new FormatException($"Command Build Error. {Desc}|{Template}|{string.Join(",", pars)}");
            }
        }

        public override string ToString()
        {
            return $"Command({Desc}):[{Template.HexStrFormat()}]    |    {Response?.ToString()}";
        }
    }
}