# U.F.B. — Unified Feedback Board

Платформа сдачи и проверки домашних заданий с уведомлениями в ВКонтакте.

Методист создаёт задание → студент сдаёт работу файлами или ссылками → преподаватель ставит оценку и комментарий → студент получает уведомление в VK.

> Проект создан на хакатоне.

## Возможности

| Роль | Что делает |
|------|------------|
| **Студент** | Видит задания «К сдаче», сдаёт и правит работы, смотрит оценки и комментарии, статистику за месяц |
| **Преподаватель** | Просматривает все сдачи с фильтрами, скачивает файлы, выставляет балл и комментарий |
| **Методист** | Создаёт и редактирует задания: проект, направление, курс, дедлайн |

**VK-бот** (Bots Long Poll): привязка аккаунта, команды `/start`, `/help`, `/status`, `/deadlines`, уведомления о сдачах и оценках, напоминания о дедлайнах за 24 часа и за 1 час.

## Стек

| Часть | Технологии |
|-------|------------|
| Frontend | Vue 3, TypeScript, Vite, Pinia, Vue Router, Naive UI, Axios · Feature-Sliced Design |
| Backend | .NET 8, ASP.NET Core, EF Core, JWT (httpOnly-cookie) |
| Данные | PostgreSQL 15 |
| Интеграции | VK Bots Long Poll API |
| Окружение | Docker, Docker Compose, nginx |

## Быстрый старт

Нужен Docker с Compose v2.

```bash
git clone https://github.com/bone1nis/ufb.git
cd ufb

cp .env.example .env                    # необязательно — есть dev-значения
cp backend/.env.example backend/.env    # токен VK; без него бот отключён

docker compose -f docker-compose.dev.yml up -d --build
```

| Сервис | Адрес |
|--------|-------|
| Frontend | http://localhost:5173 |
| API | http://localhost:8080 |
| Swagger | http://localhost:8080/swagger |

Демо-аккаунты (только для разработки), пароль у всех `password`:

| Email | Роль |
|-------|------|
| `student@demo.ru` | Студент |
| `teacher@demo.ru` | Преподаватель |
| `methodist@demo.ru` | Методист |

## Конфигурация

Секреты не хранятся в репозитории — только в переменных окружения. Шаблоны: [.env.example](./.env.example), [backend/.env.example](./backend/.env.example), [frontend/.env.example](./frontend/.env.example).

| Переменная | Обязательна | Описание |
|-----------|-------------|----------|
| `DB_HOST`, `DB_PORT`, `DB_NAME`, `DB_USER` | нет | Подключение к PostgreSQL (по умолчанию `postgres`, `5432`, `hack_backend`, `hack_user`) |
| `DB_PASS` | да, вне dev | Пароль БД |
| `JWT_SECRET` | да, вне dev | Секрет подписи JWT, ≥ 32 символов. Вне Development без него приложение не запустится |
| `JWT_EXPIRES_HOURS` | нет | Время жизни токена, по умолчанию `24` |
| `UPLOADS_PATH` | нет | Каталог загрузок, по умолчанию `/app/data/uploads` |
| `VK_GROUP_ACCESS_TOKEN` | нет | Токен сообщества VK. Пусто — бот и уведомления отключены |
| `VK_GROUP_ID` | нет | Числовой id сообщества |
| `VK_MESSAGES_INCLUDE_GROUP_ID` | нет | `1` — передавать `group_id` в `messages.send` |
| `VITE_API_URL` | фронт | Адрес API, по умолчанию `http://localhost:8080` |
| `VITE_VK_BOT_URL` | фронт | Ссылка «Открыть бота» (`https://vk.com/im?sel=-<id>`) |

### Настройка VK-бота

1. В сообществе: Управление → Работа с API → создать ключ с правом на сообщения.
2. Там же включить Bots Long Poll и событие `message_new`.
3. Разрешить сообщения сообщества.
4. Записать токен и id в `backend/.env`.

## Структура

```
backend/    ASP.NET Core 8 API: контроллеры, сервисы, EF-миграции
frontend/   Vue 3 + Vite, слои FSD
docs/       подробная документация
```

- [docs/BACKEND.md](./docs/BACKEND.md) — API, модели, фоновые сервисы
- [docs/FRONTEND.md](./docs/FRONTEND.md) — слои FSD, маршруты, правила разработки

## Планы

- Пагинация списка сдач и rate limiting для логина
- VK OAuth для автоматической привязки аккаунта
- S3-совместимое хранилище файлов
- Push-уведомления в браузере (SSE)
- Очередь уведомлений
- Тесты: Testcontainers для бэкенда, Playwright для фронта
- Роль администратора
