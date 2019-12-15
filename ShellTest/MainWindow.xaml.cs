using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ShellTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            this.WindowStyle = WindowStyle.None;
            //this.Topmost = true;
            textBox1.Text = string.Empty;
            Printing.PrintLog += (s) => { textBox1.Text += $"{s}\n"; };
            OnStart();
        }

        private async Task OnStart()
        {
            try
            {
                await Task.Delay(500);
                RunProc();
                await GetProcStat();
                PrintQueueStatus stat;
                for (int i = 0; i < 6; i++)
                {
                    stat = Printing.GetPrintStat(); await Task.Delay(500);
                    if (stat != PrintQueueStatus.None) break;
                }
                Printing.AllPrintersStat();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void RunProc(bool isFull=false)
        {
            if(isFull)
                Process.Start(@"C:\Program Files\Custom Engineering\CePrnStatusMonitor\CePrnStatusMonitor.exe");
            if(!isFull)
                Process.Start(@"CePrnStatusMonitor");
            textBox1.Text += $"Launched CePrnStatusMonitor;\n";
        }

        async Task GetProcStat()
        {
            Process proc = Process.GetProcessesByName("CePrnStatusMonitor").FirstOrDefault();
            int? procCount = Process.GetProcessesByName("CePrnStatusMonitor")?.Length;
            textBox1.Text += $"proc count={procCount};\n";
            textBox1.Text += $"process found={proc};\n";
            if (proc == null)
            { textBox1.Text += $"proc == null\n"; return; }

            textBox1.Text += $"start time={proc?.StartTime}\n";
            proc.Exited += (s, arg) =>
            {
                textBox1.Text += $"Proc Has Exited event!!!\n";
                textBox1.Text += $"exit time={proc?.ExitTime}\n";
                textBox1.Text += $"code={proc?.ExitCode}\n";
            };
        }

        #region buttons
        private void Button_Click_CheckProcMonitor(object sender, RoutedEventArgs e)
        {
            GetProcStat();
        }

        private void Button_Click_FullMonitorRun(object sender, RoutedEventArgs e)
        {
            RunProc(true);
        }

        private void Button_Click_ShortMonitorRun(object sender, RoutedEventArgs e)
        {
            RunProc(false);
        }
        private void Button_Click_Explorer(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("explorer");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Button_Click_RegExplorerShellUser(object sender, RoutedEventArgs e)
        {
            try
            {
                RegeditManager.SetShellDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Button_Click_RegCustomShellUser(object sender, RoutedEventArgs e)
        {
            try
            {
                RegeditManager.SetShellApp();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Button_Click_SetRedirectorRegShell(object sender, RoutedEventArgs e)
        {
            try
            {
                RegeditManager.SetShellRedirector();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button_Click_Print(object sender, RoutedEventArgs e)
        {
            Printing.RunPrintingsAnotherThread("page 2\n bla bla\n oalolaolaola\n page2\n34534543543\n34543543534543\n34534534543\n34524332423432\npage2");
        }

        private void button1_Click_Clear(object sender, RoutedEventArgs e)
        {
            textBox1.Clear();
        }

        #endregion

        private void Button_Click_PrinterStatus(object sender, RoutedEventArgs e)
        {
            Printing.GetPrintStat();
        }

        private void Button_Click_AllPrintStat(object sender, RoutedEventArgs e)
        {
            Printing.AllPrintersStat();
        }

        private void Button_Click_RestartMonitor(object sender, RoutedEventArgs e)
        {
            Process proc = Process.GetProcessesByName("CePrnStatusMonitor").FirstOrDefault();
            proc.Kill();
            proc.WaitForExit();
            RunProc();
        }

        private void Button_Click_DelWMIPrinters(object sender, RoutedEventArgs e)
        {
            Printing.DeleteAllPrinters(textBox.Text);
        }

        private void Button_Click_RebootPC(object sender, RoutedEventArgs e)
        {
            Printing.RebootPC();
        }
    }
}
