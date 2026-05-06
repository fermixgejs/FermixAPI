using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Exiled.API.Features;
using FermixAPI.Core;
using Newtonsoft.Json;

namespace FermixAPI.Utils
{
    /// <summary>
    /// Система хранения и управления данными
    /// </summary>
    public static class FermixData
    {
        private static readonly string DataDirectory = Path.Combine(Paths.Configs, "FermixAPI", "Data");
        
        /// <summary>
        /// Инициализация системы данных
        /// </summary>
        public static void Initialize()
        {
            if (!Directory.Exists(DataDirectory))
                Directory.CreateDirectory(DataDirectory);
        }
        
        #region JSON операции
        
        /// <summary>
        /// Сохранить объект в JSON файл
        /// </summary>
        public static void SaveJson<T>(string fileName, T data, bool prettyPrint = true)
        {
            Initialize();
            var path = GetDataPath(fileName, ".json");
            
            try
            {
                var formatting = prettyPrint ? Formatting.Indented : Formatting.None;
                var json = JsonConvert.SerializeObject(data, formatting);
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                FermixLog.Error($"Ошибка сохранения JSON {fileName}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Загрузить объект из JSON файла
        /// </summary>
        public static T LoadJson<T>(string fileName, T defaultValue = default)
        {
            var path = GetDataPath(fileName, ".json");
            
            if (!File.Exists(path))
                return defaultValue;
            
            try
            {
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                FermixLog.Error($"Ошибка загрузки JSON {fileName}: {ex.Message}");
                return defaultValue;
            }
        }
        
        /// <summary>
        /// Загрузить или создать JSON
        /// </summary>
        public static T LoadOrCreate<T>(string fileName) where T : new()
        {
            var data = LoadJson<T>(fileName);
            if (data == null)
            {
                data = new T();
                SaveJson(fileName, data);
            }
            return data;
        }
        
        #endregion
        
        #region Текстовые данные
        
        /// <summary>
        /// Сохранить текст в файл
        /// </summary>
        public static void SaveText(string fileName, string content)
        {
            Initialize();
            var path = GetDataPath(fileName, ".txt");
            
            try
            {
                File.WriteAllText(path, content);
            }
            catch (Exception ex)
            {
                FermixLog.Error($"Ошибка сохранения текста {fileName}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Загрузить текст из файла
        /// </summary>
        public static string LoadText(string fileName, string defaultValue = "")
        {
            var path = GetDataPath(fileName, ".txt");
            
            if (!File.Exists(path))
                return defaultValue;
            
            try
            {
                return File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                FermixLog.Error($"Ошибка загрузки текста {fileName}: {ex.Message}");
                return defaultValue;
            }
        }
        
        /// <summary>
        /// Добавить строку в файл
        /// </summary>
        public static void AppendLine(string fileName, string line)
        {
            Initialize();
            var path = GetDataPath(fileName, ".txt");
            
            try
            {
                File.AppendAllText(path, line + Environment.NewLine);
            }
            catch (Exception ex)
            {
                FermixLog.Error($"Ошибка добавления строки в {fileName}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Загрузить все строки из файла
        /// </summary>
        public static string[] LoadLines(string fileName)
        {
            var path = GetDataPath(fileName, ".txt");
            
            if (!File.Exists(path))
                return Array.Empty<string>();
            
            try
            {
                return File.ReadAllLines(path);
            }
            catch (Exception ex)
            {
                FermixLog.Error($"Ошибка загрузки строк из {fileName}: {ex.Message}");
                return Array.Empty<string>();
            }
        }
        
        #endregion
        
        #region Управление файлами
        
        /// <summary>
        /// Получить путь к файлу данных
        /// </summary>
        public static string GetDataPath(string fileName, string extension = "")
        {
            if (!string.IsNullOrEmpty(extension) && !fileName.EndsWith(extension))
                fileName += extension;
            return Path.Combine(DataDirectory, fileName);
        }
        
        /// <summary>
        /// Проверить существование файла
        /// </summary>
        public static bool Exists(string fileName, string extension = "")
            => File.Exists(GetDataPath(fileName, extension));
        
        /// <summary>
        /// Удалить файл
        /// </summary>
        public static bool Delete(string fileName, string extension = "")
        {
            var path = GetDataPath(fileName, extension);
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Получить список всех файлов данных
        /// </summary>
        public static IEnumerable<string> GetAllFiles(string pattern = "*.*")
        {
            Initialize();
            return Directory.GetFiles(DataDirectory, pattern)
                .Select(Path.GetFileName);
        }
        
        /// <summary>
        /// Очистить все данные
        /// </summary>
        public static void ClearAll()
        {
            Initialize();
            foreach (var file in Directory.GetFiles(DataDirectory))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    FermixLog.Error($"Ошибка удаления файла {file}: {ex.Message}");
                }
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Хранилище данных для игроков
    /// </summary>
    public class PlayerDataStore<T> where T : class, new()
    {
        private readonly Dictionary<string, T> _cache = new();
        private readonly string _fileName;
        private readonly bool _autoSave;
        
        public PlayerDataStore(string fileName, bool autoSave = true)
        {
            _fileName = fileName;
            _autoSave = autoSave;
            Load();
        }
        
        /// <summary>
        /// Получить данные игрока
        /// </summary>
        public T Get(Player player) => Get(player.UserId);
        
        /// <summary>
        /// Получить данные по UserId
        /// </summary>
        public T Get(string userId)
        {
            if (!_cache.TryGetValue(userId, out var data))
            {
                data = new T();
                _cache[userId] = data;
                if (_autoSave) Save();
            }
            return data;
        }
        
        /// <summary>
        /// Установить данные игрока
        /// </summary>
        public void Set(Player player, T data) => Set(player.UserId, data);
        
        /// <summary>
        /// Установить данные по UserId
        /// </summary>
        public void Set(string userId, T data)
        {
            _cache[userId] = data;
            if (_autoSave) Save();
        }
        
        /// <summary>
        /// Проверить наличие данных
        /// </summary>
        public bool Has(Player player) => Has(player.UserId);
        
        /// <summary>
        /// Проверить наличие данных по UserId
        /// </summary>
        public bool Has(string userId) => _cache.ContainsKey(userId);
        
        /// <summary>
        /// Удалить данные игрока
        /// </summary>
        public bool Remove(Player player) => Remove(player.UserId);
        
        /// <summary>
        /// Удалить данные по UserId
        /// </summary>
        public bool Remove(string userId)
        {
            var removed = _cache.Remove(userId);
            if (removed && _autoSave) Save();
            return removed;
        }
        
        /// <summary>
        /// Получить все данные
        /// </summary>
        public IReadOnlyDictionary<string, T> GetAll() => _cache;
        
        /// <summary>
        /// Загрузить данные из файла
        /// </summary>
        public void Load()
        {
            var data = FermixData.LoadJson<Dictionary<string, T>>(_fileName);
            _cache.Clear();
            if (data != null)
            {
                foreach (var kvp in data)
                    _cache[kvp.Key] = kvp.Value;
            }
        }
        
        /// <summary>
        /// Сохранить данные в файл
        /// </summary>
        public void Save()
        {
            FermixData.SaveJson(_fileName, _cache);
        }
        
        /// <summary>
        /// Очистить все данные
        /// </summary>
        public void Clear()
        {
            _cache.Clear();
            if (_autoSave) Save();
        }
        
        /// <summary>
        /// Изменить данные игрока
        /// </summary>
        public void Modify(Player player, Action<T> modifier)
        {
            var data = Get(player);
            modifier(data);
            if (_autoSave) Save();
        }
    }
    
    /// <summary>
    /// Временное хранилище (не сохраняется на диск)
    /// </summary>
    public class TempDataStore<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _data = new();
        
        /// <summary>
        /// Получить значение
        /// </summary>
        public TValue Get(TKey key, TValue defaultValue = default)
            => _data.TryGetValue(key, out var value) ? value : defaultValue;
        
        /// <summary>
        /// Установить значение
        /// </summary>
        public void Set(TKey key, TValue value) => _data[key] = value;
        
        /// <summary>
        /// Получить или создать значение
        /// </summary>
        public TValue GetOrCreate(TKey key, Func<TValue> factory)
        {
            if (!_data.TryGetValue(key, out var value))
            {
                value = factory();
                _data[key] = value;
            }
            return value;
        }
        
        /// <summary>
        /// Проверить наличие ключа
        /// </summary>
        public bool Has(TKey key) => _data.ContainsKey(key);
        
        /// <summary>
        /// Удалить значение
        /// </summary>
        public bool Remove(TKey key) => _data.Remove(key);
        
        /// <summary>
        /// Очистить хранилище
        /// </summary>
        public void Clear() => _data.Clear();
        
        /// <summary>
        /// Количество элементов
        /// </summary>
        public int Count => _data.Count;
        
        /// <summary>
        /// Все ключи
        /// </summary>
        public IEnumerable<TKey> Keys => _data.Keys;
        
        /// <summary>
        /// Все значения
        /// </summary>
        public IEnumerable<TValue> Values => _data.Values;
    }
    
    /// <summary>
    /// Временные данные с автоудалением
    /// </summary>
    public class ExpiringDataStore<TKey, TValue>
    {
        private readonly Dictionary<TKey, (TValue Value, DateTime Expiry)> _data = new();
        private readonly TimeSpan _defaultExpiry;
        
        public ExpiringDataStore(TimeSpan defaultExpiry)
        {
            _defaultExpiry = defaultExpiry;
        }
        
        /// <summary>
        /// Установить значение с временем жизни
        /// </summary>
        public void Set(TKey key, TValue value, TimeSpan? expiry = null)
        {
            var expiryTime = DateTime.Now + (expiry ?? _defaultExpiry);
            _data[key] = (value, expiryTime);
        }
        
        /// <summary>
        /// Получить значение (если не истекло)
        /// </summary>
        public TValue Get(TKey key, TValue defaultValue = default)
        {
            if (_data.TryGetValue(key, out var entry))
            {
                if (DateTime.Now < entry.Expiry)
                    return entry.Value;
                _data.Remove(key);
            }
            return defaultValue;
        }
        
        /// <summary>
        /// Попробовать получить значение
        /// </summary>
        public bool TryGet(TKey key, out TValue value)
        {
            if (_data.TryGetValue(key, out var entry) && DateTime.Now < entry.Expiry)
            {
                value = entry.Value;
                return true;
            }
            value = default;
            return false;
        }
        
        /// <summary>
        /// Проверить наличие и валидность ключа
        /// </summary>
        public bool Has(TKey key)
        {
            if (_data.TryGetValue(key, out var entry))
            {
                if (DateTime.Now < entry.Expiry)
                    return true;
                _data.Remove(key);
            }
            return false;
        }
        
        /// <summary>
        /// Удалить значение
        /// </summary>
        public bool Remove(TKey key) => _data.Remove(key);
        
        /// <summary>
        /// Очистить истекшие записи
        /// </summary>
        public int CleanExpired()
        {
            var expired = _data.Where(kvp => DateTime.Now >= kvp.Value.Expiry)
                .Select(kvp => kvp.Key)
                .ToList();
            
            foreach (var key in expired)
                _data.Remove(key);
            
            return expired.Count;
        }
        
        /// <summary>
        /// Очистить все данные
        /// </summary>
        public void Clear() => _data.Clear();
    }
}
