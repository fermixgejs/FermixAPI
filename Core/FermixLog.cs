using Exiled.API.Features;
using System;
using System.Diagnostics;

namespace FermixAPI.Core
{
    /// <summary>
    /// Система логирования FermixAPI с форматированным выводом.
    /// </summary>
    public static class FermixLog
    {
        private static string Prefix => $"[FermixAPI]";
        private static string TimeStamp => DateTime.Now.ToString("HH:mm:ss");

        #region Logo

        /// <summary>
        /// Выводит ASCII-логотип FermixAPI в консоль.
        /// </summary>
        public static void DrawLogo()
        {
            string logo = @"
 ███████╗███████╗██████╗ ███╗   ███╗██╗██╗  ██╗     █████╗ ██████╗ ██╗
 ██╔════╝██╔════╝██╔══██╗████╗ ████║██║╚██╗██╔╝    ██╔══██╗██╔══██╗██║
 █████╗  █████╗  ██████╔╝██╔████╔██║██║ ╚███╔╝     ███████║██████╔╝██║
 ██╔══╝  ██╔══╝  ██╔══██╗██║╚██╔╝██║██║ ██╔██╗     ██╔══██║██╔═══╝ ██║
 ██║     ███████╗██║  ██║██║ ╚═╝ ██║██║██╔╝ ██╗    ██║  ██║██║     ██║
 ╚═╝     ╚══════╝╚═╝  ╚═╝╚═╝     ╚═╝╚═╝╚═╝  ╚═╝    ╚═╝  ╚═╝╚═╝     ╚═╝
                    v" + FermixCore.Version + @" | Fast & Easy EXILED Development
";
            Log.Info(logo);
        }

        #endregion

        #region Standard Logging

        /// <summary>
        /// Информационное сообщение.
        /// </summary>
        public static void Info(string message)
        {
            Log.Info($"{Prefix} {message}");
        }

        /// <summary>
        /// Сообщение об успехе.
        /// </summary>
        public static void Success(string message)
        {
            Log.Info($"{Prefix} [OK] {message}");
        }

        /// <summary>
        /// Предупреждение.
        /// </summary>
        public static void Warn(string message)
        {
            Log.Warn($"{Prefix} {message}");
        }

        /// <summary>
        /// Ошибка.
        /// </summary>
        public static void Error(string message)
        {
            Log.Error($"{Prefix} {message}");
        }

        /// <summary>
        /// Отладочное сообщение (выводится только при Debug = true в конфиге).
        /// </summary>
        public static void Debug(string message)
        {
            if (FermixCore.Config?.Debug == true)
            {
                Log.Debug($"{Prefix} [DEBUG] {message}");
            }
        }

        /// <summary>
        /// Логирование действия (если включено в конфиге).
        /// </summary>
        public static void Action(string action, string details = null)
        {
            if (FermixCore.Config?.LogAllActions == true)
            {
                var msg = string.IsNullOrEmpty(details)
                    ? $"[ACTION] {action}"
                    : $"[ACTION] {action}: {details}";
                Log.Info($"{Prefix} {msg}");
            }
        }

        #endregion

        #region Extended Logging

        /// <summary>
        /// Логирует исключение с полной информацией.
        /// </summary>
        public static void Exception(Exception ex, string context = null)
        {
            var ctx = string.IsNullOrEmpty(context) ? "" : $" в {context}";
            Error($"Исключение{ctx}: {ex.Message}");
            Debug($"StackTrace: {ex.StackTrace}");

            if (ex.InnerException != null)
            {
                Debug($"Inner: {ex.InnerException.Message}");
            }
        }

        /// <summary>
        /// Логирует время выполнения операции.
        /// </summary>
        public static void Timed(string operation, Action action)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                action();
            }
            finally
            {
                sw.Stop();
                Debug($"{operation} выполнено за {sw.ElapsedMilliseconds}мс");
            }
        }

        /// <summary>
        /// Начинает измерение времени.
        /// </summary>
        public static Stopwatch StartTimer(string label = null)
        {
            if (!string.IsNullOrEmpty(label))
            {
                Debug($"Начало: {label}");
            }
            return Stopwatch.StartNew();
        }

        /// <summary>
        /// Завершает измерение времени.
        /// </summary>
        public static void EndTimer(Stopwatch sw, string label)
        {
            sw.Stop();
            Debug($"{label}: {sw.ElapsedMilliseconds}мс");
        }

        #endregion

        #region Conditional Logging

        /// <summary>
        /// Логирует только если условие истинно.
        /// </summary>
        public static void InfoIf(bool condition, string message)
        {
            if (condition) Info(message);
        }

        /// <summary>
        /// Логирует предупреждение только если условие истинно.
        /// </summary>
        public static void WarnIf(bool condition, string message)
        {
            if (condition) Warn(message);
        }

        /// <summary>
        /// Логирует ошибку только если условие истинно.
        /// </summary>
        public static void ErrorIf(bool condition, string message)
        {
            if (condition) Error(message);
        }

        #endregion

        #region Player Logging

        /// <summary>
        /// Логирует действие игрока.
        /// </summary>
        public static void PlayerAction(Player player, string action)
        {
            Action($"[{player.Nickname}] {action}");
        }

        /// <summary>
        /// Логирует событие с игроком.
        /// </summary>
        public static void PlayerEvent(Player player, string eventName, string details = null)
        {
            var msg = string.IsNullOrEmpty(details)
                ? $"[{player.Nickname}] {eventName}"
                : $"[{player.Nickname}] {eventName}: {details}";
            Debug(msg);
        }

        #endregion
    }
}
