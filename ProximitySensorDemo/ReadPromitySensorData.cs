using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProximitySensorDemo
{
    class ReadPromitySensorData
    {
        public delegate void ThreadDelegate();
        SerialPortUtil serialPortUtilIns = new SerialPortUtil();
        public SerialPortUtil SerialPortUtilIns
        {
            get { return serialPortUtilIns; }
            set { serialPortUtilIns = value; }
        }

        string promitySensorDataStartCmd = "G";
        string promitySensorDataStopCmd = "D";
        string promitySensorDataAggregateStartCmd = "A";
        byte[] headArray = new byte[] { 0xF1, 0xF2, 0xF3 };
        byte[] endArray = new byte[] { 0xFF };
        byte[] headArray2 = new byte[] { 0xF6, 0x28 };
        byte[] endArray2 = new byte[] { 0xFF };

        static ReadPromitySensorData instance;
        internal static ReadPromitySensorData Instance
        {
            get 
            {
                if (instance == null)
                {
                    instance = new ReadPromitySensorData();
                }
                
                return ReadPromitySensorData.instance; 
            }
        }


        //ThreadDelegate asyncReadSensorDataDel;
        //IAsyncResult asyncReadSensorDataResult;
        //public void AsyncReadSensorDataStart()
        //{

        //}

        public void OpenPort()
        {
            serialPortUtilIns.OpenPort();
        }

        public void StartReadSensorData()
        {
            serialPortUtilIns.FrameDataLength = 82;
            serialPortUtilIns.HeadArray = headArray;
            serialPortUtilIns.EndArray = endArray;
            serialPortUtilIns.WriteData(promitySensorDataStartCmd);
            serialPortUtilIns.RegeisterHandleContinusFrame();
        }

        public void StartReadSensorAggregateData()
        {
            serialPortUtilIns.FrameDataLength = 1;
            serialPortUtilIns.HeadArray = headArray2;
            serialPortUtilIns.EndArray = endArray2;
            serialPortUtilIns.WriteData(promitySensorDataAggregateStartCmd);
            serialPortUtilIns.RegeisterHandleContinusFrame();
        }

        public void StopReadSensorData()
        {
            serialPortUtilIns.WriteData(promitySensorDataStopCmd);
        }

        public void ClosePort()
        {
            serialPortUtilIns.ClosePort();
        }
    }
}
