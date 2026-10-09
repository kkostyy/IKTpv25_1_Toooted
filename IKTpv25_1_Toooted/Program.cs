using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace IKTpv25_1_Toooted
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Näitab vea akna ja logifaili, mitte ei sulge programmi vaikselt
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => NaitaViga(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => NaitaViga(e.ExceptionObject as Exception);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        private static void NaitaViga(Exception ex)
        {
            string tekst = ex != null ? ex.ToString() : "Tundmatu viga";
            try
            {
                File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "viga.log"),
                    DateTime.Now + Environment.NewLine + tekst + Environment.NewLine + Environment.NewLine);
            }
            catch { }
            MessageBox.Show(tekst, "Programmi viga", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
