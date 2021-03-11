using System;
using System.Threading;
using RJCP.IO.Ports;

namespace Limxc.Tools.DeviceComm.Utils
{
    public class SerialPortStreamHelper
    {
        /// <summary>
        ///     最后接收到数据的时间点
        /// </summary>
        private int lastReceiveTick;

        /// <summary>
        ///     接收缓冲区
        /// </summary>
        private byte[] receiveBuffer;

        /// <summary>
        ///     接到数据的长度
        /// </summary>
        private int receiveDatalen;

        protected SerialPortStream sp;

        /// <summary>
        ///     超时检查线程运行标志
        /// </summary>
        private bool TimeoutCheckThreadIsWork;

        public bool IsOpen => sp?.IsOpen ?? false;

        /// <summary>
        ///     是否使用接收超时机制
        ///     默认为真
        ///     接收到数据后计时，计时期间收到数据，累加数据，重新开始计时。超时后返回接收到的数据。
        /// </summary>
        public bool ReceiveTimeoutEnable { get; set; } = true;

        /// <summary>
        ///     读取接收数据未完成之前的超时时间
        ///     默认128ms
        /// </summary>
        public int ReceiveTimeout { get; set; } = 128;

        /// <summary>
        ///     接收缓冲区大小
        ///     默认4K
        /// </summary>
        public int BufSize
        {
            get
            {
                if (receiveBuffer == null)
                    return 4096;
                return receiveBuffer.Length;
            }
            set => receiveBuffer = new byte[value];
        }

        public static string[] GetPortNames()
        {
            return SerialPortStream.GetPortNames();
        }

        public event EventHandler<byte[]> ReceivedEvent;

        public bool Open(string portName, int baudRate, Parity parity = Parity.None, int databits = 8,
            StopBits stopBits = StopBits.One)
        {
            sp = new SerialPortStream
            {
                PortName = portName,
                BaudRate = baudRate,
                Parity = parity,
                DataBits = databits,
                StopBits = stopBits,

                DtrEnable = true,
                RtsEnable = true
            };

            if (receiveBuffer == null) receiveBuffer = new byte[BufSize];
            sp.Open();
            sp.DataReceived += Sp_DataReceived;
            return true;
        }

        public void Close()
        {
            if (sp?.IsOpen == true)
            {
                sp.DataReceived -= Sp_DataReceived;
                sp.Close();
                if (ReceiveTimeoutEnable)
                {
                    Thread.Sleep(ReceiveTimeout);
                    ReceiveTimeoutEnable = false;
                }
            }
        }

        public void CleanUp()
        {
            sp?.Dispose();
        }

        protected void Sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var canReadBytesLen = 0;
            if (ReceiveTimeoutEnable)
            {
                while (sp.BytesToRead > 0)
                {
                    canReadBytesLen = sp.BytesToRead;
                    if (receiveDatalen + canReadBytesLen > BufSize)
                    {
                        receiveDatalen = 0;
                        throw new Exception("Serial port receives buffer overflow!");
                    }

                    var receiveLen = sp.Read(receiveBuffer, receiveDatalen, canReadBytesLen);
                    if (receiveLen != canReadBytesLen)
                    {
                        receiveDatalen = 0;
                        throw new Exception("Serial port receives exception!");
                    }

                    //Array.Copy(recviceBuffer, 0, receivedBytes, receiveDatalen, receiveLen);
                    receiveDatalen += receiveLen;
                    lastReceiveTick = Environment.TickCount;
                    if (!TimeoutCheckThreadIsWork)
                    {
                        TimeoutCheckThreadIsWork = true;
                        var thread = new Thread(ReceiveTimeoutCheckFunc)
                        {
                            Name = "ComReceiveTimeoutCheckThread"
                        };
                        thread.Start();
                    }
                }
            }
            else
            {
                if (ReceivedEvent != null)
                {
                    // 获取字节长度
                    var bytesNum = sp.BytesToRead;
                    if (bytesNum == 0)
                        return;
                    // 创建字节数组
                    var resultBuffer = new byte[bytesNum];

                    var i = 0;
                    while (i < bytesNum)
                    {
                        // 读取数据到缓冲区
                        var j = sp.Read(receiveBuffer, i, bytesNum - i);
                        i += j;
                    }

                    Array.Copy(receiveBuffer, 0, resultBuffer, 0, i);
                    ReceivedEvent(this, resultBuffer);
                }

                //Array.Clear (receivedBytes,0,receivedBytes.Length );
                receiveDatalen = 0;
            }
        }

        /// <summary>
        ///     超时返回数据处理线程方法
        /// </summary>
        protected void ReceiveTimeoutCheckFunc()
        {
            while (TimeoutCheckThreadIsWork)
                if (Environment.TickCount - lastReceiveTick > ReceiveTimeout)
                {
                    if (ReceivedEvent != null)
                    {
                        var returnBytes = new byte[receiveDatalen];
                        Array.Copy(receiveBuffer, 0, returnBytes, 0, receiveDatalen);
                        ReceivedEvent(this, returnBytes);
                    }

                    //Array.Clear (receivedBytes,0,receivedBytes.Length );
                    receiveDatalen = 0;
                    TimeoutCheckThreadIsWork = false;
                }
                else
                {
                    Thread.Sleep(16);
                }
        }

        public void Write(byte[] buffer)
        {
            if (IsOpen)
                sp.Write(buffer, 0, buffer.Length);
        }

        public void Write(string text)
        {
            if (IsOpen)
                sp.Write(text);
        }

        public void WriteLine(string text)
        {
            if (IsOpen)
                sp.WriteLine(text);
        }
    }
}