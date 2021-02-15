using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form3 : Form
    {
        public Form3(int speed, int max_force, int position, int min_force)
        {
            InitializeComponent();
            this.trackBar1.Value = speed / 10;
            this.textBox1.Text = position.ToString();
            this.textBox2.Text = max_force.ToString();
            this.textBox5.Text = min_force.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
