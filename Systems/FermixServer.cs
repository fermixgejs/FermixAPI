using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using FermixAPI.Core;
using PlayerRoles;
using UnityEngine;

namespace FermixAPI.Systems
{
    /// <summary>
    /// Система управления сервером и глобальными операциями.
    /// </summary>
    public static class FermixServer
    {
        #region Player Queries - Поиск Игроков

        /// <summary>
        /// Получает всех игроков.
        /// </summary>
        public static IEnumerable<Player> GetAllPlayers()
        {
            return Player.List;
        }

        /// <summary>
        /// Получает количество игроков.
        /// </summary>
        public static int GetPlayerCount()
        {
            return Player.List.Count();
        }

        /// <summary>
        /// Получает всех живых игроков.
        /// </summary>
        public static IEnumerable<Player> GetAlivePlayers()
        {
            return Player.List.Where(p => p.IsAlive);
        }

        /// <summary>
        /// Получает количество живых игроков.
        /// </summary>
        public static int GetAliveCount()
        {
            return GetAlivePlayers().Count();
        }

        /// <summary>
        /// Получает всех мёртвых игроков (наблюдателей).
        /// </summary>
        public static IEnumerable<Player> GetDeadPlayers()
        {
            return Player.List.Where(p => p.Role.Type == RoleTypeId.Spectator);
        }

        /// <summary>
        /// Получает количество мёртвых игроков.
        /// </summary>
        public static int GetDeadCount()
        {
            return GetDeadPlayers().Count();
        }

        /// <summary>
        /// Получает игроков по стороне.
        /// </summary>
        public static IEnumerable<Player> GetBySide(Side side)
        {
            return Player.List.Where(p => p.Role.Side == side);
        }

        /// <summary>
        /// Получает игроков по команде.
        /// </summary>
        public static IEnumerable<Player> GetByTeam(Team team)
        {
            return Player.List.Where(p => p.Role.Team == team);
        }

        /// <summary>
        /// Получает игроков по роли.
        /// </summary>
        public static IEnumerable<Player> GetByRole(RoleTypeId role)
        {
            return Player.List.Where(p => p.Role.Type == role);
        }

        /// <summary>
        /// Получает случайного живого игрока.
        /// </summary>
        public static Player GetRandomAlive()
        {
            var alive = GetAlivePlayers().ToList();
            return alive.Count > 0 ? alive[UnityEngine.Random.Range(0, alive.Count)] : null;
        }

        /// <summary>
        /// Получает случайного игрока по стороне.
        /// </summary>
        public static Player GetRandom(Side side)
        {
            var players = GetBySide(side).ToList();
            return players.Count > 0 ? players[UnityEngine.Random.Range(0, players.Count)] : null;
        }

        /// <summary>
        /// Получает игрока по ID.
        /// </summary>
        public static Player GetById(int playerId)
        {
            return Player.Get(playerId);
        }

        /// <summary>
        /// Получает игрока по UserID.
        /// </summary>
        public static Player GetByUserId(string userId)
        {
            return Player.Get(userId);
        }

        /// <summary>
        /// Получает игрока по нику.
        /// </summary>
        public static Player GetByNickname(string nickname)
        {
            if (string.IsNullOrEmpty(nickname))
                return null;
            return Player.List.FirstOrDefault(p =>
                p.Nickname != null &&
                p.Nickname.IndexOf(nickname, System.StringComparison.OrdinalIgnoreCase) >= 0);
        }

        #endregion

        #region Global Hints & Messages - Глобальные Сообщения

        /// <summary>
        /// Отправляет хинт всем игрокам.
        /// </summary>
        public static void GlobalHint(string message, float duration = 5f)
        {
            foreach (var player in Player.List)
            {
                player.ShowHint(message, duration);
            }
        }

        /// <summary>
        /// Отправляет цветной хинт всем игрокам.
        /// </summary>
        public static void GlobalColorHint(string message, string color, float duration = 5f)
        {
            GlobalHint($"<color={color}>{message}</color>", duration);
        }

