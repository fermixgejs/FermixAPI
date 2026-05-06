using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using FermixAPI.Core;
using PlayerRoles;
using UnityEngine;
using BroadcastFlags = Exiled.API.Features.Broadcast;
// Alias to avoid ambiguity with extension method name


namespace FermixAPI
{
    /// <summary>
    /// Расширенные методы для работы с игроками.
    /// Обеспечивает быстрый и удобный доступ к часто используемым операциям.
    /// </summary>
    public static class PlayerExtensions
    {
        #region Health & Damage - Здоровье и Урон

        /// <summary>
        /// Полностью исцеляет игрока.
        /// </summary>
        public static Player FullHeal(this Player player)
        {
            player.Health = player.MaxHealth;
            player.ArtificialHealth = 0;
            return player;
        }

        /// <summary>
        /// Устанавливает здоровье игрока (с проверкой границ).
        /// </summary>
        public static Player SetHealth(this Player player, float health)
        {
            player.Health = Mathf.Clamp(health, 0, player.MaxHealth);
            return player;
        }

        /// <summary>
        /// Добавляет здоровье игроку.
        /// </summary>
        public static Player AddHealth(this Player player, float amount)
        {
            player.Health = Mathf.Min(player.Health + amount, player.MaxHealth);
            return player;
        }

        /// <summary>
        /// Добавляет AHP (искусственное здоровье).
        /// </summary>
        public static Player AddAHP(this Player player, float amount, float limit = 75f, float decay = 1.2f, float efficacy = 0.7f, float sustain = 0f, bool persistant = false)
        {
            player.AddAhp(amount, limit, decay, efficacy, sustain, persistant);
            return player;
        }

        /// <summary>
        /// Устанавливает максимальное здоровье.
        /// </summary>
        public static Player SetMaxHealth(this Player player, float maxHealth)
        {
            player.MaxHealth = maxHealth;
            return player;
        }

        /// <summary>
        /// Наносит урон игроку.
        /// </summary>
        public static Player Damage(this Player player, float damage, string reason = "FermixAPI")
        {
            player.Hurt(damage, reason);
            return player;
        }

        /// <summary>
        /// Убивает игрока с указанной причиной.
        /// </summary>
        public static Player Kill(this Player player, string reason = "Убит через FermixAPI")
        {
            player.Kill(reason);
            return player;
        }

        /// <summary>
        /// Проверяет, жив ли игрок.
        /// </summary>
        public static bool IsAlive(this Player player)
        {
            return player.Role.Type != RoleTypeId.Spectator &&
                   player.Role.Type != RoleTypeId.None &&
                   player.Role.Type != RoleTypeId.Overwatch;
        }

        /// <summary>
        /// Проверяет, мёртв ли игрок (наблюдатель).
        /// </summary>
        public static bool IsDead(this Player player)
        {
            return player.Role.Type == RoleTypeId.Spectator;
        }

        #endregion

        #region Role Management - Управление Ролями

        /// <summary>
        /// Быстрая смена роли с опциями.
        /// </summary>
        public static Player SetRole(this Player player, RoleTypeId role, RoleSpawnFlags flags = RoleSpawnFlags.All)
        {
            player.Role.Set(role, flags);
            FermixLog.Action($"Роль изменена", $"{player.Nickname} -> {role}");
            return player;
        }

        /// <summary>
        /// Смена роли без телепортации.
        /// </summary>
        public static Player SetRoleNoSpawn(this Player player, RoleTypeId role)
        {
            player.Role.Set(role, RoleSpawnFlags.None);
            return player;
        }

        /// <summary>
        /// Смена роли с сохранением инвентаря.
        /// </summary>
        public static Player SetRoleKeepInventory(this Player player, RoleTypeId role)
        {
            var items = player.Items.Select(i => i.Type).ToList();
            var ammo = new Dictionary<ItemType, ushort>(player.Ammo);

            player.Role.Set(role, RoleSpawnFlags.None);

            foreach (var item in items)
            {
                player.AddItem(item);
            }

            foreach (var kvp in ammo)
            {
                player.Ammo[kvp.Key] = kvp.Value;
            }

            return player;
        }

