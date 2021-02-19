using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form2());
        }
    }
}
/*
at System.Drawing.Image.get_Height()
   at System.Drawing.Image.get_Size()
   at System.Windows.Forms.PictureBox.ImageRectangleFromSizeMode(PictureBoxSizeMode mode)
   at System.Windows.Forms.PictureBox.OnPaint(PaintEventArgs pe)
   at System.Windows.Forms.Control.PaintWithErrorHandling(PaintEventArgs e, Int16 layer)
   at System.Windows.Forms.Control.WmPaint(Message& m)
   at System.Windows.Forms.Control.WndProc(Message& m)
   at System.Windows.Forms.Control.ControlNativeWindow.OnMessage(Message& m)
   at System.Windows.Forms.Control.ControlNativeWindow.WndProc(Message& m)
   at System.Windows.Forms.NativeWindow.DebuggableCallback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)
An unhandled exception of type 'System.InvalidOperationException' occurred in System.Drawing.dll
Object is currently in use elsewhere.

    at System.Drawing.Image.get_Width()
   at System.Drawing.Image.get_Size()
   at System.Windows.Forms.PictureBox.ImageRectangleFromSizeMode(PictureBoxSizeMode mode)
   at System.Windows.Forms.PictureBox.OnPaint(PaintEventArgs pe)
   at System.Windows.Forms.Control.PaintWithErrorHandling(PaintEventArgs e, Int16 layer)
   at System.Windows.Forms.Control.WmPaint(Message& m)
   at System.Windows.Forms.Control.WndProc(Message& m)
   at System.Windows.Forms.Control.ControlNativeWindow.OnMessage(Message& m)
   at System.Windows.Forms.Control.ControlNativeWindow.WndProc(Message& m)
   at System.Windows.Forms.NativeWindow.DebuggableCallback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)
 */