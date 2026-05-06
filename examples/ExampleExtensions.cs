// PlayerExtensions — fluent-расширения над Exiled Player.
// Все методы возвращают сам Player, чтобы можно было чейнить.

using Exiled.API.Enums;
using Exiled.API.Features;
using FermixAPI.Extensions;
using PlayerRoles;
using UnityEngine;

namespace MyServer.Examples
{
    public static class ExampleExtensions
    {
        public static void Demo(Player p)
        {
            // Fluent цепочка
            p.FullHeal()
             .AddAHP(50f)
             .ApplyEffect(EffectType.Invisible, duration: 5f)
             .SuccessHint("Buff активирован");

            // Роль / здоровье
            p.SetRole(RoleTypeId.NtfPrivate)
             .SetMaxHealth(150f)
             .FullHeal();

            // Эффекты
            p.Stun(2f)
             .Blind(duration: 3f)
             .Bleed(duration: 5f, intensity: 2);

            // Телепорт
            p.TeleportTo(RoomType.HczServers);
            // или к другому игроку:
            var target = Player.List.GetEnumerator();
            if (target.MoveNext()) p.TeleportTo(target.Current);

            // Заморозка / скорость
            p.Freeze(3f);
            p.SetSpeed(180);

            // Поиск ближайших / в радиусе
            foreach (var other in Player.List)
            {
                if (other != p && p.DistanceTo(other) < 5f)
                    other.SendWarning($"{p.Nickname} рядом!");
            }

            // Инвентарь
            p.ClearInventory()
             .Give(ItemType.GunCOM18, ItemType.Medkit, ItemType.Adrenaline)
             .GiveAllAmmo(amount: 200);

            if (p.HasItem(ItemType.Medkit))
                p.SendInfo($"Medkit'ов: {p.CountItem(ItemType.Medkit)}");

            // Командные проверки
            if (p.IsScp())     FermixLog.Debug("SCP");
            if (p.IsHuman())   FermixLog.Debug("Human");
            if (p.IsTeam(Team.FoundationForces)) FermixLog.Debug("MTF/FF");
        }
    }
}
