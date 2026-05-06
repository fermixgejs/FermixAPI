using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using FermixAPI.Core;
using PlayerRoles;
using UnityEngine;

namespace FermixAPI.Systems
{
    /// <summary>
    /// Система управления SCP и их способностями.
    /// </summary>
    public static class FermixScp
    {
        #region SCP Queries - Поиск SCP

        /// <summary>
        /// Получает всех SCP на сервере.
        /// </summary>
        public static IEnumerable<Player> GetAll()
        {
            return Player.List.Where(p => p.IsScp);
        }

        /// <summary>
        /// Получает количество SCP.
        /// </summary>
        public static int Count()
        {
            return GetAll().Count();
        }

        /// <summary>
        /// Проверяет, есть ли живые SCP.
        /// </summary>
        public static bool AnyAlive()
        {
            return GetAll().Any();
        }

        /// <summary>
        /// Получает SCP по типу роли.
        /// </summary>
        public static Player Get(RoleTypeId role)
        {
            return Player.List.FirstOrDefault(p => p.Role.Type == role);
        }

        /// <summary>
        /// Получает всех SCP определённого типа.
        /// </summary>
        public static IEnumerable<Player> GetAllOfType(RoleTypeId role)
        {
            return Player.List.Where(p => p.Role.Type == role);
        }

        /// <summary>
        /// Получает случайного SCP.
        /// </summary>
        public static Player GetRandom()
        {
            var scps = GetAll().ToList();
            return scps.Count > 0 ? scps[UnityEngine.Random.Range(0, scps.Count)] : null;
        }

        #endregion

        #region SCP-049 - Доктор Чума

        /// <summary>
        /// Получает SCP-049.
        /// </summary>
        public static Player Get049()
        {
            return Get(RoleTypeId.Scp049);
        }

        /// <summary>
        /// Получает роль SCP-049.
        /// </summary>
        public static Scp049Role Get049Role(this Player player)
        {
            return player.Role as Scp049Role;
        }

        /// <summary>
        /// Получает всех SCP-049-2 (зомби).
        /// </summary>
        public static IEnumerable<Player> GetAllZombies()
        {
            return GetAllOfType(RoleTypeId.Scp0492);
        }

        /// <summary>
        /// Подсчитывает зомби.
        /// </summary>
        public static int CountZombies()
        {
            return GetAllZombies().Count();
        }

        /// <summary>
        /// Превращает игрока в зомби.
        /// </summary>
        public static void TurnIntoZombie(Player player)
        {
            player.Role.Set(RoleTypeId.Scp0492);
        }

        #endregion

        #region SCP-079 - Компьютер

        /// <summary>
        /// Получает SCP-079.
        /// </summary>
        public static Player Get079()
        {
            return Get(RoleTypeId.Scp079);
        }

        /// <summary>
        /// Получает роль SCP-079.
        /// </summary>
        public static Scp079Role Get079Role(this Player player)
        {
            return player.Role as Scp079Role;
        }

        /// <summary>
        /// Устанавливает уровень SCP-079.
        /// </summary>
        public static void Set079Level(this Player player, int level)
        {
            if (player.Role is Scp079Role scp079)
            {
                scp079.Level = level;
            }
        }

        /// <summary>
        /// Добавляет опыт SCP-079.
        /// </summary>
        public static void Add079Experience(this Player player, int exp)
        {
            if (player.Role is Scp079Role scp079)
            {
                scp079.Experience += exp;
            }
        }

        /// <summary>
        /// Устанавливает энергию SCP-079.
        /// </summary>
        public static void Set079Energy(this Player player, float energy)
        {
            if (player.Role is Scp079Role scp079)
            {
                scp079.Energy = energy;
            }
        }

        /// <summary>
        /// Принудительно блэкаут от имени SCP-079.
        /// </summary>
        public static void Force079Blackout(float duration = 10f)
        {
            Map.TurnOffAllLights(duration);
            FermixLog.Action($"Принудительный блэкаут: {duration} сек");
        }

        #endregion

        #region SCP-096 - Застенчивый

        /// <summary>
        /// Получает SCP-096.
        /// </summary>
        public static Player Get096()
        {
            return Get(RoleTypeId.Scp096);
        }

        /// <summary>
        /// Получает роль SCP-096.
        /// </summary>
        public static Scp096Role Get096Role(this Player player)
        {
            return player.Role as Scp096Role;
        }

        /// <summary>
        /// Добавляет цель для SCP-096.
        /// </summary>
        public static void Add096Target(this Player scp096, Player target)
        {
            if (scp096.Role is Scp096Role role)
            {
                role.AddTarget(target);
            }
        }

