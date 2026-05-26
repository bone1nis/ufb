# U.F.B. — Unified Feedback Board

> Единая платформа обратной связи для образовательного процесса.

## Почему U.F.B.?

| Буква | Значение | Смысл |
|-------|----------|-------|
| **U** | **Unified** | Единая точка входа для всех ролей: студент, преподаватель, методист |
| **F** | **Feedback** | Суть системы — двусторонняя обратная связь: сдача → оценка → уведомление |
| **B** | **Board** | Прозрачная «доска» задач и сдач, где каждый видит свой процесс |

Платформа закрывает полный цикл учебного задания: методист создаёт ДЗ → студент сдаёт работу (файлы или ссылки) → преподаватель оценивает → студент получает уведомление в ВКонтакте.

---

## Документация

| Файл | Содержание |
|------|------------|
| [docs/README.md](./docs/README.md) | Общий обзор, быстрый старт, структура репозитория |
| [docs/BACKEND.md](./docs/BACKEND.md) | API, модели, env, сервисы бэкенда |
| [docs/FRONTEND.md](./docs/FRONTEND.md) | FSD, компоненты, маршруты, разработка UI |

---

## Архитектура

```
┌─────────────────────────────────────────────────────┐
│  Vue 3 SPA (`frontend/`)         FSD Architecture  │
│  ┌─────────┐ ┌──────────┐ ┌──────────┐ ┌─────────┐ │
│  │  pages  │ │ widgets  │ │ features │ │entities │ │
│  └─────────┘ └──────────┘ └──────────┘ └─────────┘ │
│                    shared/api  shared/lib             │
└────────────────────────┬────────────────────────────┘
                         │ HTTP + Cookie JWT
┌────────────────────────▼────────────────────────────┐
│  .NET 8 ASP.NET Core (backend/)                      │
│  ┌───────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │ Endpoints │  │ VkLongPoll   │  │DeadlineRemind│  │
│  │ (Minimal  │  │ Worker       │  │ Worker       │  │
│  │  API)     │  │ (Background) │  │ (Background) │  │
│  └───────────┘  └──────────────┘  └──────────────┘  │
│  EF Core + Npgsql         JWT Auth (cookie + bearer) │
└────────────────────────┬────────────────────────────┘
                         │
              ┌──────────▼──────────┐
              │   PostgreSQL 15     │
              └─────────────────────┘
                         │
              ┌──────────▼──────────┐
              │   VK Bots Long Poll │
              │   (message_new)     │
              └─────────────────────┘
```

### Frontend: Feature-Sliced Design (FSD)

Подробно: **[docs/FRONTEND.md](./docs/FRONTEND.md)** — слои, компоненты, маршруты.

**Правило импортов:** `shared → entities → features → widgets → pages → app`

### Backend: .NET 8 Web API

Подробно: **[docs/BACKEND.md](./docs/BACKEND.md)** — все эндпоинты, auth, multipart, env.

---

## Роли

| Роль | Возможности |
|------|-------------|
| **student** | Список ДЗ, сдача работ (файлы + ссылки), история сдач, детали с оценкой |
| **teacher** | Все сдачи с фильтрами, выставление оценок (балл + комментарий), скачивание файлов |
| **methodist** | Создание заданий (название, описание, проект, направление, курс, дедлайн) |

---

## API

| Метод | Путь | Роль | Описание |
|-------|------|------|----------|
| `POST` | `/api/auth/login` | — | Авторизация, JWT в cookie |
| `GET` | `/api/auth/me` | any | Текущий пользователь |
| `POST` | `/api/auth/vk` | any | Привязка vk_user_id |
| `POST` | `/api/auth/logout` | any | Выход |
| `GET` | `/api/student/homeworks` | student | Список ДЗ |
| `GET` | `/api/student/submissions` | student | Список сдач |
| `POST` | `/api/student/submissions` | student | Отправить сдачу (multipart) |
| `GET` | `/api/student/submissions/:id` | student | Детали сдачи |
| `GET` | `/api/teacher/submissions` | teacher | Все сдачи |
| `GET` | `/api/teacher/submissions/:id` | teacher | Детали сдачи |
| `GET` | `/api/teacher/submissions/:id/files/:itemId` | teacher | Скачать файл |
| `POST` | `/api/teacher/submissions/:id/grade` | teacher | Выставить оценку |
| `GET` | `/api/methodist/homeworks` | methodist | Список ДЗ |
| `POST` | `/api/methodist/homeworks` | methodist | Создать ДЗ |
| `GET` | `/api/vk/diag` | — | Диагностика VK env |
| `GET` | `/health` | — | Healthcheck |

---

## Переменные окружения (runtime backend)

