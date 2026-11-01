# Backend

ASP.NET Core 8 Web API · EF Core · PostgreSQL. JSON в **snake_case**. Переменные окружения — в [корневом README](../README.md#конфигурация).

## Запуск

```bash
docker compose -f docker-compose.dev.yml up -d --build backend   # из корня
# или локально: cd backend/src && dotnet run  (нужен PostgreSQL, см. DB_*)
```

Swagger: http://localhost:8080/swagger. Миграции и seed применяются при старте.

## Структура `backend/src/`

```
Program.cs        # DI, CORS, JWT, миграции, seed
Controllers/      # Auth, Student, Teacher, Methodist, Diag (VK)
Auth/             # JwtHelper, AuthExtensions (User.UserId(), UserRole())
Config/           # AppDefaults — чтение env
Data/             # AppDbContext, DbSeeder (демо-данные)
Models/  DTOs/  Migrations/
Services/         # NotificationService, VkMessagesClient, VkLongPollWorker, VkDeadlineReminderWorker
```

## Аутентификация

- `POST /api/auth/login` → JWT в HttpOnly-cookie `auth_token` (также принимается `Authorization: Bearer`).
- Роль в claim `role`: `student` | `teacher` | `methodist`; доступ через политики `[Authorize(Policy = "student")]` и т.д.
- Фронт ходит с `withCredentials: true`.

## API

### Auth — `/api/auth`

| Метод | Путь | Auth | Описание |
|-------|------|------|----------|
| POST | `/login` | — | `{ email, password }` → `{ user }` |
| GET | `/me` | ✓ | Текущий пользователь |
| POST | `/vk` | ✓ | `{ vk_user_id }` — привязка VK |
| POST | `/logout` | ✓ | Удаляет cookie |

### Student — `/api/student` (политика `student`)

| Метод | Путь | Описание |
|-------|------|----------|
| GET | `/homeworks` | Все задания |
| GET | `/homeworks/todo` | Задания без сдачи («К сдаче») |
| GET | `/stats` | Статистика за текущий месяц |
| GET | `/submissions` | Мои сдачи (`?status=pending\|graded`) |
| GET | `/submissions/{id}` | Детали: items, grade, homework |
| POST | `/submissions` | Новая сдача (multipart) |
| PATCH | `/submissions/{id}` | Правка сдачи (только `pending`, multipart) |

Поля multipart: `homework_id` (POST), `files` (файлы), `links[]` (ссылки), `keep_item_ids` (PATCH, id через запятую).

### Teacher — `/api/teacher` (политика `teacher`)

| Метод | Путь | Описание |
|-------|------|----------|
| GET | `/submissions` | Все сдачи (`?status=`) |
| GET | `/submissions/{id}` | Детали + студент |
| GET | `/submissions/{id}/files/{itemId}` | Скачать файл |
| POST | `/submissions/{id}/grade` | `{ score, comment? }` |

### Methodist — `/api/methodist` (политика `methodist`)

ДЗ видны только создателю (`created_by`).

| Метод | Путь | Описание |
|-------|------|----------|
| GET / POST | `/homeworks` | Список / создание |
| GET / PUT | `/homeworks/{id}` | Получить / обновить |

```json
{
  "title": "string",
  "description": "string | null",
  "project": "КОД | ПАЗЛ",
  "direction": "frontend | backend | ux-ui",
  "course": 1,
  "deadline": "2026-05-20T12:00:00Z | null"
}
```

### Служебные

`GET /health`, `GET /api/ping`, `GET /api/vk/diag` (диагностика VK).

## Модель данных

| Таблица | Ключевые поля |
|---------|---------------|
| `users` | id, name, email, password_hash, role, vk_user_id |
| `homeworks` | id, title, description, project, direction, course, created_by, deadline |
| `submissions` | id, homework_id, student_id, status (`pending` \| `graded`), submitted_at |
| `submission_items` | type (`file` \| `link`), url, file_path |
| `grades` | submission_id, teacher_id, score, comment, graded_at |
| `vk_deadline_reminder_logs` | учёт отправленных напоминаний |

## Загрузки

Файлы сохраняются в `UPLOADS_PATH` (по умолчанию `/app/data/uploads`). В compose это именованный volume `uploads` — без него файлы пропадут при пересоздании контейнера.

## Фоновые сервисы

- **VkLongPollWorker** — входящие сообщения бота, привязка аккаунта.
- **VkDeadlineReminderWorker** — напоминания студентам за 24 ч и за 1 ч до дедлайна (проверка раз в 30 минут).

Без `VK_GROUP_ACCESS_TOKEN` оба отключены, остальное работает.

## Сброс демо-БД

```bash
docker compose -f docker-compose.dev.yml down -v   # удалит и БД, и загрузки
docker compose -f docker-compose.dev.yml up -d --build
```
