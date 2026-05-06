using System;
using System.IO;
using Exiled.API.Features;

namespace FermixAPI.Core
{
    /// <summary>
    /// Стандартное расположение каталогов FermixAPI и помощник по их созданию.
    /// Все каталоги авто-создаются при <see cref="Initialize"/>; вручную ничего трогать не нужно.
    ///
    /// <code>
    /// {EXILED}/Configs/FermixAPI/         — корневая папка API.
    /// {EXILED}/Configs/FermixAPI/Data/    — JSON/бинарные данные (используется FermixData).
    /// {EXILED}/Configs/FermixAPI/Plugins/ — папка под конфиги/данные дочерних плагинов на FermixAPI.
    /// {EXILED}/Configs/FermixAPI/Logs/    — отдельные логи API (если включены в Config).
    /// {EXILED}/Plugins/FermixAPI/         — папка для дополнительных DLL (если плагин их подгружает).
    /// </code>
    /// </summary>
    public static class FermixPaths
    {
        /// <summary>{EXILED}/Configs/FermixAPI</summary>
        public static string Root { get; private set; } = string.Empty;

        /// <summary>{EXILED}/Configs/FermixAPI/Data</summary>
        public static string Data { get; private set; } = string.Empty;

        /// <summary>{EXILED}/Configs/FermixAPI/Plugins</summary>
        public static string Plugins { get; private set; } = string.Empty;

        /// <summary>{EXILED}/Configs/FermixAPI/Logs</summary>
        public static string Logs { get; private set; } = string.Empty;

        /// <summary>{EXILED}/Plugins/FermixAPI</summary>
        public static string PluginsAssemblyDir { get; private set; } = string.Empty;

        /// <summary>
        /// Инициализирует пути и создаёт недостающие директории. Идемпотентно.
        /// </summary>
        public static void Initialize()
        {
            try
            {
                Root  = Path.Combine(Paths.Configs, "FermixAPI");
                Data  = Path.Combine(Root, "Data");
                Plugins = Path.Combine(Root, "Plugins");
                Logs  = Path.Combine(Root, "Logs");
                PluginsAssemblyDir = Path.Combine(Paths.Plugins, "FermixAPI");

                EnsureDirectory(Root);
                EnsureDirectory(Data);
                EnsureDirectory(Plugins);
                EnsureDirectory(Logs);
                EnsureDirectory(PluginsAssemblyDir);

                FermixLog.Debug($"FermixPaths инициализирован: {Root}");
            }
            catch (Exception ex)
            {
                FermixLog.Error($"Не удалось создать структуру каталогов FermixAPI: {ex.Message}");
            }
        }

        /// <summary>Получить (или создать) подкаталог внутри <see cref="Root"/>.</summary>
        public static string GetSubDirectory(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            var path = Path.Combine(Root, name);
            EnsureDirectory(path);
            return path;
        }

        /// <summary>Получить (или создать) папку под конкретный плагин в <see cref="Plugins"/>.</summary>
        public static string GetPluginDirectory(string pluginName)
        {
            if (string.IsNullOrEmpty(pluginName))
                throw new ArgumentNullException(nameof(pluginName));

            var path = Path.Combine(Plugins, pluginName);
            EnsureDirectory(path);
            return path;
        }

        private static void EnsureDirectory(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
