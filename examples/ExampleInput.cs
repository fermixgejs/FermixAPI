// FermixInput — биндинг кнопок через Server-Specific Settings (SSS).
// API даёт только механизм регистрации; что делать в обработчике — решает плагин.
//
// Стандартные кнопки уже зарегистрированы (LMB, RMB, R, Alt, Q, F, T)
// и появляются у игрока в SSS-меню "FermixAPI: бинды действий".

using Exiled.API.Features;
using FermixAPI.Systems;
using UnityEngine;

namespace MyServer.Examples
{
    public static class ExampleInput
    {
        public static void Hook()
        {
            // Глобальный hook на любую нажатую кнопку.
            FermixInput.OnPressed += (player, buttonId) =>
            {
                FermixLog.Debug($"{player.Nickname} нажал {buttonId}");
            };

            // Per-button обработчики.
            FermixInput.RegisterPressedHandler(FermixInput.Lmb, OnLmbPressed);
            FermixInput.RegisterReleasedHandler(FermixInput.Lmb, OnLmbReleased);
            FermixInput.RegisterHeldHandler(FermixInput.Alt, OnAltHeld);

            // Свой кастомный бинд (id уникальный, не пересекается с дефолтными 300-306).
            FermixInput.RegisterCustomKeybind(
                id: 350,
                label: "Использовать абилку",
                defaultKey: KeyCode.G,
                description: "Активирует кастомную способность");

            FermixInput.RegisterPressedHandler(350, player =>
            {
                player.SendSuccess("Абилка активирована!");
            });
        }

        public static void Unhook()
        {
            FermixInput.UnregisterPressedHandler(FermixInput.Lmb, OnLmbPressed);
            FermixInput.UnregisterReleasedHandler(FermixInput.Lmb, OnLmbReleased);
            FermixInput.UnregisterHeldHandler(FermixInput.Alt, OnAltHeld);
        }

        private static void OnLmbPressed(Player p)
        {
            FermixLog.Debug($"{p.Nickname}: LMB pressed");
        }

        private static void OnLmbReleased(Player p)
        {
            FermixLog.Debug($"{p.Nickname}: LMB released");
        }

        private static void OnAltHeld(Player p)
        {
            // Вызывается ~каждые 50 мс пока Alt зажат.
        }

        public static void DemoPolling(Player p)
        {
            // Опросный стиль (каждый кадр, например в FixedUpdate-подобной корутине).
            if (FermixInput.IsButtonPressed(p, FermixInput.R))
            {
                p.SendHint("Сейчас нажата R", 1f);
            }
        }
    }
}
