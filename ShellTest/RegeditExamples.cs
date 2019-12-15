using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ShellTest
{
    public class RegeditExamples
    {
        private string AppPath = null;
        private readonly string AppName = "GameStand";
        private readonly string SettingsAppName = "TuningGameStand";
        private readonly string RegAutostart = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private readonly string RegSwipeEdge = @"SOFTWARE\Policies\Microsoft\Windows\EdgeUI";
        private readonly string RegExplorerRestart = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon";
        private readonly string swipeRegValue = "AllowEdgeSwipe";
        private readonly string explorerRestartRegValue = "AutoRestartShell";

        #region set

        private void SetRegDisableSwipeEdgeMachine(bool isChecked)
        {
            RegistryKey key = RegPath.GetCreatePath(RegSwipeEdge, 1, true);
            if (isChecked)
            {
                key.SetValue(swipeRegValue, 0, RegistryValueKind.DWord);
            }
            else
            {
                key.DeleteValue(swipeRegValue);
            }
            key.Close();
        }

        private void SetAutostartReg(bool isChecked)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(RegAutostart, true);
            if (key == null)
            { throw new Exception($@"Путь реестра не найден:\n'{RegAutostart}'\nvoid SetAutostartReg(bool isChecked)"); }
            if (isChecked)
            {
                key.SetValue(AppName, AppPath);
            }
            else
            {
                key.DeleteValue(AppName);
            }
            key.Close();
        }

        private void SetRegDisableSwipeEdgeUser(bool isChecked)
        {
            RegistryKey key = RegPath.GetCreatePath(RegSwipeEdge);
            if (isChecked)
            {
                key.SetValue(swipeRegValue, 0, RegistryValueKind.DWord);
            }
            else
            {
                key.DeleteValue(swipeRegValue);
            }
            key.Close();
        }

        private void ChangeRegistry(string path, string name, object data, int count = 1)
        {
            try
            {
                RegistryKey key = RegPath.GetCreatePath(path, count);
                key.SetValue(name, data); //sets 'someData' in 'someValue'
                key.Close();
            }
            catch (Exception ex)
            {
                Show(ex.Message);
            }
        }

        #endregion

        #region read
        private void ReadRegistry()
        {
            try
            {
                ReadAutostartReg();
                ReadSwipeEdgeReg();
            }
            catch (Exception ex)
            {

                Show(ex.Message);
            }
        }

        private void ReadSwipeEdgeReg()
        {
            try
            {
                ReadSwipeEdgeMachine();
            }
            catch (Exception ex)
            {

                Show(ex.Message);
            }
        }

        private void ReadSwipeEdgeMachine()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(RegSwipeEdge);


            if (key == null)
            { return; }

            string data = key.GetValue(swipeRegValue)?.ToString();
            key.Close();

            if (data == null)
            { return; }


        }

        private void ReadAutostartReg()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(RegAutostart);
            if (key == null)
            {
                return;
            }
            string data = key.GetValue("GameStand")?.ToString();
            key.Close();
        }
        #endregion

        #region other

        private void button1_Click(object sender, EventArgs e)
        {
            RegBlockNotepad();
        }

        private void RegBlockNotepad()
        {
            string path = @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer\DisallowRun";
            ChangeRegistry(path, "1", "notepad.exe", 3);
        }

        private static void Show(string msg)
        {
            MessageBox.Show(msg, "Error");
        }

        private void checkBoxAutoStart_Click(object sender, EventArgs e)
        {
            try
            {
                SetAutostartReg(true);
            }
            catch (Exception ex)
            {
                Show(ex.Message);
            }
        }

        private void checkBoxSwipeEdgeMachine_Click(object sender, EventArgs e)
        {
            try
            {
                SetRegDisableSwipeEdgeMachine(true);
            }
            catch (SecurityException ex1)
            {
                Show(ex1.Message + "\n\nЗапустите от имени администратора для изменения реестра.");
            }
            catch (Exception ex)
            {
                Show(ex.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ReadRegistry();
        }

        #endregion
    }
}
