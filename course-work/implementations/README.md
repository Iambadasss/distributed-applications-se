# PersonalFinanceTracker

## Данни за студента

- Име: Андон Киндеков
- Факултетен номер: 2401321075

## Описание на проекта

PersonalFinanceTracker е уеб приложение за управление на лични финанси. Системата позволява потребителят да следи приходи, разходи, категории, бюджети, абонаменти и спестовни цели.

Проектът има две основни части:

- Back-end: ASP.NET Core Web API с REST endpoints, Entity Framework Core и SQL Server LocalDB.
- Front-end: React SPA приложение, създадено с Vite, което комуникира с back-end-а чрез Axios.

Достъпът до приложението е защитен чрез JWT token. След вход токенът се пази във front-end-а в `localStorage` и се изпраща към API-то с `Authorization: Bearer <token>`.

## Основни функционалности

- Регистрация и вход в акаунт
- Защитен потребителски профил
- CRUD операции за категории
- CRUD операции за финансови записи
- CRUD операции за абонаменти
- CRUD операции за бюджети
- CRUD операции за спестовни цели
- Табло с месечно обобщение и справки по категории
- Филтриране, сортиране и странициране на списъците
- Валидация на входните данни
- Глобална обработка на грешки чрез Problem Details

## Основни модели в базата данни

- `Users`
- `Categories`
- `FinancialRecords`
- `Subscriptions`
- `Budgets`
- `SavingsGoals`

Таблиците са свързани логически чрез потребител и категории. Финансовите записи, бюджетите и абонаментите използват категории, а всички основни данни са свързани с конкретен потребител.

## Използвани технологии

### Back-end

- C#
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server LocalDB
- JWT Authentication
- Swagger / OpenAPI

### Front-end

- JavaScript
- React
- React Router DOM
- Vite
- Axios
- HTML и CSS

## Необходими програми

За стартиране на проекта са нужни:

- Visual Studio 2022
- .NET 8 SDK
- SQL Server LocalDB
- Node.js и npm

Ако Visual Studio е инсталиран с ASP.NET/.NET workload, SQL Server LocalDB обикновено вече е наличен.

## Настройка на базата данни

Back-end проектът използва следния connection string в `appsettings.json`:

```json
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;database=personal_finance_tracker;Trusted_Connection=True;TrustServerCertificate=True;"
```

Базата данни се казва:

```text
personal_finance_tracker
```

Не е нужно таблиците да се създават ръчно. Те се създават чрез Entity Framework миграциите.

### Създаване на базата през Visual Studio

1. Отвори solution-а `PersonalFinanceTracker`.
2. Отвори `Tools -> NuGet Package Manager -> Package Manager Console`.
3. В `Default project` избери `PersonalFinanceTracker`.
4. Изпълни командата:

```powershell
Update-Database
```

### Създаване на базата през терминал

От папката на solution-а изпълни:

```powershell
dotnet restore
dotnet ef database update --project .\PersonalFinanceTracker\PersonalFinanceTracker.csproj --startup-project .\PersonalFinanceTracker\PersonalFinanceTracker.csproj
```

Ако `dotnet ef` не е инсталирано:

```powershell
dotnet tool install --global dotnet-ef
```

След това изпълни отново командата за миграцията.

## Стартиране на back-end-а

1. Отвори solution-а във Visual Studio.
2. Избери back-end проекта `PersonalFinanceTracker` като startup project.
3. Стартирай проекта с HTTPS профила.
4. Swagger документацията е достъпна на адрес:

```text
https://localhost:7291/swagger
```

## Стартиране на front-end-а

Отвори терминал в папката на front-end проекта и изпълни:

```powershell
npm install
npm run dev
```

След това отвори:

```text
http://localhost:5173
```

Front-end приложението използва Vite proxy, който препраща заявките от `/api` към back-end адреса `https://localhost:7291`.

## Начин на работа

1. Потребителят създава акаунт или влиза в съществуващ.
2. След вход получава JWT token.
3. Token-ът се използва за достъп до защитените API endpoints.
4. Потребителят създава категории.
5. След това може да добавя приходи, разходи, бюджети, абонаменти и спестовни цели.
6. Таблото показва обобщение за избран период и групиране по категории.

## Забележки при преместване на друг компютър

При преместване на проекта на друг компютър:

1. Инсталирай нужните програми: Visual Studio, .NET 8 SDK, SQL Server LocalDB, Node.js.
2. Провери connection string-а в `appsettings.json`.
3. Изпълни миграциите с `Update-Database` или `dotnet ef database update`.
4. Стартирай back-end-а от Visual Studio.
5. Стартирай front-end-а с `npm install` и `npm run dev`.

Не е необходимо да се пренася самата база данни, ако искаш празна база. Миграциите ще я създадат отново.
