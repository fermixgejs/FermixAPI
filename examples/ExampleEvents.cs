// Подписка на события через FermixEvents.
//
// Зачем FermixEvents, если есть Exiled.Events.Handlers.* напрямую?
//   • Один общий "хаб" — не надо тащить десяток using'ов.
//   • Корректное снятие подписок при reload (см. FermixEvents.Refresh).
//   • Доп. события (OnRoundStart без аргументов и т.п.).

using Exiled.Events.EventArgs.Player;
using FermixAPI.Core;
using FermixAPI.Extensions;

namespace MyServer.Examples
{
    public static class ExampleEvents
    {
        public static void Hook()
        {
            FermixEvents.OnPlayerJoin   += OnJoined;
            FermixEvents.OnPlayerLeave  += OnLeft;
            FermixEvents.OnPlayerDied   += OnDied;
            FermixEvents.OnRoleChange   += OnRoleChange;
            FermixEvents.OnRoundStart   += OnRoundStart;
            FermixEvents.OnRoundEnd     += _ => FermixLog.Info("Раунд закончился.");
        }

        public static void Unhook()
        {
            FermixEvents.OnPlayerJoin  -= OnJoined;
            FermixEvents.OnPlayerLeave -= OnLeft;
            FermixEvents.OnPlayerDied  -= OnDied;
            FermixEvents.OnRoleChange  -= OnRoleChange;
            FermixEvents.OnRoundStart  -= OnRoundStart;
            // Анонимные лямбды (как _ => …) не отписываются — поэтому их лучше избегать
            // в долгоживущих подписках (см. фикс из PR #2 для FermixScheduler).
        }

        private static void OnJoined(JoinedEventArgs ev)
        {
            ev.Player.SendHint($"Привет, <color=yellow>{ev.Player.Nickname}</color>!", 4f);
            FermixLog.Info($"+1 игрок: {ev.Player.Nickname} ({ev.Player.UserId})");
        }

        private static void OnLeft(LeftEventArgs ev)
        {
            FermixLog.Info($"-1 игрок: {ev.Player?.Nickname ?? "?"}");
        }

        private static void OnDied(DiedEventArgs ev)
        {
            if (ev.Attacker != null && ev.Attacker != ev.Player)
                ev.Attacker.SendSuccess($"Убил {ev.Player.Nickname}!", 2f);
        }

        private static void OnRoleChange(ChangingRoleEventArgs ev)
        {
            FermixLog.Debug($"{ev.Player.Nickname}: {ev.Player.Role.Type} -> {ev.NewRole}");
        }

        private static void OnRoundStart()
        {
            FermixLog.Success("Раунд стартовал.");
        }
    }
}
