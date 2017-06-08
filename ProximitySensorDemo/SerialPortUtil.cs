using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Windows.Forms;

namespace ProximitySensorDemo
{
    /// <summary>
    /// 串口开发辅助类
    /// </summary>
    //public class SerialPortUtil<T> where T : IComparable<T>  //想用泛型做头尾的参数，但是运算操作符无法应用于“T”和“T”类型的操作数，其实字符串也可以转成字节型  
    public class SerialPortUtil
    {
        /// <summary>
        /// 接收事件是否有效 false表示有效
        /// </summary>
        public bool ReceiveEventFlag = false;
        /// <summary>
        /// 结束符比特
        /// </summary>
        public byte EndByte = 0x23;//string End = "#";

        /// <summary>
        /// Event Data Class
        /// </summary>
        public class DataReceivedEventArgs : EventArgs   
        {
            //public string DataReceived;
            //public DataReceivedEventArgs(string m_DataReceived)
            //{
            //    this.DataReceived = m_DataReceived;
            //}

            public byte[] receivedData;
            public DataReceivedEventArgs(byte[] m_DataReceived)
            {
                this.receivedData = m_DataReceived;
            }
        }

        public class DataReceivedEventArgs2 : EventArgs
        {
            public List<byte> receivedData;
            public DataReceivedEventArgs2(List<byte> m_DataReceived)
            {
                this.receivedData = m_DataReceived;
            }
        }

        public delegate void DataUpdatedEventHandler(DataReceivedEventArgs e);  //define delegate
        public event DataUpdatedEventHandler DataUpdated;
        public delegate void DataUpdatedEventHandler2(DataReceivedEventArgs2 e);  //define delegate
        public event DataUpdatedEventHandler2 DataUpdated2;
        public event SerialErrorReceivedEventHandler Error;

        #region 变量属性，给出默认值
        private string _portName = "COM1";//串口号，默认COM1
        private SerialPortBaudRates _baudRate = SerialPortBaudRates.BaudRate_9600;//波特率
        private Parity _parity = Parity.None;//校验位
        private StopBits _stopBits = StopBits.One;//停止位
        private SerialPortDatabits _dataBits = SerialPortDatabits.EightBits;//数据位

        private SerialPort comPort = new SerialPort();

        /// <summary>
        /// 串口号
        /// </summary>
        public string PortName
        {
            get { return _portName; }
            set { _portName = value; }
        }

        /// <summary>
        /// 波特率
        /// </summary>
        public SerialPortBaudRates BaudRate
        {
            get { return _baudRate; }
            set { _baudRate = value; }
        }

        /// <summary>
        /// 奇偶校验位
        /// </summary>
        public Parity Parity
        {
            get { return _parity; }
            set { _parity = value; }
        }

        /// <summary>
        /// 数据位
        /// </summary>
        public SerialPortDatabits DataBits
        {
            get { return _dataBits; }
            set { _dataBits = value; }
        }

        /// <summary>
        /// 停止位
        /// </summary>
        public StopBits StopBits
        {
            get { return _stopBits; }
            set { _stopBits = value; }
        }

        //T[] headArray;
        //public T[] HeadArray
        //{
        //    get { return headArray; }
        //    set { headArray = value; }
        //}

        //T[] endArray;
        //public T[] EndArray
        //{
        //    get { return endArray; }
        //    set { endArray = value; }
        //}

        byte[] headArray;
        public byte[] HeadArray
        {
            get { return headArray; }
            set { headArray = value; }
        }

        byte[] endArray;
        public byte[] EndArray
        {
            get { return endArray; }
            set { endArray = value; }
        }

        List<byte> acutalReceiveData;
        public List<byte> AcutalReceiveData
        {
            get { return acutalReceiveData; }
            set { acutalReceiveData = value; }
        }

        int frameDataLength; //一帧数据大小
        public int FrameDataLength
        {
            get { return frameDataLength; }
            set { frameDataLength = value; }
        }

        private bool listening = false; //是否没有执行完invoke相关操作  
        private bool closing = false; //是否正在关闭串口，执行Application.DoEvents，并阻止再次invoke  
        #endregion

        #region 构造函数

