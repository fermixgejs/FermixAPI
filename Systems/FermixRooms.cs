using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using FermixAPI.Core;
using UnityEngine;

namespace FermixAPI.Systems
{
    /// <summary>
    /// Система управления комнатами и зонами.
    /// </summary>
    public static class FermixRooms
    {
        #region Room Queries - Поиск Комнат

        /// <summary>
        /// Получает комнату по типу.
        /// </summary>
        public static Room Get(RoomType type)
        {
            return Room.Get(type);
        }

        /// <summary>
        /// Получает все комнаты в зоне.
        /// </summary>
        public static IEnumerable<Room> GetInZone(ZoneType zone)
        {
            return Room.List.Where(r => r.Zone == zone);
        }

        /// <summary>
        /// Получает случайную комнату.
        /// </summary>
        public static Room GetRandom()
        {
            var rooms = Room.List.ToList();
            return rooms.Count > 0 ? rooms[UnityEngine.Random.Range(0, rooms.Count)] : null;
        }

        /// <summary>
        /// Получает случайную комнату в зоне.
        /// </summary>
        public static Room GetRandom(ZoneType zone)
        {
            var rooms = GetInZone(zone).ToList();
            return rooms.Count > 0 ? rooms[UnityEngine.Random.Range(0, rooms.Count)] : null;
        }

        /// <summary>
        /// Получает ближайшую комнату к позиции.
        /// </summary>
        public static Room GetNearest(Vector3 position)
        {
            return Room.List
                .OrderBy(r => Vector3.Distance(r.Position, position))
                .FirstOrDefault();
        }

        /// <summary>
        /// Получает ближайшую комнату к игроку.
        /// </summary>
        public static Room GetNearest(Player player)
        {
            return GetNearest(player.Position);
        }

        #endregion

        #region Room Operations - Операции с Комнатами

        /// <summary>
        /// Выключает свет в комнате.
        /// </summary>
        public static void TurnOffLights(this Room room, float duration = 10f)
        {
            room.TurnOffLights(duration);
        }

        /// <summary>
        /// Выключает свет в зоне.
        /// </summary>
        public static void TurnOffLightsInZone(ZoneType zone, float duration = 10f)
        {
            foreach (var room in GetInZone(zone))
            {
                room.TurnOffLights(duration);
            }
            FermixLog.Action($"Свет выключен в зоне {zone} на {duration} сек");
        }

        /// <summary>
        /// Выключает свет на всей карте.
        /// </summary>
        public static void TurnOffAllLights(float duration = 10f)
        {
            Map.TurnOffAllLights(duration);
            FermixLog.Action($"Свет выключен везде на {duration} сек");
        }

        /// <summary>
        /// Устанавливает цвет света в комнате.
        /// </summary>
        public static void SetLightColor(this Room room, Color color)
        {
            room.Color = color;
        }

        /// <summary>
        /// Устанавливает цвет света в зоне.
        /// </summary>
        public static void SetLightColorInZone(ZoneType zone, Color color)
        {
            foreach (var room in GetInZone(zone))
            {
                room.Color = color;
            }
        }

        /// <summary>
        /// Сбрасывает цвет света в комнате.
        /// </summary>
        public static void ResetLightColor(this Room room)
        {
            room.ResetColor();
        }

        /// <summary>
        /// Сбрасывает цвет света во всех комнатах.
        /// </summary>
        public static void ResetAllLightColors()
        {
            foreach (var room in Room.List)
            {
                room.ResetColor();
            }
        }

        #endregion

        #region Player in Room - Игроки в Комнатах

        /// <summary>
        /// Получает всех игроков в комнате.
        /// </summary>
        public static IEnumerable<Player> GetPlayers(this Room room)
        {
            return room.Players;
        }

        /// <summary>
        /// Получает количество игроков в комнате.
        /// </summary>
        public static int GetPlayerCount(this Room room)
        {
            return room.Players.Count();
        }

