using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp096;
using Exiled.Events.EventArgs.Scp1344;
using FermixAPI.Core;
using FermixAPI.Hints.Core.Enum;
using InventorySystem.Items.Usables.Scp1344;
using MEC;
using PlayerRoles.FirstPersonControl.Thirdperson.Subcontrollers.Wearables;
using UnityEngine;

namespace FermixAPI.Systems
{
    /// <summary>
    /// SCP-1344 (Heart of Fortune) — расширенная версия:
    /// • спавнятся в РАЗНЫЕ комнаты (без дублей в одной точке);
    /// • работают как «глушитель» триггера SCP-096 (взгляд на лицо не делает игрока целью);
    /// • никогда не «выкалывают глаза» при снятии (cancel deactivation final-effect, drop Blinded);
    /// • имеют батарейку: расходуется пока надеты, восстанавливается пока сняты;
    /// • toggle hotkey через FermixInput (по умолчанию F).
    /// </summary>
    public static class FermixScramble
    {
        private static readonly (RoomType Room, float Weight)[] SpawnPool =
        {
            (RoomType.HczArmory,    3f),
            (RoomType.Hcz049,       2f),
            (RoomType.HczTestRoom,  2f),
            (RoomType.EzGateA,      1f),
            (RoomType.EzGateB,      1f),
            (RoomType.EzShelter,    1f),
        };

        private const string GlowId = "fermix_scramble";
        private const string BatteryHintId = "fermix_scramble_battery";

        private static readonly object _lock = new();
        private static readonly HashSet<ushort> _ourSerials = new();

        // Заряд батарейки на серийник предмета (секунды активного использования).
        private static readonly Dictionary<ushort, float> _battery = new();

        private static CoroutineHandle _tickHandle;
        private static bool _initialized;
        private static Action<Player> _toggleBindHandler;

        public static void Initialize()
        {
            if (_initialized || FermixCore.Config?.ScrambleEnabled != true) return;

            FermixEvents.OnRoundStart += OnRoundStart;
            FermixEvents.OnRoundEnd += OnRoundEnd;
            Exiled.Events.Handlers.Scp096.AddingTarget += OnAddingTarget;
            Exiled.Events.Handlers.Scp1344.Deactivating += OnDeactivating;
            Exiled.Events.Handlers.Scp1344.ChangedStatus += OnChangedStatus;
            Exiled.Events.Handlers.Player.PickingUpItem += OnPickingUp;
            Exiled.Events.Handlers.Player.DroppingItem += OnDroppingItem;

            FermixGlow.AddGlow(GlowId,
                serial => { lock (_lock) return _ourSerials.Contains(serial); },
                new Color(1f, 0.4f, 0.85f),
                intensity: 1.4f,
                range: 3.5f,
                pulseEffect: true,
                pulseSpeed: 1.2f);

            // Бинд F через FermixInput: toggle очков на лету.
            if (FermixCore.Config?.ScrambleEnableHotkeyToggle == true && FermixInput.IsInitialized)
            {
                _toggleBindHandler = OnToggleBindPressed;
                FermixInput.RegisterPressedHandler(FermixInput.F, _toggleBindHandler);
            }

            _tickHandle = FermixCore.RunCoroutine(BatteryTick(), "FermixScramble.BatteryTick");

            _initialized = true;
        }

        public static void Shutdown()
        {
            if (!_initialized) return;

            FermixEvents.OnRoundStart -= OnRoundStart;
            FermixEvents.OnRoundEnd -= OnRoundEnd;
            Exiled.Events.Handlers.Scp096.AddingTarget -= OnAddingTarget;
            Exiled.Events.Handlers.Scp1344.Deactivating -= OnDeactivating;
            Exiled.Events.Handlers.Scp1344.ChangedStatus -= OnChangedStatus;
            Exiled.Events.Handlers.Player.PickingUpItem -= OnPickingUp;
            Exiled.Events.Handlers.Player.DroppingItem -= OnDroppingItem;

            if (_toggleBindHandler != null)
            {
                FermixInput.UnregisterPressedHandler(FermixInput.F, _toggleBindHandler);
                _toggleBindHandler = null;
            }

            if (_tickHandle.IsValid) Timing.KillCoroutines(_tickHandle);

            FermixGlow.RemoveGlow(GlowId);

            lock (_lock)
            {
                _ourSerials.Clear();
                _battery.Clear();
            }

            foreach (var p in Player.List)
                FermixHintStack.RemoveHint(p, BatteryHintId);

            _initialized = false;
        }

