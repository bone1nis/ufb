# Frontend

Vue 3 + TypeScript + Vite · Pinia · Vue Router · Naive UI · Axios. Архитектура **Feature-Sliced Design**.

## Запуск

```bash
cd frontend
cp .env.example .env
npm install
npm run dev        # http://localhost:5173
npm run build      # vue-tsc + vite build
```

Переменные: `VITE_API_URL` (адрес API, по умолчанию `http://localhost:8080`), `VITE_VK_BOT_URL` (ссылка «Открыть бота», `https://vk.com/im?sel=-<VK_GROUP_ID>`). Они вшиваются при сборке, не в рантайме; в Docker передаются как build arg (см. `docker-compose.dev.yml`).

## FSD: правила

Импорты только вниз по слоям: `shared → entities → features → widgets → pages → app`.

| Слой | Что лежит |
|------|-----------|
| **app** | Роутер с role guard, точка входа |
| **pages** | Страницы: грузят данные, собирают widgets/features, задают layout |
| **widgets** | Крупные блоки (layout, шапка кабинета студента, карусель) |
| **features** | Сценарии: auth, фильтр сдач, привязка VK, статистика месяца |
| **entities** | Предметные сущности: user, homework, submission |
| **shared** | API-клиент, конфиг, утилиты, базовый UI |

Куда класть новое: атом → `shared/ui`; привязано к сущности → `entities/<name>/ui`; сценарий → `features/<name>`; блок из нескольких features → `widgets/<name>`; страница → `pages/...` + маршрут в `app/router/index.ts`.

## Маршруты

| Путь | Роль | Страница |
|------|------|----------|
| `/login` | — | LoginPage |
| `/student` | student | DashboardPage |
| `/student/due` | student | DueHomeworkPage («К сдаче») |
| `/student/submit` | student | SubmitPage |
| `/student/submissions` | student | SubmissionsPage |
| `/student/submissions/:id` | student | SubmissionDetailPage |
| `/student/submissions/:id/edit` | student | SubmitPage (редактирование) |
| `/teacher` | teacher | DashboardPage |
| `/teacher/submissions` | teacher | SubmissionsPage |
| `/teacher/submissions/:id` | teacher | SubmissionDetailPage (проверка) |
| `/methodist` | methodist | DashboardPage |
| `/methodist/homeworks/new` | methodist | HomeworkNewPage (`?edit=id`) |

## API-клиент

`shared/api/axios.ts`: `baseURL` из `VITE_API_URL`, `withCredentials: true` (JWT в cookie). Адреса берите из `shared/config/env.ts`, не хардкодьте.

## Стили

Глобальный градиент — `src/style.css`; кабинеты строятся на `HeroBand` поверх него; экраны деталей и «К сдаче» используют `assets/bg/gradient-2.png`.
