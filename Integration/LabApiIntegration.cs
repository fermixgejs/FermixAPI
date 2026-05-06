using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Exiled.API.Features;
using FermixAPI.Core;

namespace FermixAPI.Integration
{
    /// <summary>
    /// Интеграция с LabAPI 1.1.x.
    /// <para>
    /// Так как FermixAPI собирается с прямой ссылкой на <c>LabApi.dll</c>,
    /// мы можем использовать строго типизированные обертки для часто
    /// нужных функций. Динамические (reflection-based) вызовы остаются
    /// доступными для случаев, когда LabAPI не подгружен в процесс
    /// (например, на сервере без LabAPI Loader).
    /// </para>
    /// </summary>
    public static class LabApiIntegration
    {
        private static bool _initialized;
        private static bool _isLabApiAvailable;
        private static Assembly _labApiAssembly;
        private static Version _labApiVersion;

        /// <summary>
        /// Доступен ли LabAPI в текущем процессе.
        /// </summary>
        public static bool IsAvailable
        {
            get
            {
                if (!_initialized)
                    Initialize();
                return _isLabApiAvailable;
            }
        }

        /// <summary>
        /// Версия загруженного LabAPI или <c>null</c>, если он недоступен.
        /// </summary>
        public static Version Version
        {
            get
            {
                if (!_initialized)
                    Initialize();
                return _labApiVersion;
            }
        }

        /// <summary>
        /// Инициализация интеграции с LabAPI.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            try
            {
                _labApiAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a =>
                    {
                        var name = a.GetName().Name ?? string.Empty;
                        return string.Equals(name, "LabApi", StringComparison.OrdinalIgnoreCase)
                               || string.Equals(name, "LabAPI", StringComparison.OrdinalIgnoreCase);
                    });

                _isLabApiAvailable = _labApiAssembly != null;

