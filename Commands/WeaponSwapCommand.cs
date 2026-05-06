using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using MEC;

namespace FermixAPI.Commands
{
    /// <summary>
    /// Клиентская команда <c>.weaponswap</c> / <c>.wps</c> — меняет текущее оружие
    /// игрока на "противоположное" по карте свопа (Crossvec ↔ FSP9 ↔ A7,
    /// E11SR ↔ AK, Logicer ↔ FRMG0). Не трогает кастомное оружие.
    /// </summary>
    [CommandHandler(typeof(ClientCommandHandler))]
    public sealed class WeaponSwapCommand : ICommand
    {
        private static readonly Dictionary<ItemType, ItemType> SwapMap = new()
        {
            // SMG-цикл
            { ItemType.GunFSP9,     ItemType.GunA7 },
            { ItemType.GunA7,       ItemType.GunCrossvec },
            { ItemType.GunCrossvec, ItemType.GunFSP9 },
            // штурмовые
            { ItemType.GunE11SR,    ItemType.GunAK },
            { ItemType.GunAK,       ItemType.GunE11SR },
            // тяжёлые
            { ItemType.GunLogicer,  ItemType.GunFRMG0 },
            { ItemType.GunFRMG0,    ItemType.GunLogicer },
            // Пистолеты/револьвер/дробовик не имеют пары — Execute вернёт ошибку,
            // не уничтожая текущее оружие (чтобы не сбросить патроны/моды).
        };

        /// <summary>Оружие, которое нельзя свапать (уникальное / событийное).</summary>
        private static readonly HashSet<ItemType> NonSwappable = new()
        {
            ItemType.MicroHID,
            ItemType.ParticleDisruptor,
            ItemType.Jailbird,
            ItemType.GunSCP127,
        };

        public string Command => "weaponswap";

        public string[] Aliases => new[] { "wps" };

        public string Description => "Поменять текущее оружие на парное.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            var player = Player.Get(sender);
            if (player == null)
            {
                response = "Команда доступна только игрокам.";
                return false;
            }

            if (!player.IsAlive)
            {
                response = "Вы мертвы!";
                return false;
            }

            // Если в руке нет оружия — берём первое подходящее из инвентаря.
            if (player.CurrentItem == null || !player.CurrentItem.IsWeapon)
            {
                var firstWeapon = player.Items
                    .FirstOrDefault(i => i != null && i.IsWeapon && !NonSwappable.Contains(i.Type));

                if (firstWeapon == null)
                {
                    response = "У вас нет оружия для замены!";
                    return false;
                }

                player.CurrentItem = firstWeapon;
            }

            var current = player.CurrentItem;
            if (current == null)
            {
                response = "Не удалось определить текущее оружие.";
                return false;
            }

            if (NonSwappable.Contains(current.Type))
            {
                response = "Это оружие нельзя заменить!";
                return false;
            }

            if (!SwapMap.TryGetValue(current.Type, out var target) || target == current.Type)
            {
                // Не пересоздаём оружие, если для него нет реальной пары —
                // иначе пропадут патроны и attachment'ы (см. Devin Review).
                response = "Для этого оружия нет парного варианта.";
                return false;
            }

            player.RemoveItem(current, true);
            var newWeapon = player.AddItem(target);

            // Через тик ставим новое оружие в руку.
            Timing.CallDelayed(0.1f, () =>
            {
                if (player != null && player.IsAlive && newWeapon != null)
                    player.CurrentItem = newWeapon;
            });

            response = "Вы сменили оружие!";
            return true;
        }
    }
}
