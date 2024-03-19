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
            Console.WriteLine(receivedData);
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
