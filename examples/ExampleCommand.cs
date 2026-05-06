// Свои команды через CommandSystem (как у EXILED).
// FermixAPI ничего особенного для команд не требует — но даёт удобные хелперы для ответа.

using System;
using CommandSystem;
using Exiled.API.Features;
using FermixAPI.Extensions;

namespace MyServer.Examples
{
    /// <summary>
    /// Клиентская команда: <c>.heal &lt;hp&gt;</c> — даёт игроку указанное HP (для теста).
    /// Регистрируется автоматически по атрибуту <see cref="CommandHandlerAttribute"/>.
    /// </summary>
    [CommandHandler(typeof(ClientCommandHandler))]
    public sealed class HealCommand : ICommand
    {
        public string Command => "heal";
        public string[] Aliases => new[] { "hp" };
        public string Description => "Восстановить здоровье.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            var p = Player.Get(sender);
            if (p == null) { response = "Только для игроков."; return false; }
            if (!p.IsAlive) { response = "Ты мёртв!"; return false; }

            float amount = 50f;
            if (arguments.Count > 0 && float.TryParse(arguments.At(0), out var parsed))
                amount = parsed;

            p.Heal(amount);
            p.SendSuccess($"+{amount:F0} HP");

            response = $"Восстановлено {amount:F0} HP.";
            return true;
        }
    }

    /// <summary>
    /// Серверная команда (RA): <c>kickbots</c> — выгоняет всех тестовых ботов.
    /// </summary>
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public sealed class KickBotsCommand : ICommand
    {
        public string Command => "kickbots";
        public string[] Aliases => Array.Empty<string>();
        public string Description => "Выгнать всех ботов с сервера.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            int kicked = 0;
            foreach (var p in Player.List)
            {
                if (p.IsNPC)
                {
                    p.Kick("Тестовая чистка ботов.");
                    kicked++;
                }
            }

            response = $"Выгнано ботов: {kicked}";
            return true;
        }
    }
}
