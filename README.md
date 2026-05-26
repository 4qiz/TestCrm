# BestAuth

BestAuth — это демонстрационный веб-проект на .NET, реализующий простую авторизацию и аутентификацию с JWT и refresh-токенами.

## Структура проекта

- `BestAuth.Api/` — Web API приложение на ASP.NET Core.
  - `Program.cs` — точка входа, конфигурация сервисов, аутентификации и middleware.
  - `Controllers/AccountController.cs` — API-эндпоинты регистрации, логина и обновления токена.
  - `Handlers/GlobalExceptionHandler.cs` — глобальная обработка ошибок.

- `BestAuth.Application/` — бизнес-логика и интерфейсы.
  - `Abstracts/` — интерфейсы, например `IAccountService` и `IUserRepository`.
  - `Services/AccountService.cs` — реализация логики регистрации, логина и refresh.
  - `Constants/` — константы, например `CookieNames`.

- `BestAuth.Domain/` — доменные сущности и DTO-запросы.
  - `Entities/User.cs` — пользовательская сущность, расширяющая `IdentityUser<Guid>`.
  - `Requests/` — модели запросов `LoginRequest` и `RegisterRequest`.
  - `Exceptions/` — специальные исключения для ошибок аутентификации.

- `BestAuth.Infrastructure/` — реализация работы с БД и JWT.
  - `AppDbContext.cs` — EF Core `IdentityDbContext`.
  - `Repositories/UserRepository.cs` — поиск пользователя по refresh-токену.
  - `Processors/AuthTokenProcessor.cs` — генерация access/refresh токенов и запись cookie.
  - `Options/JwtOptions.cs` — настройки JWT из конфигурации.

## Основные библиотеки

- `Microsoft.AspNetCore.Authentication.JwtBearer` — JWT-аутентификация.
- `Microsoft.AspNetCore.Identity` — управление пользователями и паролями.
- `Microsoft.EntityFrameworkCore` + `Npgsql.EntityFrameworkCore.PostgreSQL` — PostgreSQL + EF Core.
- `Microsoft.IdentityModel.Tokens` — подпись и проверка JWT.
- `Scalar.AspNetCore` — OpenAPI/документация (используется для `AddOpenApi`).

## Как работает

1. В `BestAuth.Api/Program.cs` настраивается:
   - `AddIdentity<User, Role>()` для хранения пользователей через EF Core.
   - `AddDbContext<AppDbContext>()` с подключением к PostgreSQL.
   - JWT-аутентификация, где токен берётся из cookie `ACCESS_TOKEN`.
   - scoped-сервисы `IAccountService`, `IUserRepository`, `IAuthTokenProcessor`.

2. В `AccountController` доступны три маршрута:
   - `POST /api/account/register` — регистрация нового пользователя.
   - `POST /api/account/login` — проверка email/password и выдача токенов.
   - `POST /api/account/refresh` — обновление access и refresh токенов.

3. `AccountService` выполняет логику:
   - при регистрации создаёт `User`, хэширует пароль и сохраняет пользователя.
   - при логине ищет пользователя по email и проверяет пароль.
   - при refresh ищет пользователя по refresh-токену и проверяет срок действия.
   - если всё успешно, создаёт новую сессию с `CreateAuthSessionAsync`.

4. `AuthTokenProcessor` создаёт:
   - access-токен JWT с информацией о пользователе и сроком действия.
   - refresh-токен в виде случайной строки.
   - HttpOnly cookie `ACCESS_TOKEN` и `REFRESH_TOKEN`.

5. `AppDbContext` хранит пользователей и используется как Identity-хранилище.

## Настройка и запуск

В `appsettings.json` или `appsettings.Development.json` нужно указать:

- `ConnectionStrings:DefaultConnection` — строка подключения к PostgreSQL.
- `Jwt` — параметры `Issuer`, `Audience`, `Key`, `AccessExpireMinutes`, `RefreshExpireDays`.

Примеры команд EF Core:

```powershell
dotnet ef migrations add Init -s .\BestAuth.Api\ -p .\BestAuth.Infrastructure\
```

```powershell
dotnet ef database update Init -s .\BestAuth.Api\ -p .\BestAuth.Infrastructure\
```

## Примечания

- В проекте используется cookie-based передача JWT, а не стандартный `Authorization: Bearer`.
- Для простоты парольные требования ослаблены (`RequiredLength = 1`, не требуются цифры и спецсимволы).
- Refresh-токен хранится в БД в поле `User.RefreshToken` и имеет дату `ExpiresAtUtc`.

Роли

- При старте приложения создаются роли `Admin` и `User`.
- При регистрации новому пользователю автоматически назначается роль `User`.
- Пример защищённого эндпоинта: `GET /api/values/admin` доступен только пользователям с ролью `Admin`.
