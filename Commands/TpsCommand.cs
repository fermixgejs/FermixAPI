using System;
using System.Collections.Generic;
using CommandSystem;
using Exiled.API.Features;
using MEC;
using UnityEngine;

namespace FermixAPI.Commands
{
    /// <summary>
    /// Клиентская команда <c>.tps</c> — показывает текущий TPS сервера с цветовой
    /// индикацией. Чистая утилита, не требующая внешних модулей.
    /// </summary>
    [CommandHandler(typeof(ClientCommandHandler))]
    public sealed class TpsCommand : ICommand
    {
        /// <summary>Корутина-монитор: считает TPS как 1/Time.deltaTime раз в секунду.</summary>
        private static CoroutineHandle _monitor;

        private static float _lastTps;

        public string Command => "tps";

        public string[] Aliases => Array.Empty<string>();

        public string Description => "Показать TPS сервера.";

        /// <summary>Запустить мониторинг TPS (вызывается из FermixCore при необходимости).</summary>
        public static void StartMonitor()
        {
            if (_monitor.IsRunning) return;
            _monitor = Timing.RunCoroutine(MonitorTps(), Segment.Update);
        }

        /// <summary>Остановить мониторинг TPS.</summary>
        public static void StopMonitor()
        {
            if (_monitor.IsRunning)
                Timing.KillCoroutines(_monitor);
        }

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            // Если мониторинг ещё не запускался — стартуем по запросу.
            if (!_monitor.IsRunning)
                StartMonitor();

            var tps = _lastTps > 0f ? _lastTps : 1f / Mathf.Max(Time.deltaTime, 0.001f);

            string label, color;
            if (tps >= 50f) { label = "Отлично"; color = "green"; }
            else if (tps >= 40f) { label = "Хорошо"; color = "yellow"; }
            else if (tps >= 30f) { label = "Средне"; color = "orange"; }
            else { label = "Плохо"; color = "red"; }

            response = $"<color={color}>TPS: {tps:F1} ({label})</color>\n";
            return true;
        }

        private static IEnumerator<float> MonitorTps()
        {
            while (true)
            {
                _lastTps = 1f / Mathf.Max(Time.deltaTime, 0.001f);
                yield return Timing.WaitForSeconds(1f);
            }
        }
    }
}