        /// <summary>
        /// Успокаивает SCP-096.
        /// </summary>
        public static void CalmDown096(this Player player)
        {
            if (player.Role is Scp096Role role)
            {
                role.Calm();
            }
        }

        /// <summary>
        /// Вводит SCP-096 в ярость.
        /// </summary>
        public static void Enrage096(this Player player, float duration = 30f)
        {
            if (player.Role is Scp096Role role)
            {
                role.Enrage(duration);
            }
        }

        #endregion

        #region SCP-106 - Старик

        /// <summary>
        /// Получает SCP-106.
        /// </summary>
        public static Player Get106()
        {
            return Get(RoleTypeId.Scp106);
        }

        /// <summary>
        /// Получает роль SCP-106.
        /// </summary>
        public static Scp106Role Get106Role(this Player player)
        {
            return player.Role as Scp106Role;
        }

        /// <summary>
        /// Отправляет игрока в карманное измерение.
        /// </summary>
        public static void SendToPocket(Player target)
        {
            target.EnableEffect(EffectType.PocketCorroding);
            target.Position = new Vector3(0, -1997f, 0);
        }

        #endregion

        #region SCP-173 - Скульптура

        /// <summary>
        /// Получает SCP-173.
        /// </summary>
        public static Player Get173()
        {
            return Get(RoleTypeId.Scp173);
        }

        /// <summary>
        /// Получает роль SCP-173.
        /// </summary>
        public static Scp173Role Get173Role(this Player player)
        {
            return player.Role as Scp173Role;
        }

        /// <summary>
        /// Устанавливает время перезарядки мигания.
        /// </summary>
        public static void Set173BlinkCooldown(this Player player, float cooldown)
        {
            if (player.Role is Scp173Role role)
            {
                role.BlinkCooldown = cooldown;
            }
        }

        #endregion

        #region SCP-939 - Собаки

        /// <summary>
        /// Получает всех SCP-939.
        /// </summary>
        public static IEnumerable<Player> GetAll939()
        {
            return Player.List.Where(p => 
                p.Role.Type == RoleTypeId.Scp939);
        }

        /// <summary>
        /// Получает первого SCP-939.
        /// </summary>
        public static Player Get939()
        {
            return GetAll939().FirstOrDefault();
        }

        /// <summary>
        /// Получает роль SCP-939.
        /// </summary>
        public static Scp939Role Get939Role(this Player player)
        {
            return player.Role as Scp939Role;
        }

        /// <summary>
        /// Подсчитывает SCP-939.
        /// </summary>
        public static int Count939()
        {
            return GetAll939().Count();
        }

        #endregion

        #region SCP-3114 - Скелет

        /// <summary>
        /// Получает SCP-3114.
        /// </summary>
        public static Player Get3114()
        {
            return Get(RoleTypeId.Scp3114);
        }

        /// <summary>
        /// Получает роль SCP-3114.
        /// </summary>
        public static Scp3114Role Get3114Role(this Player player)
        {
            return player.Role as Scp3114Role;
        }

        #endregion

        #region Bulk Operations - Массовые Операции

        /// <summary>
        /// Исцеляет всех SCP.
        /// </summary>
        public static void HealAll()
        {
            foreach (var scp in GetAll())
            {
                scp.Health = scp.MaxHealth;
            }
            FermixLog.Action("Все SCP исцелены");
        }

        /// <summary>
        /// Убивает всех SCP.
        /// </summary>
        public static void KillAll()
        {
            foreach (var scp in GetAll().ToList())
            {
                scp.Kill("Убит через FermixAPI");
            }
            FermixLog.Action("Все SCP убиты");
        }

        /// <summary>
        /// Телепортирует всех SCP к позиции.
        /// </summary>
        public static void TeleportAll(Vector3 position)
        {
            foreach (var scp in GetAll())
            {
                scp.Position = position;
            }
        }

        /// <summary>
        /// Телепортирует всех SCP в комнату.
        /// </summary>
        public static void TeleportAllToRoom(RoomType room)
        {
            var targetRoom = Room.Get(room);
            if (targetRoom != null)
            {
                TeleportAll(targetRoom.Position + Vector3.up);
            }
        }

        /// <summary>
        /// Отправляет хинт всем SCP.
        /// </summary>
        public static void HintToAll(string message, float duration = 5f)
        {
            foreach (var scp in GetAll())
            {
                scp.ShowHint(message, duration);
            }
        }

        /// <summary>
        /// Применяет действие ко всем SCP.
        /// </summary>
        public static void ForEach(Action<Player> action)
        {
            foreach (var scp in GetAll())
            {
                action(scp);
            }
        }

        #endregion

