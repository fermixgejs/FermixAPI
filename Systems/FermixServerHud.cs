using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Waves;
using Exiled.Events.EventArgs.Player;
using FermixAPI.Core;
using FermixAPI.Hints.Core.Enum;
using FermixAPI.Hints.Core.Utilities;
using MEC;
using PlayerRoles;
using HsmHint = FermixAPI.Hints.Core.Models.Hints.Hint;

namespace FermixAPI.Systems
{
    /// <summary>
    /// Серверный HUD: «шапка» сервера, статус игрока, активные SCP с HP,
    /// индикатор SCP-чата и таймер ближайшей волны для зрителей.
    /// Портировано из <c>Hazbin.NoRules.Hud</c> с переписыванием под
    /// архитектуру FermixAPI:
    /// <list type="bullet">
    /// <item>каждая надпись — отдельный <see cref="HsmHint"/>, висящий в
    /// собственной группе PlayerDisplay (<c>FermixAPI.ServerHud</c>), чтобы
    /// не мешать центральному <see cref="FermixHintStack"/> и другим
    /// «угловым» HUD'ам (FermixChat, FermixCallvote, FermixGeneratorHud);</item>
    /// <item>привязка к игроку идёт через <see cref="FermixEvents.OnPlayerJoin"/>
    /// с тем же 5-секундным defer'ом, что и FermixChat, чтобы Mirror успел
    /// доинициализировать NetworkBehaviour'ы и не сломать join (исторический
    /// баг 2.5.5 → 2.5.6);</item>
    /// <item>конфигурация в <see cref="Config"/>: название сервера, список
    /// подсказок для зрителей, интервал смены, и флаги отображения отдельных
    /// блоков (уровень / SCP-HP-лист / таймер волн).</item>
    /// </list>
    /// </summary>
    public static class FermixServerHud
    {
        private const string HsmGroupName = "FermixAPI.ServerHud";

        // Координаты подобраны под идиому этого форка HSM, наблюдённую в
        // существующих подсистемах (FermixCallvote/FermixGeneratorHud):
        // YCoordinate небольшое + YCoordinateAlign.Bottom = у нижней кромки;
        // YCoordinate небольшое + YCoordinateAlign.Top = у верхней кромки;
        // YCoordinateAlign.Middle ~ 540 = центр экрана.
        //
        // Все координаты — для соотношения 16:9. Для других ratio (4:3, 16:10)
        // SCP:SL автоматически масштабирует pixel-space, так что отклонение
        // в районе ±15% по X на крайних значениях допустимо. При желании
        // можно вынести в config, но Hazbin-оригинал тоже захардкожен.

        // Шапка раунда/TPS — самый верх, по центру.
        private const float RoundTimeY = 20f;
        private const int   RoundTimeFontSize = 16;

        // Название сервера — низ экрана, со смещением влево (header).
        private const float ServerNameY = 80f;
        private const float ServerNameX = -380f;
        private const int   ServerNameFontSize = 36;

        // Карточка игрока (ник/роль/уровень) — низ экрана, чуть выше нижнего края.
        private const float PlayerInfoY = 25f;
        private const float PlayerInfoX = -380f;
        private const int   PlayerInfoFontSize = 18;

        // Статус SCP-чата — справа в центре.
        private const float ScpChatY = 380f;
        private const float ScpChatX = 380f;
        private const int   ScpChatFontSize = 22;

        // Список SCP с HP — справа, выше центра.
        private const float ScpListY = 180f;
        private const float ScpListX = 380f;
        private const int   ScpListFontSize = 20;

        // Таймер волны — низ экрана, по центру (только для мёртвых).
        private const float WaveY = 180f;
        private const float WaveFontSize = 18;

        private const float UpdateTickFast = 0.45f; // SCP info, server name pos
        private const float UpdateTickSlow = 1.00f; // round time, wave timer, player info

        private static readonly object _lock = new object();
        private static readonly Dictionary<Player, ServerHudData> _hud = new Dictionary<Player, ServerHudData>();
        private static readonly System.Random _rand = new System.Random();

        private static CoroutineHandle _slowTick;
        private static CoroutineHandle _fastTick;
        private static CoroutineHandle _infoTick;
        private static string _currentSpectatorTip = string.Empty;
        private static bool _initialized;

