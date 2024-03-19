using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        public Form1()
        {
            InitializeComponent();
            this.FormClosed += new FormClosedEventHandler(Form1_FormClosed);
            if(port == null)
            {
                port = new SerialPort("COM4", 9600);
                port.DataReceived += port_DataRecieved;
                port.Open();
            }
        }
        private void port_DataRecieved(object sender, SerialDataReceivedEventArgs e)
        {
            String receivedData = port.ReadLine();
            String[] data = receivedData.Split(';');
            int x = int.Parse(data[0]);
            int y = int.Parse(data[1]);
            AddPointToChart(x, y);
        }
        delegate void SetChartCallBack(int x, int y);
        private void AddPointToChart(int x, int y)
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
                chart1.Series["Series1"].Points.AddXY(x, y);
                Console.WriteLine(x);
                Console.WriteLine(y);
                Console.WriteLine("------------------");
            }
        }
        void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (port != null && port.IsOpen)
            {
                port.Close();
            }
        }

    }
}
