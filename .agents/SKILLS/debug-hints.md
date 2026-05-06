---
name: debug-hints
description: Чек-лист для дебага, когда хинты не показываются на сервере.
---

# SKILL — Дебаг hint-системы

## Симптомы, на которые применять

- «Хинты вообще не появляются».
- «Хинты появляются на полсекунды и пропадают».
- «На моём сервере с другим плагином — наши хинты пустые».
- «У одного игрока хинты есть, у другого нет».

## Шаг 1 — Проверь, что Patcher применился

В логе сервера (`EXILED/Logs/...`) должно быть:

```
[Info] [FermixAPI] Применить Harmony-патчи hint-движка ... ok
```

Если строки нет — Harmony-патч не применился. Возможные причины:

- `0Harmony.dll` отсутствует в `EXILED/dependencies/` (EXILED грузит
  его сам, если плагин подгружается раньше — мы можем не успеть).
  Проверка: положить `0Harmony.dll` в `EXILED/dependencies/` руками.
- Версия Harmony несовместима с EXILED 9.13.3. Проверка: брать
  тот же Harmony, который EXILED ставит сам (см. `refs/0Harmony.dll`).

## Шаг 2 — Проверь, что наша HSM не конфликтует с другой

Если на сервере параллельно стоит **отдельный** HintServiceMeow.dll
(например, остался от старого плагина), они оба патчат один и тот же
метод и тогда побеждает тот, кто пропатчил первым.

Проверка:

```bash
ls EXILED/Plugins/ | grep -i hint
# Не должно быть HintServiceMeow.dll рядом с FermixAPI.dll
```

Если есть — попроси пользователя удалить отдельный `HintServiceMeow.dll`,
у нас он встроен в `FermixAPI.dll`.

## Шаг 3 — Проверь FermixHintStack

В `Core/FermixHintStack.cs` есть тиковая корутина (раз в 0.5 с)
`Tick()`, которая собирает коллекцию хинтов и пушит в HSM
PlayerDisplay. Если её нет в логе — проверь, что
`FermixHintStack.IsInitialized == true` через debug-команду или
просто через `FermixCore.IsHintEnginePatched`.

## Шаг 4 — Проверь, что вызывается именно `FermixHint.Send`

Добавь временный лог:

```csharp
public static void Send(Player player, string message, float duration = 5f)
{
    Log.Info($"[HintTrace] Send -> {player?.Nickname}: {message}");
    FermixHintStack.ShowHint(player, message, duration, category: HintCategory.Custom);
}
```

Собери, дай пользователю поставить — он подкинет монетку — и
посмотри в логе, появляется ли строка `[HintTrace] Send -> ...`. Если
**не появляется** — значит, виновата логика плагина-вызывалки, а не
hint-движок. Если появляется, но хинт не виден — лезь в HSM.

## Шаг 5 — Проверь PlayerDisplay напрямую

Через консольный inject (или временный `.fermixtest`-команда):

```csharp
var hub = player.ReferenceHub;
var pd = FermixAPI.Hints.Core.Utilities.PlayerDisplay.Get(hub);
pd.AddHint(new FermixAPI.Hints.Core.Models.Hints.Hint
{
    Text = "ТЕСТ HSM НАПРЯМУЮ",
    YCoordinate = 500f,
    Alignment = FermixAPI.Hints.Core.Enum.HintAlignment.Center,
});
```

Если **этот** хинт не появился — проблема в HSM-движке (рендер /
mirror connection не готов). Если появился — проблема в
`FermixHintStack`-обвязке (стэкинг, экспирейшен, RichText).

## Шаг 6 — RichText / парсер ломается

Если хинт показывается, но без цвета / выглядит как сырой текст —
парсер RichText в `Hints/Core/Utilities/Parser/RichTextParser.cs`
упал. Чаще всего из-за:

- Несбалансированных тегов (`<color=red>` без `</color>`).
- Размер шрифта не задан (`<size=>`).
- Цвет в формате, который парсер не распознаёт.

Проверь, что строка хинта не «битая». Если она формируется
динамически — добавь Log.Debug перед `FermixHint.Send`.

## Шаг 7 — Mirror connection ещё не готов

Если хинт пытается уйти игроку, который только что заспавнился, и
его `NetworkConnection` ещё не `isReady` — пакет молча дропается.
HSM это учитывает в `ScpslDisplayOutput.ShowHint` (early-return при
not ready). Но если ты вызываешь `FermixHint.Send` сразу в
`Player.Spawned` — попробуй обернуть в
`FermixScheduler.Delay(0.5f, ...)`.

## Шаг 8 — Если всё перепробовал и не помогло

Эскалируй пользователю с **полным** stack trace из лога сервера и
**списком плагинов** в `EXILED/Plugins/`. Это критическая
информация. Не делай tag нового релиза, пока не починили.