        private sealed class ServerHudData
        {
            public HsmHint RoundTime;
            public HsmHint ServerName;
            public HsmHint PlayerInfo;
            public HsmHint ScpChat;
            public HsmHint ScpList;
            public HsmHint WaveTimer;

            public IEnumerable<HsmHint> All()
            {
                if (RoundTime != null) yield return RoundTime;
                if (ServerName != null) yield return ServerName;
                if (PlayerInfo != null) yield return PlayerInfo;
                if (ScpChat != null) yield return ScpChat;
                if (ScpList != null) yield return ScpList;
                if (WaveTimer != null) yield return WaveTimer;
            }
        }

        public static void Initialize()
        {
            if (_initialized) return;
            if (FermixCore.Config?.ServerHudEnabled != true) return;

            _currentSpectatorTip = PickSpectatorTip();

            FermixEvents.OnPlayerJoin += OnPlayerJoinedDeferred;
            FermixEvents.OnPlayerLeave += OnPlayerLeft;
            FermixEvents.OnRoleChange += OnRoleChange;

            _slowTick = FermixCore.RunCoroutine(SlowTickLoop(), "FermixServerHud.SlowTick");
            _fastTick = FermixCore.RunCoroutine(FastTickLoop(), "FermixServerHud.FastTick");
            _infoTick = FermixCore.RunCoroutine(InfoRotationLoop(), "FermixServerHud.InfoTick");

            _initialized = true;
            FermixLog.Info("FermixServerHud инициализирован.");
        }

        public static void Shutdown()
        {
            if (!_initialized) return;

            FermixEvents.OnPlayerJoin -= OnPlayerJoinedDeferred;
            FermixEvents.OnPlayerLeave -= OnPlayerLeft;
            FermixEvents.OnRoleChange -= OnRoleChange;

            if (_slowTick.IsValid) Timing.KillCoroutines(_slowTick);
            if (_fastTick.IsValid) Timing.KillCoroutines(_fastTick);
            if (_infoTick.IsValid) Timing.KillCoroutines(_infoTick);

            DetachAll();

            _initialized = false;
        }

        // ── Player lifecycle ────────────────────────────────────────────

        private static void OnPlayerJoinedDeferred(JoinedEventArgs ev)
        {
            if (ev?.Player == null) return;
            var player = ev.Player;

            // Тот же фикс, что и в FermixChat: PlayerDisplay внутри HSM
            // запускает фоновый Mirror.Send из ThreadPool — если сделать
            // это слишком рано (Mirror NetworkBehaviour'ы ещё инициализируются),
            // QueryProcessor.OnDestroy роняется с NRE и игрока выкидывает.
            FermixScheduler.Delay(5f, () =>
            {
                try
                {
                    if (player == null || !player.IsConnected) return;
                    AttachFor(player);
                }
                catch (Exception ex)
                {
                    FermixLog.Warn($"FermixServerHud.AttachFor (deferred): {ex.Message}");
                }
            });
        }

        private static void OnPlayerLeft(LeftEventArgs ev)
        {
            if (ev?.Player == null) return;
            DetachFor(ev.Player);
        }

        private static void OnRoleChange(Exiled.Events.EventArgs.Player.ChangingRoleEventArgs ev)
        {
            if (ev?.Player == null) return;
            // SCP-chat индикатор нужно сбросить, так как при смене роли
            // прежняя «вы говорите в чат SCP» подпись теряет актуальность.
            ServerHudData data;
            lock (_lock) _hud.TryGetValue(ev.Player, out data);
            if (data?.ScpChat != null) data.ScpChat.Text = string.Empty;
        }

        // ── Public API: индикатор SCP-чата (вызывается из проксимити-чата
        // если он будет добавлен; пока используем как заглушку для будущей
        // интеграции — публичный метод оставлен сознательно). ────────────

        public static void SetScpChatStatus(Player player, ScpChatStatus status)
        {
            if (player == null) return;
            ServerHudData data;
            lock (_lock) _hud.TryGetValue(player, out data);
            if (data?.ScpChat == null) return;

            data.ScpChat.Text = status switch
            {
                ScpChatStatus.ScpOnly => "<b>Вы говорите в <color=red>чат SCP</color></b>",
                ScpChatStatus.Proximity => "<b>Вы говорите в <color=green>общий чат</color></b>",
                _ => string.Empty,
            };
        }

        public enum ScpChatStatus { None, ScpOnly, Proximity }

