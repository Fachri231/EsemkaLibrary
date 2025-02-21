using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EsemkaLibrary
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            if (Environment.OSVersion.Version.Major >= 0)
            {
                SetProcessDPIAware();
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Login());
        }

        [DllImport("user32.dll")]
        public static extern bool SetProcessDPIAware();
    }
}