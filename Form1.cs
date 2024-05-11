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
        private float area = 10;
        private float maxI = 0;
        private float maxV = 0;
        private double maxP = 0;
        private List<float> iBuffer = new List<float>();
        private List<float> vBuffer = new List<float>();
        private List<double> pBuffer = new List<double>();

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
        private void UpdateTextField(string text)
        {
            if (textBox1.InvokeRequired)
            {
                textBox1.Invoke(new Action<string>(UpdateTextField), ((int)float.Parse(text)).ToString());
            }
            else
            {
                textBox1.Text = text;
            }
        }
        private void Finished()
        {
            port.WriteLine("f");
            double v = maxP / (maxI * maxV * 0.001);
            double e = (maxP * 10) / area;
            F_Score.Text = TrucatedValue((float)v).ToString();
            Eff.Text = TrucatedValue((float)(e * 100)).ToString() + "%";
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = true;
            textBox1.Enabled = true;
            comboBox1.Enabled = true;
        }
        private void Running()
        {
            ClearChart(0, 0);
            F_Score.Text = String.Empty;
            Eff.Text = String.Empty;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            textBox1.Enabled = false;
            comboBox1.Enabled = false;
        }
        private void Connected()
        {
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = false;
            textBox1.Enabled = true;
            comboBox1.Enabled = true;
        }
        private void NotConnected()
        {
            button2.Enabled = true;
            comboBox1.Enabled = true;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            textBox1.Enabled = false;
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
                float x = float.Parse(data[0]);
                float y = float.Parse(data[1]);

                if (x > maxV)
                {
                    maxV = x;
                }
                if (y > maxI)
                {
                    maxI = y;
                }
                if (x * y * 0.001 > maxP)
                {
                    maxP = x * y * 0.001;
                }
                UpdateTextField(data[2]);

                AddPointToChart(TrucatedValue(x), TrucatedValue(y));

                vBuffer.Add(x);
                iBuffer.Add(y);
                pBuffer.Add(x * y * 0.001);
            }catch(Exception ex)
            {
                this.Invoke(new MethodInvoker(delegate () {
                    messageBox.Text = ex.Message;
                }));
            }
        }
        delegate void SetChartCallBack(float x, float y);
        private void ClearChart(float x, float y)
        {
            if (chart1.InvokeRequired)
            {
                var cb = new SetChartCallBack(ClearChart);
                chart1.Invoke(cb, new Object[]
                {
                    x, y
                });
            }
            else
            {
                chart1.Series["c"].Points.Clear();
                chart2.Series["p"].Points.Clear();

            }
        }
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
                chart1.Series["c"].Points.AddXY(x, y/area);
                chart2.Series["p"].Points.AddXY(x, x * y * 0.001);

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
            if(port != null)
            {
                try
                {
                    area = float.Parse(textBox2.Text);
                } catch(Exception)
                {

                }
                if(area > 0)
                {
                    Running();
                    port.WriteLine("a");
                }
            }
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

            csv.AppendLine("V,I,P");



            var dataCount = iBuffer.Count;

            for (int i = 0; i < dataCount; i++)
            {
                csv.AppendLine($"{vBuffer[i]}, {iBuffer[i]}, {pBuffer[i]}");

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
