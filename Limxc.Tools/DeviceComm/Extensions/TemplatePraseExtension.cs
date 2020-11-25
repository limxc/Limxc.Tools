using System;
using System.Collections.Generic;
using System.Text;

namespace Limxc.Tools.DeviceComm.Extensions
{
    public static class TemplatePraseExtension
    {
        /// <summary>
        /// 计算指令长度,$n 0-9位
        /// </summary>
        /// <param name="cmd"></param>
        public static int TemplateLength(this string cmd, char sep = '$')
        {
            try
            {
                cmd = cmd.Replace(" ", "");
                var arr = cmd.ToCharArray();
                int totalLen = arr.Length;
                for (int i = 0; i < arr.Length - 1; i++)
                {
                    if (arr[i] == sep)
                    {
                        var len = Convert.ToInt32(arr[i + 1].ToString());
                        len = len > 1 ? len - 1 : 0;
                        totalLen += len * 2;
                    }
                }
                return totalLen;
            }
            catch
            {
                return -1;
            }
        }
    }
}