        // ── round lifecycle ───────────────────────────────────────────

        private static void OnRoundStart()
        {
            int count = Mathf.Clamp(FermixCore.Config?.ScrambleSpawnCount ?? 2, 0, 8);
            FermixScheduler.Delay(FermixCore.Config?.ScrambleSpawnDelay ?? 4f, () => SpawnItems(count));
        }

        private static void OnRoundEnd(Exiled.Events.EventArgs.Server.RoundEndedEventArgs _)
        {
            lock (_lock)
            {
                _ourSerials.Clear();
                _battery.Clear();
            }
        }

        private static void SpawnItems(int count)
        {
            if (count <= 0) return;

            // Берём комнаты по уникальному room.Position (а не по RoomType), потому что
            // некоторые типы (HczTestRoom, EzGateA) могут отсутствовать на конкретной seed-генерации,
            // и Room.Get(rt) тогда вернёт null — fallback нам нужен.
            var available = SpawnPool
                .Select(p => (Room: Room.Get(p.Room), p.Weight))
                .Where(t => t.Room != null)
                .ToList();

            if (available.Count == 0)
            {
                FermixLog.Warn("FermixScramble: ни одной из конфигурируемых комнат не существует на текущей seed — спавн пропущен.");
                return;
            }

            var usedRooms = new HashSet<Room>();

            for (int i = 0; i < count; i++)
            {
                // Если все «свежие» комнаты уже использованы — разрешаем повтор,
                // но добавляем сильный jitter чтобы не оказаться в одной точке.
                var pool = available.Where(t => !usedRooms.Contains(t.Room)).ToList();
                if (pool.Count == 0) pool = available;

                float total = pool.Sum(p => p.Weight);
                float roll = UnityEngine.Random.value * total;
                Room chosen = null;
                foreach (var (room, w) in pool)
                {
                    roll -= w;
                    if (roll <= 0f) { chosen = room; break; }
                }
                chosen ??= pool[0].Room;
                usedRooms.Add(chosen);

                Vector3 pos = chosen.Position
                              + Vector3.up * 1.0f
                              + new Vector3(UnityEngine.Random.Range(-1.5f, 1.5f), 0f, UnityEngine.Random.Range(-1.5f, 1.5f));

                Pickup pickup;
                try
                {
                    pickup = Pickup.CreateAndSpawn(ItemType.SCP1344, pos);
                }
                catch (Exception ex)
                {
                    FermixLog.Warn($"FermixScramble: spawn failed in {chosen.Name}: {ex.Message}");
                    continue;
                }
                if (pickup == null) continue;

                lock (_lock)
                {
                    _ourSerials.Add(pickup.Serial);
                    _battery[pickup.Serial] = Math.Max(0f, FermixCore.Config?.ScrambleBatteryMax ?? 60f);
                }
            }
        }

        // ── 096 immunity ──────────────────────────────────────────────

        private static void OnAddingTarget(AddingTargetEventArgs ev)
        {
            if (ev?.Target == null || !ev.IsLooking) return;
            if (ev.Target.Items == null) return;
            foreach (var item in ev.Target.Items)
            {
                if (item != null && item.Type == ItemType.SCP1344)
                {
                    // Учитываем только активные/надетые очки. Если в инвентаре
                    // лежат, но не активны — иммунитета нет.
                    if (item is Scp1344 scp1344 && scp1344.Status != Scp1344Status.Active)
                        continue;

                    ev.IsAllowed = false;
                    FermixHint.SendColored(ev.Target, "SCP-1344 заглушил взгляд 096", FermixHint.Magenta, 2f);
                    return;
                }
            }
        }

