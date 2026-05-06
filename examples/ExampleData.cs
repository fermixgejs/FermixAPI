// FermixData — JSON / текст / per-player хранилища.
// Все файлы лежат в Configs/FermixAPI/Data (FermixPaths.Data).

using System;
using Exiled.API.Features;
using FermixAPI.Utils;

namespace MyServer.Examples
{
    public static class ExampleData
    {
        // ===== JSON =====
        public sealed class ServerStats
        {
            public int RoundsPlayed { get; set; }
            public DateTime LastReset { get; set; } = DateTime.UtcNow;
        }

        public static void DemoJson()
        {
            // Загрузить или создать.
            var stats = FermixData.LoadOrCreate<ServerStats>("server_stats");
            stats.RoundsPlayed++;
            FermixData.SaveJson("server_stats", stats);

            FermixLog.Info($"Сыграно раундов: {stats.RoundsPlayed}");
        }

        // ===== Текстовый лог =====
        public static void DemoText()
        {
            FermixData.AppendLine("kill_log", $"[{DateTime.Now:HH:mm:ss}] kill happened");
            string[] lines = FermixData.LoadLines("kill_log");
            FermixLog.Info($"В логе строк: {lines.Length}");
        }

        // ===== Per-player хранилище =====
        public sealed class PlayerProfile
        {
            public int Kills { get; set; }
            public int Deaths { get; set; }
            public string FavouriteRole { get; set; } = "";
        }

        // Создаётся один на плагин, кэширует данные в памяти и сохраняет в JSON.
        private static readonly PlayerDataStore<PlayerProfile> _profiles =
            new PlayerDataStore<PlayerProfile>("profiles", autoSave: true);

        public static void OnPlayerKill(Player killer, Player victim)
        {
            var killerProfile = _profiles.Get(killer);
            killerProfile.Kills++;

            var victimProfile = _profiles.Get(victim);
            victimProfile.Deaths++;
            // autoSave=true — сохранится автоматически.
        }
    }
}
