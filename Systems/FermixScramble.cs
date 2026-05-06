using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Scp096;
using Exiled.Events.EventArgs.Scp1344;
using FermixAPI.Core;
using InventorySystem.Items.Usables.Scp1344;
using PlayerRoles.FirstPersonControl.Thirdperson.Subcontrollers.Wearables;
using UnityEngine;

namespace FermixAPI.Systems
{
    /// <summary>
    /// SCP-1344 (Heart of Fortune) — глушитель триггера SCP-096.
    /// • спавнятся в РАЗНЫЕ комнаты (без дублей в одной точке);
    /// • работают как passive «глушитель» взгляда 096 (просто наличие в инвентаре);
    /// • НАДЕТЬ ИХ НЕЛЬЗЯ (любая попытка перейти в Active/Activating
    ///   блокируется через ChangingStatus — это ради того, чтобы не было
    ///   эффекта Blinded и побочных «вырываний глаз»).
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

        private static readonly object _lock = new();
        private static readonly HashSet<ushort> _ourSerials = new();

        private static bool _initialized;

        public static void Initialize()
        {
            if (_initialized || FermixCore.Config?.ScrambleEnabled != true) return;

            FermixEvents.OnRoundStart += OnRoundStart;
            FermixEvents.OnRoundEnd += OnRoundEnd;
            Exiled.Events.Handlers.Scp096.AddingTarget += OnAddingTarget;
            Exiled.Events.Handlers.Scp1344.ChangingStatus += OnChangingStatus;
            Exiled.Events.Handlers.Scp1344.ChangedStatus += OnChangedStatus;

            FermixGlow.AddGlow(GlowId,
                serial => { lock (_lock) return _ourSerials.Contains(serial); },
                new Color(1f, 0.4f, 0.85f),
                intensity: 1.4f,
                range: 3.5f,
                pulseEffect: true,
                pulseSpeed: 1.2f);

            _initialized = true;
        }

        public static void Shutdown()
        {
            if (!_initialized) return;

            FermixEvents.OnRoundStart -= OnRoundStart;
            FermixEvents.OnRoundEnd -= OnRoundEnd;
            Exiled.Events.Handlers.Scp096.AddingTarget -= OnAddingTarget;
            Exiled.Events.Handlers.Scp1344.ChangingStatus -= OnChangingStatus;
            Exiled.Events.Handlers.Scp1344.ChangedStatus -= OnChangedStatus;

            FermixGlow.RemoveGlow(GlowId);

            lock (_lock) _ourSerials.Clear();

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
            lock (_lock) _ourSerials.Clear();
        }

        private static void SpawnItems(int count)
        {
            if (count <= 0) return;

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

                lock (_lock) _ourSerials.Add(pickup.Serial);
            }
        }

        // ── 096 immunity (passive: достаточно носить в инвентаре) ────

        private static void OnAddingTarget(AddingTargetEventArgs ev)
        {
            if (ev?.Target == null || !ev.IsLooking) return;
            if (ev.Target.Items == null) return;
            foreach (var item in ev.Target.Items)
            {
                if (item != null && item.Type == ItemType.SCP1344)
                {
                    ev.IsAllowed = false;
                    FermixHint.SendColored(ev.Target, "SCP-1344 заглушил взгляд 096", FermixHint.Magenta, 2f);
                    return;
                }
            }
        }

        // ── HARD-BLOCK активации (нельзя надеть) ─────────────────────

        private static void OnChangingStatus(ChangingStatusEventArgs ev)
        {
            if (ev == null || !ev.IsAllowed) return;

            // Блокируем любые переходы в Active/Activating. Allow только Idle и
            // Deactivating (если игра вдруг захотела закрыть очки самостоятельно
            // — пусть закрывает безопасно).
            switch (ev.Scp1344StatusNew)
            {
                case Scp1344Status.Activating:
                case Scp1344Status.Active:
                case Scp1344Status.Stabbing:
                case Scp1344Status.Dropping:
                    ev.Scp1344StatusNew = Scp1344Status.Idle;
                    ev.IsAllowed = false;

                    if (ev.Player != null)
                    {
                        try
                        {
                            FermixHint.SendColored(
                                ev.Player,
                                "SCP-1344 опасны: их нельзя надевать. Просто носи их с собой — этого достаточно для защиты от 096.",
                                FermixHint.Magenta,
                                3.5f);
                        }
                        catch { /* hint failure must not break the deny */ }
                    }
                    break;
            }
        }

        private static void OnChangedStatus(ChangedStatusEventArgs ev)
        {
            if (ev?.Player == null || ev.Scp1344 == null) return;

            try
            {
                // На всякий случай: если несмотря на ChangingStatus игра всё-таки
                // довела до non-Idle статуса (другой плагин выставил его напрямую,
                // через Status setter), форсируем Idle и снимаем эффекты.
                if (ev.Scp1344Status != Scp1344Status.Idle)
                {
                    ev.Scp1344.Status = Scp1344Status.Idle;
                }

                ev.Player.DisableEffect(EffectType.Scp1344);
                ev.Player.DisableEffect(EffectType.Blinded);
                ev.Player.ReferenceHub?.DisableWearables(WearableElements.Scp1344Goggles);
            }
            catch (Exception ex)
            {
                FermixLog.Warn($"FermixScramble.OnChangedStatus: {ex.Message}");
            }
        }
    }
}
