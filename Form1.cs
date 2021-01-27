using Accord.Video.FFMPEG;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        //force range
        double min_force;
        double max_force;
        int default_speed;
        Form3 form3;

        //camera feed
        FilterInfoCollection usbCams;
        VideoCaptureDevice cam = null;

        //Stop watch for graphs
        private System.Diagnostics.Stopwatch mStopWatch = new System.Diagnostics.Stopwatch();

        //Graphs limits to update x-axis with time
        private double leftLimit = 0;
        private double rightLimit = 0;

        //Data management for graph
        private double prev = 0;
        int count = 0;

        private List<double> array = new List<double>();

        //lot and dept numbers entered by user
        private String lot = "";

        private String dept = "";

        //Screen recording variables
        private Timer timer1;

        private VideoFileWriter vf;
        private Bitmap bp;
        private Graphics gr;
        private PictureBox pictureBox1 = new PictureBox();
        private string filename;

        //Button text switch flags
        private int first = 0;

        private int second = 0;

        public Form1()
        {
            InitializeComponent();
            min_force = 0;
            max_force = 1000;
            default_speed = 30;

            usbCams = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo Device in usbCams)
                System.Diagnostics.Debug.WriteLine(Device.Name);
            cam = new VideoCaptureDevice(usbCams[0].MonikerString);
            System.Diagnostics.Debug.WriteLine(cam);
            cam.NewFrame += FinalFrame_NewFrame;
            cam.Start();

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

        //private delegate void FinalFrame_Callback(object sender, NewFrameEventArgs eventArgs);

        private void FinalFrame_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap bmp = (Bitmap)eventArgs.Frame.Clone();
            bool source_is_wider = (float)bmp.Width / bmp.Height > (float)pictureBox2.Width / pictureBox2.Height;
            var resized = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            var g = Graphics.FromImage(resized);
            var dest_rect = new Rectangle(0, 0, pictureBox2.Width, pictureBox2.Height);
            Rectangle src_rect;

            if (source_is_wider)
            {
                float size_ratio = (float)pictureBox2.Height / bmp.Height;
                int sample_width = (int)(pictureBox2.Width / size_ratio);
                src_rect = new Rectangle((bmp.Width - sample_width) / 2, 0, sample_width, bmp.Height);
            }
            else
            {
                float size_ratio = (float)pictureBox2.Width / bmp.Width;
                int sample_height = (int)(pictureBox2.Height / size_ratio);
                src_rect = new Rectangle(0, (bmp.Height - sample_height) / 2, bmp.Width, sample_height);
            }

            g.DrawImage(bmp, dest_rect, src_rect, GraphicsUnit.Pixel);
            g.Dispose();
            this.pictureBox2.Image = resized;
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
                this.button5.Enabled = false;
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
            if (second == 0)
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
            if (first == 0)
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
                if (System.IO.File.Exists(filename)) System.IO.File.Delete(filename);
                vf.Open(filename, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height, 25, VideoCodec.MPEG4, 1000000);
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
                        this.button5.Enabled = true;
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

        private delegate void SetTextCallback(string text);

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
                System.Diagnostics.Debug.WriteLine("time is: " + time + " and force : " + text);

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
            cam.Stop();
            System.Windows.Forms.Application.Exit();
        }

        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            ComboBox c = (ComboBox)sender;
            dept = (String)c.Text;
            System.Diagnostics.Debug.WriteLine("here " + dept);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            count += 1;
            //Create a new bitmap.
            var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                           Screen.PrimaryScreen.Bounds.Height,
                                           PixelFormat.Format32bppArgb);

            // Create a graphics object from the bitmap.
            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);

            // Take the screenshot from the upper left corner to the right bottom corner.
            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                        Screen.PrimaryScreen.Bounds.Y,
                                        0,
                                        0,
                                        Screen.PrimaryScreen.Bounds.Size,
                                        CopyPixelOperation.SourceCopy);
            DateTime today = DateTime.Today;
            // Save the screenshot to the specified path that the user has chosen.
            bmpScreenshot.Save("d:\\Recordings_NHT\\" + dept+"\\lot_"+count.ToString()+".png", ImageFormat.Png);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            form3 = new Form3(default_speed);
            form3.Show();
            form3.button2.Click += new EventHandler(configButton);
        }

        private void configButton(object sender, EventArgs e)
        {
            String minF = form3.textBox1.Text;
            String maxF = form3.textBox2.Text;
            if (!String.IsNullOrEmpty(minF))
            {
                double temp = double.Parse(minF);
                if (temp < max_force)
                {
                    min_force = temp;
                }
            }
            if (!String.IsNullOrEmpty(maxF))
            {
                double temp = double.Parse(maxF);
                if (temp > min_force)
                {
                    max_force = temp;
                }
            }
            default_speed = form3.trackBar1.Value * 10;
            form3.Hide();
            System.Diagnostics.Debug.WriteLine("button " + min_force + max_force+"  "+ default_speed);
        }
    }
}