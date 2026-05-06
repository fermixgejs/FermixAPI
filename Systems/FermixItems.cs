using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using FermixAPI.Core;
using UnityEngine;
using Firearm = Exiled.API.Features.Items.Firearm;

namespace FermixAPI.Systems
{
    /// <summary>
    /// Система управления предметами с расширенными возможностями.
    /// </summary>
    public static class FermixItems
    {
        #region Spawn Items - Создание Предметов

        /// <summary>
        /// Спавнит предмет на позиции.
        /// </summary>
        public static Pickup Spawn(ItemType type, Vector3 position, Quaternion rotation = default)
        {
            var pickup = Item.Create(type).CreatePickup(position, rotation);
            FermixLog.Action($"Предмет создан: {type}");
            return pickup;
        }

        /// <summary>
        /// Спавнит предмет в комнате.
        /// </summary>
        public static Pickup SpawnInRoom(ItemType type, Room room)
        {
            return Spawn(type, room.Position + Vector3.up);
        }

        /// <summary>
        /// Спавнит предмет в комнате по типу.
        /// </summary>
        public static Pickup SpawnInRoom(ItemType type, RoomType roomType)
        {
            var room = Room.Get(roomType);
            return room != null ? SpawnInRoom(type, room) : null;
        }

        /// <summary>
        /// Спавнит несколько предметов на позиции.
        /// </summary>
        public static List<Pickup> SpawnMultiple(ItemType type, Vector3 position, int count)
        {
            var pickups = new List<Pickup>();
            for (int i = 0; i < count; i++)
            {
                var offset = new Vector3(
                    UnityEngine.Random.Range(-0.5f, 0.5f),
                    0,
                    UnityEngine.Random.Range(-0.5f, 0.5f)
                );
                pickups.Add(Spawn(type, position + offset));
            }
            return pickups;
        }

        /// <summary>
        /// Спавнит случайный предмет на позиции.
        /// </summary>
        public static Pickup SpawnRandom(Vector3 position, params ItemType[] pool)
        {
            if (pool.Length == 0) return null;
            var type = pool[UnityEngine.Random.Range(0, pool.Length)];
            return Spawn(type, position);
        }

        /// <summary>
        /// Спавнит набор предметов.
        /// </summary>
        public static List<Pickup> SpawnKit(Vector3 position, params ItemType[] items)
        {
            var pickups = new List<Pickup>();
            var offset = 0f;
            
            foreach (var item in items)
            {
                pickups.Add(Spawn(item, position + new Vector3(offset, 0, 0)));
                offset += 0.3f;
            }
            
            return pickups;
        }

        #endregion

        #region Pickup Management - Управление Пикапами

        /// <summary>
        /// Получает все пикапы определённого типа.
        /// </summary>
        public static IEnumerable<Pickup> GetAll(ItemType type)
        {
            return Pickup.List.Where(p => p.Type == type);
        }

        /// <summary>
        /// Получает все пикапы в зоне.
        /// </summary>
        public static IEnumerable<Pickup> GetInZone(ZoneType zone)
        {
            return Pickup.List.Where(p => p.Room?.Zone == zone);
        }

        /// <summary>
        /// Получает все пикапы в радиусе.
        /// </summary>
        public static IEnumerable<Pickup> GetInRange(Vector3 position, float radius)
        {
            return Pickup.List.Where(p => Vector3.Distance(p.Position, position) <= radius);
        }

        /// <summary>
        /// Получает ближайший пикап к позиции.
        /// </summary>
        public static Pickup GetNearest(Vector3 position, ItemType? type = null)
        {
            var pickups = type.HasValue 
                ? Pickup.List.Where(p => p.Type == type.Value)
                : Pickup.List;
            
            return pickups
                .OrderBy(p => Vector3.Distance(p.Position, position))
                .FirstOrDefault();
        }

        /// <summary>
        /// Получает ближайший пикап к игроку.
        /// </summary>
        public static Pickup GetNearest(Player player, ItemType? type = null)
        {
            return GetNearest(player.Position, type);
        }

        /// <summary>
        /// Удаляет все пикапы определённого типа.
        /// </summary>
        public static int RemoveAll(ItemType type)
        {
            var count = 0;
            foreach (var pickup in GetAll(type).ToList())
            {
                pickup.Destroy();
                count++;
            }
            FermixLog.Action($"Удалено {count} предметов типа {type}");
            return count;
        }

        /// <summary>
        /// Удаляет все пикапы в зоне.
        /// </summary>
        public static int RemoveInZone(ZoneType zone)
        {
            var count = 0;
            foreach (var pickup in GetInZone(zone).ToList())
            {
                pickup.Destroy();
                count++;
            }
            FermixLog.Action($"Удалено {count} предметов в зоне {zone}");
            return count;
        }

