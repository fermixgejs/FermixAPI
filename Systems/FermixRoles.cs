using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using FermixAPI.Core;
using PlayerRoles;
using UnityEngine;

namespace FermixAPI.Systems
{
    /// <summary>
    /// Расширенная система управления ролями игроков
    /// </summary>
    public static class FermixRoles
    {
        #region Получение игроков по ролям
        
        /// <summary>
        /// Получить всех игроков с указанной ролью
        /// </summary>
        public static IEnumerable<Player> GetPlayers(RoleTypeId role)
            => Player.List.Where(p => p.Role.Type == role);
        
        /// <summary>
        /// Получить всех игроков с указанными ролями
        /// </summary>
        public static IEnumerable<Player> GetPlayers(params RoleTypeId[] roles)
            => Player.List.Where(p => roles.Contains(p.Role.Type));
        
        /// <summary>
        /// Получить всех игроков команды
        /// </summary>
        public static IEnumerable<Player> GetPlayers(Team team)
            => Player.List.Where(p => p.Role.Team == team);
        
        /// <summary>
        /// Получить всех игроков стороны
        /// </summary>
        public static IEnumerable<Player> GetPlayers(Side side)
            => Player.List.Where(p => p.Role.Side == side);
        
        /// <summary>
        /// Получить всех SCP
        /// </summary>
        public static IEnumerable<Player> GetAllScp()
            => Player.List.Where(p => p.Role.Team == Team.SCPs);
        
        /// <summary>
        /// Получить всех людей (живых, не SCP)
        /// </summary>
        public static IEnumerable<Player> GetAllHumans()
            => Player.List.Where(p => p.IsHuman && p.IsAlive);
        
        /// <summary>
        /// Получить всех MTF
        /// </summary>
        public static IEnumerable<Player> GetAllMtf()
            => Player.List.Where(p => p.Role.Team == Team.FoundationForces);
        
        /// <summary>
        /// Получить всех Хаосов
        /// </summary>
        public static IEnumerable<Player> GetAllChaos()
            => Player.List.Where(p => p.Role.Team == Team.ChaosInsurgency);
        
        /// <summary>
        /// Получить всех Class-D
        /// </summary>
        public static IEnumerable<Player> GetAllClassD()
            => Player.List.Where(p => p.Role.Type == RoleTypeId.ClassD);
        
        /// <summary>
        /// Получить всех Ученых
        /// </summary>
        public static IEnumerable<Player> GetAllScientists()
            => Player.List.Where(p => p.Role.Type == RoleTypeId.Scientist);
        
        /// <summary>
        /// Получить всех наблюдателей
        /// </summary>
        public static IEnumerable<Player> GetAllSpectators()
            => Player.List.Where(p => p.Role.Type == RoleTypeId.Spectator);
        
        /// <summary>
        /// Получить количество игроков по роли
        /// </summary>
        public static int CountPlayers(RoleTypeId role)
            => Player.List.Count(p => p.Role.Type == role);
        
        /// <summary>
        /// Получить количество игроков по команде
        /// </summary>
        public static int CountPlayers(Team team)
            => Player.List.Count(p => p.Role.Team == team);
        
        #endregion
        
        #region Массовые операции
        
        /// <summary>
        /// Установить роль всем игрокам с указанной ролью
        /// </summary>
        public static int SetRoleToAll(RoleTypeId currentRole, RoleTypeId newRole, RoleSpawnFlags flags = RoleSpawnFlags.All)
        {
            int count = 0;
            foreach (var player in GetPlayers(currentRole).ToList())
            {
                player.Role.Set(newRole, flags);
                count++;
            }
            return count;
        }
        
        /// <summary>
        /// Установить роль всем игрокам команды
        /// </summary>
        public static int SetRoleToTeam(Team team, RoleTypeId newRole, RoleSpawnFlags flags = RoleSpawnFlags.All)
        {
            int count = 0;
            foreach (var player in GetPlayers(team).ToList())
            {
                player.Role.Set(newRole, flags);
                count++;
            }
            return count;
        }
        
        /// <summary>
        /// Убить всех игроков с указанной ролью
        /// </summary>
        public static int KillAll(RoleTypeId role, string reason = "")
        {
            int count = 0;
            foreach (var player in GetPlayers(role).ToList())
            {
                player.Kill(reason);
                count++;
            }
            return count;
        }
        
        /// <summary>
        /// Убить всех игроков команды
        /// </summary>
        public static int KillTeam(Team team, string reason = "")
        {
            int count = 0;
            foreach (var player in GetPlayers(team).ToList())
            {
                player.Kill(reason);
                count++;
            }
            return count;
        }
        
        /// <summary>
        /// Телепортировать всех игроков с ролью
        /// </summary>
        public static int TeleportAll(RoleTypeId role, Vector3 position)
        {
            int count = 0;
            foreach (var player in GetPlayers(role).ToList())
            {
                player.Position = position;
                count++;
            }
            return count;
        }
        
