# U.F.B. — документация проекта

**Unified Feedback Board** — платформа сдачи и проверки домашних заданий для студентов, преподавателей и методистов.

## Содержание

| Документ | Описание |
|----------|----------|
| [BACKEND.md](./BACKEND.md) | API, структура .NET, БД, переменные окружения, VK-бот |
| [FRONTEND.md](./FRONTEND.md) | FSD-архитектура, компоненты, маршруты, разработка UI |
| [DEPLOY.md](./DEPLOY.md) | **Для DevOps:** переменные backend + frontend (team-1) |

## Быстрый старт

```bash
# из корня репозитория
docker compose -f docker-compose.dev.yml up --build
```

| Сервис | URL |
|--------|-----|
| Frontend | http://localhost:5173 |
| Backend API | http://localhost:8080 |
| Swagger | http://localhost:8080/swagger |

### Демо-аккаунты

| Email | Пароль | Роль |
|-------|--------|------|
| `student@demo.ru` | `password` | Студент |
| `teacher@demo.ru` | `password` | Преподаватель |
| `methodist@demo.ru` | `password` | Методист |

## Структура монорепозитория

```
xakaton_2026/
├── backend/          # ASP.NET Core 8 API
├── frontend/         # Vue 3 + Vite (FSD)
├── docs/             # Документация (этот каталог)
├── docker-compose.dev.yml
└── README.md         # Краткий обзор и ссылки
```

## Роли и сценарии

1. **Методист** создаёт ДЗ (`POST /api/methodist/homeworks`).
2. **Студент** видит задания «К сдаче», сдаёт работу (`POST /api/student/submissions`).
3. **Преподаватель** проверяет сдачи и выставляет оценку (`POST /api/teacher/submissions/{id}/grade`).
4. **Студент** получает уведомление в VK (если привязан аккаунт).

## Стек

- **Frontend:** Vue 3, TypeScript, Vue Router, Pinia, Naive UI, Axios
- **Backend:** .NET 8, EF Core, PostgreSQL, JWT (cookie `auth_token`)
- **Интеграции:** VK Long Poll, напоминания о дедлайнах

## После изменений API

Пересоберите контейнер бэкенда, иначе в Docker может работать старая версия без новых маршрутов:

```bash
docker compose -f docker-compose.dev.yml build backend
docker compose -f docker-compose.dev.yml up -d backend
```
