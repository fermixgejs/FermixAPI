---
name: update-hint-engine
description: Как перетянуть свежую версию HintServiceMeow в Hints/.
---

# SKILL — Обновить hint-движок (HintServiceMeow upstream)

## Когда применять

- Появилась новая версия HintServiceMeow на GitHub.
- Нужно багфикс, который уже исправлен в upstream.
- Поднимается версия EXILED / LabAPI, и upstream HSM уже это
  поддерживает.

**Не** делай это превентивно — у нас закреплена конкретная версия
для стабильности (см. `vendor/HintServiceMeow/README.md`). Обновляй
только если есть конкретная причина.

## Шаги

### 1. Скачать новую версию upstream

```bash
mkdir -p /tmp/hsm-update
cd /tmp/hsm-update
wget https://github.com/MeowServer/HintServiceMeow/archive/refs/heads/main.zip
unzip main.zip
ls HintServiceMeow-main/HintServiceMeow/
```

Должны быть папки `Core/`, `UI/`, файл `TextWidth`, и (опционально)
`Plugin/`, `Properties/`, `HintServiceMeow.csproj` — последние мы
**игнорируем**.

### 2. Сделать diff с нашей версией

```bash
diff -r /tmp/hsm-update/HintServiceMeow-main/HintServiceMeow/Core/ \
        /home/ubuntu/repos/FERMIXAPI1/Hints/Core/ | head -80
```

Просмотри изменения. Если новый код добавляет:

- Новые файлы в `Plugin/` — игнорируй (у нас стаб-версии).
- Новые сlasses, использующие `HintServiceMeow.Plugin.Plugin.Instance`
  — будут использовать наш стаб, но проверь что наш `PluginConfig`
  имеет все нужные поля. Если нет — добавь в наш стаб.
- Новые `using HintServiceMeow.X` — нужно будет переименовать в
  `using FermixAPI.Hints.X`.

### 3. Скопировать новый код

```bash
rsync -av --delete \
    /tmp/hsm-update/HintServiceMeow-main/HintServiceMeow/Core/ \
    /home/ubuntu/repos/FERMIXAPI1/Hints/Core/
rsync -av --delete \
    /tmp/hsm-update/HintServiceMeow-main/HintServiceMeow/UI/ \
    /home/ubuntu/repos/FERMIXAPI1/Hints/UI/
cp /tmp/hsm-update/HintServiceMeow-main/HintServiceMeow/TextWidth \
    /home/ubuntu/repos/FERMIXAPI1/Hints/TextWidth
```

### 4. Переименовать namespaces

```bash
cd /home/ubuntu/repos/FERMIXAPI1/Hints
find . -name '*.cs' -exec sed -i 's/HintServiceMeow\./FermixAPI.Hints./g' {} +
```

### 5. Восстановить наши локальные правки

В файлах ниже мы заменили обращения к нативному SCP:SL `Hints` на
`global::Hints`, чтобы не конфликтовать с нашим `FermixAPI.Hints`.
Применить заново:

```bash
sed -i 's/typeof(Hints\./typeof(global::Hints./g; s/nameof(Hints\./nameof(global::Hints./g' \
    Core/Utilities/Patch/Patcher.cs

# Patches.cs — заменить `using Hints;` на `using global::Hints;`
sed -i 's|^    using Hints;|    using global::Hints;|' \
    Core/Utilities/Patch/Patches.cs

# ScpslDisplayOutput.cs — заменить любое `Hints.` на `global::Hints.`
# (если автозамена сверху не сработала)
```

### 6. Восстановить наши Plugin-стабы

`Hints/Plugin/Plugin.cs` и `Hints/Plugin/PluginConfig.cs` — наши
файлы, не из upstream. Если `rsync` их перетёр (он не должен, мы
синхронизируем `Core/` и `UI/`, не `Plugin/`) — восстанови из
`git diff`:

```bash
git checkout HEAD -- Hints/Plugin/Plugin.cs Hints/Plugin/PluginConfig.cs
```

### 7. Собрать

```bash
cd /home/ubuntu/repos/FERMIXAPI1
dotnet build -c Release 2>&1 | tail -30
```

Возможные ошибки и их фиксы:

- `error CS0246: type 'Hint' could not be found` →
  забыл `global::Hints.X` где-то. См. шаг 5.
- `error CS1069: ZipArchive forwarded` →
  нужны рефы `System.IO.Compression`/`System.IO.Compression.FileSystem`
  в csproj (они уже там, но если HSM добавил новый файл с zip-ом — ок).
- `error CS0103: The name 'Plugin' does not exist` →
  HSM добавил код, использующий `Plugin.Instance.Config.X`, где X —
  поле, которого нет в нашем стабе. Добавь поле в
  `Hints/Plugin/PluginConfig.cs`.

### 8. Обновить вендор-метаданные

Открой `vendor/HintServiceMeow/README.md` и обнови:

- Версия (см. `Plugin.cs::Version` в upstream или changelog)
- Дата интеграции (текущая)
- Список изменений от upstream (если что-то новое сломало и ты
  это патчил локально)

### 9. Проверь, что наш `FermixHintStack` всё ещё работает

Это самый важный пункт. Если HSM upstream сломал API
`PlayerDisplay.Get(...)`, `AddHint(hint, groupName)` или
`Hint.Text/Hide` — наш `FermixHintStack.RenderToPlayer` упадёт в
рантайме. Проверь сигнатуры этих методов в новом коде; при
необходимости подправь `Core/FermixHintStack.cs`.

### 10. Тэг и релиз

Минорный bump (`v2.4.0` или подобное), потому что обновляется
зависимость. Не патч-bump.

## Чек-лист

- [ ] Скачана конкретная версия (не master, а tagged release)
- [ ] Diff просмотрен
- [ ] `Core/` и `UI/` синхронизированы; `Plugin/` НЕ перетёрт
- [ ] Все `HintServiceMeow.` заменены на `FermixAPI.Hints.`
- [ ] Все `Hints.X` (нативные) → `global::Hints.X`
- [ ] `Hints/Plugin/Plugin.cs` и `PluginConfig.cs` на месте
- [ ] Если HSM добавил поля в `PluginConfig`, наш стаб их добавил
- [ ] `dotnet build -c Release` 0/0
- [ ] `vendor/HintServiceMeow/README.md` обновлён
- [ ] `FermixHintStack.RenderToPlayer` ещё валиден
