using Limxc.Tools.DeviceComm.Extensions;
using System;
using System.Linq;

namespace Limxc.Tools.DeviceComm.Contracts
{
    /// <summary>
    /// 自定义版指令
    /// </summary>
    public class CommCmd
    {
        /// <summary>
        /// 初始化指令模板,占位符 $n n=1-9位 length=n*2
        /// </summary>
        /// <param name="command"></param>
        /// <param name="respTemplate"></param>
        /// <param name="cmdDesc"></param>
        /// <param name="respDesc"></param>
        public CommCmd(string command, string respTemplate, string cmdDesc = "", string respDesc = "")
        {
            Desc = cmdDesc;
            Template = command.Replace(" ", "").ToUpper();

            //校验
            if (Template.Length <= 0 || Template.Length % 2 != 0) throw new Exception($"指令格式错误{Template}");

            Response = new CommResp(respTemplate, respDesc);
        }

        /// <summary>
        /// 返回值
        /// </summary>
        public CommResp Response { get; }

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
                throw new Exception($"指令模板与参数不匹配({Desc}|{Template}|{string.Join(",", pars)}): {ex.Message}");
            }
        }

        public override string ToString()
        {
            return $"Command:[ 描述:{Desc} 指令:{Template.HexStrFormat()}]"
                + Environment.NewLine
                + Response.ToString();
        }
    }
}