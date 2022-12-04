using System;
using System.Collections.Generic;
using System.Text;

namespace AtomNini
{
    public static class IniFileManager
    {
        private static Dictionary<string, WeakReference<IniFile>> _iniFileCache = new Dictionary<string, WeakReference<IniFile>>();

        private static object _iniFileCacheLocker = new object();

        public static IniFile GetIniFile(string filePath, Encoding encoding, IniFileType iniFileType = IniFileType.WindowsStyle)
        {
            lock (_iniFileCacheLocker)
            {
                if (_iniFileCache.TryGetValue(filePath, out WeakReference<IniFile> weakReference))
                {
                    if (weakReference.TryGetTarget(out IniFile target))
                    {
                        return target;
                    }
                }

                IniFile iniFile = new IniFile(filePath, encoding, iniFileType);
                weakReference = new WeakReference<IniFile>(iniFile);
                _iniFileCache[filePath] = weakReference;
                return iniFile;
            }
        }

        public static IniFile GetIniFile(string filePath, IniFileType iniFileType = IniFileType.WindowsStyle)
        {
            return GetIniFile(filePath, Encoding.Default, iniFileType);
        }
    }
}