        /// <summary>
        /// Проверяет, есть ли игроки в комнате.
        /// </summary>
        public static bool HasPlayers(this Room room)
        {
            return room.Players.Any();
        }

        /// <summary>
        /// Получает всех игроков в зоне.
        /// </summary>
        public static IEnumerable<Player> GetPlayersInZone(ZoneType zone)
        {
            return Player.List.Where(p => p.CurrentRoom?.Zone == zone);
        }

        /// <summary>
        /// Получает количество игроков в зоне.
        /// </summary>
        public static int GetPlayerCountInZone(ZoneType zone)
        {
            return GetPlayersInZone(zone).Count();
        }

        /// <summary>
        /// Телепортирует всех игроков из комнаты.
        /// </summary>
        public static void EvacuatePlayers(this Room room, Vector3 destination)
        {
            foreach (var player in room.Players.ToList())
            {
                player.Position = destination;
            }
        }

        /// <summary>
        /// Телепортирует всех игроков из зоны.
        /// </summary>
        public static void EvacuateZone(ZoneType zone, Vector3 destination)
        {
            foreach (var player in GetPlayersInZone(zone).ToList())
            {
                player.Position = destination;
            }
            FermixLog.Action($"Зона {zone} эвакуирована");
        }

        #endregion

        #region Zone Utilities - Утилиты Зон

        /// <summary>
        /// Получает центральную позицию зоны.
        /// </summary>
        public static Vector3 GetZoneCenter(ZoneType zone)
        {
            var rooms = GetInZone(zone).ToList();
            if (rooms.Count == 0) return Vector3.zero;

            var center = Vector3.zero;
            foreach (var room in rooms)
            {
                center += room.Position;
            }
            return center / rooms.Count;
        }

        /// <summary>
        /// Проверяет, пуста ли зона (нет игроков).
        /// </summary>
        public static bool IsZoneEmpty(ZoneType zone)
        {
            return !GetPlayersInZone(zone).Any();
        }

        /// <summary>
        /// Получает случайную позицию в зоне.
        /// </summary>
        public static Vector3 GetRandomPosition(ZoneType zone)
        {
            var room = GetRandom(zone);
            return room?.Position + Vector3.up ?? Vector3.zero;
        }

        /// <summary>
        /// Получает все комнаты с игроками.
        /// </summary>
        public static IEnumerable<Room> GetOccupiedRooms()
        {
            return Room.List.Where(r => r.HasPlayers());
        }

        /// <summary>
        /// Получает все пустые комнаты.
        /// </summary>
        public static IEnumerable<Room> GetEmptyRooms()
        {
            return Room.List.Where(r => !r.HasPlayers());
        }

        #endregion

        #region Special Rooms - Специальные Комнаты

        /// <summary>
        /// Получает комнату SCP-914.
        /// </summary>
        public static Room Get914()
        {
            return Room.Get(RoomType.Lcz914);
        }

        /// <summary>
        /// Получает комнату SCP-173.
        /// </summary>
        public static Room Get173()
        {
            return Room.Get(RoomType.Lcz173);
        }

        /// <summary>
        /// Получает комнату SCP-049.
        /// </summary>
        public static Room Get049()
        {
            return Room.Get(RoomType.Hcz049);
        }

        /// <summary>
        /// Получает комнату SCP-079.
        /// </summary>
        public static Room Get079()
        {
            return Room.Get(RoomType.Hcz079);
        }

        /// <summary>
        /// Получает комнату SCP-096.
        /// </summary>
        public static Room Get096()
        {
            return Room.Get(RoomType.Hcz096);
        }

        /// <summary>
        /// Получает комнату SCP-106.
        /// </summary>
        public static Room Get106()
        {
            return Room.Get(RoomType.Hcz106);
        }

        /// <summary>
        /// Получает комнату интеркома.
        /// </summary>
        public static Room GetIntercom()
        {
            return Room.Get(RoomType.EzIntercom);
        }