        /// <summary>
        /// Проверяет, является ли игрок SCP.
        /// </summary>
        public static bool IsScp(this Player player)
        {
            return player.Role.Side == Side.Scp;
        }

        /// <summary>
        /// Проверяет, является ли игрок человеком.
        /// </summary>
        public static bool IsHuman(this Player player)
        {
            return player.IsHuman;
        }

        /// <summary>
        /// Проверяет сторону игрока.
        /// </summary>
        public static bool IsSide(this Player player, Side side)
        {
            return player.Role.Side == side;
        }

        /// <summary>
        /// Проверяет команду игрока.
        /// </summary>
        public static bool IsTeam(this Player player, Team team)
        {
            return player.Role.Team == team;
        }

        /// <summary>
        /// Проверяет фракцию игрока.
        /// </summary>
        public static bool IsFaction(this Player player, Faction faction)
        {
            return player.Role.Type.GetFaction() == faction;
        }

        #endregion

        #region Movement & Position - Движение и Позиция

        /// <summary>
        /// Телепортирует игрока к позиции.
        /// </summary>
        public static Player TeleportTo(this Player player, Vector3 position)
        {
            player.Position = position;
            return player;
        }

        /// <summary>
        /// Телепортирует игрока к другому игроку.
        /// </summary>
        public static Player TeleportTo(this Player player, Player target)
        {
            player.Position = target.Position;
            return player;
        }

        /// <summary>
        /// Телепортирует игрока в комнату.
        /// </summary>
        public static Player TeleportTo(this Player player, Room room)
        {
            player.Position = room.Position + Vector3.up;
            return player;
        }

        /// <summary>
        /// Телепортирует игрока в комнату по типу.
        /// </summary>
        public static Player TeleportTo(this Player player, RoomType roomType)
        {
            var room = Room.Get(roomType);
            if (room != null)
            {
                player.Position = room.Position + Vector3.up;
            }
            return player;
        }

        /// <summary>
        /// Телепортирует всех игроков к позиции.
        /// </summary>
        public static void TeleportAll(Vector3 position)
        {
            foreach (var player in Player.List.Where(p => p.IsAlive()))
            {
                player.Position = position;
            }
        }

        /// <summary>
        /// Замораживает игрока на месте.
        /// </summary>
        public static Player Freeze(this Player player, float duration = -1f)
        {
            player.EnableEffect(EffectType.Ensnared);

            if (duration > 0)
            {
                FermixScheduler.Delay(duration, () => player.Unfreeze());
            }

            return player;
        }

        /// <summary>
        /// Размораживает игрока.
        /// </summary>
        public static Player Unfreeze(this Player player)
        {
            player.DisableEffect(EffectType.Ensnared);
            return player;
        }

        /// <summary>
        /// Устанавливает скорость передвижения.
        /// </summary>
        public static Player SetSpeed(this Player player, byte intensity)
        {
            player.ChangeEffectIntensity(EffectType.MovementBoost, intensity);
            return player;
        }

        /// <summary>
        /// Делает игрока невидимым.
        /// </summary>
        public static Player SetInvisible(this Player player, bool invisible = true, float duration = -1f)
        {
            if (invisible)
            {
                player.EnableEffect(EffectType.Invisible);

                if (duration > 0)
                {
                    FermixScheduler.Delay(duration, () => player.DisableEffect(EffectType.Invisible));
                }
            }
            else
            {
                player.DisableEffect(EffectType.Invisible);
            }

            return player;
        }

        /// <summary>
        /// Расстояние до другого игрока.
        /// </summary>
        public static float DistanceTo(this Player player, Player other)
        {
            return Vector3.Distance(player.Position, other.Position);
        }

        /// <summary>
        /// Расстояние до позиции.
        /// </summary>
        public static float DistanceTo(this Player player, Vector3 position)
        {
            return Vector3.Distance(player.Position, position);
        }

        /// <summary>
        /// Проверяет, находится ли игрок в радиусе от позиции.
        /// </summary>
        public static bool IsInRange(this Player player, Vector3 position, float radius)
        {
            return player.DistanceTo(position) <= radius;
        }

