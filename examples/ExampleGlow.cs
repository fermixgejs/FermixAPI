// FermixGlow — кастомная подсветка предметов (pickup на земле + предмет в руке).
// Под капотом — LightSourceToy от Mirror, авто-обновление по корутине.
//
// Подсветка идёт по предикату на ushort-серийный номер предмета,
// чтобы плагин сам решал, какие конкретно экземпляры подсвечивать.

using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Player;
using FermixAPI.Core;
using FermixAPI.Systems;
using UnityEngine;

namespace MyServer.Examples
{
    public static class ExampleGlow
    {
        // Помним, какой пикап — "магический"
        private static readonly HashSet<ushort> _magicSerials = new();

        public static void Hook()
        {
            // Подсветить все Coin'ы фиолетовым.
            FermixGlow.AddGlowHex(
                itemCheck: serial => Pickup.Get(serial)?.Type == ItemType.Coin,
                hexColor: "#9933FF",
                intensity: 1.5f,
                range: 4f);

            // Пульсирующий красный ножик.
            FermixGlow.AddPulsingGlow(
                itemCheck: serial => Pickup.Get(serial)?.Type == ItemType.Jailbird,
                hexColor: "#FF2222",
                pulseSpeed: 1.2f);

            // Радуга на конкретные сериалы (например, для кастомного предмета).
            FermixGlow.AddRainbowGlow(
                itemCheck: serial => _magicSerials.Contains(serial),
                intensity: 1f,
                range: 6f);

            // При спавне нашего кастомного предмета — добавляем его сериал в _magicSerials.
            FermixEvents.OnItemPickup += OnItemPickup;
        }

        public static void Unhook()
        {
            FermixEvents.OnItemPickup -= OnItemPickup;
        }

        private static void OnItemPickup(PickingUpItemEventArgs ev)
        {
            // Допустим, "магический" предмет — это любая SCP-500 (как пример).
            if (ev.Pickup.Type == ItemType.SCP500)
                _magicSerials.Add(ev.Pickup.Serial);
        }

        public static void DemoOneShot(Player p)
        {
            // Подсветить конкретно тот предмет, что сейчас у игрока в руке.
            var current = p.CurrentItem;
            if (current == null) return;

            var serial = current.Serial;
            FermixGlow.AddGlowHex(
                itemCheck: s => s == serial,
                hexColor: "#00FFAA");
        }

        public static void RemoveAllGlows()
        {
            // Если получали id из Add*Glow — можно снять конкретную:
            // FermixGlow.RemoveGlow(id);
            //
            // На конец раунда подсветка авто-чистится.
        }
    }
}
