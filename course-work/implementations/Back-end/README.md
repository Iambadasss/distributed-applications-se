# Personal Finance Tracker

Факултетен номер: 2401321075  
Студент: Андон Киндеков

## Описание

Personal Finance Tracker е клиент-сървър приложение за лични финанси. Системата позволява управление на потребителски профил, категории, приходи и разходи, регулярни абонаменти, месечни бюджети, спестовни цели и справки.

## Технологии

- Back-end: ASP.NET Core Web API, REST, Entity Framework Core
- Database: SQL Server LocalDB
- Authentication: JWT Bearer token
- Front-end: React, Vite, React Router, Axios

## Стартиране на back-end

1. Отвори `PersonalFinanceTracker.sln` във Visual Studio.
2. Провери connection string-а в `PersonalFinanceTracker/appsettings.json`.
3. Ако базата още не е създадена, изпълни в Package Manager Console:

```powershell
Add-Migration InitialCreate
Update-Database
```

4. Стартирай проекта с HTTPS профила.
5. Swagger адрес:

```text
https://localhost:7291/swagger
```

## Стартиране на front-end

1. Отвори front-end папката във Visual Studio Code.
2. Инсталирай зависимостите:

```powershell
npm.cmd install
```

3. Стартирай клиента:

```powershell
npm.cmd run dev
```

4. Отвори:

```text
http://localhost:5173
```

Vite proxy пренасочва заявките от `/api` към back-end адреса `https://localhost:7291`.

## Основни модули

- Потребители и профил
- Категории
- Финансови записи
- Абонаменти
- Бюджети
- Спестовни цели
- Справки