        /// <summary>
        /// 参数构造函数（使用枚举参数构造）
        /// </summary>
        /// <param name="name">串口号</param>
        /// <param name="baud">波特率</param>
        /// <param name="par">奇偶校验位</param>
        /// <param name="sBits">停止位</param>
        /// <param name="dBits">数据位</param>
        public SerialPortUtil(string name, SerialPortBaudRates baud, Parity par, SerialPortDatabits dBits, StopBits sBits)
        {
            _portName = name;
            _baudRate = baud;
            _parity = par;
            _dataBits = dBits;
            _stopBits = sBits;

            //comPort.DataReceived += new SerialDataReceivedEventHandler(comPort_DataReceived);
            //comPort.ErrorReceived += new SerialErrorReceivedEventHandler(comPort_ErrorReceived);
        }

        /// <summary>
        /// 参数构造函数（使用字符串参数构造）
        /// </summary>
        /// <param name="name">串口号</param>
        /// <param name="baud">波特率</param>
        /// <param name="par">奇偶校验位</param>
        /// <param name="sBits">停止位</param>
        /// <param name="dBits">数据位</param>
        public SerialPortUtil(string name, string baud, string par, string dBits, string sBits)
        {
            _portName = name;
            _baudRate = (SerialPortBaudRates)Enum.Parse(typeof(SerialPortBaudRates), baud);
            _parity = (Parity)Enum.Parse(typeof(Parity), par);
            _dataBits = (SerialPortDatabits)Enum.Parse(typeof(SerialPortDatabits), dBits);
            _stopBits = (StopBits)Enum.Parse(typeof(StopBits), sBits);

            //comPort.DataReceived += new SerialDataReceivedEventHandler(comPort_DataReceived);
            //comPort.ErrorReceived += new SerialErrorReceivedEventHandler(comPort_ErrorReceived);
        }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public SerialPortUtil()
        {
            _portName = "COM1";
            _baudRate = SerialPortBaudRates.BaudRate_9600;
            _parity = Parity.None;
            _dataBits = SerialPortDatabits.EightBits;
            _stopBits = StopBits.One;

            //comPort.DataReceived += new SerialDataReceivedEventHandler(comPort_DataReceived);
            //comPort.ErrorReceived += new SerialErrorReceivedEventHandler(comPort_ErrorReceived);
        }

        public SerialPortUtil(SerialPort sp)
        {
            this.comPort = sp;
        }

	    #endregion


        //public void RegeisterHandle(T[] headArray, T[] endArray)
        //{
        //    this.headArray = headArray;
        //    this.endArray = endArray;
        //    comPort.DataReceived += new SerialDataReceivedEventHandler(comPort_DataReceived);
        //    comPort.ErrorReceived += new SerialErrorReceivedEventHandler(comPort_ErrorReceived);
        //}

        public void RegeisterHandleOneFrame()
        {
            comPort.DataReceived += new SerialDataReceivedEventHandler(comPort_DataReceived);
            comPort.ErrorReceived += new SerialErrorReceivedEventHandler(comPort_ErrorReceived);
        }

        public void RegeisterHandleContinusFrame()
        {
            comPort.DataReceived += new SerialDataReceivedEventHandler(comPort_DataReceived2);
            comPort.ErrorReceived += new SerialErrorReceivedEventHandler(comPort_ErrorReceived);
        }

    

