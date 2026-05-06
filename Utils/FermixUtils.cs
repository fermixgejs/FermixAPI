using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exiled.API.Enums;
using Exiled.API.Features;
using PlayerRoles;
using UnityEngine;

namespace FermixAPI.Utils
{
    /// <summary>
    /// Общие утилиты FermixAPI
    /// </summary>
    public static class FermixUtils
    {
        private static readonly System.Random Random = new();
        
        #region Математические утилиты
        
        /// <summary>
        /// Случайное число в диапазоне
        /// </summary>
        public static int RandomRange(int min, int max) => Random.Next(min, max + 1);
        
        /// <summary>
        /// Случайное число float в диапазоне
        /// </summary>
        public static float RandomRange(float min, float max) 
            => (float)(Random.NextDouble() * (max - min) + min);
        
        /// <summary>
        /// Случайный шанс (0-100)
        /// </summary>
        public static bool Chance(int percent) => Random.Next(100) < percent;
        
        /// <summary>
        /// Случайный шанс (0.0-1.0)
        /// </summary>
        public static bool Chance(float percent) => Random.NextDouble() < percent;
        
        /// <summary>
        /// Случайный элемент из списка
        /// </summary>
        public static T RandomElement<T>(IList<T> list) 
            => list.Count > 0 ? list[Random.Next(list.Count)] : default;
        
        /// <summary>
        /// Случайный элемент из массива
        /// </summary>
        public static T RandomElement<T>(params T[] array) 
            => array.Length > 0 ? array[Random.Next(array.Length)] : default;
        
        /// <summary>
        /// Перемешать список
        /// </summary>
        public static void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
        
        /// <summary>
        /// Получить перемешанную копию списка
        /// </summary>
        public static List<T> Shuffled<T>(IEnumerable<T> source)
        {
            var list = source.ToList();
            Shuffle(list);
            return list;
        }
        
