using System;
using CommandSystem;
using Exiled.API.Features;

namespace FermixAPI.Commands
{
    /// <summary>
    /// Клиентская команда <c>.rt</c> / <c>.roundtime</c> / <c>.time</c> — показывает
    /// прошедшее время раунда в формате HH:MM:SS.
    /// </summary>
    [CommandHandler(typeof(ClientCommandHandler))]
    public sealed class RoundTimeCommand : ICommand
    {
        public string Command => "rt";

        public string[] Aliases => new[] { "roundtime", "time" };

        public string Description => "Показать время раунда.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!Round.IsStarted)
            {
                response = "<color=red>Раунд ещё не начался!</color>";
                return false;
            }

            var elapsed = Round.ElapsedTime;
            response = $"<color=yellow>Время раунда:</color> {elapsed.Hours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}\n";
            return true;
        }
    }
}
