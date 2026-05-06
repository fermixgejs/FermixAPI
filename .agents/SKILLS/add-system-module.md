---
name: add-system-module
description: Шаблон для добавления нового публичного модуля в FermixAPI.Systems.
---

# SKILL — Добавить новый System-модуль в FermixAPI

## Когда применять

Пользователь просит «сделай в FermixAPI помощник для X», где X —
какая-то область игровой логики (двери, гранаты, эффекты, спавн
SCP, что угодно). Если такого помощника ещё нет — заводим его как
новый System-модуль в `Systems/`.

## Шаги

### 1. Решить — это static helper или stateful manager?

| Признак | Что выбрать |
| --- | --- |
| Просто набор удобных методов поверх EXILED API | static class в `Systems/Fermix<X>.cs` |
| Состояние, которое нужно подписывать на события | `Initialize()`/`Shutdown()` + статическое поле `_handlers` |
| Состояние per-player | `Dictionary<Player, T>` + cleanup на Player.Left |

Большинство наших модулей — `static class` с
`Initialize`/`Shutdown` (см. `FermixGlow`, `FermixInput`).

### 2. Скелет файла

```csharp
using System;
using System.Collections.Generic;
using Exiled.API.Features;
using FermixAPI.Core;
using FermixAPI.Utils;
using Handlers = Exiled.Events.Handlers;

namespace FermixAPI.Systems
{
    /// <summary>
    /// Описание модуля одной фразой.
    /// </summary>
    public static class FermixDoors
    {
        private static bool _initialized;

        /// <summary>Инициализация — вызывается из FermixCore.</summary>
        public static void Initialize()
        {
            if (_initialized) return;
            Handlers.Player.InteractingDoor += OnInteractingDoor;
            _initialized = true;
            FermixLog.Info("FermixDoors инициализирован.");
        }

        /// <summary>Снимает все подписки.</summary>
        public static void Shutdown()
        {
            if (!_initialized) return;
            Handlers.Player.InteractingDoor -= OnInteractingDoor;
            _initialized = false;
        }

        // === Public API ===

        /// <summary>
        /// Открывает все двери в комнате игрока на N секунд.
        /// </summary>
        public static void OpenAllInRoom(Player player, float duration)
        {
            if (player?.CurrentRoom == null) return;
            foreach (var door in Door.List)
            {
                if (door.Room == player.CurrentRoom)
                {
                    door.IsOpen = true;
                    FermixScheduler.Delay(duration, () => { if (door != null) door.IsOpen = false; });
                }
            }
        }

        // === Internals ===

        private static void OnInteractingDoor(Exiled.Events.EventArgs.Player.InteractingDoorEventArgs ev)
        {
            // ... обработка события
        }
    }
}
```

### 3. Зарегистрировать в FermixCore

Открыть `Core/FermixCore.cs`, найти блок
`Systems.FermixGlow.Initialize()` в `Initialize()` и
`Systems.FermixGlow.Shutdown()` в `Shutdown()`. Добавить рядом:

```csharp
// В Initialize:
Systems.FermixGlow.Initialize();
Systems.FermixDoors.Initialize();   // ← новый

// В Shutdown:
Systems.FermixGlow.Shutdown();
Systems.FermixDoors.Shutdown();      // ← новый
```

### 4. Документ публичного API

В корневом `README.md` найди таблицу системных модулей и добавь
строку для нового. Если есть нетривиальные сценарии —
сделай файл-пример в `examples/Example<X>.cs` (он не входит в
DLL, см. `<Compile Remove="examples/**" />` в csproj).

### 5. Сборка

```bash
dotnet build -c Release
```

0/0 предупреждений / ошибок. Если warning — почини, не подавляй
без согласования.

### 6. Если модуль работает с per-player состоянием

Добавь cleanup на `Player.Left` (через `OnPlayerLeft` handler в
самом модуле, либо хук в `FermixCore.OnPlayerLeft`). Иначе при
disconnect'е игрока остаются висящие ссылки → memory leak.

### 7. Если модуль использует Harmony-патчи

См. как сделано в `Hints/Core/Utilities/Patch/Patcher.cs`. Главное —
не патчить в `Initialize` (плагины ещё подгружаются), а на
`WaitingForPlayers`. Снимать патчи в `Shutdown`.

## Чек-лист

- [ ] Файл создан в `Systems/Fermix<X>.cs`
- [ ] Namespace — `FermixAPI.Systems`
- [ ] Есть `Initialize()` / `Shutdown()` со флагом `_initialized`
- [ ] Подписки и отписки на события сбалансированы
- [ ] Нет `player.ShowHint(...)` напрямую — через `FermixHint`
- [ ] Нет `Timing.CallDelayed(...)` — через `FermixScheduler`
- [ ] Зарегистрирован в `FermixCore.Initialize` / `Shutdown`
- [ ] Добавлен в `README.md` таблицу модулей
- [ ] Если есть state-per-player — есть cleanup на `Player.Left`
- [ ] `dotnet build` 0/0