        // ── Attach / Detach ─────────────────────────────────────────────

        private static void AttachFor(Player player)
        {
            if (player == null || player.ReferenceHub == null) return;

            ServerHudData data = new ServerHudData();

            data.RoundTime = MakeHint(
                yCoord: RoundTimeY, xCoord: 0f,
                vAlign: HintVerticalAlign.Top,
                hAlign: HintAlignment.Center,
                fontSize: RoundTimeFontSize);

            data.ServerName = MakeHint(
                yCoord: ServerNameY, xCoord: ServerNameX,
                vAlign: HintVerticalAlign.Bottom,
                hAlign: HintAlignment.Left,
                fontSize: ServerNameFontSize);

            data.PlayerInfo = MakeHint(
                yCoord: PlayerInfoY, xCoord: PlayerInfoX,
                vAlign: HintVerticalAlign.Bottom,
                hAlign: HintAlignment.Left,
                fontSize: PlayerInfoFontSize);

            data.ScpChat = MakeHint(
                yCoord: ScpChatY, xCoord: ScpChatX,
                vAlign: HintVerticalAlign.Middle,
                hAlign: HintAlignment.Left,
                fontSize: ScpChatFontSize);

            data.ScpList = MakeHint(
                yCoord: ScpListY, xCoord: ScpListX,
                vAlign: HintVerticalAlign.Middle,
                hAlign: HintAlignment.Left,
                fontSize: ScpListFontSize);

            data.WaveTimer = MakeHint(
                yCoord: WaveY, xCoord: 0f,
                vAlign: HintVerticalAlign.Bottom,
                hAlign: HintAlignment.Center,
                fontSize: (int)WaveFontSize);

            lock (_lock)
            {
                if (_hud.ContainsKey(player)) return; // race с reattach
                _hud[player] = data;
            }

            try
            {
                var pd = PlayerDisplay.Get(player.ReferenceHub);
                foreach (var hint in data.All())
                    pd.AddHint(hint, HsmGroupName);

                // Сразу залить актуальный текст, чтобы не было пустого экрана
                // первую секунду до тика.
                UpdateAllForPlayer(player, data);
            }
            catch (Exception ex)
            {
                FermixLog.Warn($"FermixServerHud.AttachFor({player?.Nickname}): {ex.Message}");
                lock (_lock) _hud.Remove(player);
            }
        }

        private static HsmHint MakeHint(float yCoord, float xCoord, HintVerticalAlign vAlign, HintAlignment hAlign, int fontSize)
        {
            return new HsmHint
            {
                YCoordinate = yCoord,
                XCoordinate = xCoord,
                YCoordinateAlign = vAlign,
                Alignment = hAlign,
                SyncSpeed = HintSyncSpeed.Normal,
                FontSize = fontSize,
                Text = string.Empty,
            };
        }

        private static void DetachFor(Player player)
        {
            if (player == null) return;
            ServerHudData data;
            lock (_lock)
            {
                if (!_hud.TryGetValue(player, out data)) return;
                _hud.Remove(player);
            }

            try
            {
                if (player.ReferenceHub != null)
                {
                    var pd = PlayerDisplay.Get(player.ReferenceHub);
                    foreach (var hint in data.All())
                        pd.RemoveHint(hint, HsmGroupName);
                }
            }
            catch (Exception ex)
            {
                FermixLog.Warn($"FermixServerHud.DetachFor({player?.Nickname}): {ex.Message}");
            }
        }

        private static void DetachAll()
        {
            List<KeyValuePair<Player, ServerHudData>> snapshot;
            lock (_lock)
            {
                snapshot = new List<KeyValuePair<Player, ServerHudData>>(_hud);
                _hud.Clear();
            }

            foreach (var kv in snapshot)
            {
                try
                {
                    if (kv.Key?.ReferenceHub != null)
                    {
                        var pd = PlayerDisplay.Get(kv.Key.ReferenceHub);
                        foreach (var hint in kv.Value.All())
                            pd.RemoveHint(hint, HsmGroupName);
                    }
                }
                catch (Exception ex) { FermixLog.Warn($"FermixServerHud.DetachAll: {ex.Message}"); }
            }
        }

        // ── Tick coroutines ─────────────────────────────────────────────

        private static IEnumerator<float> SlowTickLoop()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(UpdateTickSlow);

