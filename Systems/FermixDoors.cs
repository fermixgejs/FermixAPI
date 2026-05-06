using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using FermixAPI.Core;
using UnityEngine;

namespace FermixAPI.Systems
{
    /// <summary>
    /// Система управления дверями с расширенными возможностями.
    /// </summary>
    public static class FermixDoors
    {
        #region Single Door Operations - Операции с Одной Дверью

        /// <summary>
        /// Открывает дверь.
        /// </summary>
        public static Door Open(this Door door)
        {
            door.IsOpen = true;
            return door;
        }

        /// <summary>
        /// Закрывает дверь.
        /// </summary>
        public static Door Close(this Door door)
        {
            door.IsOpen = false;
            return door;
        }

        /// <summary>
        /// Переключает состояние двери.
        /// </summary>
        public static Door Toggle(this Door door)
        {
            door.IsOpen = !door.IsOpen;
            return door;
        }

        /// <summary>
        /// Блокирует дверь.
        /// </summary>
        public static Door Lock(this Door door, DoorLockType lockType = DoorLockType.AdminCommand)
        {
            door.ChangeLock(lockType);
            return door;
        }

        /// <summary>
        /// Разблокирует дверь.
        /// </summary>
        public static Door Unlock(this Door door)
        {
            door.ChangeLock(DoorLockType.None);
            return door;
        }

        /// <summary>
        /// Блокирует дверь на время.
        /// </summary>
        public static Door LockFor(this Door door, float seconds, DoorLockType lockType = DoorLockType.AdminCommand)
        {
            door.Lock(lockType);
            FermixScheduler.Delay(seconds, () => door.Unlock());
            return door;
        }

        /// <summary>
        /// Уничтожает дверь (если разрушаемая).
        /// </summary>
        public static Door Destroy(this Door door)
        {
            if (door is BreakableDoor breakable)
            {
                breakable.Break();
            }
            return door;
        }

        /// <summary>
        /// Наносит урон двери.
        /// </summary>
        public static Door Damage(this Door door, float damage)
        {
            if (door is BreakableDoor breakable)
            {
                breakable.Health -= damage;
            }
            return door;
        }

        /// <summary>
        /// Восстанавливает здоровье двери.
        /// </summary>
        public static Door Repair(this Door door)
        {
            if (door is BreakableDoor breakable)
            {
                breakable.Health = breakable.MaxHealth;
            }
            return door;
        }

        /// <summary>
        /// Проверяет, заблокирована ли дверь.
        /// </summary>
        public static bool IsLocked(this Door door)
        {
            return door.DoorLockType != DoorLockType.None;
        }

        /// <summary>
        /// Проверяет, разрушена ли дверь.
        /// </summary>
        public static bool IsDestroyed(this Door door)
        {
            return door is BreakableDoor breakable && breakable.IsDestroyed;
        }

        #endregion

        #region Zone Operations - Операции по Зонам

        /// <summary>
        /// Получает все двери в зоне.
        /// </summary>
        public static IEnumerable<Door> InZone(ZoneType zone)
        {
            return Door.List.Where(d => d.Zone == zone);
        }

        /// <summary>
        /// Блокирует все двери в зоне.
        /// </summary>
        public static void LockZone(ZoneType zone, DoorLockType lockType = DoorLockType.AdminCommand)
        {
            foreach (var door in InZone(zone))
            {
                door.Lock(lockType);
            }
            FermixLog.Action("Зона заблокирована", zone.ToString());
        }

        /// <summary>
        /// Разблокирует все двери в зоне.
        /// </summary>
        public static void UnlockZone(ZoneType zone)
        {
            foreach (var door in InZone(zone))
            {
                door.Unlock();
            }
            FermixLog.Action("Зона разблокирована", zone.ToString());
        }

        /// <summary>
        /// Открывает все двери в зоне.
        /// </summary>
        public static void OpenZone(ZoneType zone)
        {
            foreach (var door in InZone(zone))
            {
                door.Open();
            }
        }

        /// <summary>
        /// Закрывает все двери в зоне.
        /// </summary>
        public static void CloseZone(ZoneType zone)
        {
            foreach (var door in InZone(zone))
            {
                door.Close();
            }
        }

        /// <summary>
        /// Уничтожает все двери в зоне.
        /// </summary>
        public static void DestroyZone(ZoneType zone)
        {
            foreach (var door in InZone(zone))
            {
                door.Destroy();
            }
            FermixLog.Warn($"Все двери в зоне {zone} уничтожены!");
        }

        /// <summary>
        /// Блокирует зону на время.
        /// </summary>
        public static void LockZoneFor(ZoneType zone, float seconds, DoorLockType lockType = DoorLockType.AdminCommand)
        {
            LockZone(zone, lockType);
            FermixScheduler.Delay(seconds, () => UnlockZone(zone));
        }

        #endregion

        #region Global Operations - Глобальные Операции

        /// <summary>
        /// Блокирует все двери на карте.
        /// </summary>
        public static void LockAll(DoorLockType lockType = DoorLockType.AdminCommand)
        {
            foreach (var door in Door.List)
            {
                door.Lock(lockType);
            }
            FermixLog.Action("Все двери заблокированы");
        }

        /// <summary>
        /// Разблокирует все двери на карте.
        /// </summary>
        public static void UnlockAll()
        {
            foreach (var door in Door.List)
            {
                door.Unlock();
            }
            FermixLog.Action("Все двери разблокированы");
        }