        /// <summary>
        /// 端口是否已经打开
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return comPort.IsOpen;
            }
        }

        /// <summary>
        /// 打开端口
        /// </summary>
        /// <returns></returns>
        public void OpenPort()
        {
            if (comPort.IsOpen) comPort.Close();

            comPort.PortName = _portName;
            comPort.BaudRate = (int)_baudRate;
            comPort.Parity = _parity;
            comPort.DataBits = (int)_dataBits;
            comPort.StopBits = _stopBits;

            comPort.Open();
        }

        /// <summary>
        /// 关闭端口
        /// </summary>
        public void ClosePort()
        {
            if (comPort.IsOpen)
            {
                closing = true;
                while (listening) Application.DoEvents();  
                comPort.Close();
                closing = false;  
            }
        }

        /// <summary>
        /// 丢弃来自串行驱动程序的接收和发送缓冲区的数据
        /// </summary>
        public void DiscardBuffer()
        {
            comPort.DiscardInBuffer();
            comPort.DiscardOutBuffer();
        }

        /// <summary>
        /// 数据接收处理，解析数据，发送完一次命令，等待内容返回，返回内容并不是持续的，只有一帧
        /// </summary>
        void comPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (closing) return;  //如果正在关闭，忽略操作，直接返回，尽快的完成串口监听线程的一次循环 
            //禁止接收事件时直接退出
            if (ReceiveEventFlag) return;

            listening = true;//设置标记，说明我已经开始处理数据，一会儿要使用系统UI的。  
            #region 根据结束字节来判断是否全部获取完成
            List<byte> _byteData = new List<byte>();
            //bool found = false;//是否检测到结束符号
            int headLength = headArray.Length;
            int endLength = endArray.Length;
            while (comPort.BytesToRead > 0) //|| !found
            {
                byte[] readBuffer = new byte[comPort.ReadBufferSize + 1];   //注意ReadBufferSize，不是BytesToRead 
                int count = comPort.Read(readBuffer, 0, comPort.ReadBufferSize);
                for (int i = 0; i < count; i++)
                {
                    _byteData.Add(readBuffer[i]);  //累积每次到达的数据

                    //if (readBuffer[i] == EndByte)
                    //{
                    //    found = true;
                    //}
               
                }
            }
         
            #endregion

            bool headFlag = true, endFlag = true;
            int receiveDataLength = _byteData.Count;
            for (int i = 0; i < headLength; i++)   //验证头
            {
                if (_byteData[i] != headArray[i])
                {
                    headFlag = false;
                }
            }

            for (int i = 0; i < endLength; i++)    //验证尾
            {
                if (_byteData[receiveDataLength - endLength + i] != endArray[i])
                {
                    endFlag = false;
                }
            }

            if (headFlag && endFlag)  //头尾正确
            {

                for (int i = 0; i < receiveDataLength - headLength - endLength; i++)
                {
                    AcutalReceiveData.Add(_byteData[i + headLength]);
                }
            }
            else
            {
                AcutalReceiveData = new List<byte>();
            }
            if (DataUpdated2 != null) DataUpdated2(new DataReceivedEventArgs2(AcutalReceiveData));
            listening = false; //我用完了，ui可以关闭串口了
            
            ////字符转换
            //string readString = System.Text.Encoding.Default.GetString(_byteData.ToArray(), 0, _byteData.Count);
            
            ////触发整条记录的处理，将收到的内容赋给字段
            //if (DataReceived != null)
            //{
            //    DataReceived(new DataReceivedEventArgs(readString));  //？没有将DataReceived事件和处理函数绑定
            //}
        }

        byte[] frameByteArray;  //用来临时存放一帧数据，判断是否是正确帧的数组

        public byte[] FrameByteArray
        {
            get { return frameByteArray; }
            set { frameByteArray = value; }
        }
        List<byte> receivedByteBuffer = new List<byte>();
        /// <summary>
        /// 发送一个命令后，得到连续的数据帧
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comPort_DataReceived2(object sender, SerialDataReceivedEventArgs e)
        {
            if (closing) return;  //如果正在关闭，忽略操作，直接返回，尽快的完成串口监听线程的一次循环 

            //禁止接收事件时直接退出
            if (ReceiveEventFlag) return;

            listening = true;//设置标记，说明我已经开始处理数据，一会儿要使用系统UI的  
            frameByteArray = new byte[FrameDataLength];
            int n = 0;
            n = comPort.BytesToRead;
            byte[] receivedByteArray = new byte[n];
            receivedByteBuffer.AddRange(receivedByteArray);
            while (receivedByteBuffer.Count>=1)
            {
                bool headFlag = true, endFlag = true;
                int headLength = headArray.Length;
                int endLength = endArray.Length;
                for (int i = 0; i < headLength; i++)
                {
                    if (receivedByteBuffer[i] != headArray[i])
                    {
                        headFlag = false;
                    }
                }
                if (headFlag)  //头正确
                {
                    if (receivedByteBuffer.Count < FrameDataLength) break;
                    for (int i = 0; i < endLength; i++)
                    {
                        if (receivedByteBuffer[FrameDataLength - endLength + i] != endArray[i])
                        {
                            endFlag = false;
                        }
                    }
                    if (endFlag)  //尾正确
                    {
                        receivedByteBuffer.CopyTo(0, FrameByteArray, 0, FrameDataLength);  
                        if(DataUpdated!=null) DataUpdated(new DataReceivedEventArgs(FrameByteArray));  //触发事件就不用读加锁了
                        receivedByteBuffer.RemoveRange(0, FrameDataLength);
                    }
                }
                else
                {
                    receivedByteBuffer.RemoveRange(0, headLength);
                }
            }
            listening = false;//我用完了，ui可以关闭串口了
        }

        /// <summary>
        /// 错误处理函数
        /// </summary>
        void comPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            if (Error != null)
            {
                Error(sender, e);  //没有将Error事件和处理函数绑定，直接调用API默认的事件处理函数
            }
        }

        #region 数据写入操作

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="msg">写入字符串</param>
        public void WriteData(string msg)
        {
            if (!(comPort.IsOpen)) comPort.Open();

            comPort.Write(msg);
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="msg">写入端口的字节数组</param>
        public void WriteData(byte[] msg)
        {
            if (!(comPort.IsOpen)) comPort.Open();

            comPort.Write(msg, 0, msg.Length);
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="msg">包含要写入端口的字节数组</param>
        /// <param name="offset">参数从0字节开始的字节偏移量</param>
        /// <param name="count">要写入的字节数</param>
        public void WriteData(byte[] msg, int offset, int count)
        {
            if (!(comPort.IsOpen)) comPort.Open();

            comPort.Write(msg, offset, count);
        }

        /// <summary>
        /// 发送串口命令，一问一答，接收字节型返回数据，如果是字符串的话，头尾转化成ASCII码传入，返回的字节数据再转化成字符串
        /// </summary>
        /// <param name="SendData">发送数据</param>
        /// <param name="ReceiveData">接收数据</param>
        /// <param name="Overtime">延时计数</param>
        /// <returns></returns>
        public int SendCommandAndReceiveData(byte[] SendData, ref byte[] ReceiveData, int Overtime, int wantedReceiveDataLength, byte[] headArray, byte[] endArray, out List<byte> actualReceiveData)
        {
            if (!(comPort.IsOpen)) comPort.Open();

            ReceiveEventFlag = true;        //关闭接收事件

            comPort.DiscardInBuffer();      //清空接收缓冲区                 
            comPort.Write(SendData, 0, SendData.Length);
            
            int num = 0, ret = 0;
            while (num++ < Overtime)  //延时等待
            {
                if (comPort.BytesToRead >= ReceiveData.Length) break;
                System.Threading.Thread.Sleep(1);
            }

            actualReceiveData = new List<byte>();
            if (comPort.BytesToRead >= ReceiveData.Length)  //数据已到达全
            {
                ret = comPort.Read(ReceiveData, 0, ReceiveData.Length);  //总体接收
                bool headFlag = true, endFlag = true;
                int receiveDataLength = ReceiveData.Length;
                int headLength = headArray.Length;
                int endLength = endArray.Length;
                int i = 0;
                if (receiveDataLength==wantedReceiveDataLength )  //验证数据
                {
                    
                    for ( i = 0; i < headLength; i++)   //验证头
                    {
                        if (ReceiveData[i]!=headArray[i])
                        {
                            headFlag = false;
                        }
                    }
                 
                    for (i = 0; i < endLength; i++)
                    {
                        if (ReceiveData[receiveDataLength - endLength + i] != endArray[i])  //验证尾
                        {
                            endFlag = false;
                        }
                    }
                }
                if (headFlag && endFlag)
                {
                   
                    for (i = 0; i < receiveDataLength-headLength-endLength; i++)
                    {
                        actualReceiveData.Add(ReceiveData[i + headLength]);
                    }

                    ReceiveEventFlag = false;       //打开事件
                    return 1;
                }
            }

            ReceiveEventFlag = false;       //打开事件
            return 0;

            //ReceiveEventFlag = false;       //打开事件
            //return ret;  //直接return API的返回值
        }

        #endregion

        #region 常用的列表数据获取和绑定操作

        /// <summary>
        /// 封装获取串口号列表
        /// </summary>
        /// <returns></returns>
        public static string[] GetPortNames()
        {
            return SerialPort.GetPortNames();
        }

        /// <summary>
        /// 设置串口号
        /// </summary>
        /// <param name="obj"></param>
        public static void SetPortNameValues(ComboBox obj)
        {
            obj.Items.Clear();
            foreach (string str in SerialPort.GetPortNames())
            {
                obj.Items.Add(str);
            }
        }

        /// <summary>
        /// 设置波特率
        /// </summary>
        public static void SetBauRateValues(ComboBox obj)
        {
            obj.Items.Clear();
            foreach (SerialPortBaudRates rate in Enum.GetValues(typeof(SerialPortBaudRates)))
            {
                obj.Items.Add(((int)rate).ToString());
            }
        }

        /// <summary>
        /// 设置数据位
        /// </summary>
        public static void SetDataBitsValues(ComboBox obj)
        {
            obj.Items.Clear();
            foreach (SerialPortDatabits databit in Enum.GetValues(typeof(SerialPortDatabits)))
            {
                obj.Items.Add(((int)databit).ToString());
            }
        }

        /// <summary>
        /// 设置校验位列表
        /// </summary>
        public static  void SetParityValues(ComboBox obj)
        {
            obj.Items.Clear();
            foreach (string str in Enum.GetNames(typeof(Parity)))
            {
                obj.Items.Add(str);
            }
            //foreach (Parity party in Enum.GetValues(typeof(Parity)))
            //{
            //    obj.Items.Add(((int)party).ToString());
            //}
        }

        /// <summary>
        /// 设置停止位
        /// </summary>
        public static void SetStopBitValues(ComboBox obj)
        {
            obj.Items.Clear();
            foreach (string str in Enum.GetNames(typeof(StopBits)))
            {
                obj.Items.Add(str);
            }
            //foreach (StopBits stopbit in Enum.GetValues(typeof(StopBits)))
            //{
            //    obj.Items.Add(((int)stopbit).ToString());
            //}   
        }

        #endregion

        #region 格式转换
        /// <summary>
        /// 转换十六进制字符串到字节数组
        /// </summary>
        /// <param name="msg">待转换字符串</param>
        /// <returns>字节数组</returns>
        public static byte[] HexToByte(string msg)
        {
            msg = msg.Replace(" ", "");//移除空格

            //create a byte array the length of the
            //divided by 2 (Hex is 2 characters in length)
            byte[] comBuffer = new byte[msg.Length / 2];
            for (int i = 0; i < msg.Length; i += 2)
            {
                //convert each set of 2 characters to a byte and add to the array
                comBuffer[i / 2] = (byte)Convert.ToByte(msg.Substring(i, 2), 16);
            }

            return comBuffer;
        }

        /// <summary>
        /// 转换字节数组到十六进制字符串
        /// </summary>
        /// <param name="comByte">待转换字节数组</param>
        /// <returns>十六进制字符串</returns>
        public static string ByteToHex(byte[] comByte)
        {
            StringBuilder builder = new StringBuilder(comByte.Length * 3);
            foreach (byte data in comByte)
            {
                builder.Append(Convert.ToString(data, 16).PadLeft(2, '0').PadRight(3, ' '));
            }

            return builder.ToString().ToUpper();
        }
        #endregion

        /// <summary>
        /// 检查端口名称是否存在
        /// </summary>
        /// <param name="port_name"></param>
        /// <returns></returns>
        public static bool Exists(string port_name)
        {
            foreach (string port in SerialPort.GetPortNames()) if (port == port_name) return true;
            return false;
        }

        /// <summary>
        /// 格式化端口相关属性
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static string Format(SerialPort port)
        {
            return String.Format("{0} ({1},{2},{3},{4},{5})", 
                port.PortName, port.BaudRate, port.DataBits, port.StopBits, port.Parity, port.Handshake);
        }
    }


    /// <summary>
    /// 串口数据位列表（5,6,7,8）
    /// </summary>
    public enum SerialPortDatabits : int
    {
        FiveBits = 5,
        SixBits = 6,
        SeventhBits = 7,
        EightBits = 8
    }

    /// <summary>
    /// 串口波特率列表。
    /// 75,110,150,300,600,1200,2400,4800,9600,14400,19200,28800,38400,56000,57600,
    /// 115200,128000,230400,256000
    /// </summary>
    public enum SerialPortBaudRates : int
    {
        BaudRate_75 = 75,
        BaudRate_110 = 110,
        BaudRate_150 = 150,
        BaudRate_300 = 300,
        BaudRate_600 = 600,
        BaudRate_1200 = 1200,
        BaudRate_2400 = 2400,
        BaudRate_4800 = 4800,
        BaudRate_9600 = 9600,
        BaudRate_14400 = 14400,
        BaudRate_19200 = 19200,
        BaudRate_28800 = 28800,
        BaudRate_38400 = 38400,
        BaudRate_56000 = 56000,
        BaudRate_57600 = 57600,
        BaudRate_115200 = 115200,
        BaudRate_128000 = 128000,
        BaudRate_230400 = 230400,
        BaudRate_256000 = 256000
    }
}