        /// <summary>
        /// Телепортировать всю команду
        /// </summary>
        public static int TeleportTeam(Team team, Vector3 position)
        {
            int count = 0;
            foreach (var player in GetPlayers(team).ToList())
            {
                player.Position = position;
                count++;
            }
            return count;
        }
        
        /// <summary>
        /// Выдать предмет всем игрокам с ролью
        /// </summary>
        public static int GiveItemToAll(RoleTypeId role, ItemType item)
        {
            int count = 0;
            foreach (var player in GetPlayers(role).ToList())
            {
                player.AddItem(item);
                count++;
            }
            return count;
        }
        
        /// <summary>
        /// Выдать предмет всей команде
        /// </summary>
        public static int GiveItemToTeam(Team team, ItemType item)
        {
            int count = 0;
            foreach (var player in GetPlayers(team).ToList())
            {
                player.AddItem(item);
                count++;
            }
            return count;
        }
        
        #endregion
        
        #region Билдер ролей
        
        /// <summary>
        /// Создать билдер для настройки роли
        /// </summary>
        public static RoleBuilder CreateBuilder(Player player) => new(player);
        
        /// <summary>
        /// Билдер для настройки роли игрока
        /// </summary>
        public class RoleBuilder
        {
            private readonly Player _player;
            private RoleTypeId _role = RoleTypeId.None;
            private RoleSpawnFlags _flags = RoleSpawnFlags.All;
            private Vector3? _position;
            private int? _health;
            private int? _maxHealth;
            private readonly List<ItemType> _items = new();
            private readonly List<Action<Player>> _postActions = new();
            private float _scale = 1f;
            private string _customInfo;
            private string _broadcastMessage;
            private ushort _broadcastDuration;
            
            public RoleBuilder(Player player)
            {
                _player = player;
            }
            
            /// <summary>
            /// Установить роль
            /// </summary>
            public RoleBuilder SetRole(RoleTypeId role, RoleSpawnFlags flags = RoleSpawnFlags.All)
            {
                _role = role;
                _flags = flags;
                return this;
            }
            
            /// <summary>
            /// Установить позицию спавна
            /// </summary>
            public RoleBuilder AtPosition(Vector3 position)
            {
                _position = position;
                return this;
            }
            
            /// <summary>
            /// Установить здоровье
            /// </summary>
            public RoleBuilder WithHealth(int health)
            {
                _health = health;
                return this;
            }
            
            /// <summary>
            /// Установить максимальное здоровье
            /// </summary>
            public RoleBuilder WithMaxHealth(int maxHealth)
            {
                _maxHealth = maxHealth;
                return this;
            }
            
            /// <summary>
            /// Добавить предмет
            /// </summary>
            public RoleBuilder WithItem(ItemType item)
            {
                _items.Add(item);
                return this;
            }
            
            /// <summary>
            /// Добавить предметы
            /// </summary>
            public RoleBuilder WithItems(params ItemType[] items)
            {
                _items.AddRange(items);
                return this;
            }
            
            /// <summary>
            /// Установить масштаб
            /// </summary>
            public RoleBuilder WithScale(float scale)
            {
                _scale = scale;
                return this;
            }
            
            /// <summary>
            /// Установить кастомную информацию
            /// </summary>
            public RoleBuilder WithCustomInfo(string info)
            {
                _customInfo = info;
                return this;
            }
            
            /// <summary>
            /// Показать сообщение после смены роли
            /// </summary>
            public RoleBuilder WithBroadcast(string message, ushort duration = 5)
            {
                _broadcastMessage = message;
                _broadcastDuration = duration;
                return this;
            }
            
            /// <summary>
            /// Добавить действие после смены роли
            /// </summary>
            public RoleBuilder Then(Action<Player> action)
            {
                _postActions.Add(action);
                return this;
            }
            
            /// <summary>
            /// Применить настройки
            /// </summary>
            public Player Apply()
            {
                if (_role != RoleTypeId.None)
                    _player.Role.Set(_role, _flags);
                
                FermixScheduler.Delay(0.5f, () =>
                {
                    if (_position.HasValue)
                        _player.Position = _position.Value;
                    
                    if (_maxHealth.HasValue)
                        _player.MaxHealth = _maxHealth.Value;
                    
                    if (_health.HasValue)
                        _player.Health = _health.Value;
                    
                    foreach (var item in _items)
                        _player.AddItem(item);
                    
                    if (Math.Abs(_scale - 1f) > 0.01f)
                        _player.Scale = new Vector3(_scale, _scale, _scale);
                    
                    if (!string.IsNullOrEmpty(_customInfo))
                        _player.CustomInfo = _customInfo;
                    
                    if (!string.IsNullOrEmpty(_broadcastMessage))
                        _player.Broadcast(_broadcastDuration, _broadcastMessage);
                    
                    foreach (var action in _postActions)
                        action?.Invoke(_player);
                });
                
                return _player;
            }
        }

