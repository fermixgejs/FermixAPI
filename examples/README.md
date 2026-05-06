# Примеры использования FermixAPI

Все файлы в этой папке — справочные сэмплы. Они **не** компилируются в `FermixAPI.dll`
(в `FermixAPI.csproj` стоит `<Compile Remove="examples/**" />`). Скопируй нужный
файл в свой плагин и используй как шаблон.

## Структура

| Файл | Что показывает |
|---|---|
| [ExamplePlugin.cs](ExamplePlugin.cs) | Скелет плагина на FermixAPI: `OnEnabled`/`OnDisabled`, регистрация подписок, доступ к Singleton'у |
| [ExampleConfig.cs](ExampleConfig.cs) | Конфиг плагина (`IConfig`) + `FermixConfigUtils` для дополнительных YAML файлов |
| [ExampleEvents.cs](ExampleEvents.cs) | Подписка на `FermixEvents` — `OnPlayerJoin`, `OnRoundStart`, `OnPlayerDied` и т.д. |
| [ExampleHints.cs](ExampleHints.cs) | `FermixHint`: одиночные хинты, цвета, прогресс-бар, многострочные |
| [ExampleHintStack.cs](ExampleHintStack.cs) | `FermixHintStack`: приоритетные/persistent/dynamic хинты, индикаторы HP и патронов |
| [ExampleCommand.cs](ExampleCommand.cs) | Свои команды — клиентская и админская, с argparse |
| [ExampleGlow.cs](ExampleGlow.cs) | `FermixGlow`: статическая, пульсирующая и радужная подсветка предметов |
| [ExampleInput.cs](ExampleInput.cs) | `FermixInput`: бинды SSS (LMB/R/Alt), регистрация своего бинда, обработчики |
| [ExampleScheduler.cs](ExampleScheduler.cs) | `FermixScheduler`: задержки, повторяющиеся таймеры, отмена задач |
| [ExampleData.cs](ExampleData.cs) | `FermixData`: JSON/binary/text сохранения, статистика игроков |
| [ExampleExtensions.cs](ExampleExtensions.cs) | Полезные расширения `Player`/`Map` (раскраска, телепорт, поиск ближайших и т.д.) |

## Минимальный плагин

Все примеры пишутся внутри обычного EXILED-плагина, который ссылается на `FermixAPI.dll`.
Минимум нужно:

```csharp
public sealed class MyPlugin : Plugin<MyConfig>
{
    public override string Name => "MyPlugin";
    public override string Author => "you";

    public override void OnEnabled()
    {
        // FermixAPI должен быть загружен раньше нас (положи FermixAPI.dll в Plugins/).
        FermixCore.EnsureInitialized();

        // Дальше — твой код.
        base.OnEnabled();
    }
}
```

## Что есть на чём построено

```
FermixCore           ← ядро, инициализируется автоматически из Plugin.OnEnabled
├─ FermixPaths       ← папки {EXILED}/Configs/FermixAPI/{Data,Plugins,Logs}
├─ FermixEvents      ← один универсальный hub для всех событий EXILED
├─ FermixScheduler   ← Delay/Repeat/Cancel поверх MEC
├─ FermixHint        ← плоские helper'ы (под капотом — FermixHintStack)
├─ FermixHintStack   ← стэкуемые хинты (приоритеты, категории, dynamic/persistent)
├─ FermixLog         ← цветной лог
├─ FermixData        ← JSON / бинарные / текстовые файлы
├─ FermixConfigUtils ← YAML-конфиги
├─ Systems.FermixGlow    ← кастомная подсветка предметов
├─ Systems.FermixInput   ← SSS-биндинги (LMB/RMB/R/Alt/Q/F/T + кастомные)
└─ Commands.*        ← готовые команды (.tps .rt .kill .res .weaponswap)
```
