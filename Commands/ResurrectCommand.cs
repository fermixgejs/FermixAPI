using System;
using System.Collections.Generic;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using PlayerRoles;
using Random = UnityEngine.Random;

namespace FermixAPI.Commands
{
    /// <summary>
    /// Клиентская команда <c>.res</c> / <c>.revive</c> — воскрешает игрока в случайной
    /// человеческой роли (<see cref="RoleTypeId.ClassD"/> или <see cref="RoleTypeId.Scientist"/>).
    /// Имеет ограничения по времени раунда и кулдаун между воскрешениями.
    /// </summary>
    [CommandHandler(typeof(ClientCommandHandler))]
    public sealed class ResurrectCommand : ICommand
    {
        /// <summary>Кулдаун между воскрешениями одного игрока.</summary>
        public static TimeSpan Cooldown { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>Окно с начала раунда, в котором разрешено воскрешение.</summary>
        public static TimeSpan AllowedWindow { get; set; } = TimeSpan.FromMinutes(5);

        private static readonly Dictionary<string, DateTime> _lastResurrect = new(StringComparer.Ordinal);

        public string Command => "res";

        public string[] Aliases => new[] { "revive" };

        public string Description => "Воскреснуть в случайной человеческой роли.";

        /// <summary>Сбросить все кулдауны (вызывать в начале раунда).</summary>
        public static void ResetCooldowns() => _lastResurrect.Clear();

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            var player = Player.Get(sender);
            if (player == null)
            {
                response = "Команда доступна только игрокам.";
                return false;
            }

            if (!Round.IsStarted)
            {
                response = "Раунд ещё не начался!";
                return false;
            }

            if (player.IsAlive)
            {
                response = "Вы уже живы!";
                return false;
            }

            if (Round.ElapsedTime > AllowedWindow)
            {
                response = $"Прошло уже более {AllowedWindow.TotalMinutes:F0} минут с начала раунда!";
                return false;
            }

            if (_lastResurrect.TryGetValue(player.UserId, out var last) && DateTime.UtcNow - last < Cooldown)
            {
                response = $"Вы можете воскреснуть только раз в {Cooldown.TotalSeconds:F0} секунд!";
                return false;
            }

            var roles = new[] { RoleTypeId.ClassD, RoleTypeId.Scientist };
            var role = roles[Random.Range(0, roles.Length)];
            player.Role.Set(role, SpawnReason.Resurrected);

            _lastResurrect[player.UserId] = DateTime.UtcNow;
            response = "Вы воскресли!";
            return true;
        }
    }
}
