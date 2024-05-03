using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace GUI_Serial_Interface
{
    public partial class Form1 : Form
    {
        SerialPort port;

        private float maxI = 0;
        private float maxV = 0;

        private List<float> iBuffer;
        private List<float> vBuffer;
        private List<double> pBuffer;
        private List<float> tempretureBuffer;
        private List<float> fBuffer;

        private string csvFilePath;

        public Form1()
        {
            InitializeComponent();
            this.FormClosed += new FormClosedEventHandler(Form1_FormClosed);
            PopulateSerialPortList();
        }
        private void PopulateSerialPortList()
        {
            try
            {
                string[] ports = SerialPort.GetPortNames();
                listBox1.Items.Clear();
                foreach (string port in ports)
                {
                    listBox1.Items.Add(port);
                }
                if (listBox1.Items.Count > 0)
                {
                    listBox1.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private float TrucatedValue(float value)
        {
            return (float) Math.Floor(value * 100f) / 100f;
        }
        private void UpdateTextField(string text)
        {
            if (textBox1.InvokeRequired)
            {
                textBox1.Invoke(new Action<string>(UpdateTextField), text);
            }
            else
            {
                textBox1.Text = text;
            }
        }
        private void port_DataRecieved(object sender, SerialDataReceivedEventArgs e)
        {
            String receivedData = port.ReadLine();
            String[] data = receivedData.Split(';');
            Console.WriteLine(receivedData);
            float x = float.Parse(data[0]);
            float y = float.Parse(data[1]);
            float temp = float.Parse(data[2]);
            UpdateTextField(temp+"");
            AddPointToChart(TrucatedValue(x), TrucatedValue(y));

            iBuffer.Add(x);
            vBuffer.Add(y);
            pBuffer.Add(x * y * 0.001);
            tempretureBuffer.Add(temp);
            fBuffer.Add(0); // Add the F equation
        }
        delegate void SetChartCallBack(float x, float y);
        private void AddPointToChart(float x, float y)
        {
            if(chart1.InvokeRequired)
            {
                var cb = new SetChartCallBack(AddPointToChart);
                chart1.Invoke(cb, new Object[]
                {
                    x, y
                });
            }else
            {
                if (x > maxI) maxI = x;
                if (y > maxV) maxV = y;
                
                chart1.Series["Current(mA)"].Points.AddXY(x, y); //X(mA) /A(cm) 
                chart2.Series["Power(W)"].Points.AddXY(x, x * y * 0.001);
            }
        }
        void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (port != null && port.IsOpen)
            {
                port.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PopulateSerialPortList();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (port == null)
            {
                port = new SerialPort("COM3", 9600);
                port.DataReceived += port_DataRecieved;
                port.Open();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            port.WriteLine("a");
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void ExportToCSV()
        {

            if(String.IsNullOrWhiteSpace(csvFilePath))
            {
                return;
            }

            var csv = new StringBuilder();

            //Init Headers
            csv.AppendLine("I,V,P,Tempreture, F");


            var dataCount = iBuffer.Count;

            for(int i = 0; i < dataCount; i++)
            {
                csv.AppendLine($"{iBuffer[i]}, {vBuffer[i]}, {pBuffer[i]}, {tempretureBuffer[i]}, {fBuffer[i]}");
            }

            File.WriteAllText(csvFilePath + "/Export.csv", csv.ToString());
        }
    }
}