        /// <summary>
        /// Телепортирует все пикапы к позиции.
        /// </summary>
        public static void TeleportAll(Vector3 position)
        {
            foreach (var pickup in Pickup.List)
            {
                pickup.Position = position + new Vector3(
                    UnityEngine.Random.Range(-1f, 1f),
                    0,
                    UnityEngine.Random.Range(-1f, 1f)
                );
            }
        }

        #endregion

        #region Item Kits - Наборы Предметов

        /// <summary>
        /// Стандартные наборы предметов.
        /// </summary>
        public static class Kits
        {
            public static readonly ItemType[] MTF = {
                ItemType.GunE11SR,
                ItemType.Medkit,
                ItemType.ArmorCombat,
                ItemType.Radio,
                ItemType.GrenadeHE
            };

            public static readonly ItemType[] ChaosInsurgency = {
                ItemType.GunAK,
                ItemType.Adrenaline,
                ItemType.ArmorCombat,
                ItemType.GrenadeHE
            };

            public static readonly ItemType[] Guard = {
                ItemType.GunFSP9,
                ItemType.Medkit,
                ItemType.ArmorLight,
                ItemType.Radio,
                ItemType.KeycardGuard
            };

            public static readonly ItemType[] Scientist = {
                ItemType.Medkit,
                ItemType.KeycardScientist,
                ItemType.Flashlight
            };

            public static readonly ItemType[] DClass = {
                ItemType.Coin
            };

            public static readonly ItemType[] Commander = {
                ItemType.GunE11SR,
                ItemType.Medkit,
                ItemType.ArmorHeavy,
                ItemType.Radio,
                ItemType.GrenadeHE,
                ItemType.KeycardMTFCaptain
            };

            public static readonly ItemType[] Medic = {
                ItemType.GunFSP9,
                ItemType.Medkit,
                ItemType.Medkit,
                ItemType.Adrenaline,
                ItemType.ArmorLight,
                ItemType.Radio
            };

            public static readonly ItemType[] Sniper = {
                ItemType.GunCrossvec,
                ItemType.Medkit,
                ItemType.ArmorLight
            };
        }

        /// <summary>
        /// Выдаёт набор игроку.
        /// </summary>
        public static void GiveKit(Player player, ItemType[] kit, bool clearFirst = false)
        {
            if (clearFirst)
            {
                player.ClearInventory();
            }

            foreach (var item in kit)
            {
                if (player.Items.Count < 8)
                {
                    player.AddItem(item);
                }
            }
        }

        /// <summary>
        /// Выдаёт набор MTF.
        /// </summary>
        public static void GiveMTFKit(Player player, bool clearFirst = true)
        {
            GiveKit(player, Kits.MTF, clearFirst);
        }

        /// <summary>
        /// Выдаёт набор Chaos Insurgency.
        /// </summary>
        public static void GiveChaosKit(Player player, bool clearFirst = true)
        {
            GiveKit(player, Kits.ChaosInsurgency, clearFirst);
        }

        /// <summary>
        /// Выдаёт набор командира.
        /// </summary>
        public static void GiveCommanderKit(Player player, bool clearFirst = true)
        {
            GiveKit(player, Kits.Commander, clearFirst);
        }

        /// <summary>
        /// Выдаёт набор медика.
        /// </summary>
        public static void GiveMedicKit(Player player, bool clearFirst = true)
        {
            GiveKit(player, Kits.Medic, clearFirst);
        }

        #endregion

        #region Weapon Operations - Операции с Оружием

        /// <summary>
        /// Получает все оружия в инвентаре игрока.
        /// </summary>
        public static IEnumerable<Firearm> GetWeapons(this Player player)
        {
            return player.Items.OfType<Firearm>();
        }

        /// <summary>
        /// Перезаряжает всё оружие игрока.
        /// </summary>
        public static void ReloadAllWeapons(this Player player)
        {
            foreach (var weapon in player.GetWeapons())
            {
                weapon.MagazineAmmo = weapon.TotalMaxAmmo;
            }
        }

        /// <summary>
        /// Выдаёт заряженное оружие.
        /// </summary>
        public static Firearm GiveLoadedWeapon(this Player player, ItemType weaponType)
        {
            var item = player.AddItem(weaponType);
            
            if (item is Firearm firearm)
            {
                firearm.MagazineAmmo = firearm.TotalMaxAmmo;
                return firearm;
            }
            
            return null;
        }

        /// <summary>
        /// Опустошает всё оружие игрока.
        /// </summary>
        public static void EmptyAllWeapons(this Player player)
        {
            foreach (var weapon in player.GetWeapons())
            {
                weapon.MagazineAmmo = 0;
            }
        }

        #endregion

        #region Ammo Operations - Операции с Патронами

        /// <summary>
        /// Типы патронов.
        /// </summary>
        public static readonly ItemType[] AmmoTypes = {
            ItemType.Ammo9x19,
            ItemType.Ammo556x45,
            ItemType.Ammo762x39,
            ItemType.Ammo12gauge,
            ItemType.Ammo44cal
        };