        // ── safe unequip (no eye-tearing) ────────────────────────────

        private static void OnDeactivating(DeactivatingEventArgs ev)
        {
            if (ev == null || FermixCore.Config?.ScrambleSafeUnequip != true) return;

            // Cancel'им стандартную деактивацию через "ActivateFinalEffects + drop".
            // StopDeactivation внутри патча: `instance.Status = Idle`,
            // `_useTime = 0`, `OwnerInventory.ServerSelectItem(0)`. Никаких эффектов,
            // никакого выпадения предмета. Игрок просто снимает очки и идёт дальше.
            ev.NewStatus = Scp1344Status.Idle;
            ev.IsAllowed = false;
        }

        private static void OnChangedStatus(ChangedStatusEventArgs ev)
        {
            if (ev?.Player == null || ev.Scp1344 == null) return;

            try
            {
                if (ev.Scp1344Status == Scp1344Status.Idle)
                {
                    // Срываем «остатки» эффекта если игра успела что-то навесить.
                    ev.Player.DisableEffect(EffectType.Scp1344);
                    ev.Player.DisableEffect(EffectType.Blinded);
                    ev.Player.ReferenceHub?.DisableWearables(WearableElements.Scp1344Goggles);
                }
            }
            catch (Exception ex)
            {
                FermixLog.Warn($"FermixScramble.OnChangedStatus: {ex.Message}");
            }
        }

        // ── pickups: assign battery to vanilla SCP-1344 too ──────────

        private static void OnPickingUp(PickingUpItemEventArgs ev)
        {
            if (ev?.Pickup == null || ev.Pickup.Type != ItemType.SCP1344) return;
            ushort s = ev.Pickup.Serial;
            lock (_lock)
            {
                if (!_battery.ContainsKey(s))
                    _battery[s] = Math.Max(0f, FermixCore.Config?.ScrambleBatteryMax ?? 60f);
            }
        }

        private static void OnDroppingItem(DroppingItemEventArgs ev)
        {
            if (ev?.Item == null || ev.Item.Type != ItemType.SCP1344) return;
            // Если очки были активны — сначала аккуратно гасим их (без эффектов),
            // только потом разрешаем drop.
            if (ev.Item is Scp1344 scp1344 && scp1344.Status == Scp1344Status.Active)
            {
                try
                {
                    scp1344.Status = Scp1344Status.Idle;
                    if (ev.Player != null)
                    {
                        ev.Player.DisableEffect(EffectType.Scp1344);
                        ev.Player.DisableEffect(EffectType.Blinded);
                        ev.Player.ReferenceHub?.DisableWearables(WearableElements.Scp1344Goggles);
                    }
                }
                catch (Exception ex)
                {
                    FermixLog.Warn($"FermixScramble.OnDroppingItem: {ex.Message}");
                }
            }
        }

        // ── F-bind: toggle on/off ────────────────────────────────────

        private static void OnToggleBindPressed(Player player)
        {
            if (player == null || player.IsDead) return;
            if (FermixCore.Config?.ScrambleEnableHotkeyToggle != true) return;

            // Ищем SCP-1344 в инвентаре (предпочитаем currently held).
            Scp1344 target = null;
            if (player.CurrentItem is Scp1344 held)
                target = held;
            else
                target = player.Items?.OfType<Scp1344>().FirstOrDefault();

            if (target == null) return;

            // Защита от toggle при пустой батарее.
            float batteryMax = Math.Max(0f, FermixCore.Config?.ScrambleBatteryMax ?? 60f);
            ushort s = target.Serial;
            float battery;
            lock (_lock) battery = _battery.TryGetValue(s, out var b) ? b : batteryMax;

            if (target.Status == Scp1344Status.Active || target.Status == Scp1344Status.Activating)
            {
                // Уже активны — гасим без эффектов.
                target.Status = Scp1344Status.Idle;
                try
                {
                    player.DisableEffect(EffectType.Scp1344);
                    player.DisableEffect(EffectType.Blinded);
                    player.ReferenceHub?.DisableWearables(WearableElements.Scp1344Goggles);
                }
                catch { /* ignore — DisableEffect/DisableWearables не должны валить toggle */ }
            }
            else
            {
                // Неактивны — пробуем включить, если есть заряд.
                if (batteryMax > 0f && battery <= 0f)
                {
                    FermixHint.SendColored(player, "SCP-1344: нет заряда (подожди, пока зарядится)", FermixHint.Red, 2f);
                    return;
                }

                target.Status = Scp1344Status.Active;
            }
        }

