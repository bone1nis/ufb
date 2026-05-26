# Backend — U.F.B.

ASP.NET Core 8 Web API, PostgreSQL, EF Core. JSON в **snake_case**.

## Запуск

```bash
cd backend
# или через Docker из корня:
docker compose -f docker-compose.dev.yml up backend
```

Swagger: http://localhost:8080/swagger

## Структура `backend/src/`

```
src/
├── Program.cs                 # DI, CORS, JWT, миграции, seed
├── Controllers/
│   ├── AuthController.cs      # /api/auth/*
│   ├── StudentController.cs   # /api/student/*
│   ├── TeacherController.cs   # /api/teacher/*
│   ├── MethodistController.cs # /api/methodist/*
│   └── DiagController.cs      # диагностика VK
├── Auth/
│   ├── JwtHelper.cs
│   └── AuthExtensions.cs      # User.UserId(), UserRole()
├── Data/
│   ├── AppDbContext.cs
│   └── DbSeeder.cs            # демо-пользователи и ДЗ
├── Models/                    # User, Homework, Submission, Grade, …
├── DTOs/                      # Request/Response records
├── Migrations/
└── Services/
    ├── NotificationService.cs
    ├── VkMessagesClient.cs
    ├── VkLongPollWorker.cs
    └── VkDeadlineReminderWorker.cs
```

## Аутентификация

- Логин: `POST /api/auth/login` → JWT в **HttpOnly cookie** `auth_token`
- Запросы с фронта: `credentials: 'include'` (axios)
- Роли в claim `role`: `student` | `teacher` | `methodist`
- Политики: `[Authorize(Policy = "student")]`, и т.д.

### Auth API

| Метод | Путь | Auth | Описание |
|-------|------|------|----------|
| POST | `/api/auth/login` | — | `{ email, password }` → `{ user }` |
| GET | `/api/auth/me` | ✓ | Текущий пользователь |
| POST | `/api/auth/vk` | ✓ | `{ vk_user_id }` — привязка VK |
| POST | `/api/auth/logout` | — | Удаление cookie |

## Student API

Префикс: `/api/student`, политика `student`.

| Метод | Путь | Описание |
|-------|------|----------|
| GET | `/homeworks` | Все задания (для выбора при сдаче) |
| GET | `/homeworks/todo` | Задания без сдачи студента («К сдаче») |
| GET | `/submissions` | Список сдач (`?status=pending\|graded`) |
| GET | `/submissions/{id}` | Детали: items, grade, homework |
| POST | `/submissions` | Новая сдача (multipart) |
| PATCH | `/submissions/{id}` | Редактирование (только `pending`) |

### POST/PATCH submissions (multipart)

| Поле | Тип | Описание |
|------|-----|----------|
| `homework_id` | int | ID задания (POST) |
| `files` | file[] | Файлы |
| `links[]` | string[] | Ссылки |
| `keep_item_ids` | string | PATCH: id через запятую |

Ответы списка сдач: `SubmissionBriefResponse` — `id`, `homework_id`, `status`, `submitted_at`, `homework`, `grade: { score }`.

## Teacher API

Префикс: `/api/teacher`, политика `teacher`.

| Метод | Путь | Описание |
|-------|------|----------|
| GET | `/submissions` | Все сдачи (`?status=`) |
| GET | `/submissions/{id}` | Детали + student |
| GET | `/submissions/{id}/files/{itemId}` | Скачать файл |
| POST | `/submissions/{id}/grade` | `{ score, comment? }` |

## Methodist API

Префикс: `/api/methodist`, политика `methodist`.  
ДЗ доступны только создателю (`created_by = текущий user id`).

| Метод | Путь | Описание |
|-------|------|----------|
| GET | `/homeworks` | Список своих ДЗ |
| POST | `/homeworks` | Создать ДЗ |
| GET | `/homeworks/{id}` | Одно ДЗ (редактирование) |
| PUT | `/homeworks/{id}` | Обновить ДЗ |

### HomeworkRequest (POST/PUT)

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

## Модель данных (упрощённо)

| Таблица | Ключевые поля |
|---------|----------------|
| `users` | id, name, email, password_hash, role, vk_user_id |
| `homeworks` | id, title, description, project, direction, course, created_by, deadline |
| `submissions` | id, homework_id, student_id, status, submitted_at |
| `submission_items` | type: file \| link, url, file_path, … |
| `grades` | submission_id, teacher_id, score, comment, graded_at |

Статусы сдачи: `pending`, `graded`.

## Переменные окружения

Файл `backend/.env` (пример: `backend/env.example`).

### База данных

| Переменная | Пример | Описание |
|------------|--------|----------|
| `DB_HOST` | `postgres` | Хост PostgreSQL (имя сервиса в Docker) |
| `DB_PORT` | `5432` | Порт |
| `DB_NAME` | `hack_backend` | Название БД |
| `DB_USER` | `hack_user` | Пользователь |
| `DB_PASS` | `secret` | Пароль |

### JWT

| Переменная | Пример | Описание |
|------------|--------|----------|
| `JWT_SECRET` | `super-secret-32-chars-minimum` | Секрет подписи токена (**≥32 символа**, иначе ошибка) |
| `JWT_EXPIRES_HOURS` | `24` | Время жизни токена в часах |

### Общее

| Переменная | Пример | Описание |
|------------|--------|----------|
| `FRONTEND_ORIGIN` | `http://localhost:5173` | Разрешённый CORS-источник фронтенда |
| `UPLOADS_PATH` | `/app/data/uploads` | Путь внутри контейнера, куда сохраняются загружаемые файлы. **Нужно монтировать как volume**, иначе файлы пропадут при рестарте контейнера |

### VK (опционально)

| Переменная | Пример | Описание |
|------------|--------|----------|
| `VK_GROUP_ACCESS_TOKEN` | `vk1.a.xxx...` | Токен группы ВК (доступ к messages) |
| `VK_GROUP_ID` | `238684561` | ID группы (без минуса) |
| `VK_MESSAGES_INCLUDE_GROUP_ID` | `1` | Передавать ли `group_id` в API VK (обычно `1`) |

Без VK-переменных бот и уведомления просто не работают, всё остальное функционирует нормально.

### Docker Compose — монтирование uploads

В `docker-compose.dev.yml` volume для файлов уже настроен:

```yaml
services:
  backend:
    environment:
      UPLOADS_PATH: /app/data/uploads
    volumes:
      - uploads:/app/data/uploads   # ← именованный volume

volumes:
  uploads:
```

На продакшн-сервере можно заменить на bind mount к конкретной папке на хосте:

```yaml
volumes:
  - /srv/ufb/uploads:/app/data/uploads
```

Пример: `backend/env.example`, `backend/.env` для Docker.

## Фоновые сервисы

- **VkLongPollWorker** — входящие сообщения VK, привязка аккаунта
- **VkDeadlineReminderWorker** — напоминания студентам о дедлайнах

## Health

- `GET /` — `{ service, status }`
- `GET /health`
- `GET /api/ping` — `pong`

## Сброс демо-БД

```bash
./scripts/reset-dev-db.sh
```