        /// <summary>
        /// Проверяет, находится ли игрок в радиусе от другого игрока.
        /// </summary>
        public static bool IsInRange(this Player player, Player other, float radius)
        {
            return player.DistanceTo(other) <= radius;
        }

        #endregion

        #region Effects - Эффекты

        /// <summary>
        /// Включает эффект на время.
        /// </summary>
        public static Player ApplyEffect(this Player player, EffectType effect, float duration = 0f, byte intensity = 1)
        {
            player.EnableEffect(effect, intensity, duration);
            return player;
        }

        /// <summary>
        /// Отключает эффект.
        /// </summary>
        public static Player RemoveEffect(this Player player, EffectType effect)
        {
            player.DisableEffect(effect);
            return player;
        }

        /// <summary>
        /// Очищает все эффекты.
        /// </summary>
        public static Player ClearEffects(this Player player)
        {
            player.DisableAllEffects();
            return player;
        }

        /// <summary>
        /// Ослепляет игрока.
        /// </summary>
        public static Player Blind(this Player player, float duration = 3f, byte intensity = 1)
        {
            player.EnableEffect(EffectType.Blinded, intensity, duration);
            return player;
        }

        /// <summary>
        /// Применяет кровотечение.
        /// </summary>
        public static Player Bleed(this Player player, float duration = 5f, byte intensity = 1)
        {
            player.EnableEffect(EffectType.Bleeding, intensity, duration);
            return player;
        }

        /// <summary>
        /// Отравляет игрока.
        /// </summary>
        public static Player Poison(this Player player, float duration = 5f, byte intensity = 1)
        {
            player.EnableEffect(EffectType.Poisoned, intensity, duration);
            return player;
        }

        /// <summary>
        /// Применяет эффект горения.
        /// </summary>
        public static Player Burn(this Player player, float duration = 3f, byte intensity = 1)
        {
            player.EnableEffect(EffectType.Burned, intensity, duration);
            return player;
        }

        /// <summary>
        /// Оглушает игрока.
        /// </summary>
        public static Player Stun(this Player player, float duration = 2f)
        {
            player.EnableEffect(EffectType.Flashed, 1, duration);
            return player;
        }

        /// <summary>
        /// Делает игрока невидимым на время.
        /// </summary>
        public static Player Cloak(this Player player, float duration)
        {
            player.EnableEffect(EffectType.Invisible, 1, duration);
            return player;
        }

        #endregion

        #region Hints & Messages - Хинты и Сообщения

        /// <summary>
        /// Показывает хинт игроку.
        /// </summary>
        public static Player Hint(this Player player, string message, float duration = 5f)
        {
            player.ShowHint(message, duration);
            return player;
        }

        /// <summary>
        /// Показывает цветной хинт.
        /// </summary>
        public static Player ColorHint(this Player player, string message, string color = "white", float duration = 5f)
        {
            player.ShowHint($"<color={color}>{message}</color>", duration);
            return player;
        }

        /// <summary>
        /// Показывает хинт успеха (зелёный).
        /// </summary>
        public static Player SuccessHint(this Player player, string message, float duration = 3f)
        {
            return player.ColorHint(message, "green", duration);
        }

        /// <summary>
        /// Показывает хинт ошибки (красный).
        /// </summary>
        public static Player ErrorHint(this Player player, string message, float duration = 3f)
        {
            return player.ColorHint($"[!] {message}", "red", duration);
        }

        /// <summary>
        /// Показывает хинт предупреждения (жёлтый).
        /// </summary>
        public static Player WarningHint(this Player player, string message, float duration = 3f)
        {
            return player.ColorHint($"[!] {message}", "yellow", duration);
        }

        /// <summary>
        /// Показывает хинт информации (голубой).
        /// </summary>
        public static Player InfoHint(this Player player, string message, float duration = 3f)
        {
            return player.ColorHint(message, "cyan", duration);
        }

        /// <summary>
        /// Отправляет сообщение в консоль игрока.
        /// </summary>
        public static Player Console(this Player player, string message, string color = "white")
        {
            player.SendConsoleMessage(message, color);
            return player;
        }

