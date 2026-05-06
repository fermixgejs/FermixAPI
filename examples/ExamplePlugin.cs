// Пример минимального плагина на FermixAPI.
//
// 1. Положи FermixAPI.dll в EXILED/Plugins/ — он загружается раньше твоего плагина.
// 2. Сошлись из своего .csproj на FermixAPI.dll и Exiled.API.dll.
// 3. Этот класс используй как стартовый шаблон.

using System;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using FermixAPI.Core;

namespace MyServer
{
    public sealed class MyPlugin : Plugin<MyPluginConfig>
    {
        public static MyPlugin Instance { get; private set; }

        public override string Name   => "MyPlugin";
        public override string Author => "you";
        public override string Prefix => "myplugin";
        public override Version Version => new Version(1, 0, 0);

        public override Version RequiredExiledVersion => new Version(9, 13, 3);

        public override void OnEnabled()
        {
            Instance = this;

            // Убедимся, что FermixAPI запустился и его модули доступны.
            FermixCore.EnsureInitialized();

            FermixLog.Success($"{Name} v{Version} включён.");

            // Любая подписка на FermixEvents — здесь.
            FermixEvents.OnRoundStart += OnRoundStart;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            FermixEvents.OnRoundStart -= OnRoundStart;

            FermixLog.Info($"{Name} выключен.");
            Instance = null;
            base.OnDisabled();
        }

        private void OnRoundStart()
        {
            FermixLog.Info("Раунд начался — здесь можно стартовать свою логику.");
        }
    }

    /// <summary>Конфиг плагина (см. <see cref="ExamplePluginConfig"/> для расширенных примеров).</summary>
    public sealed class MyPluginConfig : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        public string Greeting { get; set; } = "Добро пожаловать!";
    }
}
