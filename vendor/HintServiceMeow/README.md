# HintServiceMeow — атрибуция

Каталог `Hints/` в этом репозитории содержит код, заимствованный
из открытого проекта **HintServiceMeow** (далее «HSM») и
переименованный из пространства имён `HintServiceMeow.*` в
`FermixAPI.Hints.*`. Лицензия — MIT, оригинал лицензии лежит рядом
в файле [`LICENSE`](LICENSE).

- Upstream: <https://github.com/MeowServer/HintServiceMeow>
- Версия, на которую опирался импорт: **HintServiceMeow 5.5.1**
- Дата интеграции: 2026-05
- Изменения, внесённые при импорте:
  - Удалены `Plugin/`, `Properties/`, `HintServiceMeow.csproj`
    (мы встраиваем код, а не используем как отдельный плагин).
  - В `Hints/Plugin/` оставлены минимальные стабы `Plugin` и
    `PluginConfig`, чтобы не править ссылки `Plugin.Instance.Config`
    в заимствованном коде (см. `Hints/Plugin/Plugin.cs`).
  - Все namespace'ы переименованы:
    `HintServiceMeow.X.Y` → `FermixAPI.Hints.X.Y`.
  - В `Patches.cs` / `Patcher.cs` / `ScpslDisplayOutput.cs`
    обращения к нативному пространству `Hints` заменены на
    `global::Hints`, чтобы не конфликтовать с нашим `FermixAPI.Hints`.
  - Жизненным циклом hint-движка управляет
    `FermixAPI.Core.FermixCore` (см. `OnWaitingForPlayers` /
    `OnPlayerLeft` / `Shutdown`), а не отдельный `Plugin.OnEnabled`.

Полный текст лицензии MIT см. в [`LICENSE`](LICENSE). Все права на
оригинальный код принадлежат MeowServer~. Эта атрибуция нужна, чтобы
соблюсти условие MIT «include the above copyright notice and this
permission notice in all copies».
