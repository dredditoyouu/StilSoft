# StilsoftIRS

WinForms MDI-приложение для управления инцидентами информационной безопасности на базе .NET Framework 4.7.2 и Microsoft SQL Server 2019.

## Установка на чистый компьютер

Двойной щелчок на `Установка.cmd` — скрипт сам:

1. Проверит наличие .NET Framework 4.7.2 (входит в Windows 10/11).
2. Установит **SQL Server LocalDB** через winget, если не найден.
3. Соберёт приложение (установит .NET SDK 8.0 если нужно).
4. Создаст и заполнит базу данных.
5. Создаст ярлык на рабочем столе.
6. Запустит программу.

Учётные записи по умолчанию: `admin`, `operator`, `analyst` — пароль **1**.

## Структура

- `src/StilsoftIRS.WinForms` — приложение, формы, сервисы, репозитории, SQL-скрипты.
- `tests/StilsoftIRS.Tests` — тесты ключевой логики.
- `scripts` — сборка, проверка, инициализация БД и запуск.
- `docs/user-guide.html` — руководство пользователя.

## Сборка

```powershell
.\scripts\build-app.ps1
```

## Инициализация БД

```powershell
.\scripts\init-database.ps1
```

По умолчанию используется `MSSQLLocalDB`. При наличии полноценного SQL Server строку подключения можно переопределить через `StilsoftIRS.env`.

## Запуск

```powershell
.\scripts\start-stilsoftirs.ps1
```

или:

```cmd
Start-StilsoftIRS.cmd
```

## Проверка

```powershell
.\scripts\run-verification.ps1
```
