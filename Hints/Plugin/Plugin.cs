// ReSharper disable once CheckNamespace
namespace FermixAPI.Hints.Plugin
{
    /// <summary>
    /// Внутренний singleton hint-движка. Не является EXILED-плагином —
    /// это просто контейнер для конфига, нужный коду, заимствованному
    /// из HintServiceMeow (MIT, см. <c>vendor/HintServiceMeow/LICENSE</c>).
    /// Жизненный цикл движка управляется из <see cref="FermixAPI.Core.FermixHintEngine"/>.
    /// </summary>
    internal class Plugin
    {
        public static Plugin Instance { get; } = new Plugin();

        public PluginConfig Config { get; set; } = new PluginConfig();
    }
}
