using System;
using System.Threading;
using System.Windows.Forms;
using o_RA.Forms;

namespace o_RA
{
    static class Program
    {
        private static readonly Mutex Mutex = new Mutex(false, "o!RA");
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (!Mutex.WaitOne(TimeSpan.FromSeconds(0), false))
            {
                MessageBox.Show(@"o!RA is already running!", "", MessageBoxButtons.OK);
                return;
            }
            try
            {
                AppDomain.CurrentDomain.UnhandledException += HandleExcep;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new oRAMainForm());
            }
            finally { Mutex.ReleaseMutex(); }
        }

        static void HandleExcep(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show(((Exception)e.ExceptionObject).Message + '\n' + ((Exception)e.ExceptionObject).InnerException.Message + '\n' + ((Exception)e.ExceptionObject).StackTrace);
        }
    }
}