        /// <summary>
        /// Отправляет broadcast всем игрокам.
        /// </summary>
        public static void GlobalBroadcast(string message, ushort duration = 5)
        {
            Map.Broadcast(duration, message);
        }

        /// <summary>
        /// Очищает broadcast у всех игроков.
        /// </summary>
        public static void ClearAllBroadcasts()
        {
            Map.ClearBroadcasts();
        }

        /// <summary>
        /// Отправляет консольное сообщение всем игрокам.
        /// </summary>
        public static void GlobalConsole(string message, string color = "white")
        {
            foreach (var player in Player.List)
            {
                player.SendConsoleMessage(message, color);
            }
        }

        /// <summary>
        /// Отправляет хинт игрокам стороны.
        /// </summary>
        public static void HintToSide(Side side, string message, float duration = 5f)
        {
            foreach (var player in GetBySide(side))
            {
                player.ShowHint(message, duration);
            }
        }

        /// <summary>
        /// Отправляет хинт игрокам команды.
        /// </summary>
        public static void HintToTeam(Team team, string message, float duration = 5f)
        {
            foreach (var player in GetByTeam(team))
            {
                player.ShowHint(message, duration);
            }
        }

        /// <summary>
        /// Отправляет хинт по условию.
        /// </summary>
        public static void HintWhere(Func<Player, bool> predicate, string message, float duration = 5f)
        {
            foreach (var player in Player.List.Where(predicate))
            {
                player.ShowHint(message, duration);
            }
        }

        #endregion

        #region Respawn Control - Контроль Респавна

        /// <summary>
        /// Принудительный респавн MTF.
        /// </summary>
        public static void ForceRespawnMTF()
        {
            Respawn.ForceWave(Faction.FoundationStaff);
            FermixLog.Action("Принудительный респавн MTF");
        }

        /// <summary>
        /// Принудительный респавн Chaos Insurgency.
        /// </summary>
        public static void ForceRespawnChaos()
        {
            Respawn.ForceWave(Faction.FoundationEnemy);
            FermixLog.Action("Принудительный респавн Chaos Insurgency");
        }

        /// <summary>
        /// Принудительный респавн определённой фракции.
        /// </summary>
        public static void ForceRespawn(Faction faction)
        {
            Respawn.ForceWave(faction);
            FermixLog.Action($"Принудительный респавн: {faction}");
        }

        #endregion

        #region Round Control - Контроль Раунда

        /// <summary>
        /// Блокирует/разблокирует раунд.
        /// </summary>
        public static void SetRoundLock(bool locked)
        {
            Round.IsLocked = locked;
            FermixLog.Warn($"Блокировка раунда: {(locked ? "ВКЛ" : "ВЫКЛ")}");
        }

        /// <summary>
        /// Переключает блокировку раунда.
        /// </summary>
        public static void ToggleRoundLock()
        {
            Round.IsLocked = !Round.IsLocked;
            FermixLog.Warn($"Блокировка раунда: {(Round.IsLocked ? "ВКЛ" : "ВЫКЛ")}");
        }

        /// <summary>
        /// Проверяет, заблокирован ли раунд.
        /// </summary>
        public static bool IsRoundLocked => Round.IsLocked;

        /// <summary>
        /// Завершает раунд.
        /// </summary>
        public static void EndRound()
        {
            Round.EndRound();
            FermixLog.Action("Раунд завершён принудительно");
        }

        /// <summary>
        /// Завершает раунд с указанием победителя.
        /// </summary>
        public static void EndRound(bool forceEnd = true)
        {
            Round.EndRound(forceEnd);
        }

        /// <summary>
        /// Перезапускает раунд.
        /// </summary>
        public static void RestartRound()
        {
            Round.Restart();
            FermixLog.Action("Раунд перезапущен");
        }

        /// <summary>
        /// Перезапускает раунд с задержкой.
        /// </summary>
        public static void RestartRoundIn(float seconds)
        {
            FermixScheduler.Delay(seconds, RestartRound);
            GlobalHint($"Раунд перезапустится через {seconds:F0} секунд...", seconds);
        }

