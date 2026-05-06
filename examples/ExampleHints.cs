// Простые хинты через FermixHint (плоский API, под капотом — стэк).
// Если нужно несколько хинтов одновременно с приоритетами / persistent —
// смотри ExampleHintStack.cs.

using Exiled.API.Features;
using FermixAPI.Extensions;

namespace MyServer.Examples
{
    public static class ExampleHints
    {
        public static void Demo(Player p)
        {
            // Базовые
            p.SendHint("Простой хинт", 3f);
            p.SendSuccess("Готово!", 2f);
            p.SendError("Что-то пошло не так", 4f);
            p.SendWarning("Осторожно!", 3f);
            p.SendInfo("Информация", 3f);

            // Цвет на лету
            p.SendColored("Розовый текст", "#ff66cc", 3f);

            // Многострочный
            p.SendMultiline(new[]
            {
                "Строка 1",
                "<color=yellow>Строка 2</color>",
                "Строка 3",
            }, 5f);

            // Список
            p.SendList(
                title: "Магазин",
                items: new[] { "AK — 200$", "Аптечка — 50$", "Граната — 75$" },
                duration: 5f);

            // Прогресс-бар (например, индикатор кулдауна)
            p.SendProgress("Перезарядка", current: 2.5f, max: 5f, duration: 2f);
        }

        public static void DemoBroadcast(string msg)
        {
            // Всем игрокам
            FermixHint.SendToAll(msg, 4f);
            FermixHint.SendSuccessToAll("Команда сработала", 2f);
        }

        public static void DemoAnimated(Player p)
        {
            // Печатающийся текст (анимация — не идёт через стэк)
            p.SendTyping("Печатается по буквам...", charDelay: 0.05f, duration: 5f);

            // Мигающий
            p.SendBlinking("ВНИМАНИЕ", color1: "red", color2: "white", blinkRate: 0.4f, duration: 4f);
        }
    }
}
