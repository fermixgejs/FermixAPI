# vendor/

Копии исходников зависимостей FermixAPI — **только для справки**.
Сборку FermixAPI эта папка не использует — компилятор берёт скомпилированные
DLL из `refs/`. Если хочешь обновить версию зависимости — обнови DLL в `refs/`,
не здесь.

| Папка | Источник | Версия |
| --- | --- | --- |
| [`EXILED/`](EXILED/) | https://github.com/ExMod-Team/EXILED | **9.13.3** |
| [`LabAPI/`](LabAPI/) | https://github.com/northwood-studios/LabAPI | **1.1.6** |

## Зачем это здесь

Чтобы не лазить в чужие репо: рядом с твоим кодом FermixAPI лежат:

* `vendor/EXILED/EXILED/Exiled.API/Features/Player.cs` — все методы Player из EXILED.
* `vendor/EXILED/EXILED/Exiled.API/Features/Round.cs` — Round.
* `vendor/EXILED/EXILED/Exiled.API/Features/Cassie.cs` — CASSIE.
* `vendor/LabAPI/LabApi/Features/Wrappers/Player.cs` — Player из LabAPI.
* и т.д.

Это особенно полезно, когда AI-аналитик утверждает, что какой-то метод EXILED
не существует — можно сразу проверить grep'ом по `vendor/` без выхода из репо.

## Как обновить

```bash
# EXILED
cd vendor && rm -rf EXILED
git clone --depth=1 --branch v9.13.4 https://github.com/ExMod-Team/EXILED.git EXILED
rm -rf EXILED/.git EXILED/.github EXILED/.gitlab

# LabAPI
rm -rf LabAPI
git clone --depth=1 --branch 1.1.7 https://github.com/northwood-studios/LabAPI.git LabAPI
rm -rf LabAPI/.git LabAPI/.github LabAPI/.gitlab
```

После этого обнови `refs/Exiled.API.dll` / `refs/LabApi.dll` соответствующими DLL
из релизов — и не забудь поправить `refs/README` / FermixAPI `<Description>`.

## Лицензии

См. `EXILED/LICENSE` и `LabAPI/LICENSE`.
