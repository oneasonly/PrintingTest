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
    public struct PRINTER_DEFAULTS
    {
        public IntPtr pDatatype;
        public IntPtr pDevMode;
        public int DesiredAccess;
    }
    public static class PrintersDll
    {
        public const int STANDARD_RIGHTS_REQUIRED = 0xF0000;
        public const int PRINTER_ACCESS_ADMINISTER = 0x4;
        public const int PRINTER_ACCESS_USE = 0x8;
        public const int PRINTER_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | PRINTER_ACCESS_ADMINISTER | PRINTER_ACCESS_USE);
        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetDefaultPrinter(string Name);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern bool DeletePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", SetLastError = true)]
        static extern int ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", CharSet = CharSet.Unicode, ExactSpelling = false, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern int OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

        public static bool DeletePrinter(string printerName)
        {
            var pd = new PRINTER_DEFAULTS { DesiredAccess = PRINTER_ALL_ACCESS, pDatatype = IntPtr.Zero, pDevMode = IntPtr.Zero };
            var rawsize = Marshal.SizeOf(pd);
            var pdPtr = Marshal.AllocHGlobal(rawsize);
            Marshal.StructureToPtr(pd, pdPtr, true);
            IntPtr hPrinter;
            if (OpenPrinter(printerName, out hPrinter, pdPtr) != 0)
            {
                if (hPrinter != IntPtr.Zero)
                {
                    var result = DeletePrinter(hPrinter);
                    ClosePrinter(hPrinter);
                    return result;
                }
            }
            return false;
        }

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
            //bool isDone = PrintersDll.OpenPrinter(name, out handler, new IntPtr());
        }
        public static void DeleteAllPrintersDLL()
        {
            var all = GetAllPrinters();
            foreach (PrintQueue item in all)
            {
                try
                {
                    PrintersDll.DeletePrinter(item?.FullName);
                    PrintLog($"Deleted={item?.FullName};");
                }
                catch (Exception ex)
                {
                    PrintLog($"Try Del={item?.FullName} ===> {ex.Message};");
                }
            }
        }

        public static void DeleteAllPrinters(string sPrinterName)
        {
            var all = GetAllPrinters();
            foreach (PrintQueue item in all)
            {
                string del = WMIDeletePrinter(item?.FullName);
                PrintLog($"Try Del={item?.FullName} => Deleted={del}");
            }
        }

        public static void WMIDeletePrinterConnections()
        {
            ConnectionOptions options = new ConnectionOptions();
            options.EnablePrivileges = true;
            ManagementScope scope = new ManagementScope(ManagementPath.DefaultPath, options);
            scope.Connect();
            ManagementClass win32Printer = new ManagementClass("Win32_Printer");
            ManagementObjectCollection printers = win32Printer.GetInstances();
            foreach (ManagementObject printer in printers)
            {
                try
                {
                    if (printer.ToString().ToLower().Contains("vkp80"))
                    {
                        PrintLog($"Delete: {printer}");
                        printer.Delete();
                    }
                }
                catch (Exception ex)
                {
                    PrintLog(ex.Message);
                }                
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
                    try
                    {
                        var hz = oItem.Path;
                        oItem.Delete();
                        return oItem.ToString();
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }

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
            PrintLog($"All VKP80 count={allVKP80?.Count}; server={server?.Name}");
            return allVKP80;
        }

        public static void AllPrintersStat()
        {        
            List<PrintQueue> allVKP80 = GetAllPrinters();            
            foreach (PrintQueue printQueue in allVKP80)
            {                
                var Name = printQueue.Name;
                var FullName = printQueue.FullName;
                var stat = printQueue.QueueStatus;
                var defaultPriority = printQueue.DefaultPriority;
                var prior = printQueue.Priority;
                bool? isDisabled = CheckDisabledWMI(FullName);
                PrintLog($"Printer={FullName}; State={stat}; Enabled={!isDisabled}");
                PrintQueue one = allVKP80?.FirstOrDefault(x => x.QueueStatus != PrintQueueStatus.None);
                if(one!=null)
                {
                    PrintLog($"\nSET DEFAULT PRINTER = {one?.FullName}");
                    PrintersDll.SetDefaultPrinter(one?.FullName);
                }
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
