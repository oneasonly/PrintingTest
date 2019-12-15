using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Management;
using System.Printing;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Brushes = System.Drawing.Brushes;
using FontFamily = System.Drawing.FontFamily;

namespace ShellTest
{
    public static class PrintersDll
    {
        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetDefaultPrinter(string Name);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DeletePrinter(string Name);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DeletePrinterConnection(string Name);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

    }
    public static class Printing
    {
        public static event Action<string> PrintLog = (str) => { };
        private static ManagementScope oManagementScope = null;
        public static void test()
        {
            LocalPrintServer server = new LocalPrintServer();

            PrintQueue queue = server.DefaultPrintQueue;

            var name = queue.FullName;
            //various properties of printQueue
            var isOffLine = queue.IsOffline;
            var isPaperJam = queue.IsPaperJammed;
            var requiresUser = queue.NeedUserIntervention;
            var hasPaperProblem = queue.HasPaperProblem;
            var isBusy = queue.IsBusy;
            
        }

        public static  void RebootPC()
        {
            Process.Start("shutdown", "/r /t 0");
        }
        public static void OpenPrinter(string name)
        {
            IntPtr handler;
            bool isDone = PrintersDll.OpenPrinter(name, out handler, new IntPtr());
        }

        public static void DeleteAllPrinters(string sPrinterName)
        {
            var all = GetAllPrinters();
            foreach (var item in all)
            {
                string del = WMIDeletePrinter(item.FullName);
                PrintLog($"Try Del={item.FullName} => Deleted={del}");
            }
        }

        private static string WMIDeletePrinter(string sPrinterName)
        {
            oManagementScope = new ManagementScope(ManagementPath.DefaultPath);
            oManagementScope.Connect();

            SelectQuery oSelectQuery = new SelectQuery();
            oSelectQuery.QueryString = @"SELECT * FROM Win32_Printer WHERE Name = '" + sPrinterName.Replace(@"\", @"\\") + "'";

            ManagementObjectSearcher oObjectSearcher = new ManagementObjectSearcher(oManagementScope, oSelectQuery);
            ManagementObjectCollection oObjectCollection = oObjectSearcher.Get();

            if (oObjectCollection.Count != 0)
            {
                foreach (ManagementObject oItem in oObjectCollection)
                {
                    var hz = oItem.Path;
                    oItem.Delete();
                    return oItem.ToString();
                }
            }
            return null;
        }

        static bool? CheckDisabledWMI(string PrinterNameArg)
        {
            try
            {
                ManagementScope scope = new ManagementScope(@"\root\cimv2");
                scope.Connect();

                // Select Printers from WMI Object Collections
                ManagementObjectSearcher searcher = new
                 ManagementObjectSearcher("SELECT * FROM Win32_Printer");

                string printerName = "";
                foreach (ManagementObject printer in searcher.Get())
                {
                    printerName = printer["Name"].ToString().ToLower();
                    if (printerName.ToLower().Equals(PrinterNameArg?.ToLower()))
                    {
                        //PrintLog("WMI Disabled: Printer = " + printer["Name"]);
                        if (printer["WorkOffline"].ToString().ToLower().Equals("true"))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;                
            }

            return null;
        }

        static List<PrintQueue> GetAllPrinters()
        {
            var server = new PrintServer();
            var printQueues = server.GetPrintQueues(new[] { EnumeratedPrintQueueTypes.Local, EnumeratedPrintQueueTypes.Connections });
            List<PrintQueue> allVKP80 = printQueues?.Where(x => x.FullName.ToLower().Contains("vkp80"))?.ToList();
            return allVKP80;
        }

        public static void AllPrintersStat()
        {
            var server = new PrintServer();            
            var printQueues = server.GetPrintQueues(new[] { EnumeratedPrintQueueTypes.Local, EnumeratedPrintQueueTypes.Connections });
            List<PrintQueue> allVKP80 = printQueues?.Where(x => x.FullName.ToLower().Contains("vkp80"))?.ToList();

            PrintLog($"All VKP80 count={allVKP80?.Count}; server={server.Name}");
            foreach (PrintQueue printQueue in allVKP80)
            {                
                var Name = printQueue.Name;
                var FullName = printQueue.FullName;
                var stat = printQueue.QueueStatus;
                var defaultPriority = printQueue.DefaultPriority;
                var prior = printQueue.Priority;
                bool? isDisabled = CheckDisabledWMI(FullName);
                var path = $@"{server.Name}\{FullName}";
                PrintLog($"Printer={FullName}; State={stat}; Enabled={!isDisabled}");
                IntPtr handler;
            }            
        }

        public static PrintQueueStatus GetPrintStat()
        {
            var server = new LocalPrintServer();
            PrintQueue queue = server.DefaultPrintQueue;
            var myStat = queue.QueueStatus;
            bool? isDisabled = CheckDisabledWMI(queue.FullName);
            PrintLog($"Default Printer=={queue.FullName}; State={myStat}; Enabled={!isDisabled}");
            return myStat;
        }

        public static async Task RunPrintingsAnotherThread(string CheckRunOpResp)
        {
            try
            {
                Font font = SetFont();
                var posReceipt = "page1\n bla lba\n blabkakb\n page1\n345435435\n34534534\n45654343543\npage1";
                Print(new string[] { posReceipt, CheckRunOpResp }, font);
            }
            catch (Exception ex)
            {
                //ex.Message;
            }
        }
        [HandleProcessCorruptedStateExceptions]
        private static void Print(string[] textArg, Font font)
        {
            try
            {
                int i = 1;
                //PrintLog("Print server waiting...");
                var server = new LocalPrintServer();
                PrintQueue queue = server.DefaultPrintQueue;
                var myStat = queue.QueueStatus;
                PrintLog($"Printer Status={myStat}");
                var strStat = myStat.ToString();
                int count = strStat.Split('|').Length;
                if (myStat.HasFlag(PrintQueueStatus.PaperOut))
                {
                    //return;
                }
                //if(myStat.HasFlag(PrintQueueStatus.wa)
                PrintDocument printDocument = new PrintDocument();
                printDocument.PrintPage += (s, e) =>
                {
                    //Ex.Log($"XmlTransactionsManager.Print.PrintPage():\n{textArg[i]}\n");
                    //PrintLog($"PrintPage before statuc={queue.QueueStatus}");
                    e.HasMorePages = i < 1;
                    e.Graphics.DrawString(textArg[i], font, Brushes.Black, 0, 0);
                    i++;
                    PrintLog("Print Page END");
                    //Ex.Log($"XmlTransactionsManager.Print.PrintPage() END DONE");
                };
                printDocument.Print();
                PrintLog("Ater printDocument.Print()");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private static Font SetFont()
        {
            Font font = new Font(FontFamily.GenericMonospace, 8.25f);
            return font;
        }

    }
}
