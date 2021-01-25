using Accord.Video.FFMPEG;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        //Stop watch for graphs
        System.Diagnostics.Stopwatch mStopWatch = new System.Diagnostics.Stopwatch();
        
        //Graphs limits to update x-axis with time
        double leftLimit = 0;
        double rightLimit = 0;
        
        //Data management for graph
        double prev = 0;
        List<double> array = new List<double>();
        
        //lot and dept numbers entered by user
        String lot = "";
        String dept = "";
        
        //Screen recording variables
        private Timer timer1;
        private VideoFileWriter vf;
        private Bitmap bp;
        private Graphics gr;
        private PictureBox pictureBox1 = new PictureBox();
        private string filename;
       
        //Button text switch flags
        int first = 0;
        int second = 0;

        public Form1()
        {
    
            InitializeComponent();
            
            this.button2.Enabled = false;
            this.button3.Enabled = false;
            
            timer1 = new Timer();
            timer1.Tick += timer1_Tick;
            vf = new VideoFileWriter();

            this.chart1.Series["Force vs Time"].Points.AddXY(0, 0);
            leftLimit = this.chart1.ChartAreas[0].AxisX.Minimum;
            rightLimit = this.chart1.ChartAreas[0].AxisX.Maximum;
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(SerialPort1_DataReceived);
            //serialPort1.Open();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            bp = new Bitmap(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);
            gr = Graphics.FromImage(bp);
            gr.CopyFromScreen(0, 0, 0, 0, new Size(bp.Width, bp.Height));
            pictureBox1.Image = bp;
            pictureBox1.SizeMode = PictureBoxSizeMode.Normal;
            vf.WriteVideoFrame(bp);
         }

        private void button1_Click(object sender, EventArgs e)
        {
                System.Diagnostics.Debug.WriteLine("pressed");
            if (!String.IsNullOrEmpty(this.textBox3.Text) && dept != "")
            {
                this.label6.Visible = false;
                lot = this.textBox3.Text;
                this.button1.Enabled = false;
                this.button3.Enabled = true;
                this.button2.Enabled = false;
            }
            else
            {
                this.label6.Visible = true;
            }
                //serialPort1.Write("H");
                //System.Threading.Thread.Sleep(500);
                //serialPort1.Write(" ");
            
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(second == 0)
            {
                mStopWatch.Stop();
                this.button2.Text = "Resume";
                second = 1;
            }
            else
            {
                mStopWatch.Start();
                this.button2.Text = "Pause";
                second = 0;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(first == 0)
            {
                string pathToNewFolder = System.IO.Path.Combine("d:\\Recordings_NHT", dept);
                DirectoryInfo directory = Directory.CreateDirectory(pathToNewFolder);
                DateTime today = DateTime.Today;
                string mypath = System.IO.Path.Combine("d:\\Recordings_NHT\\" + dept, today.Date.ToString("dddd_dd MMMM yyyy "));
                String name = pathToNewFolder + "lot_" + today + ".avi";
                filename = @mypath + lot + ".avi";
                if (System.IO.File.Exists(filename))
                    System.IO.File.Delete(filename);

                this.button1.Enabled = false;
                this.button2.Enabled = true;
                mStopWatch.Start();
                if (System.IO.File.Exists(filename))System.IO.File.Delete(filename);
                vf.Open(filename, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height, 30, VideoCodec.MPEG4, 1000000);
                timer1.Start();
                //serialPort1.Write("S");
                //System.Threading.Thread.Sleep(1000);
                //serialPort1.Write(" ");
                this.button3.Text = "Stop";
                first = 1;
            }
            else
            {
                DialogResult dr = MessageBox.Show("Stopping will end and save the recording, proceed?",
                      "Confirmation", MessageBoxButtons.YesNo);
                switch (dr)
                {
                    case DialogResult.Yes:
                        mStopWatch.Stop();
                        timer1.Stop();
                        vf.Close();
                        first = 0;
                        this.button3.Text = "Start";
                        this.button1.Enabled = true;
                        this.button3.Enabled = false;
                        this.button2.Enabled = false;
                        break;
                    case DialogResult.No:
                        break;
                }

            }

        }

        private void SerialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            String line = sp.ReadExisting();
            SetText(line);
            //UpdateGraph(double.Parse(line), time);
            System.Diagnostics.Debug.WriteLine(line);
           
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            //string data = serialPort1.ReadLine();
            //System.Diagnostics.Debug.WriteLine(data);

            //MessageBox.Show(serialPort1.ReadLine());
            //this.textBox1.Text = "" + line;
        }

        delegate void SetTextCallback(string text);

        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                double time = mStopWatch.ElapsedMilliseconds / 1000.0;
                System.Diagnostics.Debug.WriteLine("time is: " + time + " and force : "+ text);
                
                this.textBox1.Text = text;
                this.textBox2.Text = time.ToString();
                double data = 0.0;
                try
                {
                    data = double.Parse(text);
                    prev = data;
                }
                catch (System.FormatException)
                {
                    data = prev;
                }
                if (data < 0) { data = 0; }
                array.Add(data);
                this.chart1.ChartAreas[0].AxisY.Maximum = array.Max();
                if (array.Count >= 15)
                {
                    double mt = array.Max();
                    array.Clear();
                    array.Add(mt);
                }
                this.chart1.Series["Force vs Time"].Points.AddXY(time, data);
                if (this.chart1.Series["Force vs Time"].Points.Count >= 14)
                {
                    this.chart1.ChartAreas[0].AxisX.Minimum += 1;
                    this.chart1.ChartAreas[0].AxisX.Maximum += 1;
                }
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            ComboBox c = (ComboBox)sender;
            dept = (String)c.Text;
            System.Diagnostics.Debug.WriteLine("here " + dept);
        }
    }
}
