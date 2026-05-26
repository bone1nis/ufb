# Backend — Документация проекта

ASP.NET Core 8 Web API. Система управления домашними заданиями: сдача работ, проверка преподавателем, уведомления через ВКонтакте.

---

## Содержание

- [Стек](#стек)
- [Запуск](#запуск)
- [Архитектура](#архитектура)
- [База данных](#база-данных)
- [API — эндпоинты](#api--эндпоинты)
- [Авторизация и роли](#авторизация-и-роли)
- [Сервисы](#сервисы)
- [Переменные окружения](#переменные-окружения)
- [Docker](#docker)

---

## Стек

| Технология | Версия | Роль |
|---|---|---|
| ASP.NET Core | 8 | Web-фреймворк |
| Entity Framework Core | ^8.x | ORM |
| PostgreSQL | 15 | База данных |
| BCrypt.Net | — | Хеширование паролей |
| JWT Bearer | — | Авторизация |
| Swagger / Swashbuckle | — | Документация API |

---

## Запуск

### Через Docker (рекомендуется)

```bash
# Из корня монорепы
docker compose -f docker-compose.dev.yml up --build -d
```

API: **http://localhost:8080**  
Swagger UI: **http://localhost:8080/swagger**  
PostgreSQL: **localhost:5432** (база `hack_backend`, юзер `hack_user`, пароль `secret`)

### Локально без Docker

```bash
cd backend
cp env.example .env
# Заполни DB_HOST, DB_USER, DB_PASS и JWT_SECRET в .env

dotnet restore
dotnet run --project src
```

---

## Архитектура

```
backend/src/
├── Controllers/         # HTTP-контроллеры по ролям
│   ├── AuthController.cs
│   ├── StudentController.cs
│   ├── TeacherController.cs
│   ├── MethodistController.cs
│   └── DiagController.cs
├── Models/              # EF-сущности (таблицы БД)
│   ├── User.cs
│   ├── Homework.cs
│   ├── Submission.cs
│   ├── SubmissionItem.cs
│   ├── Grade.cs
│   └── VkDeadlineReminderLog.cs
├── DTOs/                # Request/Response объекты
│   ├── Requests.cs
│   └── Responses.cs
├── Data/                # AppDbContext (EF Core)
├── Auth/                # JwtHelper, расширения ClaimsPrincipal
├── Services/            # Бизнес-сервисы
│   ├── INotificationService.cs
│   ├── NotificationService.cs
│   ├── VkLongPollWorker.cs
│   ├── VkDeadlineReminderWorker.cs
│   └── VkMessagesClient.cs
├── Options/             # Strongly-typed настройки (UploadSettings и др.)
├── Config/              # Статические конфиги
├── Migrations/          # EF Migrations
├── Program.cs           # Точка входа, DI, Middleware
└── appsettings.json
```

---

## База данных

### Схема

```
Users
├── Id (PK)
├── Name
├── Email (unique)
├── PasswordHash
├── Role (student | teacher | methodist)
├── VkUserId (nullable)
└── CreatedAt

Homeworks
├── Id (PK)
├── Title
├── Description (nullable)
├── Project        — «КОД» | «ПАЗЛ»
├── Direction      — «frontend» | «backend» | «ux-ui»
├── Course         — номер курса (1/2/3)
├── CreatedBy (FK → Users.Id)
├── Deadline (nullable)
└── CreatedAt

Submissions
├── Id (PK)
├── HomeworkId (FK → Homeworks.Id)
├── StudentId (FK → Users.Id)
├── Status         — pending | graded
├── SubmittedAt
└── CreatedAt

SubmissionItems
├── Id (PK)
├── SubmissionId (FK → Submissions.Id)
├── Type           — file | link
├── Url (nullable)
├── FilePath (nullable)
├── OriginalName (nullable)
├── MimeType (nullable)
└── FileSize (nullable)

Grades
├── Id (PK)
├── SubmissionId (FK → Submissions.Id)
├── TeacherId (FK → Users.Id)
├── Score          — 0–100
├── Comment (nullable)
└── GradedAt
```

### Миграции

```bash
# Применить миграции
dotnet ef database update --project src

# Создать новую миграцию
dotnet ef migrations add <Name> --project src
```

---

## API — эндпоинты

Все ответы в JSON (snake_case). Авторизация — HTTP-only cookie `auth_token` или Bearer-заголовок.

---

### Auth — `/api/auth`

#### `POST /api/auth/login`

Логин по email и паролю. Устанавливает cookie `auth_token` (7 дней).

**Тело запроса:**
```json
{ "email": "student@demo.ru", "password": "password" }
```

**Ответ `200`:**
```json
{
  "user": {
    "id": 1,
    "name": "Студент Тест",
    "email": "student@demo.ru",
    "role": "student",
    "vk_user_id": null
  }
}
```

**Ошибки:** `401` — неверный email или пароль.

---

#### `GET /api/auth/me` 🔒

Возвращает текущего авторизованного пользователя.

**Ответ `200`:** объект `user` (см. выше).  
**Ошибки:** `401` — не авторизован.

---

#### `POST /api/auth/logout`

Удаляет cookie `auth_token`.

**Ответ `200`:** `{ "message": "ok" }`

---

#### `POST /api/auth/vk` 🔒

Привязывает или отвязывает ВКонтакте-аккаунт.

**Тело запроса:**
```json
{ "vk_user_id": "123456789" }
```
Передайте `null` для отвязки.

**Ответ `200`:** `{ "vk_user_id": "123456789" }`

---

### Student — `/api/student` 🔒 (роль: student)

#### `GET /api/student/stats`

Статистика студента за текущий календарный месяц. Вычисляется на сервере через агрегацию в БД.

**Ответ `200`:**
```json
{
  "submissions_this_month": 3,
  "avg_score": 85,
  "success_percent": 67
}
```

| Поле | Тип | Описание |
|---|---|---|
| `submissions_this_month` | int | Кол-во сдач в текущем месяце |
| `avg_score` | int? | Средняя оценка по проверенным сдачам месяца. `null` если нет проверенных |
| `success_percent` | int | Доля проверенных сдач от всех сдач месяца (0–100). 0 если сдач нет |

---

#### `GET /api/student/homeworks`

Все домашние задания (упорядочены по дедлайну).

**Ответ `200`:** массив `HomeworkBriefResponse`:
```json
[
  {
    "id": 1,
    "title": "Верстка лендинга",
    "project": "КОД",
    "direction": "frontend",
    "course": 2,
    "deadline": "2026-05-20T00:00:00Z",
    "description": "..."
  }
]
```

---

#### `GET /api/student/homeworks/todo`

Задания, по которым студент ещё не делал ни одной сдачи (для раздела «К сдаче»).

**Ответ `200`:** массив `HomeworkBriefResponse` (сортировка по ближайшему дедлайну).

---

#### `GET /api/student/submissions`

Все сдачи текущего студента.

**Query параметры:** `?status=pending|graded` (опционально).

**Ответ `200`:** массив `SubmissionBriefResponse`:
```json
[
  {
    "id": 5,
    "homework_id": 1,
    "status": "pending",
    "submitted_at": "2026-05-15T10:00:00Z",
    "homework": { ... },
    "grade": null
  }
]
```

---

#### `GET /api/student/submissions/:id`

Детали конкретной сдачи (только своей).

**Ответ `200`:** `SubmissionDetailResponse`:
```json
{
  "id": 5,
  "status": "graded",
  "submitted_at": "2026-05-15T10:00:00Z",
  "homework": { ... },
  "items": [
    { "id": 1, "type": "file", "original_name": "index.html", "file_size": 2048, "url": null }
  ],
  "grade": {
    "score": 85,
    "comment": "Хорошая работа",
    "graded_at": "2026-05-16T12:00:00Z",
    "teacher_name": "Преподаватель"
  }
}
```

**Ошибки:** `404` — сдача не найдена или чужая.

---

#### `POST /api/student/submissions`

Создать новую сдачу. Принимает `multipart/form-data`.

**Поля формы:**

| Поле | Тип | Обязательное | Описание |
|---|---|---|---|
| `homework_id` | number | да | ID задания |
| `files` | File[] | нет* | Файлы (один или несколько) |
| `links[]` | string[] | нет* | Ссылки |

*Нужен хотя бы один файл или ссылка.

**Ответ `201`:** `{ "id": 6 }`

**Ошибки:**
- `400` — нет `homework_id` или нет файлов/ссылок
- `404` — задание не найдено
- `409` — по этому заданию уже есть сдача «на проверке» (нужно редактировать)

---

#### `PATCH /api/student/submissions/:id`

Обновить сдачу в статусе `pending`. Принимает `multipart/form-data`.

**Поля формы:**

| Поле | Тип | Описание |
|---|---|---|
| `keep_item_ids` | string | Список ID вложений через запятую, которые нужно оставить |
| `files` | File[] | Новые файлы |
| `links[]` | string[] | Новые ссылки |

**Ответ `200`:** `{ "id": 5 }`

**Ошибки:**
- `404` — сдача не найдена
- `409` — сдача не в статусе `pending`

---

### Teacher — `/api/teacher` 🔒 (роль: teacher)

#### `GET /api/teacher/submissions`

Все сдачи всех студентов.

**Query параметры:** `?homeworkId=1` (фильтр по заданию, опционально).

**Ответ `200`:** массив `SubmissionBriefResponse` (с полем `student`).

---

#### `GET /api/teacher/submissions/:id`

Детали сдачи студента (с информацией о студенте и вложениями).

**Ответ `200`:** `SubmissionDetailResponse` (с полем `student`).

---

#### `GET /api/teacher/submissions/:id/files/:itemId`

Скачать файл из сдачи.

**Ответ `200`:** бинарный файл (`Content-Disposition: attachment`).

---

#### `POST /api/teacher/submissions/:id/grade`

Выставить или обновить оценку.

**Тело запроса:**
```json
{ "score": 85, "comment": "Хорошая работа, но есть замечания" }
```

`score` — целое число от 0 до 100.

**Ответ `200`:** `{ "message": "ok" }`

После выставления оценки студенту отправляется VK-уведомление (если привязан VK).

---

### Methodist — `/api/methodist` 🔒 (роль: methodist)

#### `GET /api/methodist/homeworks`

Задания, созданные текущим методистом (упорядочены по дате создания, новые первыми).

**Ответ `200`:** массив `HomeworkResponse`:
```json
[
  {
    "id": 1,
    "title": "Верстка лендинга",
    "description": "...",
    "project": "КОД",
    "direction": "frontend",
    "course": 2,
    "deadline": "2026-05-20T00:00:00Z",
    "created_at": "2026-05-10T08:00:00Z",
    "created_by": 3
  }
]
```

---

#### `POST /api/methodist/homeworks`

Создать новое задание.

**Тело запроса:**
```json
{
  "title": "Верстка лендинга",
  "description": "Создайте адаптивный лендинг",
  "project": "КОД",
  "direction": "frontend",
  "course": 2,
  "deadline": "2026-05-20T00:00:00Z"
}
```

`description` и `deadline` — опциональны.

**Ответ `201`:** созданный объект `HomeworkResponse`.

После создания — VK-уведомление всем преподавателям (если привязаны).

---

#### `GET /api/methodist/homeworks/:id`

Получить конкретное задание (только созданное этим методистом).

**Ответ `200`:** `HomeworkResponse`.  
**Ошибки:** `404`.

---

#### `PUT /api/methodist/homeworks/:id`

Обновить задание (только своё).

**Тело запроса:** аналогично `POST`.

**Ответ `200`:** обновлённый `HomeworkResponse`.  
**Ошибки:** `404`.

---

### Диагностика — `/api/diag`

#### `GET /health`

Healthcheck-эндпоинт (используется Docker).

**Ответ `200`:** `{ "status": "ok" }`

---

## Авторизация и роли

Авторизация работает через **JWT-токен** в HTTP-only cookie `auth_token`.

- Токен действителен **7 дней**.
- Cookie устанавливается при `POST /api/auth/login` и удаляется при `POST /api/auth/logout`.
- В development-режиме принимается также **Bearer-заголовок**: `Authorization: Bearer <token>` (удобно для Swagger).
- Политики ролей: `"student"`, `"teacher"`, `"methodist"` — настроены в `Program.cs`.

### Demo-аккаунты

| Email | Пароль | Роль |
|---|---|---|
| `student@demo.ru` | `password` | Студент |
| `teacher@demo.ru` | `password` | Преподаватель |
| `methodist@demo.ru` | `password` | Методист |

---

## Сервисы

### INotificationService / NotificationService

Отправляет VK-уведомления через VK API. Методы:

| Метод | Когда вызывается |
|---|---|
| `NotifyStudentGradedAsync` | Преподаватель выставил оценку |
| `NotifyStudentSubmissionReceivedAsync` | Студент создал новую сдачу |
| `NotifyTeachersNewSubmissionAsync` | Студент сдал работу (уведомление преподавателям) |
| `NotifyTeachersNewHomeworkAsync` | Методист создал новое задание |

Если `VkUserId` у пользователя не привязан — уведомление тихо пропускается.

### VkLongPollWorker

Background-сервис (hosted service): слушает VK LongPoll API, обрабатывает входящие сообщения от пользователей.

### VkDeadlineReminderWorker

Background-сервис: периодически рассылает студентам напоминания о приближающихся дедлайнах (через VK).

---

## Переменные окружения

| Переменная | Обязательная | По умолчанию | Описание |
|---|---|---|---|
| `DB_HOST` | да | `postgres` | Хост PostgreSQL |
| `DB_PORT` | нет | `5432` | Порт PostgreSQL |
| `DB_NAME` | нет | `hack_backend` | Имя базы данных |
| `DB_USER` | да | — | Пользователь БД |
| `DB_PASS` | да | — | Пароль БД |
| `JWT_SECRET` | да | — | Секрет для подписи JWT (мин. 32 символа) |
| `UPLOADS_PATH` | нет | `/app/data/uploads` | Путь для хранения загруженных файлов |
| `FRONTEND_ORIGIN` | нет | `http://localhost:5173` | Origin фронтенда для CORS |
| `ASPNETCORE_ENVIRONMENT` | нет | `Production` | `Development` включает Swagger |
| `BOT_TOKEN` | нет | — | VK Bot Token для уведомлений |

Для локального запуска скопируй `env.example` в `.env` и заполни обязательные поля.

---

## Docker

```dockerfile
# Многоэтапная сборка:
# 1. mcr.microsoft.com/dotnet/sdk:8.0  — dotnet publish
# 2. mcr.microsoft.com/dotnet/aspnet:8.0 — runtime-образ
# Порт: 8080
```

```bash
# Сборка
docker build -t backend ./backend

# Запуск (нужен запущенный Postgres)
docker run -p 8080:8080 \
  -e DB_HOST=host.docker.internal \
  -e DB_USER=hack_user \
  -e DB_PASS=secret \
  -e JWT_SECRET=dev-secret-change-in-production-32ch \
  backend
```

Файлы загрузок хранятся в volume `/app/data/uploads` — монтируй через `-v uploads:/app/data/uploads` для сохранения при перезапусках.