        #region SCP Stats - Статистика SCP

        /// <summary>
        /// Получает общее здоровье всех SCP.
        /// </summary>
        public static float GetTotalHealth()
        {
            return GetAll().Sum(scp => scp.Health);
        }

        /// <summary>
        /// Получает среднее здоровье SCP.
        /// </summary>
        public static float GetAverageHealth()
        {
            var scps = GetAll().ToList();
            return scps.Count > 0 ? scps.Average(scp => scp.Health) : 0;
        }

        /// <summary>
        /// Получает самого здорового SCP.
        /// </summary>
        public static Player GetHealthiest()
        {
            return GetAll().OrderByDescending(scp => scp.Health).FirstOrDefault();
        }

        /// <summary>
        /// Получает самого слабого SCP.
        /// </summary>
        public static Player GetWeakest()
        {
            return GetAll().OrderBy(scp => scp.Health).FirstOrDefault();
        }

        /// <summary>
        /// Получает ближайшего SCP к игроку.
        /// </summary>
        public static Player GetNearest(Player player)
        {
            return GetAll()
                .Where(scp => scp != player)
                .OrderBy(scp => Vector3.Distance(scp.Position, player.Position))
                .FirstOrDefault();
        }

        /// <summary>
        /// Получает ближайшего SCP к позиции.
        /// </summary>
        public static Player GetNearest(Vector3 position)
        {
            return GetAll()
                .OrderBy(scp => Vector3.Distance(scp.Position, position))
                .FirstOrDefault();
        }

        #endregion

        #region SCP Spawn Control - Контроль Спавна SCP

        /// <summary>
        /// Заменяет случайного игрока на SCP.
        /// </summary>
        public static Player SpawnRandomAs(RoleTypeId scpRole)
        {
            var humans = Player.List.Where(p => p.IsHuman).ToList();
            if (humans.Count == 0) return null;

            var player = humans[UnityEngine.Random.Range(0, humans.Count)];
            player.Role.Set(scpRole);
            return player;
        }

        /// <summary>
        /// Спавнит дополнительного SCP.
        /// </summary>
        public static Player SpawnAdditional(RoleTypeId scpRole)
        {
            var spectators = Player.List.Where(p => p.Role.Type == RoleTypeId.Spectator).ToList();
            if (spectators.Count == 0) return null;

            var player = spectators[UnityEngine.Random.Range(0, spectators.Count)];
            player.Role.Set(scpRole);
            FermixLog.Action($"Спавнен дополнительный {scpRole}");
            return player;
        }

        /// <summary>
        /// Убивает SCP и возрождает его как другого SCP.
        /// </summary>
        public static void RespawnAs(this Player scp, RoleTypeId newRole)
        {
            if (scp.IsScp)
            {
                scp.Role.Set(newRole);
                scp.Health = scp.MaxHealth;
            }
        }

        #endregion

        #region SCP Role Checks - Проверки Ролей SCP

        /// <summary>
        /// Проверяет, является ли игрок определённым SCP.
        /// </summary>
        public static bool Is(this Player player, RoleTypeId scpRole)
        {
            return player.Role.Type == scpRole;
        }

        /// <summary>
        /// Проверяет, является ли игрок SCP-049.
        /// </summary>
        public static bool Is049(this Player player) => player.Is(RoleTypeId.Scp049);

        /// <summary>
        /// Проверяет, является ли игрок SCP-049-2.
        /// </summary>
        public static bool IsZombie(this Player player) => player.Is(RoleTypeId.Scp0492);

        /// <summary>
        /// Проверяет, является ли игрок SCP-079.
        /// </summary>
        public static bool Is079(this Player player) => player.Is(RoleTypeId.Scp079);

        /// <summary>
        /// Проверяет, является ли игрок SCP-096.
        /// </summary>
        public static bool Is096(this Player player) => player.Is(RoleTypeId.Scp096);

        /// <summary>
        /// Проверяет, является ли игрок SCP-106.
        /// </summary>
        public static bool Is106(this Player player) => player.Is(RoleTypeId.Scp106);

        /// <summary>
        /// Проверяет, является ли игрок SCP-173.
        /// </summary>
        public static bool Is173(this Player player) => player.Is(RoleTypeId.Scp173);

        /// <summary>
        /// Проверяет, является ли игрок SCP-939.
        /// </summary>
        public static bool Is939(this Player player) => player.Role.Type == RoleTypeId.Scp939;

        /// <summary>
        /// Проверяет, является ли игрок SCP-3114.
        /// </summary>
        public static bool Is3114(this Player player) => player.Is(RoleTypeId.Scp3114);

        #endregion
    }
}
