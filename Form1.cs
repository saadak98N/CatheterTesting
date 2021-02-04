﻿using Accord.Video.FFMPEG;
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
        int max_force;
        int position;
        double peak_force = 0.0;
        int default_speed;
        Form3 form3;
        StreamWriter writer;

        //camera feed
        FilterInfoCollection usbCams;
        VideoCaptureDevice cam = null;
        //VideoCaptureDevice cam2 = null;
        //VideoCaptureDevice cam3 = null;
        //VideoCaptureDevice cam4 = null;


        //Data management for graph
        private double prev = 0;
        long count = 0;
        String textName;
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
            max_force = 1000;
            default_speed = 30;
            position = 1;
            
            form3 = new Form3(default_speed, peak_force, max_force, position);
            form3.button2.Click += new EventHandler(configButton);
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
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(SerialPort1_DataReceived);
            form3.TopMost = true;
            form3.Show();
            try
            {
                serialPort1.Open();

            }
            catch (System.IO.IOException)
            {
                form3.Hide();
                cam.Stop();
                MessageBox.Show("No Serial port found, close application and try again.");
                System.Windows.Forms.Application.Exit();

            }
        }


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
            try
            {
                g.DrawImage(bmp, dest_rect, src_rect, GraphicsUnit.Pixel);
                g.Dispose();
                try
                {
                    this.pictureBox2.Image = resized;
                }
                catch (System.InvalidOperationException)
                {
                }
            }
            catch(System.OutOfMemoryException)
            {

            }
            
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                bp = new Bitmap(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);
                gr = Graphics.FromImage(bp);
                gr.CopyFromScreen(0, 0, 0, 0, new Size(bp.Width, bp.Height));
                pictureBox1.Image = bp;
                pictureBox1.SizeMode = PictureBoxSizeMode.Normal;
                vf.WriteVideoFrame(bp);
                gr.Dispose();
            }
            catch(System.ArgumentException)
            {

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("pressed");
            if (!String.IsNullOrEmpty(this.textBox3.Text) && dept != "")
            {
                serialPort1.WriteLine("*CH000\'");
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
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (second == 0)
            {
                serialPort1.WriteLine("*CP000\'");
                this.button2.BackgroundImage = Properties.Resources.play;
                second = 1;
            }
            else
            {
                serialPort1.WriteLine("*CR000\'");
                this.button2.BackgroundImage = Properties.Resources.pause;
                second = 0;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (first == 0)
            {
                button3.BackgroundImage = Properties.Resources.stop;
                string pathToNewFolder = System.IO.Path.Combine("g:\\Recordings_NHT", dept);
                DirectoryInfo directory = Directory.CreateDirectory(pathToNewFolder);
                DateTime today = DateTime.Today;
                string mypath = System.IO.Path.Combine("g:\\Recordings_NHT\\" + dept, today.Date.ToString("dddd_dd MMMM yyyy "));
                String name = pathToNewFolder + "lot_" + lot+ today + ".avi";
                filename = @mypath + lot + ".avi";
                if (System.IO.File.Exists(filename))
                    System.IO.File.Delete(filename);
                textName = mypath + lot + ".txt";
                writer = new StreamWriter(textName, true);
                using (writer)
                {
                    writer.Write("time,force");
                }
                this.button1.Enabled = false;
                this.button2.Enabled = true;
                if (System.IO.File.Exists(filename)) System.IO.File.Delete(filename);
                vf.Open(filename, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height, 25, VideoCodec.MPEG4, 1000000);
                timer1.Start();
                serialPort1.WriteLine("*CS000\'");
                first = 1;
            }
            else
            {
                DialogResult dr = MessageBox.Show("Stopping will end and save the recording, proceed?",
                      "Confirmation", MessageBoxButtons.YesNo);
                switch (dr)
                {
                    case DialogResult.Yes:
                        timer1.Stop();
                        vf.Close();
                        first = 0;
                        serialPort1.WriteLine("*CX000\'");
                        button3.BackgroundImage = Properties.Resources.play;
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
            System.Diagnostics.Debug.WriteLine("Recv: "+line+" "+line.Count());
            if(line.Count()>6 && line.Count()<15)
            {
                if(line[0]=='*' && line[line.Count()-2] == '\'')
                {
                    String t = line.Substring(1, line.Count()-3);
                    System.Diagnostics.Debug.WriteLine("Formatted: "+t);
                    SetText(t);
                }
            }
            if (line.Count() == 4)
            {
                if(line[0]=='*' && line[2]=='\'' && line[3] == '\n')
                {
                    SetText("Enable");
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private delegate void SetTextCallback(string text);

        private void SetText(string text)
        {
            if (this.textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                if (text == "Enable")
                {
                    this.button3.Enabled = true;
                    System.Diagnostics.Debug.WriteLine("Button set");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("time is: " + count + " and force : " + text);
                    double data = 0.0;
                    int neg = 1;
                    try
                    {
                        if(text[text.Count()-1] == '.')
                        {
                            text = text.Substring(0, text.Length - 1);
                        }
                        if(text[0]=='-')
                        {
                            text = text.Substring(1, text.Length - 1);
                            neg = -1;
                        }
                        data = double.Parse(text);
                        if(neg==-1)
                        {
                            data *= -1;
                        }
                        prev = data;
                    }
                    catch (System.FormatException)
                    {
                        data = prev;
                    }

                    if(data <= max_force)
                    {
                        String toSend = "";
                        String t = DateTime.Now.ToString();
                        toSend = String.Concat(t, ",");
                        toSend = String.Concat(toSend, data);
                        writer = new StreamWriter(textName, true);
                        using (writer)
                        {
                            writer.Write(toSend);
                        }
                        if (data > peak_force)
                        {
                            peak_force = data;
                            this.textBox1.Text = peak_force.ToString();
                        }

                        this.chart1.ChartAreas[0].RecalculateAxesScale();
                        this.chart1.Series["Force vs Time"].Points.AddXY(count, data);
                        if (count > 14)
                        {
                            this.chart1.ChartAreas[0].AxisX.Minimum += 1;
                            this.chart1.ChartAreas[0].AxisX.Maximum += 1;
                        }
                        count++;
                    }
                    else
                    {
                        serialPort1.WriteLine("*CP000\'");
                        this.button2.BackgroundImage = Properties.Resources.play;
                        second = 1;
                        MessageBox.Show("Force values exceeded max force, testing paused.");
                    }

                }
            }
        }

        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            ComboBox c = (ComboBox)sender;
            dept = (String)c.Text;
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
            bmpScreenshot.Save("g:\\Recordings_NHT\\" + dept+"\\lot_"+count.ToString()+".png", ImageFormat.Png);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            form3.Show();
        }

        private void configButton(object sender, EventArgs e)
        {
            String maxF = form3.textBox2.Text;
            String position = form3.textBox1.Text;
            if (!String.IsNullOrEmpty(maxF))
            {
                int temp = int.Parse(maxF);
                max_force = temp;
            }
            if (!String.IsNullOrEmpty(position))
            {
                int temp = int.Parse(position);
                this.position = temp;
            }
            default_speed = form3.trackBar1.Value * 10;
            form3.Hide();
            String tosend = "";
            if(default_speed==10)
            {
                tosend = "*S40";
                //serialPort1.WriteLine();
            }else if (default_speed == 20)
            {
                tosend = "*S30";
                //serialPort1.WriteLine("*S30\'");
            } else if(default_speed == 30)
            {
                tosend = "*S20";
                //serialPort1.WriteLine("*S20\'");
            } else if(default_speed == 40)
            {
                tosend = "*S13";
                //serialPort1.WriteLine("*S13\'");
            } else if(default_speed == 50)
            {
                tosend = "*S10";
                //serialPort1.WriteLine("*S10\'");
            }

            if (this.position == 1)
            {
                tosend = String.Concat(tosend, "P1\'");
                serialPort1.WriteLine(tosend);
            }else if(this.position == 2)
            {
                tosend = String.Concat(tosend, "P2\'");
                serialPort1.WriteLine(tosend);
            }
            else if (this.position == 3)
            {
                tosend = String.Concat(tosend, "P3\'");
                serialPort1.WriteLine(tosend);
            }
            else if (this.position == 4)
            {
                tosend = String.Concat(tosend, "P4\'");
                serialPort1.WriteLine(tosend);
            }
            else if (this.position == 5)
            {
                tosend = String.Concat(tosend, "P5\'");
                serialPort1.WriteLine(tosend);
            }
            System.Diagnostics.Debug.WriteLine("button " + max_force + "  " + default_speed+ "     " + tosend);

        }

        private void Form1_FormClosing_1(object sender, FormClosingEventArgs e)
        {
            cam.Stop();
            serialPort1.Close();
            System.Windows.Forms.Application.Exit();
        }
    }
}

//if (usbCams.Count > 1)
//{
//    cam2 = new VideoCaptureDevice(usbCams[1].MonikerString);
//    cam2.NewFrame += FinalFrame_NewFrame2;
//    cam2.Start();
//    if(usbCams.Count > 2)
//    {
//        cam3 = new VideoCaptureDevice(usbCams[2].MonikerString);
//        cam3.NewFrame += FinalFrame_NewFrame3;
//        cam3.Start();
//        if(usbCams.Count >3)
//        {
//            cam4 = new VideoCaptureDevice(usbCams[3].MonikerString);
//            cam4.NewFrame += FinalFrame_NewFrame4;
//            cam4.Start();
//        }
//    }
//}
//if (usbCams.Count > 1)
//{
//    cam2.Stop();
//    if(usbCams.Count > 2)
//    {
//        cam3.Stop();
//        if (usbCams.Count > 3)
//        {
//            cam4.Stop();
//        }
//    }

//}
//private void FinalFrame_NewFrame2(object sender, NewFrameEventArgs eventArgs)
//{
//    Bitmap bmp = (Bitmap)eventArgs.Frame.Clone();
//    bool source_is_wider = (float)bmp.Width / bmp.Height > (float)pictureBox3.Width / pictureBox3.Height;
//    var resized = new Bitmap(pictureBox3.Width, pictureBox3.Height);
//    var g = Graphics.FromImage(resized);
//    var dest_rect = new Rectangle(0, 0, pictureBox3.Width, pictureBox3.Height);
//    Rectangle src_rect;

//    if (source_is_wider)
//    {
//        float size_ratio = (float)pictureBox3.Height / bmp.Height;
//        int sample_width = (int)(pictureBox3.Width / size_ratio);
//        src_rect = new Rectangle((bmp.Width - sample_width) / 2, 0, sample_width, bmp.Height);
//    }
//    else
//    {
//        float size_ratio = (float)pictureBox3.Width / bmp.Width;
//        int sample_height = (int)(pictureBox3.Height / size_ratio);
//        src_rect = new Rectangle(0, (bmp.Height - sample_height) / 2, bmp.Width, sample_height);
//    }

//    g.DrawImage(bmp, dest_rect, src_rect, GraphicsUnit.Pixel);
//    g.Dispose();
//    try
//    {
//        this.pictureBox3.Image = resized;
//    }
//    catch (System.InvalidOperationException)
//    {
//    }
//}

//private void FinalFrame_NewFrame3(object sender, NewFrameEventArgs eventArgs)
//{
//    Bitmap bmp = (Bitmap)eventArgs.Frame.Clone();
//    bool source_is_wider = (float)bmp.Width / bmp.Height > (float)pictureBox4.Width / pictureBox4.Height;
//    var resized = new Bitmap(pictureBox4.Width, pictureBox4.Height);
//    var g = Graphics.FromImage(resized);
//    var dest_rect = new Rectangle(0, 0, pictureBox4.Width, pictureBox4.Height);
//    Rectangle src_rect;

//    if (source_is_wider)
//    {
//        float size_ratio = (float)pictureBox4.Height / bmp.Height;
//        int sample_width = (int)(pictureBox4.Width / size_ratio);
//        src_rect = new Rectangle((bmp.Width - sample_width) / 2, 0, sample_width, bmp.Height);
//    }
//    else
//    {
//        float size_ratio = (float)pictureBox4.Width / bmp.Width;
//        int sample_height = (int)(pictureBox4.Height / size_ratio);
//        src_rect = new Rectangle(0, (bmp.Height - sample_height) / 2, bmp.Width, sample_height);
//    }

//    g.DrawImage(bmp, dest_rect, src_rect, GraphicsUnit.Pixel);
//    g.Dispose();
//    try
//    {
//        this.pictureBox4.Image = resized;
//    }
//    catch (System.InvalidOperationException)
//    {
//    }
//}

//private void FinalFrame_NewFrame4(object sender, NewFrameEventArgs eventArgs)
//{
//    Bitmap bmp = (Bitmap)eventArgs.Frame.Clone();
//    bool source_is_wider = (float)bmp.Width / bmp.Height > (float)pictureBox5.Width / pictureBox5.Height;
//    var resized = new Bitmap(pictureBox5.Width, pictureBox5.Height);
//    var g = Graphics.FromImage(resized);
//    var dest_rect = new Rectangle(0, 0, pictureBox5.Width, pictureBox5.Height);
//    Rectangle src_rect;

//    if (source_is_wider)
//    {
//        float size_ratio = (float)pictureBox5.Height / bmp.Height;
//        int sample_width = (int)(pictureBox5.Width / size_ratio);
//        src_rect = new Rectangle((bmp.Width - sample_width) / 2, 0, sample_width, bmp.Height);
//    }
//    else
//    {
//        float size_ratio = (float)pictureBox5.Width / bmp.Width;
//        int sample_height = (int)(pictureBox5.Height / size_ratio);
//        src_rect = new Rectangle(0, (bmp.Height - sample_height) / 2, bmp.Width, sample_height);
//    }

//    g.DrawImage(bmp, dest_rect, src_rect, GraphicsUnit.Pixel);
//    g.Dispose();
//    try
//    {
//        this.pictureBox5.Image = resized;
//    }
//    catch (System.InvalidOperationException)
//    {
//    }
//}