        // GiveAllAmmo — каноническая версия в Extensions/PlayerExtensions.cs
        // (она ограничивает 12gauge/44cal до 50, как в реальном инвентаре).

        /// <summary>
        /// Очищает все патроны.
        /// </summary>
        public static void ClearAmmo(this Player player)
        {
            foreach (var ammoType in AmmoTypes)
            {
                player.Ammo[ammoType] = 0;
            }
        }

        /// <summary>
        /// Добавляет патроны определённого типа.
        /// </summary>
        public static void AddAmmo(this Player player, ItemType ammoType, ushort amount)
        {
            if (AmmoTypes.Contains(ammoType))
            {
                player.Ammo[ammoType] = (ushort)Math.Min(player.Ammo[ammoType] + amount, ushort.MaxValue);
            }
        }

        /// <summary>
        /// Устанавливает бесконечные патроны (максимальное значение).
        /// </summary>
        public static void SetInfiniteAmmo(this Player player)
        {
            foreach (var ammoType in AmmoTypes)
            {
                player.Ammo[ammoType] = ushort.MaxValue;
            }
        }

        #endregion

        #region Special Items - Специальные Предметы

        /// <summary>
        /// Проверяет, является ли предмет SCP-объектом.
        /// </summary>
        public static bool IsScp(this ItemType type)
        {
            return type.ToString().StartsWith("SCP");
        }

        /// <summary>
        /// Проверяет, является ли предмет оружием.
        /// </summary>
        public static bool IsWeapon(this ItemType type)
        {
            return type.ToString().StartsWith("Gun") || type == ItemType.MicroHID;
        }

        /// <summary>
        /// Проверяет, является ли предмет медицинским.
        /// </summary>
        public static bool IsMedical(this ItemType type)
        {
            return type == ItemType.Medkit || type == ItemType.Painkillers || type == ItemType.Adrenaline || type == ItemType.SCP500;
        }

        /// <summary>
        /// Проверяет, является ли предмет картой доступа.
        /// </summary>
        public static bool IsKeycard(this ItemType type)
        {
            return type.ToString().StartsWith("Keycard");
        }

        /// <summary>
        /// Проверяет, является ли предмет бронёй.
        /// </summary>
        public static bool IsArmor(this ItemType type)
        {
            return type.ToString().StartsWith("Armor");
        }

        /// <summary>
        /// Проверяет, является ли предмет гранатой.
        /// </summary>
        public static bool IsGrenade(this ItemType type)
        {
            return type.ToString().StartsWith("Grenade") || type == ItemType.SCP018;
        }

        /// <summary>
        /// Получает все SCP-предметы на карте.
        /// </summary>
        public static IEnumerable<Pickup> GetAllScpItems()
        {
            return Pickup.List.Where(p => p.Type.IsScp());
        }

        /// <summary>
        /// Получает все оружия на карте.
        /// </summary>
        public static IEnumerable<Pickup> GetAllWeapons()
        {
            return Pickup.List.Where(p => p.Type.IsWeapon());
        }

        #endregion

        #region Inventory Helpers - Помощники Инвентаря

        // HasItem — каноническая версия в Extensions/PlayerExtensions.cs.

        /// <summary>
        /// Проверяет, есть ли любой из указанных предметов.
        /// </summary>
        public static bool HasAnyItem(this Player player, params ItemType[] types)
        {
            return player.Items.Any(i => types.Contains(i.Type));
        }

        /// <summary>
        /// Проверяет, есть ли все указанные предметы.
        /// </summary>
        public static bool HasAllItems(this Player player, params ItemType[] types)
        {
            return types.All(t => player.HasItem(t));
        }

        /// <summary>
        /// Считает количество предметов типа.
        /// </summary>
        public static int CountItems(this Player player, ItemType type)
        {
            return player.Items.Count(i => i.Type == type);
        }

        /// <summary>
        /// Удаляет первый предмет указанного типа.
        /// </summary>
        public static bool TryRemoveItem(this Player player, ItemType type)
        {
            var item = player.Items.FirstOrDefault(i => i.Type == type);
            if (item != null)
            {
                player.RemoveItem(item);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Заменяет предмет на другой.
        /// </summary>
        public static bool ReplaceItem(this Player player, ItemType oldType, ItemType newType)
        {
            if (player.TryRemoveItem(oldType))
            {
                player.AddItem(newType);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Получает свободные слоты инвентаря.
        /// </summary>
        public static int GetFreeSlots(this Player player)
        {
            return 8 - player.Items.Count;
        }

        /// <summary>
        /// Проверяет, полон ли инвентарь.
        /// </summary>
        public static bool IsInventoryFull(this Player player)
        {
            return player.Items.Count >= 8;
        }

        #endregion
    }
}
