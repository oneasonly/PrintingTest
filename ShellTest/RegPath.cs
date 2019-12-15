using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShellTest
{
    public static class RegPath
    {
        private static List<string> SubKeys;
        private static bool isLocalMachine = false;
        private static string LastDelete(string path)
        {
            int index = path.LastIndexOf('\\');
            SubKeys.Add(path.Substring(index).Trim('\\'));
            string Return = path.Remove(index);
            return Return;
        }
        public static RegistryKey GetCreatePath(string _path, int count = 1, bool _isLocalMachine = false)
        {
            isLocalMachine = _isLocalMachine;
            SubKeys = new List<string>();
            string path = _path.Trim('\\');
            for (int i = 0; i < count; i++)
            {
                path = RecursFindExist(path);
            }
            return CreateReg(path);
        }
        private static RegistryKey CreateReg(string path)
        {
            RegistryKey key = isLocalMachine ?
                Registry.LocalMachine.OpenSubKey(path, true)
                : Registry.CurrentUser.OpenSubKey(path, true);
            if (key == null)
            {
                throw new Exception($"Не удалось получить ключ реестра:\n{path}.");
            }
            foreach (var item in SubKeys)
            {
                key = key.CreateSubKey(item);
            }
            return key;
        }
        private static string RecursFindExist(string path)
        {
            RegistryKey key = isLocalMachine ?
                Registry.LocalMachine.OpenSubKey(path, true)
                : Registry.CurrentUser.OpenSubKey(path, true);
            //RegistryKey key = Registry.CurrentUser.OpenSubKey(path, true);
            if (key == null)
            {
                path = LastDelete(path);
            }
            return path;
        }
    }
}