        /// <summary>
        /// Очищает все broadcast сообщения.
        /// </summary>
        public static Player ClearBroadcasts(this Player player)
        {
            player.ClearBroadcasts();
            return player;
        }

        #endregion

        #region Inventory - Инвентарь

        /// <summary>
        /// Выдаёт предмет игроку.
        /// </summary>
        public static Player Give(this Player player, ItemType item)
        {
            player.AddItem(item);
            return player;
        }

        /// <summary>
        /// Выдаёт несколько предметов.
        /// </summary>
        public static Player Give(this Player player, params ItemType[] items)
        {
            foreach (var item in items)
            {
                player.AddItem(item);
            }
            return player;
        }

        /// <summary>
        /// Полностью очищает инвентарь.
        /// </summary>
        public static Player ClearInventory(this Player player)
        {
            player.ClearItems();
            player.Ammo.Clear();
            return player;
        }

        /// <summary>
        /// Выдаёт полный набор патронов.
        /// </summary>
        public static Player GiveAllAmmo(this Player player, ushort amount = 200)
        {
            player.Ammo[ItemType.Ammo9x19] = amount;
            player.Ammo[ItemType.Ammo556x45] = amount;
            player.Ammo[ItemType.Ammo762x39] = amount;
            player.Ammo[ItemType.Ammo12gauge] = (ushort)Math.Min((int)amount, 50);
            player.Ammo[ItemType.Ammo44cal] = (ushort)Math.Min((int)amount, 50);
            return player;
        }

        /// <summary>
        /// Проверяет наличие предмета в инвентаре.
        /// </summary>
        public static bool HasItem(this Player player, ItemType item)
        {
            return player.Items.Any(i => i.Type == item);
        }

        /// <summary>
        /// Считает количество предметов указанного типа.
        /// </summary>
        public static int CountItem(this Player player, ItemType item)
        {
            return player.Items.Count(i => i.Type == item);
        }

        /// <summary>
        /// Удаляет предмет из инвентаря.
        /// </summary>
        public static Player RemoveItem(this Player player, ItemType item)
        {
            var toRemove = player.Items.FirstOrDefault(i => i.Type == item);
            if (toRemove != null)
            {
                player.RemoveItem(toRemove);
            }
            return player;
        }

        /// <summary>
        /// Выбрасывает все предметы.
        /// </summary>
        public static Player DropAll(this Player player)
        {
            player.DropItems();
            return player;
        }

        /// <summary>
        /// Заполняет инвентарь указанным предметом.
        /// </summary>
        public static Player FillInventory(this Player player, ItemType item)
        {
            while (player.Items.Count < 8)
            {
                player.AddItem(item);
            }
            return player;
        }

        #endregion

        #region Status - Статус

        /// <summary>
        /// Проверяет, является ли игрок администратором.
        /// </summary>
        public static bool IsAdmin(this Player player)
        {
            return player.RemoteAdminAccess;
        }

        /// <summary>
        /// Проверяет, находится ли игрок в overwatch.
        /// </summary>
        public static bool IsOverwatch(this Player player)
        {
            return player.Role.Type == RoleTypeId.Overwatch;
        }

        /// <summary>
        /// Проверяет, в наручниках ли игрок.
        /// </summary>
        public static bool IsCuffed(this Player player)
        {
            return player.IsCuffed;
        }

        /// <summary>
        /// Надевает наручники на игрока.
        /// </summary>
        public static Player Cuff(this Player player, Player cuffer = null)
        {
            if (cuffer != null)
                player.Handcuff(cuffer);
            else
                player.Handcuff();
            return player;
        }

        /// <summary>
        /// Снимает наручники.
        /// </summary>
        public static Player Uncuff(this Player player)
        {
            player.RemoveHandcuffs();
            return player;
        }

        /// <summary>
        /// Устанавливает god mode.
        /// </summary>
        public static Player SetGodMode(this Player player, bool enabled = true)
        {
            player.IsGodModeEnabled = enabled;
            return player;
        }

