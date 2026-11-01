# U.F.B. — правила для агента

Монорепо: `backend/` (.NET 8 API, PostgreSQL) и `frontend/` (Vue 3 + Vite + TS, FSD). Язык общения, комментариев и документации — русский.

## Секреты и env

- Никогда не коммитить секреты, токены, пароли, `.env`. Реальные значения — только в окружении.
- В коде не держать дефолтов для секретов (VK-токен, JWT в проде). Новые переменные добавлять в `.env.example` соответствующего каталога и в таблицу README.
- Перед коммитом проверять diff на токены (`vk1.a.`, `secret`, `password`).

## Backend (`backend/src`)

- Конфиг читать через `AppDefaults`, не через `Environment.GetEnvironmentVariable` напрямую.
- Схема БД меняется только EF-миграциями (`backend/src/Migrations`), модель и snapshot не править руками.
- Роли — custom claim `role`, доступ через политики `student` / `teacher` / `methodist`.
- Проверка сборки: `cd backend/src && dotnet build`.

## Frontend (`frontend`)

- FSD, импорты строго вниз: `shared → entities → features → widgets → pages → app`.
- Проверка: `cd frontend && npm run build` (включает `vue-tsc`).
- Адреса API и бота — только через `VITE_*` переменные.

## Docker

- Локально: `docker compose -f docker-compose.dev.yml up -d --build`.
- После изменений API пересобрать `backend`, иначе работает старая версия.

## Документация

- Корневой `README.md` — краткий; детали в `docs/`. Не дублировать `docs/BACKEND.md` и `docs/FRONTEND.md` в подкаталогах.
- Менял API, env или запуск — обнови README/docs в том же коммите.

## Git

- Коммиты и пуш — только по просьбе пользователя. Формат: `type: описание` (feat, fix, chore, docs).
- Не переписывать историю и не делать force-push без явного согласия.
- Не коммитить `.claude/`, `node_modules`, `bin/`, `obj/`, `dist/`, загрузки.