        /// <summary>
        /// Ограничить значение в диапазоне
        /// </summary>
        public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0) return min;
            if (value.CompareTo(max) > 0) return max;
            return value;
        }
        
        /// <summary>
        /// Линейная интерполяция
        /// </summary>
        public static float Lerp(float a, float b, float t) => a + (b - a) * Clamp(t, 0f, 1f);
        
        /// <summary>
        /// Расстояние между двумя точками
        /// </summary>
        public static float Distance(Vector3 a, Vector3 b) => Vector3.Distance(a, b);
        
        /// <summary>
        /// Точка находится в радиусе от другой
        /// </summary>
        public static bool InRange(Vector3 point, Vector3 center, float radius) 
            => Distance(point, center) <= radius;
        
        #endregion
        
        #region Строковые утилиты
        
        /// <summary>
        /// Форматировать время в читаемый формат
        /// </summary>
        public static string FormatTime(float seconds)
        {
            var ts = TimeSpan.FromSeconds(seconds);
            if (ts.TotalHours >= 1)
                return $"{(int)ts.TotalHours}:{ts.Minutes:D2}:{ts.Seconds:D2}";
            return $"{ts.Minutes}:{ts.Seconds:D2}";
        }
        
        /// <summary>
        /// Форматировать время с миллисекундами
        /// </summary>
        public static string FormatTimePrecise(float seconds)
        {
            var ts = TimeSpan.FromSeconds(seconds);
            return $"{ts.Minutes}:{ts.Seconds:D2}.{ts.Milliseconds / 100}";
        }
        
        /// <summary>
        /// Сократить текст с многоточием
        /// </summary>
        public static string Truncate(string text, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;
            return text.Substring(0, maxLength - suffix.Length) + suffix;
        }
        
        /// <summary>
        /// Повторить строку N раз
        /// </summary>
        public static string Repeat(string text, int count)
        {
            var sb = new StringBuilder(text.Length * count);
            for (int i = 0; i < count; i++)
                sb.Append(text);
            return sb.ToString();
        }
        
        /// <summary>
        /// Центрировать текст
        /// </summary>
        public static string Center(string text, int width, char padChar = ' ')
        {
            if (text.Length >= width) return text;
            int padding = width - text.Length;
            int padLeft = padding / 2 + text.Length;
            return text.PadLeft(padLeft, padChar).PadRight(width, padChar);
        }
        
        /// <summary>
        /// Создать прогресс-бар
        /// </summary>
        public static string ProgressBar(float value, float max, int length = 20, char filled = '█', char empty = '░')
        {
            float percent = Clamp(value / max, 0f, 1f);
            int filledLength = (int)(percent * length);
            return new string(filled, filledLength) + new string(empty, length - filledLength);
        }
        
        /// <summary>
        /// Создать прогресс-бар с процентами
        /// </summary>
        public static string ProgressBarWithPercent(float value, float max, int length = 20)
        {
            float percent = Clamp(value / max, 0f, 1f) * 100;
            return $"{ProgressBar(value, max, length)} {percent:F0}%";
        }
        
        /// <summary>
        /// Colorize text для Unity Rich Text
        /// </summary>
        public static string Colorize(string text, string color) => $"<color={color}>{text}</color>";
        
        /// <summary>
        /// Bold text
        /// </summary>
        public static string Bold(string text) => $"<b>{text}</b>";
        
        /// <summary>
        /// Italic text
        /// </summary>
        public static string Italic(string text) => $"<i>{text}</i>";
        
        /// <summary>
        /// Установить размер текста
        /// </summary>
        public static string Size(string text, int size) => $"<size={size}>{text}</size>";
        
        #endregion
        
        #region Цветовые утилиты
        
        /// <summary>
        /// Цвет в HEX формат
        /// </summary>
        public static string ColorToHex(Color color) 
            => $"#{ColorUtility.ToHtmlStringRGB(color)}";
        
        /// <summary>
        /// HEX в цвет
        /// </summary>
        public static Color HexToColor(string hex)
        {
            if (!hex.StartsWith("#"))
                hex = "#" + hex;
            ColorUtility.TryParseHtmlString(hex, out var color);
            return color;
        }
        
        /// <summary>
        /// Интерполяция между цветами
        /// </summary>
        public static Color LerpColor(Color a, Color b, float t) => Color.Lerp(a, b, t);
        
        /// <summary>
        /// Получить цвет по здоровью (зеленый -> желтый -> красный)
        /// </summary>
        public static Color HealthColor(float health, float maxHealth)
        {
            float percent = health / maxHealth;
            if (percent > 0.5f)
                return Color.Lerp(Color.yellow, Color.green, (percent - 0.5f) * 2);
            return Color.Lerp(Color.red, Color.yellow, percent * 2);
        }
        
        /// <summary>
        /// Получить HEX цвет по здоровью
        /// </summary>
        public static string HealthColorHex(float health, float maxHealth) 
            => ColorToHex(HealthColor(health, maxHealth));
        
        #endregion
        
        #region Игроки
        
        /// <summary>
        /// Найти игрока по частичному имени
        /// </summary>
        public static Player FindPlayer(string name)
        {
            name = name.ToLower();
            return Player.List.FirstOrDefault(p => 
                p.Nickname.ToLower().Contains(name) ||
                p.UserId.ToLower().Contains(name) ||
                p.Id.ToString() == name);
        }
        
        /// <summary>
        /// Найти всех игроков по частичному имени
        /// </summary>
        public static IEnumerable<Player> FindPlayers(string name)
        {
            name = name.ToLower();
            return Player.List.Where(p => 
                p.Nickname.ToLower().Contains(name) ||
                p.UserId.ToLower().Contains(name));
        }
        
        /// <summary>
        /// Получить ближайшего игрока к позиции
        /// </summary>
        public static Player GetNearestPlayer(Vector3 position, Func<Player, bool> predicate = null)
        {
            Player nearest = null;
            float nearestDist = float.MaxValue;
            
            foreach (var player in Player.List)
            {
                if (!player.IsAlive) continue;
                if (predicate != null && !predicate(player)) continue;
                
                float dist = Distance(position, player.Position);
                if (dist < nearestDist)
                {
                    nearest = player;
                    nearestDist = dist;
                }
            }
            
            return nearest;
        }
        
        /// <summary>
        /// Получить всех игроков в радиусе
        /// </summary>
        public static IEnumerable<Player> GetPlayersInRange(Vector3 position, float radius, Func<Player, bool> predicate = null)
        {
            return Player.List
                .Where(p => p.IsAlive && InRange(p.Position, position, radius))
                .Where(p => predicate == null || predicate(p));
        }
        
        /// <summary>
        /// Получить случайного живого игрока
        /// </summary>

        /// <summary>
        /// Получить случайного мертвого игрока
        /// </summary>

        
        #endregion
        
        #region Позиции
        
        /// <summary>
        /// Получить случайную позицию в радиусе
        /// </summary>
        public static Vector3 RandomPositionInRadius(Vector3 center, float radius)
        {
            var randomDir = UnityEngine.Random.insideUnitSphere * radius;
            return center + randomDir;
        }
        
        /// <summary>
        /// Получить позицию на поверхности
        /// </summary>
        public static Vector3? GetGroundPosition(Vector3 position)
        {
            if (Physics.Raycast(position, Vector3.down, out var hit, 100f))
                return hit.point;
            return null;
        }
        
        /// <summary>
        /// Получить направление от точки A к точке B
        /// </summary>
        public static Vector3 DirectionTo(Vector3 from, Vector3 to) 
            => (to - from).normalized;
        
        /// <summary>
        /// Получить точку между двумя позициями
        /// </summary>
        public static Vector3 MidPoint(Vector3 a, Vector3 b) 
            => (a + b) / 2f;
        
        #endregion
        
        #region Коллекции
        
        /// <summary>
        /// Безопасно получить элемент по индексу
        /// </summary>
        public static T SafeGet<T>(IList<T> list, int index, T defaultValue = default)
            => index >= 0 && index < list.Count ? list[index] : defaultValue;
        
        /// <summary>
        /// Безопасно получить значение из словаря
        /// </summary>
        public static TValue SafeGet<TKey, TValue>(IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default)
            => dict.TryGetValue(key, out var value) ? value : defaultValue;
        
        /// <summary>
        /// Получить или создать значение в словаре
        /// </summary>
        public static TValue GetOrCreate<TKey, TValue>(IDictionary<TKey, TValue> dict, TKey key) where TValue : new()
        {
            if (!dict.TryGetValue(key, out var value))
            {
                value = new TValue();
                dict[key] = value;
            }
            return value;
        }
        
        /// <summary>
        /// Удалить элементы по условию
        /// </summary>
        public static int RemoveWhere<T>(IList<T> list, Func<T, bool> predicate)
        {
            int count = 0;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (predicate(list[i]))
                {
                    list.RemoveAt(i);
                    count++;
                }
            }
            return count;
        }
        
        /// <summary>
        /// Выполнить действие для каждого элемента
        /// </summary>
        public static void ForEach<T>(IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);
        }
        
        /// <summary>
        /// Выполнить действие для каждого элемента с индексом
        /// </summary>
        public static void ForEach<T>(IEnumerable<T> source, Action<T, int> action)
        {
            int index = 0;
            foreach (var item in source)
                action(item, index++);
        }
        
        #endregion
        
        #region Валидация
        
        /// <summary>
        /// Проверить, что игрок валиден
        /// </summary>
        public static bool IsValid(Player player) 
            => player != null && player.IsConnected;
        
        /// <summary>
        /// Проверить, что игрок валиден и жив
        /// </summary>
        public static bool IsValidAndAlive(Player player) 
            => IsValid(player) && player.IsAlive;
        
        /// <summary>
        /// Выполнить действие если игрок валиден
        /// </summary>
        public static void IfValid(Player player, Action<Player> action)
        {
            if (IsValid(player))
                action(player);
        }
        
        /// <summary>
        /// Выполнить действие если игрок валиден и жив
        /// </summary>
        public static void IfAlive(Player player, Action<Player> action)
        {
            if (IsValidAndAlive(player))
                action(player);
        }
        
        #endregion
    }
    
    /// <summary>
    /// Расширения для работы с перечислениями
    /// </summary>
    public static class EnumExtensions
    {
        private static readonly System.Random Random = new();
        
        /// <summary>
        /// Получить случайное значение enum
        /// </summary>
        public static T GetRandom<T>() where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(Random.Next(values.Length));
        }
        
        /// <summary>
        /// Получить все значения enum
        /// </summary>
        public static IEnumerable<T> GetAll<T>() where T : Enum
            => Enum.GetValues(typeof(T)).Cast<T>();
        
        /// <summary>
        /// Попробовать распарсить enum
        /// </summary>
        public static bool TryParse<T>(string value, out T result) where T : struct, Enum
            => Enum.TryParse(value, true, out result);
        
        /// <summary>
        /// Безопасный парсинг enum
        /// </summary>
        public static T ParseOrDefault<T>(string value, T defaultValue = default) where T : struct, Enum
            => Enum.TryParse<T>(value, true, out var result) ? result : defaultValue;
    }
    
    /// <summary>
    /// Расширения для nullable типов
    /// </summary>
    public static class NullableExtensions
    {
        /// <summary>
        /// Получить значение или дефолт
        /// </summary>
        public static T GetValueOrDefault<T>(this T? nullable, T defaultValue) where T : struct
            => nullable ?? defaultValue;
        
        /// <summary>
        /// Выполнить действие если значение есть
        /// </summary>
        public static void IfHasValue<T>(this T? nullable, Action<T> action) where T : struct
        {
            if (nullable.HasValue)
                action(nullable.Value);
        }
        
        /// <summary>
        /// Преобразовать если значение есть
        /// </summary>
        public static TResult? Map<T, TResult>(this T? nullable, Func<T, TResult> selector) 
            where T : struct 
            where TResult : struct
            => nullable.HasValue ? selector(nullable.Value) : null;
    }
}
