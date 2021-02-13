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

namespace WindowsFormsApp1
{
    public partial class Form2 : Form
    {

        String password = "abc123";
        public Form2()
        {
            InitializeComponent();
            string[] portNames = SerialPort.GetPortNames();     //<-- Reads all available comPorts
            foreach (var portName in portNames)
            {
                combox.Items.Add(portName);                  //<-- Adds Ports to combobox
            }
            string[] drives = System.IO.Directory.GetLogicalDrives();
            foreach (var drive in drives)
            {
                drivebox.Items.Add(drive);                  //<-- Adds Ports to combobox
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(this.textBox1.Text == password && combox.SelectedIndex>-1 && drivebox.SelectedIndex>-1)
            {
                Form1 form1 = new Form1(combox.SelectedItem.ToString(), drivebox.SelectedItem.ToString());
                form1.Show();
                this.Hide();
            }
            else
            {
                if(this.textBox1.Text != password)
                {
                    this.textBox1.Clear();
                    this.label2.Visible = true;
                }
                if (drivebox.SelectedIndex<0)
                {
                    this.label6.Visible = true;
                }
                if (combox.SelectedIndex < 0)
                {
                    this.label7.Visible = true;
                }
            }
            
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }
    }
}
