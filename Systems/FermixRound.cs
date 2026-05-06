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
    /// Система управления раундом и игровыми режимами.
    /// </summary>
    public static class FermixRound
    {
        #region Round State - Состояние Раунда

        /// <summary>
        /// Проверяет, начался ли раунд.
        /// </summary>
        public static bool IsStarted => Round.IsStarted;

        /// <summary>
        /// Проверяет, завершился ли раунд.
        /// </summary>
        public static bool IsEnded => Round.IsEnded;

        /// <summary>
        /// Проверяет, заблокирован ли раунд.
        /// </summary>
        public static bool IsLocked => Round.IsLocked;

        /// <summary>
        /// Получает время раунда.
        /// </summary>
        public static TimeSpan ElapsedTime => Round.ElapsedTime;

        /// <summary>
        /// Получает время раунда в секундах.
        /// </summary>
        public static float ElapsedSeconds => (float)Round.ElapsedTime.TotalSeconds;

        /// <summary>
        /// Получает время раунда в минутах.
        /// </summary>
        public static float ElapsedMinutes => (float)Round.ElapsedTime.TotalMinutes;

        /// <summary>
        /// Получает номер раунда.
        /// </summary>
        public static int Number => Round.UptimeRounds;

        #endregion

        #region Round Control - Контроль Раунда

        /// <summary>
        /// Завершает раунд.
        /// </summary>
        public static void End(bool force = true)
        {
            Round.EndRound(force);
            FermixLog.Action("Раунд завершён");
        }

        /// <summary>
        /// Перезапускает раунд.
        /// </summary>
        public static void Restart()
        {
            Round.Restart();
            FermixLog.Action("Раунд перезапущен");
        }

        /// <summary>
        /// Перезапускает раунд с задержкой и уведомлением.
        /// </summary>
        public static void RestartIn(float seconds, bool notify = true)
        {
            if (notify)
            {
                FermixServer.GlobalHint($"<color=yellow>Раунд перезапустится через {seconds:F0} секунд...</color>", seconds);
            }
            
            FermixScheduler.Delay(seconds, Restart);
        }

        /// <summary>
        /// Блокирует раунд.
        /// </summary>
        public static void Lock()
        {
            Round.IsLocked = true;
            FermixLog.Action("Раунд заблокирован");
        }

        /// <summary>
        /// Разблокирует раунд.
        /// </summary>
        public static void Unlock()
        {
            Round.IsLocked = false;
            FermixLog.Action("Раунд разблокирован");
        }

        /// <summary>
        /// Переключает блокировку раунда.
        /// </summary>
        public static void ToggleLock()
        {
            Round.IsLocked = !Round.IsLocked;
            FermixLog.Action($"Блокировка раунда: {(Round.IsLocked ? "ВКЛ" : "ВЫКЛ")}");
        }

        /// <summary>
        /// Блокирует раунд на время.
        /// </summary>
        public static void LockFor(float seconds)
        {
            Lock();
            FermixScheduler.Delay(seconds, Unlock);
        }

        #endregion

        #region Win Conditions - Условия Победы

        /// <summary>
        /// Проверяет, остались ли SCP.
        /// </summary>
        public static bool AreScpsAlive()
        {
            return Player.List.Any(p => p.IsScp);
        }

        /// <summary>
        /// Проверяет, остались ли люди.
        /// </summary>
        public static bool AreHumansAlive()
        {
            return Player.List.Any(p => p.IsHuman && p.IsAlive);
        }

        /// <summary>
        /// Проверяет, остались ли MTF/Охрана.
        /// </summary>
        public static bool AreMtfAlive()
        {
            return Player.List.Any(p => 
                p.Role.Team == Team.FoundationForces && p.IsAlive);
        }

        /// <summary>
        /// Проверяет, остались ли Chaos Insurgency.
        /// </summary>
        public static bool AreChaosAlive()
        {
            return Player.List.Any(p => 
                p.Role.Team == Team.ChaosInsurgency && p.IsAlive);
        }

        /// <summary>
        /// Проверяет, остались ли учёные.
        /// </summary>
        public static bool AreScientistsAlive()
        {
            return Player.List.Any(p => 
                p.Role.Type == RoleTypeId.Scientist && p.IsAlive);
        }

        /// <summary>
        /// Проверяет, остались ли класс D.
        /// </summary>
        public static bool AreClassDAlive()
        {
            return Player.List.Any(p => 
                p.Role.Type == RoleTypeId.ClassD && p.IsAlive);
        }

        /// <summary>
        /// Получает текущую лидирующую сторону.
        /// </summary>
        public static Side GetLeadingSide()
        {
            var sides = new Dictionary<Side, int>
            {
                [Side.Scp] = FermixServer.GetBySide(Side.Scp).Count(),
                [Side.Mtf] = FermixServer.GetBySide(Side.Mtf).Count(),
                [Side.ChaosInsurgency] = FermixServer.GetBySide(Side.ChaosInsurgency).Count()
            };

            return sides.OrderByDescending(x => x.Value).First().Key;
        }

        /// <summary>
        /// Получает количество выживших по сторонам.
        /// </summary>
        public static Dictionary<Side, int> GetSurvivorCounts()
        {
            return new Dictionary<Side, int>
            {
                [Side.Scp] = FermixServer.GetBySide(Side.Scp).Count(),
                [Side.Mtf] = FermixServer.GetBySide(Side.Mtf).Count(),
                [Side.ChaosInsurgency] = FermixServer.GetBySide(Side.ChaosInsurgency).Count(),
                [Side.Tutorial] = FermixServer.GetBySide(Side.Tutorial).Count()
            };
        }

        #endregion

        #region Custom Round Modes - Пользовательские Режимы

        /// <summary>
        /// Режим "Последний выживший".
        /// </summary>
        public static void StartLastManStanding()
        {
            Lock();
            
            foreach (var player in Player.List)
            {
                player.Role.Set(RoleTypeId.ClassD);
                player.ClearInventory();
                player.AddItem(ItemType.GunCOM18);
                player.Ammo[ItemType.Ammo9x19] = 50;
            }
            
            FermixServer.GlobalHint("<color=red>РЕЖИМ: ПОСЛЕДНИЙ ВЫЖИВШИЙ</color>\n<size=20>Убей всех, чтобы победить!</size>", 10f);
            FermixLog.Action("Запущен режим: Последний выживший");
        }

        /// <summary>
        /// Режим "Зомби апокалипсис".
        /// </summary>
        public static void StartZombieApocalypse()
        {
            Lock();
            
            var players = Player.List.ToList();
            var zombieCount = Math.Max(1, players.Count / 4);
            
            var zombies = players.OrderBy(_ => UnityEngine.Random.value).Take(zombieCount).ToList();
            var survivors = players.Except(zombies).ToList();
            
            foreach (var zombie in zombies)
            {
                zombie.Role.Set(RoleTypeId.Scp0492);
            }
            
            foreach (var survivor in survivors)
            {
                survivor.Role.Set(RoleTypeId.ClassD);
                survivor.ClearInventory();
                survivor.AddItem(ItemType.GunCOM18);
                survivor.Ammo[ItemType.Ammo9x19] = 100;
            }
            
            FermixServer.GlobalHint("<color=green>РЕЖИМ: ЗОМБИ АПОКАЛИПСИС</color>\n<size=20>Выживи любой ценой!</size>", 10f);
            FermixLog.Action("Запущен режим: Зомби апокалипсис");
        }

        /// <summary>
        /// Режим "Прятки".
        /// </summary>
        public static void StartHideAndSeek(float seekDelay = 30f)
        {
            Lock();
            
            var players = Player.List.ToList();
            var seeker = players[UnityEngine.Random.Range(0, players.Count)];
            var hiders = players.Where(p => p != seeker).ToList();
            
            // Искатель - SCP-173
            seeker.Role.Set(RoleTypeId.Scp173);
            seeker.Position = FermixRooms.GetDClassSpawn();
            seeker.EnableEffect(EffectType.Blinded, 255, seekDelay); // Слепота на время прятанья
            
            // Прячущиеся - класс D
            foreach (var hider in hiders)
            {
                hider.Role.Set(RoleTypeId.ClassD);
                hider.ClearInventory();
            }
            
            FermixServer.GlobalHint($"<color=purple>РЕЖИМ: ПРЯТКИ</color>\n<size=20>У вас {seekDelay:F0} секунд, чтобы спрятаться!</size>", 10f);
            
            // Обратный отсчёт
            FermixScheduler.Countdown(seekDelay, 
                remaining => 
                {
                    if (remaining <= 10)
                    {
                        seeker.ShowHint($"<color=red>{remaining:F0}</color>", 1.1f);
                    }
                },
                () =>
                {
                    seeker.DisableEffect(EffectType.Blinded);
                    FermixServer.GlobalHint("<color=red>ОХОТА НАЧАЛАСЬ!</color>", 5f);
                }
            );
            
            FermixLog.Action("Запущен режим: Прятки");
        }

        #endregion

        #region Round Events - События Раунда

        /// <summary>
        /// Выполняет действие в определённую минуту раунда.
        /// </summary>
        public static void AtMinute(float minute, Action action)
        {
            var targetSeconds = minute * 60f;
            
            FermixScheduler.WaitUntil(
                () => ElapsedSeconds >= targetSeconds,
                action,
                1f
            );
        }

        /// <summary>
        /// Выполняет действие каждые N минут.
        /// </summary>
        public static void EveryMinutes(float minutes, Action action)
        {
            FermixScheduler.Repeat("round_interval", minutes * 60f, action);
        }

        /// <summary>
        /// Выполняет действие при достижении условия.
        /// </summary>
        public static void WhenCondition(Func<bool> condition, Action action, float checkInterval = 1f)
        {
            FermixScheduler.WaitUntil(condition, action, checkInterval);
        }

        /// <summary>
        /// Выполняет действие когда останется мало игроков.
        /// </summary>
        public static void WhenFewPlayersLeft(int threshold, Action action)
        {
            WhenCondition(() => FermixServer.GetAliveCount() <= threshold, action);
        }

        /// <summary>
        /// Выполняет действие когда все SCP мертвы.
        /// </summary>
        public static void WhenAllScpsDead(Action action)
        {
            WhenCondition(() => !AreScpsAlive() && IsStarted, action);
        }

        /// <summary>
        /// Выполняет действие когда все люди мертвы.
        /// </summary>
        public static void WhenAllHumansDead(Action action)
        {
            WhenCondition(() => !AreHumansAlive() && IsStarted, action);
        }

        #endregion

        #region Round Summary - Итоги Раунда

        /// <summary>
        /// Создаёт строку с итогами раунда.
        /// </summary>
        public static string GenerateSummary()
        {
            var duration = ElapsedTime;
            var stats = FermixServer.GetPlayerStats();
            
            return $@"
=== ИТОГИ РАУНДА #{Number} ===
Длительность: {duration.Minutes}:{duration.Seconds:D2}
Всего игроков: {stats["Total"]}
Выжившие: {stats["Alive"]}
SCP: {stats["SCP"]}
MTF: {stats["MTF"]}
Chaos: {stats["Chaos"]}
";
        }

        /// <summary>
        /// Выводит итоги раунда в лог.
        /// </summary>
        public static void LogSummary()
        {
            FermixLog.Info(GenerateSummary());
        }

        /// <summary>
        /// Показывает итоги раунда всем игрокам.
        /// </summary>
        public static void ShowSummaryToAll(float duration = 10f)
        {
            var summary = GenerateSummary();
            FermixServer.GlobalHint(summary, duration);
        }

        #endregion
    }
}
