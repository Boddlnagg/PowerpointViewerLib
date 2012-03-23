using System;
using System.Windows.Forms;

namespace PowerpointViewerDemo
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        } 
    }
}
