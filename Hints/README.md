# `Hints/` — встроенный hint-движок FermixAPI

Этот каталог — встроенный движок отображения хинтов на экране игрока.
Он происходит из проекта [HintServiceMeow](https://github.com/MeowServer/HintServiceMeow)
(MIT, см. [`vendor/HintServiceMeow/`](../vendor/HintServiceMeow/README.md)),
но включён в FermixAPI как первоклассная часть API: namespace'ы
переименованы в `FermixAPI.Hints.*`, и публичная точка входа для
плагинов-потребителей — обычный `FermixAPI.FermixHint`.

## Зачем нам свой hint-движок

Стандартный `Exiled.API.Features.Player.ShowHint(text, dur)` отрисовывает
сообщение через стандартный SCP:SL-канал. Этот канал поддерживает только
**один активный хинт на игрока**: следующий вызов перезаписывает
предыдущий. Если на сервере одновременно работают, скажем,
RespawnTimer и FermixCoin — каждый из них пушит свой хинт в один и тот
же слот, и побеждает тот, кто вызвал `ShowHint` последним.

HintServiceMeow решает эту проблему через Harmony-патчи на
`Player.ShowHint` / `LabApi.Player.SendHint`: все вызовы перехватываются
и кооперативно собираются в одном пайплайне (`PlayerDisplay`), который
сам формирует итоговую картинку и шлёт её через `Hints.HintMessage`.

Поэтому:

- Если поставить рядом с FermixAPI плагин, который умеет HSM, — он
  автоматически делит экран с нашим хинт-стеком.
- Если плагин **не** знает о HSM и просто вызывает `player.ShowHint`,
  наш Harmony-патч (через `CompatibilityAdapter`) вытащит из вызова
  текст и подложит его в общий пайплайн как ещё один хинт.

## Структура

| Подкаталог | Назначение |
| --- | --- |
| `Core/` | Сам движок: `PlayerDisplay`, `HintCollection`, парсинг RichText, планировщик задач, аплоад-выход в Mirror и т.п. |
| `Core/Utilities/Patch/` | Harmony-патчи, перехватывающие нативные `ShowHint` / `SendHint`. |
| `Core/Utilities/Tools/FontTool.cs` | Считает ширины символов TextMeshPro. Грузит таблицу из embedded resource `FermixAPI.Hints.TextWidth`. |
| `UI/Utilities/` | Помощники для CommonHint API (мы их не используем напрямую, оставлены для совместимости с заимствованным кодом). |
| `Plugin/` | Минимальные стабы `Plugin` и `PluginConfig` (см. ниже). Не путать с EXILED-плагином FermixAPI. |
| `TextWidth` | Бинарный ZIP с таблицей ширин символов; включается в FermixAPI.dll как embedded resource. |

## Точки расширения и интеграции

| Где | Что делается |
| --- | --- |
| [`Core/FermixCore.cs::OnWaitingForPlayers`](../Core/FermixCore.cs) | Вызывает `Patcher.Patch()`, чтобы Harmony перехватил `Player.ShowHint`. |
| [`Core/FermixCore.cs::OnPlayerLeft`](../Core/FermixCore.cs) | Вызывает `PlayerDisplay.Destruct(hub)` для освобождения per-player состояния. |
| [`Core/FermixCore.cs::Shutdown`](../Core/FermixCore.cs) | Вызывает `Patcher.Unpatch()` при выгрузке плагина. |
| [`Core/FermixHintStack.cs::RenderToPlayer`](../Core/FermixHintStack.cs) | Создаёт ровно один `Hints/Core/Models/Hints/Hint` на игрока и обновляет его `.Text` / `.Hide` (вместо `player.ShowHint`). |

`Hints/Plugin/Plugin.cs` и `PluginConfig.cs` — это **не** EXILED-плагин,
а простой синглтон-контейнер для конфига, нужный заимствованному коду
(`Plugin.Instance.Config.X`). Реальный жизненный цикл движка
управляется из `FermixCore`.

## Если нужно обновить движок

1. Скачать новый релиз HintServiceMeow с GitHub.
2. Распаковать в `/tmp/hsm/`.
3. `rsync -a /tmp/hsm/HintServiceMeow-main/HintServiceMeow/Core/ Hints/Core/`
   (и аналогично для `UI/`, `TextWidth`). **Не** копировать `Plugin/`,
   `Properties/`, `HintServiceMeow.csproj`.
4. `find Hints/ -name '*.cs' -exec sed -i 's/HintServiceMeow\./FermixAPI.Hints./g' {} +`
5. В `Patches.cs`, `Patcher.cs`, `ScpslDisplayOutput.cs` — заменить
   обращения к нативному `Hints.X` на `global::Hints.X` (иначе
   компилятор ловит наш `FermixAPI.Hints` и падает с CS0246).
6. Обновить версию и дату в [`vendor/HintServiceMeow/README.md`](../vendor/HintServiceMeow/README.md).
7. Собрать `dotnet build -c Release` — должно быть 0 errors / 0 warnings.

## Лицензия

Код в этом каталоге лицензирован по MIT, как и оригинальный
HintServiceMeow. Полный текст — в [`vendor/HintServiceMeow/LICENSE`](../vendor/HintServiceMeow/LICENSE).