                if (_isLabApiAvailable)
                {
                    _labApiVersion = _labApiAssembly.GetName().Version;
                    FermixLog.Info($"LabAPI v{_labApiVersion} обнаружен и интеграция активирована.");

                    if (_labApiVersion != null && _labApiVersion < FermixCore.MinimumLabApiVersion)
                    {
                        FermixLog.Warn(
                            $"Версия LabAPI ({_labApiVersion}) ниже рекомендуемой " +
                            $"({FermixCore.MinimumLabApiVersion}). Возможна несовместимость.");
                    }

                    LoadLabApiFeatures();
                }
                else
                {
                    FermixLog.Debug("LabAPI не обнаружен, интеграция отключена.");
                }
            }
            catch (Exception ex)
            {
                FermixLog.Error($"Ошибка при инициализации LabAPI интеграции: {ex.Message}");
                _isLabApiAvailable = false;
            }
        }

        private static void LoadLabApiFeatures()
        {
            try
            {
                // Здесь можно добавить инициализацию специфичных для LabAPI функций.
            }
            catch (Exception ex)
            {
                FermixLog.Error($"Ошибка при загрузке LabAPI функций: {ex.Message}");
            }
        }

        #region Typed wrappers (LabAPI 1.1.x)

        /// <summary>
        /// Возвращает обертку <see cref="LabApi.Features.Wrappers.Player"/>
        /// над переданным игроком EXILED.
        /// </summary>
        /// <param name="player">Игрок EXILED.</param>
        /// <returns>
        /// Соответствующий <see cref="LabApi.Features.Wrappers.Player"/> или
        /// <c>null</c>, если LabAPI недоступен или игрок не валиден.
        /// </returns>
        public static LabApi.Features.Wrappers.Player AsLabApiPlayer(Player player)
        {
            if (!IsAvailable || player?.ReferenceHub == null)
                return null;

            try
            {
                return LabApi.Features.Wrappers.Player.Get(player.ReferenceHub);
            }
            catch (Exception ex)
            {
                FermixLog.Debug($"Не удалось получить LabAPI Player: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Возвращает версию LabAPI, как сообщает сам фреймворк.
        /// Использует <see cref="LabApi.Features.LabApiProperties"/>.
        /// </summary>
        public static Version GetReportedLabApiVersion()
        {
            if (!IsAvailable)
                return null;

            try
            {
                return LabApi.Features.LabApiProperties.CurrentVersion;
            }
            catch
            {
                return _labApiVersion;
            }
        }

        #endregion

        #region Динамический вызов методов LabAPI (fallback)

        /// <summary>
        /// Получить значение свойства LabAPI по имени класса/свойства.
        /// </summary>
        public static T GetProperty<T>(string className, string propertyName)
        {
            if (!IsAvailable)
                return default;

            try
            {
                var type = _labApiAssembly.GetTypes()
                    .FirstOrDefault(t => t.Name == className || t.FullName?.EndsWith(className) == true);

                if (type == null)
                    return default;

                var property = type.GetProperty(propertyName,
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

                if (property == null)
                    return default;

                return (T)property.GetValue(null);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// Проверить существование класса в LabAPI.
        /// </summary>
        public static bool HasClass(string className)
        {
            if (!IsAvailable) return false;

            return _labApiAssembly.GetTypes()
                .Any(t => t.Name == className || t.FullName?.EndsWith(className) == true);
        }

        /// <summary>
        /// Проверить существование метода в LabAPI.
        /// </summary>
        public static bool HasMethod(string className, string methodName)
        {
            if (!IsAvailable) return false;

            var type = _labApiAssembly.GetTypes()
                .FirstOrDefault(t => t.Name == className || t.FullName?.EndsWith(className) == true);

            return type?.GetMethod(methodName) != null;
        }

        #endregion

        #region Обертки для условного выполнения

        /// <summary>
        /// Условное выполнение - только если LabAPI доступен.
        /// </summary>
        public static void IfAvailable(Action action)
        {
            if (IsAvailable)
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception ex)
                {
                    FermixLog.Error($"Ошибка при выполнении LabAPI действия: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Условное выполнение с альтернативой.
        /// </summary>
        public static void IfAvailableOrElse(Action labApiAction, Action fallbackAction)
        {
            if (IsAvailable)
            {
                try
                {
                    labApiAction?.Invoke();
                }
                catch (Exception ex)
                {
                    FermixLog.Error($"Ошибка LabAPI, используется альтернатива: {ex.Message}");
                    fallbackAction?.Invoke();
                }
            }
            else
            {
                fallbackAction?.Invoke();
            }
        }

        /// <summary>
        /// Выполнить с LabAPI или вернуть значение по умолчанию.
        /// </summary>
        public static T ExecuteOrDefault<T>(Func<T> labApiFunc, T defaultValue = default)
        {
            if (!IsAvailable)
                return defaultValue;

            try
            {
                return labApiFunc();
            }
            catch
            {
                return defaultValue;
            }
        }

        #endregion
    }

    /// <summary>
    /// Хелпер для работы с пользовательскими командами поверх LabAPI.
    /// </summary>
    public static class LabApiCommands
    {
        private static readonly Dictionary<string, Action<Player, string[]>> CustomCommands = new();

        /// <summary>
        /// Зарегистрировать кастомную команду.
        /// </summary>
        public static void Register(string command, Action<Player, string[]> handler)
        {
            var cmd = command.ToLower();
            CustomCommands[cmd] = handler;
            FermixLog.Debug($"Зарегистрирована команда: {cmd}");
        }

        /// <summary>
        /// Удалить команду.
        /// </summary>
        public static void Unregister(string command)
        {
            CustomCommands.Remove(command.ToLower());
        }

        /// <summary>
        /// Выполнить команду.
        /// </summary>
        public static bool Execute(Player player, string command, string[] args)
        {
            var cmd = command.ToLower();
            if (CustomCommands.TryGetValue(cmd, out var handler))
            {
                try
                {
                    handler(player, args);
                    return true;
                }
                catch (Exception ex)
                {
                    FermixLog.Error($"Ошибка при выполнении команды {cmd}: {ex.Message}");
                }
            }
            return false;
        }

        /// <summary>
        /// Проверить существование команды.
        /// </summary>
        public static bool Exists(string command)
            => CustomCommands.ContainsKey(command.ToLower());

        /// <summary>
        /// Получить список всех команд.
        /// </summary>
        public static IEnumerable<string> GetAll()
            => CustomCommands.Keys;

        /// <summary>
        /// Очистить все команды.
        /// </summary>
        public static void Clear()
            => CustomCommands.Clear();
    }

    /// <summary>
    /// Хелпер для работы с произвольными пользовательскими событиями.
    /// </summary>
    public static class LabApiEvents
    {
        private static readonly Dictionary<string, List<Delegate>> EventHandlers = new();

        /// <summary>
        /// Подписаться на событие.
        /// </summary>
        public static void Subscribe<T>(string eventName, Action<T> handler)
        {
            if (!EventHandlers.ContainsKey(eventName))
                EventHandlers[eventName] = new List<Delegate>();

            EventHandlers[eventName].Add(handler);
        }

        /// <summary>
        /// Отписаться от события.
        /// </summary>
        public static void Unsubscribe<T>(string eventName, Action<T> handler)
        {
            if (EventHandlers.TryGetValue(eventName, out var handlers))
                handlers.Remove(handler);
        }

        /// <summary>
        /// Вызвать событие.
        /// </summary>
        public static void Invoke<T>(string eventName, T args)
        {
            if (!EventHandlers.TryGetValue(eventName, out var handlers))
                return;

            foreach (var handler in handlers.ToList())
            {
                try
                {
                    if (handler is Action<T> typedHandler)
                        typedHandler(args);
                }
                catch (Exception ex)
                {
                    FermixLog.Error($"Ошибка при вызове события {eventName}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Очистить все обработчики события.
        /// </summary>
        public static void Clear(string eventName)
        {
            EventHandlers.Remove(eventName);
        }

        /// <summary>
        /// Очистить все события.
        /// </summary>
        public static void ClearAll()
        {
            EventHandlers.Clear();
        }
    }
}
