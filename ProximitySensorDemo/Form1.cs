#define DebugMode
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProximitySensorDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        TextBox[] textBoxArray = new TextBox[41];
        private void Form1_Load(object sender, EventArgs e)
        {
            string textBoxPrefix = "textBoxDistanceShow"; 
            int textBoxPrefixLength = textBoxPrefix.Length;
            string contoinerPrefix="tableLayoutPanel1";
            foreach (Control containerObj in this.Controls)
            {
                string containerObjName = containerObj.Name;
                int searchIndex = containerObjName.IndexOf(contoinerPrefix);
                if (searchIndex>=0)
                {
                    foreach (Control controlObj in containerObj.Controls)  //**only get the container
                    {
                        string controlName = controlObj.Name.ToString();
                        int textBoxIndex = 0;
                        if (controlName.Contains(textBoxPrefix))
                        {
                            textBoxIndex = int.Parse(controlObj.Name.Substring(textBoxPrefixLength));
#if DebugMode
                            Console.WriteLine(textBoxIndex);
#endif
                            textBoxArray[textBoxIndex] = (TextBox)controlObj;
#if DebugMode
                            Console.WriteLine(((TextBox)controlObj).Name);
#endif       
                        }    
                    }
                    break;
                }
            }
#if DebugMode
            Console.ReadLine();
#endif
            ReadPromitySensorData.Instance.SerialPortUtilIns.OpenPort();
            ReadPromitySensorData.Instance.SerialPortUtilIns.DataUpdated += SerialPortUtilIns_DataUpdated;
        }

        int[] approximateDistanceArray = new int[41];
        const float ApproximateDistance = 1;
        bool approximateFlag = false;
        void SerialPortUtilIns_DataUpdated(SerialPortUtil.DataReceivedEventArgs e)
        {
            byte[] rawReceivedDataArray = e.receivedData;
            if (rawReceivedDataArray.Length==41)
            {

                for (int i = 0; i < 41; i++)
                {
                    approximateDistanceArray[i] = rawReceivedDataArray[i] + rawReceivedDataArray[i + 41];   //**数据按协议处理
                    if (approximateDistanceArray[i] > ApproximateDistance)
                    {
                        approximateFlag = true;
                    }
                }
                for (int i = 0; i < textBoxArray.Length; i++)
                {
                    if (textBoxArray[i].InvokeRequired)
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            textBoxArray[i].Text = approximateDistanceArray[i].ToString();
                        }));
                    }
                    else
                    {
                        textBoxArray[i].Text = approximateDistanceArray[i].ToString();
                    }
                }
                
            }
            else if (rawReceivedDataArray.Length==1)
            {
                if (rawReceivedDataArray[0] > ApproximateDistance)
                {
                    approximateFlag = true;
                }
            }

            if (approximateFlag)
            {
                if (textBoxArray[41].InvokeRequired)
                {
                    this.BeginInvoke(new Action(()=>
                    {
                        textBoxArray[41].Text = "1";
                        textBoxArray[41].BackColor = Color.Red;
                    }));
                }
                else
                {
                    textBoxArray[41].Text = "1";
                    textBoxArray[41].BackColor = Color.Red;
                }
            }
            else
            {
                if (textBoxArray[41].InvokeRequired)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        textBoxArray[41].Text = "1";
                        textBoxArray[41].BackColor = Color.Red;
                    }));
                }
                else
                {
                    textBoxArray[41].Text = "0";
                    textBoxArray[41].BackColor = SystemColors.WindowText;
                }
            }
        }

        int startStatus = 1;
        private void buttonStart_Click(object sender, EventArgs e)
        {
            
            if (startStatus == 1)
            {
                if (radioButtonSensorData.Checked)
                {
                    ReadPromitySensorData.Instance.StartReadSensorData();
                }
                else if (radioButtonSensorDataAggregate.Checked)
                {
                    ReadPromitySensorData.Instance.StartReadSensorAggregateData();
                }
            }
            else
            {
                ReadPromitySensorData.Instance.StopReadSensorData();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            ReadPromitySensorData.Instance.SerialPortUtilIns.ClosePort();
        }

    }
}
