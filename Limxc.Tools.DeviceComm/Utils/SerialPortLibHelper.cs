using RJCP.IO.Ports;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Limxc.Tools.DeviceComm.Utils
{
    public class ConnectionStatusChangedEventArgs
    {
        public readonly bool Connected;

        public ConnectionStatusChangedEventArgs(bool state)
        {
            Connected = state;
        }
    }

    public class MessageReceivedEventArgs
    {
        public readonly byte[] Data;

        public MessageReceivedEventArgs(byte[] data)
        {
            Data = data;
        }
    }

    /// <summary>
    /// Serial port I/O
    /// </summary>
    public class SerialPortLibHelper

    {
        private Action<string> Log { get; }

        #region Private Fields

        private SerialPortStream _serialPort;
        private string _portName = "";
        private int _baudRate = 115200;
        private StopBits _stopBits = StopBits.One;
        private Parity _parity = Parity.None;
        private int _dataBits = 8;

        // Read/Write error state variable
        private bool gotReadWriteError = true;

        // Serial port reader task
        private Thread reader;

        private CancellationTokenSource readerCts;

        // Serial port connection watcher
        private Thread connectionWatcher;

        private CancellationTokenSource connectionWatcherCts;

        private object accessLock = new object();
        private bool disconnectRequested = false;

        #endregion Private Fields

        #region Public Events

        /// <summary>
        /// Connected state changed event.
        /// </summary>
        public delegate void ConnectionStatusChangedEventHandler(object sender, ConnectionStatusChangedEventArgs args);

        /// <summary>
        /// Occurs when connected state changed.
        /// </summary>
        public event ConnectionStatusChangedEventHandler ConnectionStatusChanged;

        /// <summary>
        /// Message received event.
        /// </summary>
        public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs args);

        /// <summary>
        /// Occurs when message received.
        /// </summary>
        public event MessageReceivedEventHandler MessageReceived;

        private void OnConnectionStatusChanged(ConnectionStatusChangedEventArgs args) => ConnectionStatusChanged?.Invoke(this, args);

        private void OnMessageReceived(MessageReceivedEventArgs args) => MessageReceived?.Invoke(this, args);

        #endregion Public Events

        #region Public Members

        public SerialPortLibHelper()
        {
            connectionWatcherCts = new CancellationTokenSource();
            readerCts = new CancellationTokenSource();
        }

        /// <summary>
        /// Connect to the serial port.
        /// </summary>
        public bool Connect()
        {
            if (disconnectRequested)
                return false;
            lock (accessLock)
            {
                Disconnect();
                Open();
                connectionWatcherCts = new CancellationTokenSource();
                connectionWatcher = new Thread(ConnectionWatcherTask);
                connectionWatcher.Start(connectionWatcherCts.Token);
            }
            return IsConnected;
        }

        /// <summary>
        /// Disconnect the serial port.
        /// </summary>
        public void Disconnect()
        {
            if (disconnectRequested)
                return;
            disconnectRequested = true;
            Close();
            lock (accessLock)
            {
                if (connectionWatcher != null)
                {
                    if (!connectionWatcher.Join(5000))
                        connectionWatcherCts.Cancel();
                    connectionWatcher = null;
                }
                disconnectRequested = false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the serial port is connected.
        /// </summary>
        /// <value><c>true</c> if connected; otherwise, <c>false</c>.</value>
        public bool IsConnected
        {
            get { return _serialPort != null && !gotReadWriteError && !disconnectRequested; }
        }

        /// <summary>
        /// Sets the serial port options.
        /// </summary>
        /// <param name="portName">Portname.</param>
        /// <param name="baudRate">Baudrate.</param>
        /// <param name="stopBits">Stopbits.</param>
        /// <param name="parity">Parity.</param>
        /// <param name="dataBits">Databits.</param>
        public void SetPort(string portName, int baudRate = 115200, StopBits stopBits = StopBits.One, Parity parity = Parity.None, int dataBits = 8)
        {
            if (_portName != portName || _baudRate != baudRate || stopBits != _stopBits || parity != _parity || dataBits != _dataBits)
            {
                // Change Parameters request
                // Take into account immediately the new connection parameters
                // (do not use the ConnectionWatcher, otherwise strange things will occurs !)
                _portName = portName;
                _baudRate = baudRate;
                _stopBits = stopBits;
                _parity = parity;
                _dataBits = dataBits;
                if (IsConnected)
                {
                    Connect();      // Take into account immediately the new connection parameters
                }
                Log?.Invoke(string.Format("Port parameters changed (port name {0} / baudrate {1} / stopbits {2} / parity {3} / databits {4})", portName, baudRate, stopBits, parity, dataBits));
            }
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <returns><c>true</c>, if message was sent, <c>false</c> otherwise.</returns>
        /// <param name="message">Message.</param>
        public bool SendMessage(byte[] message)
        {
            bool success = false;
            if (IsConnected)
            {
                try
                {
                    _serialPort.Write(message, 0, message.Length);
                    success = true;
                    Log?.Invoke(BitConverter.ToString(message));
                }
                catch (Exception e)
                {
                    Log?.Invoke(e.ToString());
                }
            }
            return success;
        }

        #endregion Public Members

        #region Private members

        #region Serial Port handling

        private bool Open()
        {
            bool success = false;
            lock (accessLock)
            {
                Close();
                try
                {
                    bool tryOpen = true;

                    var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

                    if (!isWindows)
                    {
                        tryOpen = (tryOpen && System.IO.File.Exists(_portName));
                    }
                    if (tryOpen)
                    {
                        _serialPort = new SerialPortStream();

                        _serialPort.ErrorReceived += HandleErrorReceived;
                        _serialPort.PortName = _portName;
                        _serialPort.BaudRate = _baudRate;
                        _serialPort.StopBits = _stopBits;
                        _serialPort.Parity = _parity;
                        _serialPort.DataBits = (int)_dataBits;

                        // We are not using serialPort.DataReceived event for receiving data since this is not working under Linux/Mono.
                        // We use the readerTask instead (see below).
                        _serialPort.Open();
                        success = true;
                    }
                }
                catch (Exception e)
                {
                    Log?.Invoke(e.ToString());
                    Close();
                }
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    gotReadWriteError = false;
                    // Start the Reader task
                    readerCts = new CancellationTokenSource();
                    reader = new Thread(ReaderTask);
                    reader.Start(readerCts.Token);
                    OnConnectionStatusChanged(new ConnectionStatusChangedEventArgs(true));
                }
            }
            return success;
        }

        private void Close()
        {
            lock (accessLock)
            {
                // Stop the Reader task
                if (reader != null)
                {
                    if (!reader.Join(5000))
                        readerCts.Cancel();
                    reader = null;
                }
                if (_serialPort != null)
                {
                    _serialPort.ErrorReceived -= HandleErrorReceived;
                    if (_serialPort.IsOpen)
                    {
                        _serialPort.Close();
                        OnConnectionStatusChanged(new ConnectionStatusChangedEventArgs(false));
                    }
                    _serialPort.Dispose();
                    _serialPort = null;
                }
                gotReadWriteError = true;
            }
        }

        private void HandleErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            Log?.Invoke("EventType:" + e.EventType.ToString());
        }

        #endregion Serial Port handling

        #region Background Tasks

        private void ReaderTask(object data)
        {
            var ct = (CancellationToken)data;
            while (IsConnected && !ct.IsCancellationRequested)
            {
                int msglen = 0;

                try
                {
                    msglen = _serialPort.BytesToRead;
                    if (msglen > 0)
                    {
                        byte[] message = new byte[msglen];

                        int readbytes = 0;
                        while (_serialPort.Read(message, readbytes, msglen - readbytes) <= 0)
                            ;
                        if (MessageReceived != null)
                        {
                            OnMessageReceived(new MessageReceivedEventArgs(message));
                        }
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
                catch (Exception e)
                {
                    Log?.Invoke(e.ToString());
                    gotReadWriteError = true;
                    Thread.Sleep(1000);
                }
            }
        }

        /// <summary>
        /// 重连
        /// </summary>
        /// <param name="data"></param>
        private void ConnectionWatcherTask(object data)
        {
            var ct = (CancellationToken)data;

            while (!disconnectRequested && !ct.IsCancellationRequested)
            {
                if (gotReadWriteError)
                {
                    try
                    {
                        Close();
                        // wait 1 sec before reconnecting
                        Thread.Sleep(1000);
                        if (!disconnectRequested)
                        {
                            try
                            {
                                Open();
                            }
                            catch (Exception e)
                            {
                                Log?.Invoke(e.Message);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log?.Invoke(e.Message);
                    }
                }
                if (!disconnectRequested)
                    Thread.Sleep(1000);
            }
        }

        #endregion Background Tasks

        #endregion Private members
    }
}