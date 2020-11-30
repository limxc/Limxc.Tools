using GodSharp.SerialPort;
using Limxc.Tools.DeviceComm.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Limxc.Tools.DeviceComm.Utils
{
    public static class CPTool
    {
        /// <summary>
        /// 串口收发
        /// </summary>
        /// <param name="portName"></param>
        /// <param name="baudRate"></param>
        /// <param name="commandStr">指令字符串</param>
        /// <param name="readTimeout">毫秒</param>
        /// <returns></returns>
        public static string Send(string portName, int baudRate, string commandStr, int readTimeout = 100, bool debug = true)
        {
            string result = "";

            //var serialPort = new SerialPortInput(); //Event Driven

            GodSerialPort sp = null;
            try
            {
                sp = new GodSerialPort(portName, baudRate, 0);
                sp.ReadTimeout = readTimeout;
                //sp.TryReadSpanTime = 20;
                if (sp.Open())
                {
                    sp.WriteHexString(commandStr);
                    result = sp.ReadString();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (debug)
                    throw e;
                else
                    result = e.Message;
            }
            finally
            {
                sp?.Close();
            }
            return result;
        }

        /// <summary>
        /// 发送指令且返回解析结果
        /// </summary>
        /// <param name="portName"></param>
        /// <param name="baudRate"></param>
        /// <param name="cmd"></param>
        /// <param name="pars"></param>
        /// <returns></returns>
        public static List<int> SendAndParse(string portName, int baudRate, CPCmdTaskManager cmd, params int[] pars)
        {
            var ret = Send(portName, baudRate, cmd.ToCommand(pars), cmd.TimeOut, true);
            var rst = cmd.Response.GetIntValues(ret);
            Debug.WriteLine($" ****** Send Command {cmd.Desc} @ {DateTime.Now:hh:mm:ss:ffff} ({portName},{baudRate}) : [{cmd.ToCommand(pars)}] | Received: [{cmd.Response.Value}] | IntValue:[{string.Join(",", rst)}] ******");
            return rst;
        }
    }
}