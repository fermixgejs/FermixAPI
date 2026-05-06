// ReSharper disable once CheckNamespace
namespace FermixAPI.Hints.Plugin
{
    using System.Collections.Generic;

    /// <summary>
    /// Внутренний конфиг hint-движка. Остаётся API-совместимый с upstream
    /// HintServiceMeow.PluginConfig (часть исходников этого движка
    /// заимствована из HintServiceMeow, MIT, см. <c>vendor/HintServiceMeow/LICENSE</c>),
    /// чтобы патчи и адаптер совместимости работали без изменений.
    /// </summary>
    internal class PluginConfig
    {
        public bool IsEnabled { get; set; } = true;

        public bool Debug { get; set; } = false;

        /// <summary>
        /// Перехватывать ли сторонние вызовы <c>player.ShowHint</c> / <c>player.SendHint</c>
        /// и переводить их в собственный pipeline. Включаем, чтобы хинты от
        /// нашего FermixCoin (и любых других плагинов на том же сервере)
        /// не «съедались» Mirror-репликацией native-хинтов.
        /// </summary>
        public bool UseHintCompatibilityAdapter { get; set; } = true;

        /// <summary>Список assembly-имён, которые НЕ нужно адаптировать.</summary>
        public List<string> DisabledCompatAdapter { get; set; } = new List<string>();

        public int ItemHintDisplayTime { get; set; } = 10;

        public int ShortItemHintDisplayTime { get; set; } = 5;

        public int MapHintDisplayTime { get; set; } = 10;

        public int ShortMapHintDisplayTime { get; set; } = 7;

        public int RoleHintDisplayTime { get; set; } = 15;

        public int ShortRoleHintDisplayTime { get; set; } = 5;

        public int OtherHintDisplayTime { get; set; } = 5;
    }
}
