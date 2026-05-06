using Exiled.API.Features;
using FermixAPI.Core;
using System;

namespace FermixAPI
{
    /// <summary>
    /// Главный класс плагина FermixAPI для EXILED.
    /// Обеспечивает быстрое и удобное написание кода для SCP:SL серверов.
    /// </summary>
    public sealed class Plugin : Plugin<Config>
    {
        /// <summary>
        /// Singleton-экземпляр плагина для доступа из любого места.
        /// </summary>
        public static Plugin Instance { get; private set; }

        public override string Name => "FermixAPI";
        public override string Author => "Fermix";
        public override string Prefix => "fermix";
        public override Version Version => new Version(FermixCore.VersionMajor, FermixCore.VersionMinor, FermixCore.VersionPatch);

        /// <summary>
        /// FermixAPI is built and tested against EXILED 9.13.x (current upstream
        /// release at <see href="https://github.com/ExMod-Team/EXILED"/>).
        /// </summary>
        public override Version RequiredExiledVersion => new Version(9, 13, 3);

        public override void OnEnabled()
        {
            Instance = this;
            
            // Инициализация ядра API
            FermixCore.Initialize(this);
            
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            // Корректная очистка всех ресурсов
            FermixCore.Shutdown();
            
            Instance = null;
            base.OnDisabled();
        }

        public override void OnReloaded()
        {
            // При перезагрузке плагина переинициализируем события
            FermixEvents.Refresh();
            base.OnReloaded();
        }
    }
}
