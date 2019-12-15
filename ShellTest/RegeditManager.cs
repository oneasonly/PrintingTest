using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShellTest
{
    public class RegeditManager
    {
        private static readonly string RegExplorerRestart = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon";
        private static readonly string shellValue = "Shell";
        private static readonly string explorer = "explorer";
        public static readonly string AppPath = @"C:\Projects\ShellTest\ShellTest\bin\Debug\ShellTest.exe";
        public static readonly string redirectorPath = @"C:\Projects\ExeRedirector\ExeRedirector\bin\Debug\ExeRedirector.exe";

        public static void SetShell(string arg)
        {
            RegistryKey key = RegPath.GetCreatePath(RegExplorerRestart, 1);

            key.SetValue(shellValue, arg, RegistryValueKind.String);
            

            key.Close();
        }

        public static void SetShellDefault()
        {
            SetShell(explorer);
        }

        public static void SetShellApp()
        {
            SetShell(AppPath);
        }

        public static void SetShellRedirector()
        {
            SetShell(redirectorPath);
        }
    }
}
