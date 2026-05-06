// Работа с конфигами:
//   • Стандартный путь EXILED — IConfig в Plugin<TConfig>.
//   • FermixConfigUtils — для дополнительных YAML-конфигов рядом с основным.
//   • FermixData — для JSON / бинарных / текстовых файлов в Configs/FermixAPI/Data.

using System.Collections.Generic;
using Exiled.API.Interfaces;
using FermixAPI.Utils;

namespace MyServer.Examples
{
    /// <summary>Основной конфиг плагина — EXILED сам сохраняет/загружает.</summary>
    public sealed class ExamplePluginConfig : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        public string Greeting { get; set; } = "Добро пожаловать!";

        public List<string> AdminUserIds { get; set; } = new();

        public Dictionary<string, int> RewardsByRank { get; set; } = new()
        {
            ["owner"]  = 1000,
            ["admin"]  = 500,
            ["player"] = 100,
        };
    }

    /// <summary>Дополнительный конфиг — сохраняется в Configs/FermixAPI/{fileName}.yml.</summary>
    public sealed class ShopConfig : FermixPluginConfig
    {
        public Dictionary<string, int> Prices { get; set; } = new()
        {
            ["AK"]      = 200,
            ["Medkit"]  = 50,
            ["Grenade"] = 75,
        };
    }

    public static class ExampleConfig
    {
        private const string ShopFile = "shop";

        public static void DemoSave()
        {
            var cfg = new ShopConfig
            {
                IsEnabled = true,
                Prices = { ["AK"] = 250 }, // обновили цену
            };

            FermixConfigUtils.Save(ShopFile, cfg);
        }

        public static void DemoLoad()
        {
            // Если файла нет — создастся со значениями по умолчанию.
            var cfg = FermixConfigUtils.Load<ShopConfig>(ShopFile);
            FermixLog.Info($"AK: {cfg.Prices["AK"]}$");
        }

        public static void DemoExists()
        {
            if (FermixConfigUtils.Exists(ShopFile))
                FermixLog.Info("Конфиг магазина уже существует");
        }
    }
}
