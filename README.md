# FermixAPI

Мощная библиотека для быстрой разработки плагинов SCP:SL на Exiled с интеграцией LabAPI.

## Поддерживаемые версии фреймворков

| Зависимость | Версия |
| --- | --- |
| EXILED  | **9.13.3** ([ExMod-Team/EXILED](https://github.com/ExMod-Team/EXILED)) |
| LabAPI  | **1.1.6** ([northwood-studios/LabAPI](https://github.com/northwood-studios/LabAPI)) |
| .NET    | net48 |

Минимальная требуемая версия EXILED проверяется самим фреймворком EXILED через
`Plugin.RequiredExiledVersion`. Версия LabAPI определяется в рантайме —
если LabAPI ниже рекомендуемой версии, FermixAPI выведет предупреждение
в лог и продолжит работу в режиме совместимости.

## Установка

**Самый простой способ — скачать готовый билд:**

1. Открой страницу [Releases](../../releases) и в последнем релизе скачай
   `FermixAPI.dll` (можно ещё `.pdb` — символы для отладки).
2. Положи `FermixAPI.dll` в папку `EXILED/Plugins/`.
3. Перезапусти сервер.

Каждый push тэга вида `vX.Y.Z` собирает релиз автоматически (см.
`.github/workflows/build.yml`) — DLL сразу появляется на вкладке Releases.

Альтернатива — собрать самому: см. [«Сборка»](#сборка).

## Примеры

В папке [`examples/`](examples/) лежат файлы-шаблоны на каждый модуль FermixAPI:
плагин, события, хинты, стэк хинтов, команды, glow, SSS-input, scheduler,
работа с данными/конфигом, расширения Player. Смотри [examples/README.md](examples/README.md).

## Зависимости в репозитории

Исходники EXILED 9.13.3 и LabAPI 1.1.6 включены прямо в репозиторий:

* `vendor/EXILED/` — копия `ExMod-Team/EXILED@v9.13.3`.
* `vendor/LabAPI/` — копия `northwood-studios/LabAPI@1.1.6`.

Это позволяет всегда видеть исходники зависимостей (и их `Player.cs`,
`Round.cs`, `Cassie.cs` и т.д.) прямо в одном репо. **Сборку FermixAPI
эти папки не используют** — компилятор по-прежнему берёт скомпилированные
DLL из `refs/`. `vendor/` — только для справки/чтения исходников.

## Сборка

FermixAPI зависит от сборок SCP:SL, EXILED и LabAPI. Их нужно положить
в папку `refs/` рядом с проектом.

```bash
# 1) Скачать EXILED + LabAPI релизы:
bash scripts/fetch-references.sh v9.13.3 1.1.6

# 2) Скопировать DLL'ки из SCPSL_Data/Managed/ в refs/
#    (Assembly-CSharp.dll, Assembly-CSharp-firstpass.dll, CommandSystem.Core.dll,
#     Mirror.dll, PluginAPI.dll, NorthwoodLib.dll, Pooling.dll, UnityEngine*.dll)

# 3) Сборка
dotnet build -c Release
```

Готовый `FermixAPI.dll` появится в `bin/Release/net48/`.

## Структура проекта

```
FermixAPI/
├── Core/
│   ├── FermixCore.cs       # Ядро API и статический доступ
│   ├── FermixEvents.cs     # Система событий с обертками
│   ├── FermixLog.cs        # Расширенное логирование
│   ├── FermixScheduler.cs  # Таймеры и отложенные действия
│   ├── FermixHintStack.cs  # Стек хинтов с приоритетами
│   └── FermixPaths.cs      # Авто-создание папок плагина
├── Extensions/
│   ├── PlayerExtensions.cs # 70+ методов расширения для Player
│   └── FermixHint.cs       # Система подсказок и UI
├── Systems/
│   ├── FermixDoors.cs      # Управление дверями
│   ├── FermixWarhead.cs    # Управление боеголовкой
│   ├── FermixItems.cs      # Управление предметами
│   ├── FermixRooms.cs      # Управление комнатами
│   ├── FermixCams.cs       # Управление камерами
│   ├── FermixScp.cs        # Управление SCP
│   ├── FermixServer.cs     # Управление сервером и CASSIE
│   ├── FermixRound.cs      # Управление раундом
│   ├── FermixRoles.cs      # Управление ролями
│   ├── FermixGlow.cs       # Кастомное свечение pickup'ов
│   └── FermixInput.cs      # SSS-биндинги клавиш
├── Commands/
│   ├── TpsCommand.cs       # .tps
│   ├── RoundTimeCommand.cs # .rt
│   ├── SuicideCommand.cs   # .kill
│   ├── ResurrectCommand.cs # .res
│   └── WeaponSwapCommand.cs# .weaponswap
├── Integration/
│   └── LabApiIntegration.cs # Интеграция с LabAPI
├── Utils/
│   ├── FermixUtils.cs      # Общие утилиты
│   ├── FermixConfig.cs     # Работа с конфигурациями
│   └── FermixData.cs       # Хранение данных
├── Plugin.cs               # Точка входа плагина
└── Config.cs               # Конфигурация плагина
```

Все классы из `Systems/` лежат в namespace `FermixAPI.Systems`.
Расширения Player и базовые типы — в namespace `FermixAPI`.

## Быстрый старт

### Базовое использование

```csharp
using FermixAPI;
using FermixAPI.Core;
using FermixAPI.Systems;

// Получить всех живых игроков
var alivePlayers = FermixCore.AlivePlayers;

// Показать подсказку игроку
player.Hint("Добро пожаловать!", 5);

// Исцелить игрока до максимума
player.FullHeal();

// Телепортировать игрока
player.TeleportTo(RoomType.LczArmory);

// Выдать предмет
player.Give(ItemType.GunE11SR);
```

### Работа с событиями

```csharp
// Подписка на событие смерти. Все события используют DiedEventArgs / т.п.
FermixEvents.OnPlayerDied += args =>
{
    var attacker = args.Attacker?.Nickname ?? "сервер";
    FermixLog.Info($"{args.Player.Nickname} был убит {attacker}");
};

// Начало раунда
FermixEvents.OnRoundStart += () =>
{
    FermixServer.GlobalBroadcast("Раунд начался!", 5);
};
```

### Таймеры и отложенные действия

```csharp
// Отложенное действие
FermixScheduler.Delay(5f, () =>
{
    FermixLog.Info("Прошло 5 секунд!");
});

// Повторяющийся таймер
var handle = FermixScheduler.Repeat(10f, () =>
{
    FermixServer.GlobalBroadcast("Напоминание каждые 10 секунд");
});

// Остановить таймер
FermixScheduler.Cancel(handle);

// Отсчёт с callback каждую секунду
FermixScheduler.Countdown(10f,
    onTick: remaining => FermixLog.Info($"{remaining:F0}..."),
    onComplete: () => FermixLog.Info("Время вышло"));

// Ждать условие, потом выполнить
FermixScheduler.WaitUntil(
    condition: () => Round.IsStarted,
    action: () => FermixLog.Info("Раунд стартовал"));
```

### Работа с дверями

```csharp
// Открыть все двери в зоне
FermixDoors.OpenZone(ZoneType.LightContainment);

// Заблокировать конкретную дверь (extension на Door)
foreach (var door in Door.List.Where(d => d.Type == DoorType.GateA))
    door.Lock(DoorLockType.AdminCommand);

// Заблокировать все двери в зоне
FermixDoors.LockZone(ZoneType.HeavyContainment);

// Открыть/закрыть все чекпоинты
FermixDoors.ControlCheckpoints(open: true);

// Найти ближайшую дверь к позиции
var nearestDoor = FermixDoors.GetNearest(player.Position);
```

### Работа с предметами

```csharp
// Создать предмет на позиции (Pickup сразу спавнится на карте)
FermixItems.Spawn(ItemType.KeycardO5, player.Position);

// Создать несколько предметов
FermixItems.SpawnMultiple(ItemType.Medkit, player.Position, 5);

// Найти все pickup'ы определённого типа
var medkits = FermixItems.GetAll(ItemType.Medkit);

// Ближайший pickup к игроку
var nearest = FermixItems.GetNearest(player);
```

### Работа с комнатами

```csharp
// Выключить свет в зоне
FermixRooms.TurnOffLightsInZone(ZoneType.HeavyContainment, 30f);

// Случайная комната в зоне
var room = FermixRooms.GetRandom(ZoneType.Entrance);

// Ближайшая комната к позиции
var nearestRoom = FermixRooms.GetNearest(player.Position);
```

### Хранение данных

```csharp
// Хранилище данных игроков
var playerData = new PlayerDataStore<PlayerStats>("player_stats");

// Получить данные
var stats = playerData.Get(player);
stats.Kills++;

// Данные автоматически сохраняются

// Временные данные с истечением
var cooldowns = new ExpiringDataStore<string, bool>(TimeSpan.FromSeconds(30));
cooldowns.Set(player.UserId, true);

if (cooldowns.Has(player.UserId))
{
    player.Hint("Кулдаун ещё не прошёл!");
}
```

### Интеграция с LabAPI

```csharp
// Проверить доступность LabAPI
if (LabApiIntegration.IsAvailable)
{
    // Условное выполнение
    LabApiIntegration.IfAvailableOrElse(
        () => { /* код с LabAPI */ },
        () => { /* альтернативный код */ });

    // Получить значение свойства через рефлексию
    var ver = LabApiIntegration.GetReportedLabApiVersion();
}

// Регистрация кастомной команды через LabApi-обёртку
LabApiCommands.Register("hello", (player, args) =>
{
    player.Hint($"Hello, {player.Nickname}!");
});
```

### CASSIE

```csharp
FermixServer.CassieMessage("INTRUDER ALERT");
FermixServer.CassieMessageTranslated("PITCH_BLACK", "Pitch black");
FermixServer.CassieDelayedMessage("ALPHA WARHEAD DETONATED", 2f);
FermixServer.CassieGlitchyMessage("WARNING", glitchChance: 0.2f, jamChance: 0.1f);
FermixServer.CassieClear();
```

### Стек хинтов

```csharp
// Простой хинт (push в стек на duration секунд)
player.Hint("Попадание!", 1f);

// Стэковый хинт с приоритетом и id (заменяет хинт с тем же id)
FermixHint.ShowStacked(player, "Очки: 5", duration: 5f, priority: 1, id: "score");

// Динамический хинт (текст пересчитывается каждый тик)
FermixHint.ShowDynamic(player,
    updateFunction: p => $"HP: {p.Health:F0}/{p.MaxHealth:F0}",
    duration: 5f, updateInterval: 1f, id: "hp");

// Персистентный хинт (пока не удалить вручную)
FermixHint.ShowPersistent(player, "Найди карту O5", id: "objective");

// Удалить из стека
FermixHint.RemoveStacked(player, "score");
FermixHint.ClearStacked(player);

// Утилиты форматирования
string label = FermixHint.Bold(FermixHint.Color("Bonus", "#00FF00"));
string bar   = FermixHint.Center("=== HEADER ===");
```

### Кастомное свечение

```csharp
// Покрасить любой pickup, проходящий проверку по serial
string glowId = FermixGlow.AddGlowHex(
    itemCheck: serial => Pickup.Get(serial)?.Type == ItemType.KeycardO5,
    hexColor: "#00FFFF",
    intensity: 3f);

// Пульсирующее свечение
FermixGlow.AddPulsingGlow(
    itemCheck: serial => Pickup.Get(serial)?.Type == ItemType.Medkit,
    hexColor: "#FF00FF");

// Радужное свечение
FermixGlow.AddRainbowGlow(
    itemCheck: serial => Pickup.Get(serial)?.Type == ItemType.Adrenaline);

// Снять конкретное свечение
FermixGlow.RemoveGlow(glowId);
```

### SSS-биндинги клавиш

```csharp
// Доступные buttonId: FermixInput.LMB / RMB / R / Alt / Q / F / T

// На нажатие
FermixInput.RegisterPressedHandler(FermixInput.Q, player =>
{
    player.Hint("Q нажата!");
});

// На отпускание
FermixInput.RegisterReleasedHandler(FermixInput.LMB, player =>
{
    player.Hint("ЛКМ отпущена");
});

// Пока удерживается (пока true в IsHeld)
FermixInput.RegisterHeldHandler(FermixInput.F, player =>
{
    player.Hint("F удерживается");
});

// Кастомный бинд (берёт следующий свободный id)
var bind = FermixInput.RegisterCustomKeybind(
    id: 100,
    label: "Custom Skill",
    defaultKey: KeyCode.G,
    description: "Активирует скилл");
```

## Extension методы для Player

Все extensions лежат в namespace `FermixAPI` и `FermixAPI.Systems`,
поэтому достаточно `using FermixAPI;` + `using FermixAPI.Systems;`.

```csharp
// Здоровье
player.FullHeal();
player.SetHealth(75);
player.AddHealth(50);
player.SetMaxHealth(150);

// Урон
player.Damage(25f);
player.Kill("Причина смерти");

// Роль
player.SetRole(RoleTypeId.NtfCaptain);
player.SetRoleKeepInventory(RoleTypeId.NtfPrivate);

// Телепортация
player.TeleportTo(RoomType.HczArmory);
player.TeleportTo(otherPlayer);
player.TeleportTo(new Vector3(0, 1000, 0));

// Эффекты
player.ApplyEffect(EffectType.Scp207, duration: 30, intensity: 1);
player.RemoveEffect(EffectType.Scp207);
player.ClearEffects();
player.Blind(duration: 5f);
player.Bleed(duration: 10f);
player.Poison(duration: 5f);
player.Burn(duration: 3f);
player.Stun(duration: 2f);
player.Cloak(duration: 8f);

// Предметы
player.Give(ItemType.GunE11SR);
player.Give(ItemType.Medkit, ItemType.Adrenaline);
player.ClearInventory();
player.DropAll();
player.HasItem(ItemType.KeycardO5);
player.RemoveItem(ItemType.KeycardO5);
player.GiveAllAmmo(amount: 200);

// Хинты
player.Hint("Текст", 5);
player.SuccessHint("Успех!");
player.ErrorHint("Ошибка!");
player.WarningHint("Внимание!");
player.InfoHint("Инфо");
player.ColorHint("Цветной", color: "#FF00FF", duration: 5);

// Консоль клиента
player.Console("Лог", color: "yellow");

// Очистка broadcast'ов
player.ClearBroadcasts();

// Проверки
player.IsScp();
player.IsHuman();
player.IsSide(Side.Mtf);
player.IsTeam(Team.FoundationForces);
player.IsFaction(Faction.FoundationStaff);
player.IsAdmin();
player.IsAlive();
player.IsDead();

// Позиция
player.DistanceTo(otherPlayer);
player.DistanceTo(position);
player.IsInRange(position, 10f);
player.IsInRange(otherPlayer, 5f);

// Прочее
player.Freeze(duration: 5f);
player.Unfreeze();
player.SetSpeed(intensity: 5);
player.SetInvisible(invisible: true, duration: 10f);
player.Cuff(otherPlayer);   // через EXILED Handcuff под капотом
```

## Конфигурация

```yaml
# config.yml
is_enabled: true
debug: false
show_logo: true
show_dependency_info: true
auto_integrate_hsm: true
auto_integrate_lab_api: true
log_all_actions: false
max_scheduled_tasks: 100
```

См. [`Config.cs`](Config.cs) — описание всех опций.

## Лицензия

MIT License
