﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace AtomNini
{
    public delegate void ReloadEventHandler(string iniFilePath);

    public class IniFile
    {
        #region fields

        private static List<Type> _nullablePrimitiveTypes = new List<Type>()
        {
            typeof(bool?),typeof(char?),

            typeof(double?),typeof(float?),typeof(decimal?),

            typeof(sbyte?),typeof(short?),typeof(int?),typeof(long?),

            typeof(byte?),typeof(ushort?),typeof(uint?), typeof(ulong?),

            typeof(DateTime?)
        };

        private static List<Type> _primitiveTypes = new List<Type>()
        {
            typeof(bool),typeof(char),

            typeof(double),typeof(float),typeof(decimal),

            typeof(sbyte),typeof(short),typeof(int),typeof(long),

            typeof(byte),typeof(ushort),typeof(uint),typeof(ulong),

            typeof(DateTime)
        };

        private IniDocument _document;

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        private string _filePath;
        private Encoding _encoding;

        public string FilePath
        {
            get { return _filePath; }
        }

        private FileSystemWatcher _watcher;

        #endregion fields

        #region ctors

        internal IniFile(string filePath, IniFileType iniFileType) : this(filePath, Encoding.Default, iniFileType)
        {
        }

        internal IniFile(string filePath, Encoding encoding, IniFileType iniFileType)
        {
            _filePath = filePath;
            _encoding = encoding;

            _document = new IniDocument(new StreamReader(new FileStream(filePath, FileMode.OpenOrCreate), encoding), iniFileType);

            _watcher = new FileSystemWatcher();
            _watcher.Path = Path.GetDirectoryName(filePath);
            _watcher.Filter = Path.GetFileName(filePath);
            _watcher.NotifyFilter = NotifyFilters.LastWrite;
            _watcher.Changed += _watcher_Changed;
            _watcher.EnableRaisingEvents = true;
        }

        private void _watcher_Changed(object sender, FileSystemEventArgs e)
        {
            _lock.EnterWriteLock();
            try
            {
                _watcher.EnableRaisingEvents = false;
                if (e.ChangeType == WatcherChangeTypes.Changed)
                    Reload();
                _watcher.EnableRaisingEvents = true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        #endregion ctors

        #region Helper Method

        public static TValue ConvertToTargetType<TValue>(string valueOfKey)
        {
            Type returnType = typeof(TValue);

            try
            {
                /********** 当字符串是空字符串或NULl时 ***********/

                if (string.IsNullOrEmpty(valueOfKey))
                {
                    // 可空值类型返回NULL
                    if (Nullable.GetUnderlyingType(returnType) != null)
                    {
                        return default;
                    }
                    // 不可空值类型抛出异常
                    if (returnType.IsValueType)
                    {
                        throw new FormatException($"Failed to change {{{valueOfKey}}} to type {{{returnType.FullName}}}.");
                    }
                    // 引用类型返回NULL
                    else
                    {
                        return default;
                    }
                }

                /********** 当字符串不为空或NULL时 ***********/

                if (_primitiveTypes.Contains(returnType) || returnType == typeof(string))
                {
                    return (TValue)Convert.ChangeType(valueOfKey, returnType);
                }

                if (_nullablePrimitiveTypes.Contains(returnType))
                {
                    return (TValue)Convert.ChangeType(valueOfKey, Nullable.GetUnderlyingType(returnType));
                }
                return JsonSerializer.Deserialize<TValue>(valueOfKey);
            }
            catch
            {
                throw new InvalidCastException($"Failed to change {{{valueOfKey}}} to type {{{returnType.FullName}}}.");
            }
        }

        public static string ConvertToStringFormat(object valueToSet)
        {
            string valueStringFormat;
            if (valueToSet is null)
            {
                valueStringFormat = string.Empty;
                return valueStringFormat;
            }

            Type valueTypeToSet = valueToSet.GetType();
            if (_primitiveTypes.Contains(valueTypeToSet) || _nullablePrimitiveTypes.Contains(valueTypeToSet) || valueTypeToSet == typeof(string))
            {
                valueStringFormat = valueToSet.ToString();
            }
            else
            {
                JsonSerializerOptions options = new JsonSerializerOptions()
                {
                    WriteIndented = false
                };

                valueStringFormat = JsonSerializer.Serialize(valueToSet, options);
            }
            return valueStringFormat;
        }

        #endregion Helper Method

        #region Get

        public TValue GetValue<TValue>(string section, string key)
        {
            _lock.EnterReadLock();
            try
            {
                if (_document.Sections[section] == null)
                {
                    throw new SectionNotExistException(_filePath, nameof(section));
                }

                string value = _document.Sections[section].GetValue(key);
                if (value == null)
                {
                    throw new KeyNotExistException(_filePath, nameof(section), nameof(key));
                }

                // value的可能值不可能是NULL
                return ConvertToTargetType<TValue>(value);
            }
            finally { _lock.ExitReadLock(); }
        }

        public (TValue value, bool isExistSection, bool isExistKey) GetValue<TValue>(string section, string key, TValue defaultValue = default)
        {
            _lock.EnterReadLock();
            try
            {
                if (_document.Sections[section] == null)
                {
                    return (defaultValue, false, false);
                }

                string value = _document.Sections[section].GetValue(key);

                if (value == null)
                {
                    return (defaultValue, true, false);
                }

                return (ConvertToTargetType<TValue>(value), true, true);
            }
            finally { _lock.ExitReadLock(); }
        }

        public (string key, string value)[] GetKeyValuePairs(string section, params string[] keys)
        {
            (string key, string value)[] values = new (string key, string value)[keys.Length];

            _lock.EnterReadLock();
            try
            {
                if (_document.Sections[section] == null)
                {
                    throw new SectionNotExistException(_filePath, nameof(section));
                }
                for (int i = 0; i < keys.Length; i++)
                {
                    string value = _document.Sections[section].GetValue(keys[i]);
                    if (value == null)
                    {
                        throw new KeyNotExistException(_filePath, section, keys[i]);
                    }
                    values[i].key = keys[i];
                    values[i].value = value;
                }
                return values;
            }
            finally { _lock.ExitReadLock(); }
        }

        public (string key, string value, bool isExistSection, bool isExistKey)[] GetKeyValuePairs(string section, string defaultValue, params string[] keys)
        {
            (string key, string value, bool isExistSection, bool isExistKey)[] values = new (string key, string value, bool isExistSection, bool isExistKey)[keys.Length];

            _lock.EnterReadLock();
            try
            {
                if (_document.Sections[section] == null)
                {
                    for (int i = 0; i < keys.Length; i++)
                    {
                        values[i].key = keys[i];
                        values[i].value = defaultValue;
                        values[i].isExistSection = false;
                        values[i].isExistKey = false;
                    }
                    return values;
                }

                for (int i = 0; i < keys.Length; i++)
                {
                    string value = _document.Sections[section].GetValue(keys[i]);

                    if (value == null)
                    {
                        values[i].key = keys[i];
                        values[i].value = defaultValue;
                        values[i].isExistSection = true;
                        values[i].isExistKey = false;
                    }
                    else
                    {
                        values[i].key = keys[i];
                        values[i].value = _document.Sections[section].GetValue(keys[i]);
                        values[i].isExistSection = true;
                        values[i].isExistKey = true;
                    }
                }
                return values;
            }
            finally { _lock.ExitReadLock(); }
        }

        public HashSet<(string key, string value)> GetKeyValuePairs(string section)
        {
            HashSet<(string key, string value)> keyValuePairs = new HashSet<(string key, string value)>();

            _lock.EnterReadLock();
            try
            {
                if (_document.Sections[section] == null)
                {
                    throw new SectionNotExistException(_filePath, nameof(section));
                }
                string[] keys = _document.Sections[section].GetKeys();

                for (int i = 0; i < keys.Length; i++)
                {
                    keyValuePairs.Add((keys[i], _document.Sections[section].GetValue(keys[i])));
                }
                return keyValuePairs;
            }
            finally { _lock.ExitReadLock(); }
        }

        public HashSet<(string section, string key, string value)> GetKeyValuePairs()
        {
            HashSet<(string section, string key, string value)> keyValuePairs = new HashSet<(string section, string key, string value)>();

            _lock.EnterReadLock();
            try
            {
                for (int i = 0; i < _document.Sections.Count; i++)
                {
                    foreach (var key in _document.Sections[i].GetKeys())
                    {
                        keyValuePairs.Add((_document.Sections[i].Name, key, _document.Sections[i].GetValue(key)));
                    }
                }

                return keyValuePairs;
            }
            finally { _lock.ExitReadLock(); }
        }

        public HashSet<string> GetSections()
        {
            HashSet<string> sections = new HashSet<string>();

            _lock.EnterReadLock();
            try
            {
                for (int i = 0; i < _document.Sections.Count; i++)
                {
                    sections.Add(_document.Sections[i].Name);
                }
                return sections;
            }
            finally { _lock.ExitReadLock(); }
        }

        #endregion Get

        #region Set

        public (bool isExistSection, bool isExistKey, string oldValue) SetValue(string section, string key, object value)
        {
            string oldValue;
            string valueString;

            valueString = ConvertToStringFormat(value);

            _lock.EnterUpgradeableReadLock();
            try
            {
                if (_document.Sections[section] != null)
                {
                    oldValue = _document.Sections[section].GetValue(key);
                    if (oldValue == valueString)
                    {
                        return (true, true, oldValue);
                    }
                    else
                    {
                        _lock.EnterWriteLock();
                        try
                        {
                            _document.Sections[section].Set(key, valueString);
                            _watcher.EnableRaisingEvents = false;
                            _document.Save(_filePath, _encoding);
                            _watcher.EnableRaisingEvents = true;
                            return (true, oldValue != null, oldValue);
                        }
                        finally
                        {
                            _lock.ExitWriteLock();
                        }
                    }
                }
                else
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        _document.Sections.Add(new IniSection(section));
                        _document.Sections[section].Set(key, valueString);
                        _watcher.EnableRaisingEvents = false;
                        _document.Save(_filePath, _encoding);
                        _watcher.EnableRaisingEvents = true;
                        return (false, false, null);
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public (bool isExistSection, bool isExistKey, string oldValue)[] SetValue(params (string section, string key, object value)[] skvs)
        {
            (bool, bool, string)[] result = new (bool, bool, string)[skvs.Length];

            bool isSave = false;

            _lock.EnterWriteLock();
            try
            {
                for (int i = 0; i < skvs.Length; ++i)
                {
                    bool isExistSection = true;
                    bool isExistKey = true;
                    string oldValue = null;

                    string valueString = ConvertToStringFormat(skvs[i].value);

                    if (_document.Sections[skvs[i].section] == null)
                    {
                        isExistSection = false;
                        isExistKey = false;
                        oldValue = null;
                        isSave = true;
                        _document.Sections.Add(new IniSection(skvs[i].section));
                    }
                    else
                    {
                        oldValue = _document.Sections[skvs[i].section].GetValue(skvs[i].key);
                        if (oldValue == null)
                        {
                            isExistKey = false;
                            isSave = true;
                        }
                    }

                    if (oldValue != valueString)
                    {
                        isSave = true;
                    }

                    _document.Sections[skvs[i].section].Set(skvs[i].key, valueString);

                    result[i] = (isExistSection, isExistKey, oldValue);
                }

                if (isSave)
                {
                    _watcher.EnableRaisingEvents = false;
                    _document.Save(_filePath, _encoding);
                    _watcher.EnableRaisingEvents = true;
                }
                return result;
            }
            finally { _lock.ExitWriteLock(); }
        }

        #endregion Set

        #region section

        public bool AddConfig(string section)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_document.Sections[section] == null)
                {
                    _document.Sections.Add(new IniSection(section));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            finally { _lock.ExitWriteLock(); }
        }

        public bool RemoveConfig(string section)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_document.Sections[section] == null)
                {
                    return false;
                }
                else
                {
                    _document.Sections.Remove(section);
                    return true;
                }
            }
            finally { _lock.ExitWriteLock(); }
        }

        #endregion section

        #region Save

        public void Save()
        {
            _lock.EnterWriteLock();
            try
            {
                _watcher.EnableRaisingEvents = false;
                _document.Save(_filePath, _encoding);
                _watcher.EnableRaisingEvents = true;
            }
            finally { _lock.ExitWriteLock(); }
        }

        public void SaveAsNewFile(string filePath, Encoding encoding)
        {
            if (filePath == _filePath)
            {
                throw new ArgumentException(nameof(filePath), $"{filePath} should't same of itself.");
            }
            _lock.EnterReadLock();
            try
            {
                _document.Save(filePath, encoding);
            }
            finally { _lock.ExitReadLock(); }
        }

        #endregion Save

        public void Reload()
        {
            _lock.EnterWriteLock();
            try
            {
                _document.Load(_filePath, _encoding);
            }
            finally { _lock.ExitWriteLock(); }
        }
    }
}