        // ── battery tick + HUD ───────────────────────────────────────

        private static IEnumerator<float> BatteryTick()
        {
            // Используем 1с-тикер: для индикатора и расчёта расхода/восстановления.
            while (true)
            {
                yield return Timing.WaitForSeconds(1f);

                try
                {
                    UpdateBatteries();
                }
                catch (Exception ex)
                {
                    FermixLog.Error($"FermixScramble.BatteryTick: {ex.Message}");
                }
            }
        }

        private static void UpdateBatteries()
        {
            float max = Math.Max(0f, FermixCore.Config?.ScrambleBatteryMax ?? 60f);
            float recharge = Math.Max(0f, FermixCore.Config?.ScrambleBatteryRecharge ?? 1.5f);

            // Если max = 0 — батарейка отключена, ничего не делаем (бесконечный заряд).
            if (max <= 0f)
            {
                foreach (var p in Player.List)
                    FermixHintStack.RemoveHint(p, BatteryHintId);
                return;
            }

            foreach (var player in Player.List)
            {
                if (player == null || !player.IsConnected) continue;
                if (player.Items == null) { FermixHintStack.RemoveHint(player, BatteryHintId); continue; }

                Scp1344 scp1344 = null;
                foreach (var it in player.Items)
                {
                    if (it is Scp1344 g) { scp1344 = g; break; }
                }
                if (scp1344 == null) { FermixHintStack.RemoveHint(player, BatteryHintId); continue; }

                ushort s = scp1344.Serial;
                float battery;
                lock (_lock) battery = _battery.TryGetValue(s, out var b) ? b : max;

                bool isActive = scp1344.Status == Scp1344Status.Active
                             || scp1344.Status == Scp1344Status.Activating;

                if (isActive)
                {
                    battery = Math.Max(0f, battery - 1f);
                    if (battery <= 0f)
                    {
                        // Force-deactivate без эффектов.
                        try
                        {
                            scp1344.Status = Scp1344Status.Idle;
                            player.DisableEffect(EffectType.Scp1344);
                            player.DisableEffect(EffectType.Blinded);
                            player.ReferenceHub?.DisableWearables(WearableElements.Scp1344Goggles);
                            FermixHint.SendColored(player, "SCP-1344: заряд кончился", FermixHint.Red, 2.5f);
                        }
                        catch (Exception ex)
                        {
                            FermixLog.Warn($"FermixScramble auto-off: {ex.Message}");
                        }
                    }
                }
                else
                {
                    battery = Math.Min(max, battery + recharge);
                }

                lock (_lock) _battery[s] = battery;

                // HUD-индикатор заряда.
                int pct = (int)Math.Round(battery / max * 100f);
                string color = pct switch
                {
                    >= 50 => "#7CFC00",
                    >= 25 => "#FFD700",
                    _     => "#FF4040",
                };
                string activeMarker = isActive ? " <color=#ff6ec7>ACTIVE</color>" : string.Empty;
                string text = $"<size=20><color={color}>SCP-1344: {pct}%</color>{activeMarker}</size>";

                FermixHintStack.ShowPersistentDynamicHint(
                    player,
                    _ => text,
                    BatteryHintId,
                    updateInterval: 1f,
                    priority: -40,
                    category: HintCategory.Custom,
                    color: FermixHint.White,
                    showBullet: false);
            }
        }
    }
}