| Переменная | Обязательна | Описание |
|-----------|-------------|----------|
| `DB_HOST` | да | Хост PostgreSQL (в docker: `postgres`) |
| `DB_PORT` | нет | Порт, default `5432` |
| `DB_NAME` | нет | База, default `hack_backend` |
| `DB_USER` | нет | Пользователь, default `hack_user` |
| `DB_PASS` | **да (prod)** | Пароль БД — masked GitLab variable |
| `JWT_SECRET` | **да (prod)** | Секрет JWT — masked |
| `JWT_EXPIRES_HOURS` | нет | Время жизни токена, default `24` |
| `FRONTEND_ORIGIN` | да | URL фронта для CORS + тексты бота |
| `UPLOADS_PATH` | нет | Путь к загрузкам, default `/app/data/uploads` |
| `VK_GROUP_ACCESS_TOKEN` | нет | Токен сообщества ВКонтакте — masked |
| `VK_GROUP_ID` | нет | Числовой id сообщества (или `clubNNNN`) |
| `VK_MESSAGES_INCLUDE_GROUP_ID` | нет | `1` — передавать group_id в messages.send |

---

## VK бот

Бот работает через **Bots Long Poll API** (без Callback URL, встроен в процесс API).

**Требования:**
- В настройках сообщества ВКонтакте: Long Poll → тип событий → `message_new` ✓
- Пользователь должен разрешить ЛС от сообщества

**Команды:**
| Команда | Описание |
|---------|----------|
| `/start` | Инструкция по привязке ВК к аккаунту |
| `/help` | Список команд |
| `/status` / `/статус` | Привязан ли аккаунт |
| `/deadlines` / `/дедлайны` | Ближайшие дедлайны студента |

**Уведомления (messages.send):**
- Преподавателям → при новой сдаче, при создании нового ДЗ
- Студенту → подтверждение сдачи, результат оценки
- Автоматически → напоминания о дедлайнах (за 24ч и за 1ч, интервал 30 мин)

---

## Быстрый старт (локально)

```bash
# 1. Создать backend/.env из backend/env.example и заполнить VK_* и JWT_SECRET
cp backend/env.example backend/.env

# 2. Поднять сервисы
docker compose -f docker-compose.dev.yml up -d --build

# Ссылку на бота ВК задай в `frontend/.env.production` как `VITE_VK_BOT_URL=...` (файл `.env` в Docker-контекст не попадает).
# (файл `.env` в контекст сборки не копируется — см. `.dockerignore`).
# (полный URL, например https://vk.com/im?sel=-NNNNNNN).

# 3. Frontend доступен на http://localhost:5173
# 4. Backend API на http://localhost:8080
# 5. Swagger: http://localhost:8080/swagger
```

**Demo-аккаунты** (создаются при первом старте):
| Email | Пароль | Роль |
|-------|--------|------|
| `student@demo.ru` | `password` | Студент |
| `teacher@demo.ru` | `password` | Преподаватель |
| `methodist@demo.ru` | `password` | Методист |

---

## Технические перспективы

### Краткосрочно (следующий спринт)

| Направление | Что добавить |
|-------------|-------------|
| **Redis** | Кэш сессий / refresh token store → убрать зависимость от cookie-only JWT; rate limiting для `/api/auth/login` |
| **VK OAuth** | Создать VK App → `GET /api/auth/vk/oauth/start` → redirect → callback → автоматически привязывать id без ручного ввода |
| **Файловое хранилище** | S3-совместимое хранилище (MinIO / Яндекс Object Storage) вместо volume → масштабирование на несколько инстансов |
| **WebSockets / SSE** | Push-уведомления в браузер при появлении новой оценки (сейчас только ВКонтакте) |
| **Пагинация** | `GET /api/teacher/submissions?page=1&per_page=20` — при большом числе сдач |

### Среднесрочно

| Направление | Что добавить |
|-------------|-------------|
| **Celery / Background jobs** | Перенести Email/VK-уведомления в очередь (RabbitMQ / Redis Streams) → не блокировать HTTP-ответ |
| **Observability** | OpenTelemetry трейсы → Jaeger/Tempo; метрики → Prometheus + Grafana |
| **CI тесты** | Unit-тесты для EF-репозиториев (TestContainers + Postgres), Playwright e2e для Vue |
| **Роли расширить** | Администратор — управление пользователями, сброс паролей |
| **Комментарии** | Тред комментариев к сдаче (итерационная проверка) |

### Долгосрочно

| Направление | Описание |
|-------------|---------|
| **Mobile-first** | PWA с оффлайн-кэшем или нативное приложение (Capacitor) |
| **AI-ревью** | Автоматический первичный разбор кода через GPT API → черновой комментарий преподавателю |
| **Аналитика** | Дашборд методиста: прогресс студентов, средние баллы, тепловая карта дедлайнов |
| **Multi-tenant** | Несколько образовательных организаций в одном инстансе с изоляцией данных |