        /// <summary>
        /// Получает комнату серверной.
        /// </summary>
        public static Room GetServers()
        {
            return Room.Get(RoomType.HczServerRoom);
        }

        /// <summary>
        /// Получает комнату с MicroHID.
        /// </summary>
        public static Room GetMicroHID()
        {
            return Room.Get(RoomType.HczHid);
        }

        /// <summary>
        /// Получает позицию поверхности.
        /// </summary>
        public static Vector3 GetSurfacePosition()
        {
            var room = Room.Get(RoomType.Surface);
            return room?.Position + Vector3.up * 2 ?? new Vector3(0, 1001, 0);
        }

        /// <summary>
        /// Получает позицию спавна класса D.
        /// </summary>
        public static Vector3 GetDClassSpawn()
        {
            var room = Room.Get(RoomType.LczClassDSpawn);
            return room?.Position + Vector3.up ?? Vector3.zero;
        }

        /// <summary>
        /// Получает позицию Gate A.
        /// </summary>
        public static Vector3 GetGateAPosition()
        {
            var room = Room.Get(RoomType.EzGateA);
            return room?.Position + Vector3.up ?? Vector3.zero;
        }

        /// <summary>
        /// Получает позицию Gate B.
        /// </summary>
        public static Vector3 GetGateBPosition()
        {
            var room = Room.Get(RoomType.EzGateB);
            return room?.Position + Vector3.up ?? Vector3.zero;
        }

        #endregion

        #region Room Effects - Эффекты Комнат

        /// <summary>
        /// Мигает светом в комнате.
        /// </summary>
        public static void FlashLights(this Room room, int times = 3, float interval = 0.5f)
        {
            FermixCore.RunCoroutine(FlashCoroutine(room, times, interval));
        }

        private static IEnumerator<float> FlashCoroutine(Room room, int times, float interval)
        {
            for (int i = 0; i < times; i++)
            {
                room.TurnOffLights(interval);
                yield return MEC.Timing.WaitForSeconds(interval * 2);
            }
        }

        /// <summary>
        /// Мигает светом в зоне.
        /// </summary>
        public static void FlashLightsInZone(ZoneType zone, int times = 3, float interval = 0.5f)
        {
            foreach (var room in GetInZone(zone))
            {
                room.FlashLights(times, interval);
            }
        }

        /// <summary>
        /// Создаёт эффект красной тревоги в комнате.
        /// </summary>
        public static void AlertMode(this Room room, float duration = 30f)
        {
            room.SetLightColor(Color.red);
            FermixScheduler.Delay(duration, () => room.ResetLightColor());
        }

        /// <summary>
        /// Создаёт эффект красной тревоги в зоне.
        /// </summary>
        public static void AlertModeInZone(ZoneType zone, float duration = 30f)
        {
            SetLightColorInZone(zone, Color.red);
            FermixScheduler.Delay(duration, () =>
            {
                foreach (var room in GetInZone(zone))
                {
                    room.ResetLightColor();
                }
            });
        }

        #endregion

        #region Room Extensions - Расширения Комнат

        /// <summary>
        /// Проверяет, находится ли позиция в комнате.
        /// </summary>
        public static bool Contains(this Room room, Vector3 position)
        {
            return Vector3.Distance(room.Position, position) < 15f;
        }

        /// <summary>
        /// Получает расстояние между комнатами.
        /// </summary>
        public static float DistanceTo(this Room room, Room other)
        {
            return Vector3.Distance(room.Position, other.Position);
        }

        /// <summary>
        /// Получает все соседние комнаты.
        /// </summary>
        public static IEnumerable<Room> GetNeighbors(this Room room, float maxDistance = 30f)
        {
            return Room.List
                .Where(r => r != room && room.DistanceTo(r) <= maxDistance)
                .OrderBy(r => room.DistanceTo(r));
        }

        #endregion
    }
}
