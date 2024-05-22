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
        private List<float> i1Buffer = new List<float>();
        private List<float> i2Buffer = new List<float>();
        private List<float> dBuffer = new List<float>();

        private string selectedCOMM = "";

        public Form1()
        {
            InitializeComponent();
            this.FormClosed += new FormClosedEventHandler(Form1_FormClosed);
            PopulateSerialPortList();
            NotConnected();
        }
        private void PopulateSerialPortList()
        {
            try
            {
                
                string[] ports = SerialPort.GetPortNames();
                comboBox1.Items.Clear();
                foreach (string port in ports)
                {
                    comboBox1.Items.Add(port);
                }
                if (comboBox1.Items.Count > 0)
                {
                    comboBox1.SelectedIndex = 0;
                    selectedCOMM = comboBox1.SelectedItem.ToString();
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

        private void Finished()
        {
            port.WriteLine("f");
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = true;
            comboBox1.Enabled = true;
        }
        private void Running()
        {
            ClearChart(0, 0, 0);
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            comboBox1.Enabled = false;
        }
        private void Connected()
        {
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = false;
            comboBox1.Enabled = true;
            textBox2.Enabled = true;
        }
        private void NotConnected()
        {
            button2.Enabled = true;
            comboBox1.Enabled = true;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            textBox2.Enabled = false;
        }
        private void port_DataRecieved(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                String receivedData = port.ReadLine();
                String[] data = receivedData.Split(';');
                Console.WriteLine(receivedData);
                if (data[0] == "x")
                {
                    Console.WriteLine("Done");
                    this.Invoke(new MethodInvoker(delegate () {
                        Finished();
                    }));
                    return;
                }
                float i1 = float.Parse(data[0]);
                float i2 = float.Parse(data[1]);
                float d = float.Parse(data[2]);

                AddPointToChart(TrucatedValue(i1), TrucatedValue(i2), TrucatedValue(d));

                i1Buffer.Add(i1);
                i2Buffer.Add(i2);
                dBuffer.Add(d);

            }catch(Exception ex)
            {
                this.Invoke(new MethodInvoker(delegate () {
                    messageBox.Text = ex.Message;
                }));
            }
        }
        delegate void SetChartCallBack(float i1, float i2, float d);
        private void ClearChart(float i1, float i2, float d)
        {
            if (chart1.InvokeRequired)
            {
                var cb = new SetChartCallBack(ClearChart);
                chart1.Invoke(cb, new Object[]
                {
                    i1, i2, d
                });
            }
            else
            {
                chart1.Series["c"].Points.Clear();
                chart2.Series["c"].Points.Clear();
            }
        }
        private void AddPointToChart(float i1, float i2, float d)
        {
            if(chart1.InvokeRequired)
            {
                var cb = new SetChartCallBack(AddPointToChart);
                chart1.Invoke(cb, new Object[]
                {
                    i1, i2, d
                });
            }else
            {
                chart1.Series["c"].Points.AddXY(i1, d);
                chart2.Series["c"].Points.AddXY(i2, d);
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
            try
            {
                if (port == null)
                {
                    port = new SerialPort(selectedCOMM, 115200);
                    port.DataReceived += port_DataRecieved;
                    port.Open();
                    Connected();
                    messageBox.Text = "Connected to " + selectedCOMM + " successfully!";
                }
            }catch (Exception ex)
            {
                messageBox.Text = ex.Message;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if(port != null)
            {
                port.WriteLine("o");
            }
        }
        private void ExportToCSV(string csvFilePath)
        {

            if (String.IsNullOrWhiteSpace(csvFilePath))

            {
                return;
            }

            var csv = new StringBuilder();

            csv.AppendLine("D,I1,I2");



            var dataCount = i1Buffer.Count;

            for (int i = 0; i < dataCount; i++)
            {
                csv.AppendLine($"{dBuffer[i]}, {i1Buffer[i]}, {i2Buffer[i]}");

            }

            File.WriteAllText(csvFilePath, csv.ToString());
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if(port != null)
            {
                port.WriteLine("f");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV file (*.csv)|*.csv| All Files (*.*)|*.*";
            saveFileDialog.Title = "Export csv data";
            var result = saveFileDialog.ShowDialog();
            if(result == DialogResult.OK)
            {
                var fileName = saveFileDialog.FileName;
                ExportToCSV(fileName);
            }




        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedCOMM = comboBox1.SelectedItem.ToString();
            Console.WriteLine(selectedCOMM);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
