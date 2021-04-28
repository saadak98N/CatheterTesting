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
using Captura;
using System.Diagnostics;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        String drive;
        int ht;
        int wt;
        int s;
        //force range
        int max_force;
        int min_force;
        int position;
        double peak_force = 0.0;
        double peak_min = 0.0;
        int default_speed;
        Form3 form3;
        StreamWriter writer;
        Recorder rec;
        //int play_flag = 0;

        
        //camera feed
        FilterInfoCollection usbCams;
        String camname = "A4tech FHD 1080P PC Camera";
        VideoCaptureDevice cam = null;
        //VideoCaptureDevice cam2 = null;
        //VideoCaptureDevice cam3 = null;
        //VideoCaptureDevice cam4 = null;


        //Data management for graph
        long count = 0;
        String textName;
        private List<double> array = new List<double>();

        //lot and dept numbers entered by user
        private String lot = "";

        private String dept = "";

        //Screen recording variables
        //private Timer timer1;

        //private VideoFileWriter vf;
        private PictureBox pictureBox1 = new PictureBox();
        private string filename;


        public Form1(String comname, String drivename)
        {
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.Normal;
            this.WindowState = FormWindowState.Maximized;

            max_force = 1000;
            min_force = -1000;
            default_speed = 10;
            position = 3;

            drive = drivename;

            ht = Screen.PrimaryScreen.Bounds.Height;
            wt = Screen.PrimaryScreen.Bounds.Width;

            this.chart1.Width = wt / 2;
            double x = (ht / 5) * 3.2;
            s = (int)x;
            System.Diagnostics.Debug.WriteLine(wt + "ABC" + ht + "DEF" + s);
            pictureBox2.Height = s;
            pictureBox2.Width = (int)wt / 2;

            pictureBox2.Location = new Point(wt / 2, 0);
            
            this.chart1.Height = s;

            form3 = new Form3(default_speed, max_force, position, min_force);
            form3.Closed += form3closed;
            form3.button2.Click += new EventHandler(configButton);

            usbCams = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo Device in usbCams)
            {
                System.Diagnostics.Debug.WriteLine(Device.Name);

                if (Device.Name == camname)
                {
                    cam = new VideoCaptureDevice(Device.MonikerString);
                    cam.NewFrame += FinalFrame_NewFrame;
                    cam.Start();
                }
            }

            //timer1 = new Timer();
            //timer1.Tick += timer1_Tick;
            //vf = new VideoFileWriter();

            this.chart1.Series["Force vs Time"].Points.AddXY(0, 0);
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(SerialPort1_DataReceived);

            form3.TopMost = true;
            form3.Show();

            serialPort1.PortName = comname;
            try
            {
                serialPort1.Open();

            }
            catch (System.IO.IOException)
            {
                form3.Hide();
                //cam.Stop();
                MessageBox.Show("Invalid Serial port found, close application and try again.");
                System.Windows.Forms.Application.Exit();

            }
            sizeThings();
        }

        private void sizeThings()
        {
            home.Height = ht / 9;
            home.Width = wt / 15;
            play.Height = ht / 9;
            play.Width = wt / 15;
            stop.Height = ht / 9;
            stop.Width = wt / 15;
            res.Height = ht / 9;
            res.Width = wt / 15;
            pause.Height = ht / 9;
            pause.Width = wt / 15;
            ss.Height = ht / 9;
            ss.Width = wt / 15;
            rev.Height = ht / 9;
            rev.Width = wt / 15;

            home.Location = new Point(((((wt/2)/2)/2)/2)/2, settings.Location.Y - settings.Height - 2);
            play.Location = new Point(home.Location.X + home.Width + 10, settings.Location.Y - settings.Height - 2);
            stop.Location = new Point(play.Location.X + play.Width + 10, settings.Location.Y - settings.Height - 2);
            res.Location = new Point(stop.Location.X + stop.Width + 10, settings.Location.Y - settings.Height - 2);
            pause.Location = new Point(res.Location.X + res.Width + 10, settings.Location.Y - settings.Height - 2);
            rev.Location = new Point(pause.Location.X + pause.Width + 10, settings.Location.Y - settings.Height - 2);
            ss.Location = new Point(rev.Location.X + rev.Width + 10, settings.Location.Y - settings.Height - 2);

            label4.Location = new Point(chart1.Width / 2, chart1.Height - 2);

            play.Enabled = false;
            stop.Enabled = false;
            res.Enabled = false;
            pause.Enabled = false;
            rev.Enabled = false;
        }
        private void FinalFrame_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {

                Bitmap bmp = (Bitmap)eventArgs.Frame.Clone();
            this.Invoke(new MethodInvoker(delegate ()
            {
                int wtt = pictureBox2.Width;
                int htt = pictureBox2.Height;
                bool source_is_wider = (float)bmp.Width / bmp.Height > (float)wtt / htt;
                var resized = new Bitmap(wtt, htt);
                var dest_rect = new Rectangle(0, 0, wtt, htt);
                Rectangle src_rect;

                if (source_is_wider)
                {
                    float size_ratio = (float)htt / bmp.Height;
                    int sample_width = (int)(wtt / size_ratio);
                    src_rect = new Rectangle((bmp.Width - sample_width) / 2, 0, sample_width, bmp.Height);
                }
                else
                {
                    float size_ratio = (float)wtt / bmp.Width;
                    int sample_height = (int)(htt / size_ratio);
                    src_rect = new Rectangle(0, (bmp.Height - sample_height) / 2, bmp.Width, sample_height);
                }
                var g = Graphics.FromImage(resized);
                g.DrawImage(bmp, dest_rect, src_rect, GraphicsUnit.Pixel);
                g.Dispose();
                if (this.pictureBox2.Image != null)
                {
                    this.pictureBox2.Image.Dispose();
                }
                this.pictureBox2.Image = resized;
            }));
        }

        /*private void timer1_Tick(object sender, EventArgs e)
        {
            Bitmap bp = new Bitmap(wt, ht);
            var gr = Graphics.FromImage(bp);
            try 
            { 
                gr.CopyFromScreen(0, 0, 0, 0, new Size(bp.Width, bp.Height));
            }
            catch(System.ArgumentException)
            {
                if(gr!=null)
                {
                    gr.Dispose();
                }
            }
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
            }
            pictureBox1.Image = bp;
            //vf.WriteVideoFrame(bp);
            if(gr!=null)
            {
                gr.Dispose();
            }

        }*/

        private void button1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("pressed");
            if (!String.IsNullOrEmpty(this.textBox3.Text) && dept != "")
            {
                if(rec!=null)
                {
                    rec.Dispose();
                }
                serialPort1.WriteLine("*CH000\'");
                peak_force = 0.0;
                peak_min = 0.0;
                this.label6.Visible = false;
                this.textBox1.Text = "0.0";
                this.textBox4.Text = "0.0";
                lot = this.textBox3.Text;
                this.home.Enabled = false;
                this.play.Enabled = true;
                this.rev.Enabled = true;
                this.pause.Enabled = false;
                this.res.Enabled = false;
                this.textBox3.Enabled = false;
                this.textBox2.Enabled = false;
                this.comboBox1.Enabled = false;
            }
            else
            {
                this.label6.Visible = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            serialPort1.WriteLine("*CP000\'");
            System.Diagnostics.Debug.WriteLine("Paused");
            res.Enabled = true;
            pause.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //if(this.play_flag==1)
            //{
                this.home.Enabled = false;
                this.stop.Enabled = true;
                this.res.Enabled = false;
                this.pause.Enabled = true;
                this.settings.Enabled = false;
                this.play.Enabled = false;

                string pathToNewFolder = System.IO.Path.Combine(drive + "Recordings_NHT", dept);
                System.Diagnostics.Debug.WriteLine("time is: " + pathToNewFolder);

                DirectoryInfo directory = Directory.CreateDirectory(pathToNewFolder);
                DateTime today = DateTime.Today;
                string mypath = System.IO.Path.Combine(drive + "Recordings_NHT\\" + dept, today.Date.ToString("dddd_dd MMMM yyyy "));
                String name = pathToNewFolder + "lot_" + lot + today + ".avi";
                filename = @mypath + lot + ".avi";
                if (System.IO.File.Exists(filename))
                    System.IO.File.Delete(filename);
                rec = new Recorder(new RecorderParams(filename, 10, SharpAvi.KnownFourCCs.Codecs.MotionJpeg, 70));
                textName = mypath + lot + ".txt";
                writer = new StreamWriter(textName, true);
                using (writer)
                {
                    writer.Write("time,force \n");
                }

                //if (System.IO.File.Exists(filename)) System.IO.File.Delete(filename);

                System.Diagnostics.Debug.WriteLine("time is: " + wt + ht);

                //vf.Open(filename, wt, ht, 25, VideoCodec.MPEG4, 1000000);

                //timer1.Start();
                serialPort1.WriteLine("*CF000\'");
                chart1.Series["Force vs Time"].Points.Clear();
                this.chart1.ChartAreas[0].AxisX.Minimum = 0;
                this.chart1.ChartAreas[0].AxisX.Maximum = 15;
                count = 0;
                this.chart1.Series["Force vs Time"].Points.AddXY(0, 0);
                System.Diagnostics.Debug.WriteLine("start!");
            //this.play_flag = 0;
            //}
        }

        private void SerialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            int bytesToRead = sp.BytesToRead;
            var bytes = new byte[7];
            sp.Read(bytes, 0, 7);
            if (bytesToRead % 7 == 0)
            {
                System.Diagnostics.Debug.WriteLine("Recv: " + bytes + " " + bytesToRead);
                string bitString = BitConverter.ToString(bytes);
                System.Diagnostics.Debug.WriteLine("String: " + bitString);
                List<string> listStrLineElements = bitString.Split('-').ToList();
                /*if (listStrLineElements[0] == "2A" && listStrLineElements[6] == "2F" && listStrLineElements[1] == "31" && listStrLineElements[2] == "41")
                {
                    System.Diagnostics.Debug.WriteLine("Home flag");
                    this.play_flag = 1;
                }*/
                if (listStrLineElements[0] == "2A" && listStrLineElements[6] == "2F")
                {
                    int decValue1 = int.Parse(listStrLineElements[1], System.Globalization.NumberStyles.HexNumber);
                    int decValue2 = int.Parse(listStrLineElements[2], System.Globalization.NumberStyles.HexNumber);
                    int decValue3 = int.Parse(listStrLineElements[3], System.Globalization.NumberStyles.HexNumber);
                    int decValue4 = int.Parse(listStrLineElements[4], System.Globalization.NumberStyles.HexNumber);
                    int decValue5 = int.Parse(listStrLineElements[5], System.Globalization.NumberStyles.HexNumber);
                    int neg = 1;
                    if (decValue1 == 1)
                    {
                        neg = -1;
                    }
                    double toSend = ((100 * decValue2) + (10 * decValue3) + (decValue4) + (decValue5 * 0.1)) * neg;
                    System.Diagnostics.Debug.WriteLine("Force: " + toSend.ToString());
                    int times = bytesToRead / 7;
                    System.Diagnostics.Debug.WriteLine("Times " + times);
                    if (times < 5)
                    {
                        while (times >= 1)
                        {
                            SetText(toSend.ToString());
                            times--;
                        }
                    }
                    SetText(toSend.ToString());
                }
                System.Diagnostics.Debug.WriteLine("THE END!");
                sp.DiscardInBuffer();
            }

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
                double data = double.Parse(text);
                System.Diagnostics.Debug.WriteLine("time is: " + data + " and force : " + peak_force);

                if (data >= min_force && data <= max_force)
                {
                    String toSend = "";
                    String t = DateTime.Now.ToString();
                    toSend = String.Concat(t, ",");
                    toSend = String.Concat(toSend, data.ToString());
                    toSend = String.Concat(toSend, "\n");
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
                    else if (data < peak_min)
                    {
                        peak_min = data;
                        this.textBox4.Text = peak_min.ToString();
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
                    if (data > max_force)
                    {
                        serialPort1.WriteLine("*CPF00\'");
                        MessageBox.Show("Force values exceeded max insertion force, testing paused.");
                    }
                    else
                    {
                        serialPort1.WriteLine("*CPF00\'");
                        MessageBox.Show("Force values exceeded max retraction force, testing paused.");
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
            bmpScreenshot.Save(drive + "Recordings_NHT\\" + dept + "\\lot_" + count.ToString() + ".png", ImageFormat.Png);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            form3.Show();
        }

        private void configButton(object sender, EventArgs e)
        {
            String maxF = form3.textBox2.Text;
            String minF = form3.textBox5.Text;
            String position = form3.textBox1.Text;
            if (!String.IsNullOrEmpty(maxF))
            {
                int temp = int.Parse(maxF);
                max_force = temp;
            }
            if (!String.IsNullOrEmpty(minF))
            {
                int temp = int.Parse(minF);
                min_force = temp;
            }
            if (!String.IsNullOrEmpty(position))
            {
                int temp = int.Parse(position);
                this.position = temp;
            }

            default_speed = form3.trackBar1.Value * 10;
            form3.Hide();
            String tosend = "";

            if (default_speed == 10)
            {
                tosend = "*S40";
            }
            else if (default_speed == 20)
            {
                tosend = "*S30";
            }
            else if (default_speed == 30)
            {
                tosend = "*S20";
            }
            else if (default_speed == 40)
            {
                tosend = "*S13";
            }
            else if (default_speed == 50)
            {
                tosend = "*S10";
            }

            if (this.position == 1)
            {
                tosend = String.Concat(tosend, "P1\'");
                serialPort1.WriteLine(tosend);
            }
            else if (this.position == 2)
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
            else if (this.position == 6)
            {
                tosend = String.Concat(tosend, "P6\'");
                serialPort1.WriteLine(tosend);
            }
            else if (this.position == 7)
            {
                tosend = String.Concat(tosend, "P7\'");
                serialPort1.WriteLine(tosend);
            }
            else if (this.position == 8)
            {
                tosend = String.Concat(tosend, "P8\'");
                serialPort1.WriteLine(tosend);
            }
            else if (this.position == 9)
            {
                tosend = String.Concat(tosend, "P9\'");
                serialPort1.WriteLine(tosend);
            }
            System.Diagnostics.Debug.WriteLine("button " + max_force + "  " + default_speed + "     " + tosend + "  " + min_force + "     ");

        }

        private void Form1_FormClosing_1(object sender, FormClosingEventArgs e)
        {
            DialogResult dr = MessageBox.Show("Are you sure you want to close?",
                    "Confirmation", MessageBoxButtons.YesNo);
            switch (dr)
            {
                case DialogResult.Yes:
                    try
                    {
                        serialPort1.WriteLine("*CX000\'");
                    }
                    catch (System.InvalidOperationException)
                    {
                        if (cam != null)
                        {
                            cam.SignalToStop();
                            cam = null;
                        }
                        System.Windows.Forms.Application.Exit();

                    }
                    if (cam != null)
                    {
                        cam.SignalToStop();
                        cam = null;
                    }
                    serialPort1.Close();
                    System.Windows.Forms.Application.Exit();
                    break;

                case DialogResult.No:
                    e.Cancel = true;
                    break;
            }
        }

        void form3closed(object sender, EventArgs e)
        {
            String maxF = form3.textBox2.Text;
            String minF = form3.textBox5.Text;
            String position = form3.textBox1.Text;
            if (!String.IsNullOrEmpty(maxF))
            {
                int temp = int.Parse(maxF);
                max_force = temp;
            }
            if (!String.IsNullOrEmpty(minF))
            {
                int temp = int.Parse(minF);
                min_force = temp;
            }
            if (!String.IsNullOrEmpty(position))
            {
                int temp = int.Parse(position);
                this.position = temp;
            }

            default_speed = form3.trackBar1.Value * 10;
            form3.Hide();
            String tosend = "";

            if (default_speed == 10)
            {
                tosend = "*S40";
            }
            else if (default_speed == 20)
            {
                tosend = "*S30";
            }
            else if (default_speed == 30)
            {
                tosend = "*S20";
            }
            else if (default_speed == 40)
            {
                tosend = "*S13";
            }
            else if (default_speed == 50)
            {
                tosend = "*S10";
            }

            if (this.position == 1)
            {
                tosend = String.Concat(tosend, "P1\'");
                serialPort1.WriteLine(tosend);
            }
            else if (this.position == 2)
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
            else if (this.position == 6)
            {
                tosend = String.Concat(tosend, "P6\'");
                serialPort1.WriteLine(tosend);
            }
            else if (this.position == 7)
            {
                tosend = String.Concat(tosend, "P7\'");
                serialPort1.WriteLine(tosend);
            }
            else if (this.position == 8)
            {
                tosend = String.Concat(tosend, "P8\'");
                serialPort1.WriteLine(tosend);
            }
            else if (this.position == 9)
            {
                tosend = String.Concat(tosend, "P9\'");
                serialPort1.WriteLine(tosend);
            }
            System.Diagnostics.Debug.WriteLine("button " + max_force + "  " + default_speed + "     " + tosend + "  " + min_force + "     ");

        }

        private void stop_Click(object sender, EventArgs e)
        {
            this.home.Enabled = true;
            this.play.Enabled = false;
            this.rev.Enabled = false;
            this.stop.Enabled = false;
            this.pause.Enabled = false;
            this.res.Enabled = false;

            this.settings.Enabled = true;
            this.textBox3.Enabled = true;
            this.textBox2.Enabled = true;
            this.comboBox1.Enabled = true;
            serialPort1.WriteLine("*CP000\'");
        }

        private void res_Click(object sender, EventArgs e)
        {
            serialPort1.WriteLine("*CF000\'");
            System.Diagnostics.Debug.WriteLine("Resumed");
            res.Enabled = false;
            pause.Enabled = true;
        }

        private void rev_Click(object sender, EventArgs e)
        {
            serialPort1.WriteLine("*CB000\'");
            System.Diagnostics.Debug.WriteLine("Back");
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
/*double data = 0.0;
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
}*/

/*var charsToRemove = new string[] { "-"};
foreach (var c in charsToRemove)
{
    bitString = bitString.Replace(c, string.Empty);
}
System.Diagnostics.Debug.WriteLine("String2: "+bitString);
//string s = "10C5EC9C6";
long n = Int64.Parse(bitString, System.Globalization.NumberStyles.HexNumber);
System.Diagnostics.Debug.WriteLine("String3: " + n);
decimal d = (decimal)Int64.Parse(bitString, System.Globalization.NumberStyles.HexNumber);
System.Diagnostics.Debug.WriteLine("String4: " + n);*/

/*   if(line.Count()>6 && line.Count()<15)
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
   }*/