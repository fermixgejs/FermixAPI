using System;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using Random = UnityEngine.Random;

namespace FermixAPI.Commands
{
    /// <summary>
    /// Клиентская команда <c>.kill</c> / <c>.suicide</c> — позволяет игроку убить
    /// себя со случайной шуточной фразой. Полезно для ивентов и тестов.
    /// </summary>
    [CommandHandler(typeof(ClientCommandHandler))]
    public sealed class SuicideCommand : ICommand
    {
        private static readonly string[] DeathReasons =
        {
            "покончил с собой от безысходности",
            "решил проверить, больно ли умирать",
            "устал от этой жизни",
            "захотел посмотреть мир с другой стороны",
            "решил перезайти в следующем раунде",
            "не выдержал давления SCP",
            "забыл как дышать",
            "умер от кринжа",
            "получил передозировку SCP-500",
            "споткнулся о собственные ноги",
            "решил стать наблюдателем",
            "проиграл в рулетку",
            "съел слишком много SCP-330",
            "попытался обнять SCP-096",
            "решил что смерть — это выход",
            "не смог найти выход из комплекса",
            "устал бегать от SCP",
            "захотел отдохнуть",
            "случайно нажал Alt+F4 в жизни",
            "решил проверить что будет после смерти",
        };

        public string Command => "kill";

        public string[] Aliases => new[] { "suicide" };

        public string Description => "Убить себя.";

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
                response = "Вы уже мертвы!";
                return false;
            }

            var reason = DeathReasons[Random.Range(0, DeathReasons.Length)];
            player.Kill(DamageType.Custom, reason);

            response = "Вы убили себя!";
            return true;
        }
    }
}