        #endregion

        #region Проверки ролей
        /// <summary>
        /// Проверить, является ли роль Tutorial
        /// </summary>
        public static bool IsTutorial(RoleTypeId role)
            => role == RoleTypeId.Tutorial;
        
        /// <summary>
        /// Являются ли две роли союзниками
        /// </summary>
        public static bool AreAllies(RoleTypeId role1, RoleTypeId role2)
        {
            var side1 = role1.GetSide();
            var side2 = role2.GetSide();
            return side1 == side2;
        }
        
        /// <summary>
        /// Являются ли две роли врагами
        /// </summary>
        public static bool AreEnemies(RoleTypeId role1, RoleTypeId role2)
            => !AreAllies(role1, role2);
        
        #endregion
        
        #region Случайные роли
        
        private static readonly System.Random Random = new();
        
        /// <summary>
        /// Получить случайную SCP роль
        /// </summary>
        public static RoleTypeId GetRandomScp()
        {
            var scpRoles = new[]
            {
                RoleTypeId.Scp173,
                RoleTypeId.Scp106,
                RoleTypeId.Scp049,
                RoleTypeId.Scp096,
                RoleTypeId.Scp939
            };
            return scpRoles[Random.Next(scpRoles.Length)];
        }
        
        /// <summary>
        /// Получить случайную человеческую роль
        /// </summary>
        public static RoleTypeId GetRandomHuman()
        {
            var humanRoles = new[]
            {
                RoleTypeId.ClassD,
                RoleTypeId.Scientist,
                RoleTypeId.FacilityGuard,
                RoleTypeId.NtfPrivate,
                RoleTypeId.NtfSergeant,
                RoleTypeId.NtfSpecialist,
                RoleTypeId.NtfCaptain,
                RoleTypeId.ChaosConscript,
                RoleTypeId.ChaosRifleman,
                RoleTypeId.ChaosRepressor,
                RoleTypeId.ChaosMarauder
            };
            return humanRoles[Random.Next(humanRoles.Length)];
        }
        
        /// <summary>
        /// Получить случайную MTF роль
        /// </summary>
        public static RoleTypeId GetRandomMtf()
        {
            var mtfRoles = new[]
            {
                RoleTypeId.NtfPrivate,
                RoleTypeId.NtfSergeant,
                RoleTypeId.NtfSpecialist,
                RoleTypeId.NtfCaptain
            };
            return mtfRoles[Random.Next(mtfRoles.Length)];
        }
        
        /// <summary>
        /// Получить случайную Chaos роль
        /// </summary>
        public static RoleTypeId GetRandomChaos()
        {
            var chaosRoles = new[]
            {
                RoleTypeId.ChaosConscript,
                RoleTypeId.ChaosRifleman,
                RoleTypeId.ChaosRepressor,
                RoleTypeId.ChaosMarauder
            };
            return chaosRoles[Random.Next(chaosRoles.Length)];
        }
        
        /// <summary>
        /// Получить случайную роль из списка
        /// </summary>
        public static RoleTypeId GetRandom(params RoleTypeId[] roles)
            => roles[Random.Next(roles.Length)];
        
        /// <summary>
        /// Получить случайного игрока с ролью
        /// </summary>
        public static Player GetRandomPlayer(RoleTypeId role)
        {
            var players = GetPlayers(role).ToList();
            return players.Count > 0 ? players[Random.Next(players.Count)] : null;
        }
        
        /// <summary>
        /// Получить случайного игрока команды
        /// </summary>
        public static Player GetRandomPlayer(Team team)
        {
            var players = GetPlayers(team).ToList();
            return players.Count > 0 ? players[Random.Next(players.Count)] : null;
        }
        
        #endregion
        
        #region Статистика
        
        /// <summary>
        /// Получить статистику по ролям
        /// </summary>
        public static Dictionary<RoleTypeId, int> GetRoleStatistics()
        {
            var stats = new Dictionary<RoleTypeId, int>();
            foreach (var player in Player.List)
            {
                var role = player.Role.Type;
                if (!stats.ContainsKey(role))
                    stats[role] = 0;
                stats[role]++;
            }
            return stats;
        }
        
        /// <summary>
        /// Получить статистику по командам
        /// </summary>
        public static Dictionary<Team, int> GetTeamStatistics()
        {
            var stats = new Dictionary<Team, int>();
            foreach (var player in Player.List)
            {
                var team = player.Role.Team;
                if (!stats.ContainsKey(team))
                    stats[team] = 0;
                stats[team]++;
            }
            return stats;
        }
        
        /// <summary>
        /// Проверить баланс команд
        /// </summary>
        public static (int humans, int scps, float ratio) GetTeamBalance()
        {
            int humans = GetAllHumans().Count();
            int scps = GetAllScp().Count();
            float ratio = scps > 0 ? (float)humans / scps : humans;
            return (humans, scps, ratio);
        }
        
        #endregion
    }
}