        /// <summary>
        /// Получает время раунда.
        /// </summary>
        public static TimeSpan GetRoundTime()
        {
            return Round.ElapsedTime;
        }

        /// <summary>
        /// Получает время раунда в секундах.
        /// </summary>
        public static float GetRoundTimeSeconds()
        {
            return (float)Round.ElapsedTime.TotalSeconds;
        }

        /// <summary>
        /// Проверяет, начался ли раунд.
        /// </summary>
        public static bool HasRoundStarted => Round.IsStarted;

        /// <summary>
        /// Проверяет, завершился ли раунд.
        /// </summary>
        public static bool HasRoundEnded => Round.IsEnded;

        /// <summary>
        /// Получает номер раунда.
        /// </summary>
        public static int GetRoundNumber()
        {
            return Round.UptimeRounds;
        }

        #endregion

        #region Bulk Player Operations - Массовые Операции с Игроками

        /// <summary>
        /// Убивает всех игроков.
        /// </summary>
        public static void KillAll(string reason = "Убит через FermixAPI")
        {
            foreach (var player in GetAlivePlayers().ToList())
            {
                player.Kill(reason);
            }
            FermixLog.Action("Все игроки убиты");
        }

        /// <summary>
        /// Убивает всех игроков стороны.
        /// </summary>
        public static void KillSide(Side side, string reason = "Убит через FermixAPI")
        {
            foreach (var player in GetBySide(side).ToList())
            {
                player.Kill(reason);
            }
            FermixLog.Action($"Сторона {side} уничтожена");
        }

        /// <summary>
        /// Убивает всех игроков команды.
        /// </summary>
        public static void KillTeam(Team team, string reason = "Убит через FermixAPI")
        {
            foreach (var player in GetByTeam(team).ToList())
            {
                player.Kill(reason);
            }
            FermixLog.Action($"Команда {team} уничтожена");
        }

        /// <summary>
        /// Исцеляет всех живых игроков.
        /// </summary>
        public static void HealAll()
        {
            foreach (var player in GetAlivePlayers())
            {
                player.Health = player.MaxHealth;
            }
            FermixLog.Action("Все игроки исцелены");
        }

        /// <summary>
        /// Исцеляет игроков стороны.
        /// </summary>
        public static void HealSide(Side side)
        {
            foreach (var player in GetBySide(side))
            {
                player.Health = player.MaxHealth;
            }
        }

        /// <summary>
        /// Телепортирует всех живых к позиции.
        /// </summary>
        public static void TeleportAll(Vector3 position)
        {
            foreach (var player in GetAlivePlayers())
            {
                player.Position = position;
            }
        }

        /// <summary>
        /// Телепортирует сторону к позиции.
        /// </summary>
        public static void TeleportSide(Side side, Vector3 position)
        {
            foreach (var player in GetBySide(side))
            {
                player.Position = position;
            }
        }

        /// <summary>
        /// Очищает инвентарь у всех игроков.
        /// </summary>
        public static void ClearAllInventories()
        {
            foreach (var player in GetAlivePlayers())
            {
                player.ClearInventory();
            }
        }

        /// <summary>
        /// Применяет действие ко всем игрокам.
        /// </summary>
        public static void ForEachPlayer(Action<Player> action)
        {
            foreach (var player in Player.List)
            {
                action(player);
            }
        }

        /// <summary>
        /// Применяет действие к живым игрокам.
        /// </summary>
        public static void ForEachAlive(Action<Player> action)
        {
            foreach (var player in GetAlivePlayers())
            {
                action(player);
            }
        }

        /// <summary>
        /// Применяет действие к игрокам стороны.
        /// </summary>
        public static void ForEachInSide(Side side, Action<Player> action)
        {
            foreach (var player in GetBySide(side))
            {
                action(player);
            }
        }

        #endregion

        #region Effects - Эффекты

        /// <summary>
        /// Применяет эффект всем игрокам.
        /// </summary>
        public static void ApplyEffectToAll(EffectType effect, float duration = 0f, byte intensity = 1)
        {
            foreach (var player in GetAlivePlayers())
            {
                player.EnableEffect(effect, intensity, duration);
            }
        }

