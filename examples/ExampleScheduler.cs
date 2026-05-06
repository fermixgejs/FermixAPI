// FermixScheduler — обёртка над MEC: задержки, повторяющиеся таймеры, отмена, ожидание условий.
// Удобнее, чем плодить свои корутины: всё трекается, на конец раунда чистится.

using Exiled.API.Features;
using FermixAPI.Core;

namespace MyServer.Examples
{
    public static class ExampleScheduler
    {
        public static void Demo(Player p)
        {
            // Через 5 секунд — что-то сделать.
            FermixScheduler.Delay(5f, () =>
            {
                p?.SendInfo("Прошло 5 секунд");
            });

            // С именем — потом можно отменить по имени.
            FermixScheduler.Delay("revive_timer", 30f, () =>
            {
                p?.SendSuccess("Можно воскреснуть!");
            });

            FermixScheduler.Cancel("revive_timer"); // отмена

            // Повторяющийся таймер: 10 раз с интервалом 1с.
            FermixScheduler.Repeat(interval: 1f, action: () =>
            {
                FermixLog.Debug($"tick @ {System.DateTime.Now:HH:mm:ss}");
            }, count: 10);

            // Бесконечный (count: -1).
            FermixScheduler.Repeat("hp_pulse", 0.5f, () => FermixLog.Debug("pulse"), count: -1);

            // Обратный отсчёт: вызывает onTick каждые ~0.1с с оставшимся временем.
            FermixScheduler.Countdown(
                duration: 10f,
                onTick: remaining => p?.ShowDynamic("countdown", _ => $"Осталось: {remaining:F1}s"),
                onComplete: () => p?.SendSuccess("Готово!"));

            // Ждать условия (например, пока игрок зайдёт в комнату).
            FermixScheduler.WaitUntil(
                condition: () => p != null && p.IsAlive && p.CurrentRoom?.Name.ToString() == "EzGateA",
                action: () => p?.SendInfo("Ты зашёл в комнату"),
                checkInterval: 0.25f,
                timeout: 60f);

            // Каждые 0.5с пока выполняется условие.
            FermixScheduler.While(
                condition: () => p?.IsAlive == true,
                action: () => { /* что-то делаем */ },
                interval: 0.5f);

            // Per-player: повторять только пока игрок жив.
            FermixScheduler.RepeatForPlayer(p, 1f, pl =>
            {
                pl.ShowDynamic("hp", x => $"HP: {x.Health:F0}");
            });

            // На следующий кадр.
            FermixScheduler.NextFrame(() => FermixLog.Debug("next frame"));

            FermixLog.Debug($"Активных задач: {FermixScheduler.ActiveTaskCount}");
        }
    }
}
