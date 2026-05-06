// FermixHintStack — стэкуемые хинты с приоритетами, категориями, persistent и dynamic.
//
// Главное отличие от плоского ShowHint: несколько хинтов выводятся на экран
// одновременно (объединяются в один текст), не перезатирают друг друга,
// сортируются по приоритету.

using Exiled.API.Features;
using FermixAPI.Core;
using FermixAPI.Extensions;

namespace MyServer.Examples
{
    public static class ExampleHintStack
    {
        public static void Demo(Player p)
        {
            // Обычный стэкуемый хинт. Категория задаёт цвет по умолчанию.
            FermixHintStack.AddHint(
                p,
                message: "Сообщение об успехе",
                duration: 3f,
                category: HintCategory.Success);

            // Параллельно положим warning — оба будут видны одновременно.
            FermixHintStack.AddHint(p, "Низкое здоровье!", 5f, HintCategory.Warning);

            // Хинт с приоритетом — поднимется выше остальных.
            FermixHintStack.AddHint(
                p,
                message: "ВАЖНО",
                duration: 4f,
                category: HintCategory.Error,
                priority: 100);

            // Persistent: висит, пока не уберём вручную (id обязателен).
            FermixHintStack.AddPersistent(p, id: "boss_phase", message: "Фаза 1");

            // Поменяли тот же id — старый автоматически заменится.
            FermixHintStack.AddPersistent(p, id: "boss_phase", message: "Фаза 2");

            // Dynamic: переопределяется каждый кадр через лямбду —
            // удобно для HP/патронов/таймеров.
            FermixHintStack.AddDynamic(
                p,
                id: "hp_indicator",
                getMessage: pl => $"HP: {pl.Health:F0} / {pl.MaxHealth:F0}",
                category: HintCategory.Info);

            // ShowProgress — встроенная анимация заполнения шкалы.
            FermixHintStack.ShowProgress(p, "cooldown", "Кулдаун способности", current: 5f, max: 10f);
        }

        public static void Cleanup(Player p)
        {
            // Снять конкретный persistent/dynamic
            FermixHintStack.RemoveHint(p, "boss_phase");
            FermixHintStack.RemoveHint(p, "hp_indicator");

            // Или почистить всё разом
            FermixHintStack.Clear(p);
        }

        // Также доступно через расширения Player
        public static void DemoExtensions(Player p)
        {
            p.ShowStacked("Просто стэк-хинт", 3f);
            p.ShowDynamic("ammo", pl => $"Патронов: {pl.CurrentItem?.Type}");
            p.ShowPersistent("debug_pos", "Позиция: ...");
            p.RemoveStacked("debug_pos");
        }
    }
}