        /// <summary>
        /// Убирает эффект у всех игроков.
        /// </summary>
        public static void RemoveEffectFromAll(EffectType effect)
        {
            foreach (var player in Player.List)
            {
                player.DisableEffect(effect);
            }
        }

        /// <summary>
        /// Очищает все эффекты у всех игроков.
        /// </summary>
        public static void ClearAllEffects()
        {
            foreach (var player in Player.List)
            {
                player.DisableAllEffects();
            }
        }

        /// <summary>
        /// Ослепляет всех игроков.
        /// </summary>
        public static void BlindAll(float duration = 5f, byte intensity = 1)
        {
            ApplyEffectToAll(EffectType.Blinded, duration, intensity);
        }

        #endregion

        #region Server Stats - Статистика Сервера

        /// <summary>
        /// Получает статистику игроков.
        /// </summary>
        public static Dictionary<string, int> GetPlayerStats()
        {
            return new Dictionary<string, int>
            {
                ["Total"] = GetPlayerCount(),
                ["Alive"] = GetAliveCount(),
                ["Dead"] = GetDeadCount(),
                ["SCP"] = GetBySide(Side.Scp).Count(),
                ["MTF"] = GetByTeam(Team.FoundationForces).Count(),
                ["Chaos"] = GetByTeam(Team.ChaosInsurgency).Count(),
                ["Scientists"] = GetByRole(RoleTypeId.Scientist).Count(),
                ["ClassD"] = GetByRole(RoleTypeId.ClassD).Count()
            };
        }

        /// <summary>
        /// Выводит статистику игроков в лог.
        /// </summary>
        public static void LogPlayerStats()
        {
            var stats = GetPlayerStats();
            FermixLog.Info("=== Статистика игроков ===");
            foreach (var kvp in stats)
            {
                FermixLog.Info($"  {kvp.Key}: {kvp.Value}");
            }
        }

        /// <summary>
        /// Получает TPS сервера.
        /// </summary>
        public static float GetTps()
        {
            return (float)Server.Tps;
        }

        /// <summary>
        /// Получает максимальное количество слотов.
        /// </summary>
        public static int GetMaxPlayers()
        {
            return Server.MaxPlayerCount;
        }

        /// <summary>
        /// Проверяет, заполнен ли сервер.
        /// </summary>
        public static bool IsFull()
        {
            return GetPlayerCount() >= GetMaxPlayers();
        }

        #endregion

        #region CASSIE - Объявления

        /// <summary>
        /// Воспроизводит сообщение CASSIE.
        /// </summary>
        public static void CassieMessage(string message, bool isHeld = false, bool isNoisy = true, bool isSubtitles = false)
        {
            Exiled.API.Features.Cassie.Message(message, isHeld, isNoisy, isSubtitles);
            FermixLog.Action($"CASSIE: {message}");
        }

        /// <summary>
        /// Воспроизводит сообщение CASSIE с переводом-субтитрами.
        /// </summary>
        public static void CassieMessageTranslated(string message, string translation, bool isHeld = false, bool isNoisy = true, bool isSubtitles = true)
        {
            Exiled.API.Features.Cassie.MessageTranslated(message, translation, isHeld, isNoisy, isSubtitles);
        }

        /// <summary>
        /// Воспроизводит сообщение CASSIE с задержкой.
        /// </summary>
        public static void CassieDelayedMessage(string message, float delay, bool isHeld = false, bool isNoisy = true, bool isSubtitles = false)
        {
            Exiled.API.Features.Cassie.DelayedMessage(message, delay, isHeld, isNoisy, isSubtitles);
        }

        /// <summary>
        /// Воспроизводит «глитчи» CASSIE-сообщения.
        /// </summary>
        public static void CassieGlitchyMessage(string message, float glitchChance, float jamChance)
        {
            Exiled.API.Features.Cassie.GlitchyMessage(message, glitchChance, jamChance);
        }

        /// <summary>
        /// Очищает очередь CASSIE.
        /// </summary>
        public static void CassieClear()
        {
            Exiled.API.Features.Cassie.Clear();
        }

        #endregion
    }
}
