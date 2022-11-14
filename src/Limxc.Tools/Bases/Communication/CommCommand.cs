using System;
using System.Linq;
using Limxc.Tools.Extensions.Communication;

namespace Limxc.Tools.Bases.Communication
{
    /// <summary>
    ///     Communication Protocol Command
    /// </summary>
    public class CommCommand
    {
        /// <summary>
        ///     初始化指令模板,占位符 $n n=1-9位 length=n*2
        /// </summary>
        /// <param name="cmdTemplate"></param>
        public CommCommand(string cmdTemplate)
        {
            Template = cmdTemplate.Replace(" ", "").ToUpper();

            //校验
            if (Template.Length == 0 || Template.Length % 2 != 0)
                throw new FormatException($"Command Format Error.{Template}");
        }

        /// <summary>
        ///     指令模板,占位符 $n n=1-9位 length=n*2
        /// </summary>
        public string Template { get; }

        public int Length => Template.TemplateLength();

        /// <summary>
        ///     输出命令 hex string
        /// </summary>
        /// <param name="pars"></param>
        /// <returns></returns>
        public string Build(params int[] pars)
        {
            try
            {
                //分割
                var array = Template.ToStrArray(2);

                //替换
                var parsIndex = 0;
                for (var i = 0; i < array.Length; i++)
                    if (array[i].StartsWith("$"))
                    {
                        var bit = Convert.ToInt32(array[i].Skip(1).FirstOrDefault().ToString());
                        array[i] = pars[parsIndex].IntToHex(bit * 2);
                        parsIndex++;
                    }

                //合并
                var command = "";
                array.ToList().ForEach(p => command += p);

                //校验
                if (Length != command.Length)
                    throw new Exception();

                return command.ToUpper();
            }
            catch (Exception e)
            {
                throw new FormatException(
                    $"Command Build Error. {Template}|{string.Join(",", pars)} | Exception:{e.Message}");
            }
        }

        public override string ToString()
        {
            return $"Command:[{Template.HexFormat()}]";
        }
    }
}