        /// <summary>
        /// Устанавливает noclip.
        /// </summary>
        public static Player SetNoClip(this Player player, bool enabled = true)
        {
            player.IsNoclipPermitted = enabled;
            return player;
        }

        /// <summary>
        /// Включает/выключает bypass mode.
        /// </summary>
        public static Player SetBypass(this Player player, bool enabled = true)
        {
            player.IsBypassModeEnabled = enabled;
            return player;
        }

        #endregion

        #region Reset & Setup - Сброс и Настройка

        /// <summary>
        /// Полный сброс игрока (здоровье, эффекты, инвентарь).
        /// </summary>
        public static Player Reset(this Player player)
        {
            player.FullHeal();
            player.ClearEffects();
            player.ClearInventory();
            FermixLog.Action("Игрок сброшен", player.Nickname);
            return player;
        }

        /// <summary>
        /// Настраивает игрока как "VIP" (god mode, bypass, noclip).
        /// </summary>
        public static Player SetupAsVIP(this Player player)
        {
            player.SetGodMode(true);
            player.SetBypass(true);
            player.SetNoClip(true);
            return player;
        }

        /// <summary>
        /// Снимает все привилегии VIP.
        /// </summary>
        public static Player RemoveVIP(this Player player)
        {
            player.SetGodMode(false);
            player.SetBypass(false);
            player.SetNoClip(false);
            return player;
        }

        #endregion

        #region Queries - Поиск Игроков

        /// <summary>
        /// Получает ближайшего игрока.
        /// </summary>
        public static Player GetClosest(this Player player, bool excludeSelf = true)
        {
            return Player.List
                .Where(p => p.IsAlive() && (!excludeSelf || p != player))
                .OrderBy(p => player.DistanceTo(p))
                .FirstOrDefault();
        }

        /// <summary>
        /// Получает всех игроков в радиусе.
        /// </summary>
        public static IEnumerable<Player> GetPlayersInRange(this Player player, float radius, bool excludeSelf = true)
        {
            return Player.List
                .Where(p => p.IsAlive() && (!excludeSelf || p != player) && player.DistanceTo(p) <= radius);
        }

        /// <summary>
        /// Получает всех союзников.
        /// </summary>
        public static IEnumerable<Player> GetAllies(this Player player)
        {
            return Player.List.Where(p => p.IsAlive() && p != player && p.Role.Side == player.Role.Side);
        }

        /// <summary>
        /// Получает всех врагов.
        /// </summary>
        public static IEnumerable<Player> GetEnemies(this Player player)
        {
            return Player.List.Where(p => p.IsAlive() && p.Role.Side != player.Role.Side && p.Role.Side != Side.None);
        }

        /// <summary>
        /// Получает все SCP на карте.
        /// </summary>
        public static IEnumerable<Player> GetAllScps()
        {
            return Player.List.Where(p => p.IsScp());
        }

        /// <summary>
        /// Получает всех людей на карте.
        /// </summary>
        public static IEnumerable<Player> GetAllHumans()
        {
            return Player.List.Where(p => p.IsHuman());
        }

        /// <summary>
        /// Получает случайного живого игрока.
        /// </summary>
        public static Player GetRandomAlive()
        {
            var alive = Player.List.Where(p => p.IsAlive()).ToList();
            return alive.Count > 0 ? alive[UnityEngine.Random.Range(0, alive.Count)] : null;
        }

        #endregion

        #region Fluent API Helpers - Вспомогательные Методы

        /// <summary>
        /// Выполняет действие над игроком.
        /// </summary>
        public static Player Do(this Player player, Action<Player> action)
        {
            action?.Invoke(player);
            return player;
        }

        /// <summary>
        /// Выполняет действие с задержкой.
        /// </summary>
        public static Player DoDelayed(this Player player, float delay, Action<Player> action)
        {
            FermixScheduler.DelayForPlayer(player, delay, action);
            return player;
        }

        /// <summary>
        /// Выполняет действие если условие истинно.
        /// </summary>
        public static Player DoIf(this Player player, bool condition, Action<Player> action)
        {
            if (condition)
            {
                action?.Invoke(player);
            }
            return player;
        }

        #endregion
    }
}
