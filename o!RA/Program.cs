using System;
using System.Windows.Forms;
using o_RA.Forms;

namespace o_RA
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += HandleExcep;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new oRAMainForm());
        }

        static void HandleExcep(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show(((Exception)e.ExceptionObject).Message + '\n' + ((Exception)e.ExceptionObject).InnerException.Message + '\n' + ((Exception)e.ExceptionObject).StackTrace);
        }
    }
}