        /// <summary>
        /// Открывает все двери на карте.
        /// </summary>
        public static void OpenAll()
        {
            foreach (var door in Door.List)
            {
                door.Open();
            }
        }

        /// <summary>
        /// Закрывает все двери на карте.
        /// </summary>
        public static void CloseAll()
        {
            foreach (var door in Door.List)
            {
                door.Close();
            }
        }

        /// <summary>
        /// Блокирует все двери на время.
        /// </summary>
        public static void LockAllFor(float seconds, DoorLockType lockType = DoorLockType.AdminCommand)
        {
            LockAll(lockType);
            FermixScheduler.Delay(seconds, UnlockAll);
        }

        #endregion

        #region Query Operations - Операции Поиска

        /// <summary>
        /// Находит дверь по имени.
        /// </summary>
        public static Door Find(string name)
        {
            return Door.List.FirstOrDefault(d => 
                d.Name.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                d.Name.Contains(name));
        }

        /// <summary>
        /// Получает дверь по типу.
        /// </summary>
        public static Door Get(DoorType type)
        {
            return Door.Get(type);
        }

        /// <summary>
        /// Получает ближайшую дверь к позиции.
        /// </summary>
        public static Door GetNearest(Vector3 position)
        {
            return Door.List
                .OrderBy(d => Vector3.Distance(d.Position, position))
                .FirstOrDefault();
        }

        /// <summary>
        /// Получает ближайшую дверь к игроку.
        /// </summary>
        public static Door GetNearest(Player player)
        {
            return GetNearest(player.Position);
        }

        /// <summary>
        /// Получает все открытые двери.
        /// </summary>
        public static IEnumerable<Door> GetOpen()
        {
            return Door.List.Where(d => d.IsOpen);
        }

        /// <summary>
        /// Получает все закрытые двери.
        /// </summary>
        public static IEnumerable<Door> GetClosed()
        {
            return Door.List.Where(d => !d.IsOpen);
        }

        /// <summary>
        /// Получает все заблокированные двери.
        /// </summary>
        public static IEnumerable<Door> GetLocked()
        {
            return Door.List.Where(d => d.IsLocked());
        }

        /// <summary>
        /// Получает все разрушенные двери.
        /// </summary>
        public static IEnumerable<Door> GetDestroyed()
        {
            return Door.List.Where(d => d.IsDestroyed());
        }

        /// <summary>
        /// Получает двери в радиусе от позиции.
        /// </summary>
        public static IEnumerable<Door> GetInRange(Vector3 position, float radius)
        {
            return Door.List.Where(d => Vector3.Distance(d.Position, position) <= radius);
        }

        #endregion

        #region Special Doors - Специальные Двери

        /// <summary>
        /// Управляет дверью SCP-914.
        /// </summary>
        public static void Control914Door(bool open)
        {
            var door914 = Door.Get(DoorType.Scp914Door);
            if (door914 != null)
            {
                door914.IsOpen = open;
            }
        }

        /// <summary>
        /// Управляет входной дверью Gate A.
        /// </summary>
        public static void ControlGateA(bool open, bool locked = false)
        {
            var gate = Door.Get(DoorType.GateA);
            if (gate != null)
            {
                gate.IsOpen = open;
                if (locked) gate.Lock();
            }
        }

        /// <summary>
        /// Управляет входной дверью Gate B.
        /// </summary>
        public static void ControlGateB(bool open, bool locked = false)
        {
            var gate = Door.Get(DoorType.GateB);
            if (gate != null)
            {
                gate.IsOpen = open;
                if (locked) gate.Lock();
            }
        }

        /// <summary>
        /// Блокирует выходы на поверхность.
        /// </summary>
        public static void LockSurfaceGates()
        {
            ControlGateA(false, true);
            ControlGateB(false, true);
            FermixLog.Action("Выходы на поверхность заблокированы");
        }

        /// <summary>
        /// Разблокирует выходы на поверхность.
        /// </summary>
        public static void UnlockSurfaceGates()
        {
            var gateA = Door.Get(DoorType.GateA);
            var gateB = Door.Get(DoorType.GateB);
            gateA?.Unlock();
            gateB?.Unlock();
            FermixLog.Action("Выходы на поверхность разблокированы");
        }

        /// <summary>
        /// Управляет чекпоинтами.
        /// </summary>
        public static void ControlCheckpoints(bool open, bool locked = false)
        {
            foreach (var door in Door.List.Where(d => 
                d.Type.ToString().Contains("Checkpoint")))
            {
                door.IsOpen = open;
                if (locked) door.Lock();
                else door.Unlock();
            }
        }

        #endregion

        #region Bulk Operations - Массовые Операции

        /// <summary>
        /// Применяет действие ко всем дверям.
        /// </summary>
        public static void ForEach(Action<Door> action)
        {
            foreach (var door in Door.List)
            {
                action(door);
            }
        }

        /// <summary>
        /// Применяет действие к дверям в зоне.
        /// </summary>
        public static void ForEachInZone(ZoneType zone, Action<Door> action)
        {
            foreach (var door in InZone(zone))
            {
                action(door);
            }
        }

        /// <summary>
        /// Применяет действие к дверям по условию.
        /// </summary>
        public static void ForEachWhere(Func<Door, bool> predicate, Action<Door> action)
        {
            foreach (var door in Door.List.Where(predicate))
            {
                action(door);
            }
        }

        #endregion
    }
}