                List<KeyValuePair<Player, ServerHudData>> snapshot;
                lock (_lock) snapshot = new List<KeyValuePair<Player, ServerHudData>>(_hud);

                foreach (var kv in snapshot)
                {
                    try { UpdateRoundTime(kv.Value); } catch (Exception ex) { FermixLog.Warn($"ServerHud.RoundTime: {ex.Message}"); }
                    try { UpdatePlayerInfo(kv.Key, kv.Value); } catch (Exception ex) { FermixLog.Warn($"ServerHud.PlayerInfo: {ex.Message}"); }
                    try { UpdateWaveTimer(kv.Key, kv.Value); } catch (Exception ex) { FermixLog.Warn($"ServerHud.WaveTimer: {ex.Message}"); }
                }
            }
        }

        private static IEnumerator<float> FastTickLoop()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(UpdateTickFast);

                List<KeyValuePair<Player, ServerHudData>> snapshot;
                lock (_lock) snapshot = new List<KeyValuePair<Player, ServerHudData>>(_hud);

                foreach (var kv in snapshot)
                {
                    try { UpdateServerName(kv.Value); } catch (Exception ex) { FermixLog.Warn($"ServerHud.ServerName: {ex.Message}"); }
                    try { UpdateScpList(kv.Key, kv.Value); } catch (Exception ex) { FermixLog.Warn($"ServerHud.ScpList: {ex.Message}"); }
                }
            }
        }

        private static IEnumerator<float> InfoRotationLoop()
        {
            while (true)
            {
                float interval = Math.Max(2f, FermixCore.Config?.ServerHudInfoRotationInterval ?? 8f);
                yield return Timing.WaitForSeconds(interval);
                try { _currentSpectatorTip = PickSpectatorTip(); }
                catch (Exception ex) { FermixLog.Warn($"ServerHud.InfoRotation: {ex.Message}"); }
            }
        }

        // ── Updaters (per hint) ─────────────────────────────────────────

        private static void UpdateAllForPlayer(Player player, ServerHudData data)
        {
            UpdateRoundTime(data);
            UpdateServerName(data);
            UpdatePlayerInfo(player, data);
            UpdateScpList(player, data);
            UpdateWaveTimer(player, data);
        }

        private static void UpdateRoundTime(ServerHudData data)
        {
            if (data?.RoundTime == null) return;

            TimeSpan elapsed = Round.ElapsedTime;
            string timeFmt = elapsed.Hours > 0
                ? elapsed.ToString(@"hh\:mm\:ss")
                : elapsed.ToString(@"mm\:ss");

            int tps = (int)Server.Tps;
            int tpsMax = (int)Server.MaxTps;

            string tpsColor = tps >= tpsMax - 2 ? "#5cd45c" : tps >= tpsMax / 2 ? "#f0c44a" : "#ff4444";

            string text = $"<b>Раунд: {timeFmt}  |  TPS: <color={tpsColor}>{tps}/{tpsMax}</color></b>";
            if (data.RoundTime.Text != text) data.RoundTime.Text = text;
        }

        private static void UpdateServerName(ServerHudData data)
        {
            if (data?.ServerName == null) return;
            string serverName = FermixCore.Config?.ServerHudServerName ?? "NezerHill NoRules";
            string text = $"<b>{serverName}</b>";
            if (data.ServerName.Text != text) data.ServerName.Text = text;
        }

        private static void UpdatePlayerInfo(Player player, ServerHudData data)
        {
            if (data?.PlayerInfo == null || player == null) return;

            string nickname = player.Nickname ?? "?";

            string roleText = TranslateRole(player.Role?.Type ?? RoleTypeId.None, useColors: player.IsAlive);

            string groupTail = string.Empty;
            try
            {
                string coloredGroup = player.ReferenceHub?.serverRoles?.GetColoredRoleString();
                if (!string.IsNullOrEmpty(coloredGroup))
                    groupTail = $"\n<color=#C93E3E>Права</color>: {coloredGroup}";
            }
            catch { /* no role group */ }

            string levelText = "<color=#888888>Неизвестно</color>";
            if (FermixCore.Config?.ServerHudShowPlayerLevel == true)
            {
                try
                {
                    var lvl = FermixPlayerXp.GetLevel(player);
                    if (lvl != null)
                    {
                        string hex = lvl.Color.GetHexColor();
                        levelText = $"<color=#{hex}>{lvl.Text}</color>";
                    }
                }
                catch { /* PlayerXp недоступен — Неизвестно */ }
            }

            string text =
                $"<color=#C93E3E>Вы</color>: {nickname}\n" +
                $"<color=#C93E3E>Роль</color>: {roleText}\n" +
                $"<color=#C93E3E>Уровень</color>: {levelText}{groupTail}";

            if (data.PlayerInfo.Text != text) data.PlayerInfo.Text = text;
        }

        private static void UpdateScpList(Player viewer, ServerHudData data)
        {
            if (data?.ScpList == null) return;
            if (FermixCore.Config?.ServerHudShowScpHpList != true)
            {
                if (data.ScpList.Text != string.Empty) data.ScpList.Text = string.Empty;
                return;
            }
            // Показываем список только живым SCP-командникам — чтобы зрителям
            // и людям не светить HP активных SCP. Если хотим всем — снять
            // условие. Hazbin-оригинал тоже показывал только SCP.
            if (viewer == null || !viewer.IsAlive || viewer.Role?.Type == null
                || viewer.Role.Side != Side.Scp)
            {
                if (data.ScpList.Text != string.Empty) data.ScpList.Text = string.Empty;
                return;
            }

            var scps = Player.List.Where(p => p != null && p.IsAlive && p.IsScp).ToList();
            if (scps.Count == 0)
            {
                if (data.ScpList.Text != string.Empty) data.ScpList.Text = string.Empty;
                return;
            }

            var sb = new StringBuilder();
            for (int i = 0; i < scps.Count; i++)
            {
                var scp = scps[i];
                string roleColored = TranslateRole(scp.Role.Type, useColors: true);
                int hp = (int)Math.Round(scp.Health);
                sb.Append($"{roleColored} <color=#FFFFFF>[<color=yellow>{hp}</color>]</color>");
                if (i < scps.Count - 1) sb.Append('\n');
            }

            string text = sb.ToString();
            if (data.ScpList.Text != text) data.ScpList.Text = text;
        }

        private static void UpdateWaveTimer(Player player, ServerHudData data)
        {
            if (data?.WaveTimer == null) return;
            if (FermixCore.Config?.ServerHudShowWaveTimers != true)
            {
                if (data.WaveTimer.Text != string.Empty) data.WaveTimer.Text = string.Empty;
                return;
            }

            // Показываем волновой таймер только мёртвым (зрителям) и только
            // когда раунд активен — иначе блок пустой.
            if (player == null || player.IsAlive || !Round.IsStarted)
            {
                if (data.WaveTimer.Text != string.Empty) data.WaveTimer.Text = string.Empty;
                return;
            }

            string ntfTime = "—:—";
            string ntfMiniTime = "—:—";
            string chaosTime = "—:—";
            string chaosMiniTime = "—:—";

            try
            {
                var waves = WaveTimer.GetWaveTimers();
                if (waves != null)
                {
                    foreach (var w in waves)
                    {
                        if (w == null) continue;
                        string n = (w.Name ?? string.Empty).ToLowerInvariant();
                        bool ntf = n.Contains("ntf") || n.Contains("nineTailed".ToLower());
                        bool chaos = n.Contains("chaos");
                        string formatted = $"{w.TimeLeft.Minutes:D2}:{w.TimeLeft.Seconds:D2}";
                        if (ntf && !w.IsMiniWave) ntfTime = formatted;
                        else if (ntf && w.IsMiniWave) ntfMiniTime = formatted;
                        else if (chaos && !w.IsMiniWave) chaosTime = formatted;
                        else if (chaos && w.IsMiniWave) chaosMiniTime = formatted;
                    }
                }
            }
            catch (Exception ex)
            {
                FermixLog.Warn($"ServerHud.WaveTimer.Get: {ex.Message}");
            }

            string tip = _currentSpectatorTip ?? string.Empty;
            string text =
                "<b><size=18><color=#FFA500>Вы заспавнитесь через:</color>\n" +
                $"<color=#0000FF>МОГ</color>: {ntfTime}   " +
                $"<color=#5599FF>Подкр. МОГ</color>: {ntfMiniTime}\n" +
                $"<color=#00FF00>ПХ</color>: {chaosTime}   " +
                $"<color=#88EE88>Подкр. ПХ</color>: {chaosMiniTime}\n" +
                (string.IsNullOrEmpty(tip) ? string.Empty : $"<color=#cccccc><i>{tip}</i></color>") +
                "</size></b>";

            if (data.WaveTimer.Text != text) data.WaveTimer.Text = text;
        }

        // ── Helpers ────────────────────────────────────────────────────

        private static string PickSpectatorTip()
        {
            var tips = FermixCore.Config?.ServerHudSpectatorInfo;
            if (tips == null || tips.Count == 0) return string.Empty;
            return tips[_rand.Next(tips.Count)] ?? string.Empty;
        }

        private static string TranslateRole(RoleTypeId roleType, bool useColors)
        {
            // Перевод ролей на русский. Список покрывает все роли SCP:SL
            // 14.x (включая Flamingo / Filmmaker / Overwatch). Перенос из
            // Hazbin.Core.RoleTypeIdExtensions.TranslatedRoleType — для
            // FermixAPI как самодостаточная утилита, чтобы не тащить весь
            // Hazbin.Core.
            string ret;
            switch (roleType)
            {
                case RoleTypeId.ClassD:        ret = "<color=#ff9933>Класс D</color>"; break;
                case RoleTypeId.Scientist:     ret = "<color=#e7d573>Учёный</color>"; break;
                case RoleTypeId.FacilityGuard: ret = "<color=#808080>Охранник</color>"; break;
                case RoleTypeId.NtfPrivate:    ret = "<color=#3399ff>Рядовой МОГ</color>"; break;
                case RoleTypeId.NtfSergeant:   ret = "<color=#0066cc>Сержант МОГ</color>"; break;
                case RoleTypeId.NtfCaptain:    ret = "<color=#0047ab>Капитан МОГ</color>"; break;
                case RoleTypeId.NtfSpecialist: ret = "<color=#003366>Специалист МОГ</color>"; break;
                case RoleTypeId.Scp049:        ret = "<color=#ff0000>SCP-049</color>"; break;
                case RoleTypeId.Scp0492:       ret = "<color=#ff0000>SCP-049-2</color>"; break;
                case RoleTypeId.Scp096:        ret = "<color=#ff0000>SCP-096</color>"; break;
                case RoleTypeId.Scp106:        ret = "<color=#ff0000>SCP-106</color>"; break;
                case RoleTypeId.Scp173:        ret = "<color=#ff0000>SCP-173</color>"; break;
                case RoleTypeId.Scp939:        ret = "<color=#ff0000>SCP-939</color>"; break;
                case RoleTypeId.Scp079:        ret = "<color=#ff0000>SCP-079</color>"; break;
                case RoleTypeId.ChaosConscript:ret = "<color=#228B22>Солдат Хаоса</color>"; break;
                case RoleTypeId.ChaosRifleman: ret = "<color=#228B22>Стрелок Хаоса</color>"; break;
                case RoleTypeId.ChaosMarauder: ret = "<color=#228B22>Мародёр Хаоса</color>"; break;
                case RoleTypeId.ChaosRepressor:ret = "<color=#228B22>Усмиритель Хаоса</color>"; break;
                case RoleTypeId.Overwatch:     ret = "<color=#00bfff>Надзиратель</color>"; break;
                case RoleTypeId.Filmmaker:     ret = "<color=#000000>Режиссёр</color>"; break;
                case RoleTypeId.Flamingo:      ret = "<color=#ff96de>Фламинго</color>"; break;
                case RoleTypeId.AlphaFlamingo: ret = "<color=#ff1493>Альфа-Фламинго</color>"; break;
                case RoleTypeId.ZombieFlamingo:ret = "<color=#bfff00>Зомби-Фламинго</color>"; break;
                case RoleTypeId.Tutorial:      ret = "<color=#ff69b4>Обучение</color>"; break;
                case RoleTypeId.Spectator:     ret = "<color=#cccccc>Наблюдатель</color>"; break;
                case RoleTypeId.None:          ret = "<color=#ffffff>Никто</color>"; break;
                default:
                {
                    // SCP-3114 — RoleTypeId=23 в SCP:SL 14.x; для совместимости
                    // ниже идёт fallback на enum-строку.
                    if ((int)roleType == 23) { ret = "<color=#ff0000>SCP-3114</color>"; break; }
                    ret = $"<color=#ffffff>{roleType}</color>"; break;
                }
            }
            return useColors ? ret : System.Text.RegularExpressions.Regex.Replace(ret, "<[^<>]*>", string.Empty);
        }
    }
}
