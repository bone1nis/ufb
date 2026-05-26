# Frontend — Документация проекта

Vue 3 + TypeScript + Vite SPA. Система сдачи домашних заданий для студентов, преподавателей и методистов.

---

## Содержание

- [Стек](#стек)
- [Запуск](#запуск)
- [Архитектура](#архитектура)
- [Роли и маршруты](#роли-и-маршруты)
- [Страницы](#страницы)
- [Слои FSD](#слои-fsd)
- [API-интеграция](#api-интеграция)
- [Авторизация](#авторизация)
- [Переменные окружения](#переменные-окружения)
- [Docker](#docker)

---

## Стек

| Технология | Версия | Роль |
|---|---|---|
| Vue 3 | ^3.x | UI-фреймворк (Composition API) |
| TypeScript | ~5.x | Типизация |
| Vite | ^6.x | Сборщик и dev-сервер |
| Vue Router | ^4.x | SPA-маршрутизация |
| Pinia | ^2.x | Глобальное состояние |
| Naive UI | ^2.x | Компонентная библиотека |
| Axios | ^1.x | HTTP-клиент |

---

## Запуск

### Через Docker (рекомендуется)

```bash
# Из корня монорепы
docker compose -f docker-compose.dev.yml up --build -d
```

Фронт: **http://localhost:5173**  
API: **http://localhost:8080**

### Локально без Docker

```bash
cd frontend
cp env.example .env
npm install
npm run dev       # dev-сервер на :5173
npm run build     # production-сборка в dist/
npm run preview   # preview production-сборки
```

---

## Архитектура

Проект следует методологии **Feature-Sliced Design (FSD)**:

```
src/
├── app/            # Точка входа, роутер, провайдеры
├── pages/          # Страницы по ролям
│   ├── login/
│   ├── student/
│   ├── teacher/
│   └── methodist/
├── widgets/        # Крупные независимые блоки UI
│   ├── layout/     # AppLayout (шапка, дравер, навигация)
│   └── student-dashboard/
├── features/       # Бизнес-фичи
│   ├── auth/       # Логин, логаут, хранилище пользователя
│   ├── student-stats/
│   ├── submission-filter/
│   └── vk-link/
├── entities/       # Доменные типы и примитивы
│   ├── user/
│   ├── homework/
│   └── submission/
└── shared/         # Переиспользуемые утилиты
    ├── api/        # axios-инстанс
    ├── config/     # env, константы
    ├── lib/        # date, pageMeta, helpers
    └── ui/         # Общие компоненты (HeroBand, ProfileHeader и др.)
```

---

## Роли и маршруты

В системе три роли. После логина роутер автоматически редиректит на домашнюю страницу роли.

| Роль | Домашняя страница | Доступные разделы |
|---|---|---|
| `student` | `/student` | Кабинет, К сдаче, Мои сдачи, Сдать ДЗ |
| `teacher` | `/teacher` | Кабинет, Сдачи (проверка) |
| `methodist` | `/methodist` | Кабинет, Создать ДЗ |

Гости (без токена) всегда попадают на `/login`. Попытка зайти на страницу чужой роли — редирект на свою домашнюю.

---

## Страницы

### Студент

| Страница | Путь | Описание |
|---|---|---|
| `DashboardPage` | `/student` | Профиль, статистика за месяц, карусель сдач, кнопка дедлайнов |
| `DueHomeworkPage` | `/student/due` | Список несданных заданий с дедлайнами, сортировка |
| `SubmissionsPage` | `/student/submissions` | Все сдачи студента, фильтры, поиск, сортировка |
| `SubmissionDetailPage` | `/student/submissions/:id` | Детали сдачи: файлы, ссылки, оценка, комментарий преподавателя |
| `SubmitPage` | `/student/submit` | Форма сдачи (выбор ДЗ, загрузка файлов / ссылок) |
| `SubmitPage` (edit) | `/student/submissions/:id/edit` | Редактирование сдачи в статусе «На проверке» |

### Преподаватель

| Страница | Путь | Описание |
|---|---|---|
| `DashboardPage` | `/teacher` | Профиль, счётчик непроверенных, список студентов с последними сдачами |
| `SubmissionsPage` | `/teacher/submissions` | Все сдачи всех студентов, поиск, фильтры |
| `SubmissionDetailPage` | `/teacher/submissions/:id` | Детали сдачи + форма выставления оценки 0–100 и комментария |

### Методист

| Страница | Путь | Описание |
|---|---|---|
| `DashboardPage` | `/methodist` | Профиль, карусель созданных заданий |
| `HomeworkNewPage` | `/methodist/homeworks/new` | Форма создания ДЗ (название, описание, проект, направление, курс, дедлайн) |
| `HomeworkNewPage` (edit) | `/methodist/homeworks/new?edit=:id` | Редактирование существующего задания |

---

## Слои FSD

### features/auth

**`useAuthStore`** (Pinia) — единственный источник правды о текущем пользователе.

| Метод/свойство | Тип | Описание |
|---|---|---|
| `user` | `User \| null` | Текущий авторизованный пользователь |
| `isLoggedIn` | `boolean` | Computed |
| `loading` | `boolean` | Флаг запроса логина |
| `initialized` | `boolean` | Флаг первой проверки сессии |
| `fetchMe()` | `Promise<void>` | Проверяет сессию через `GET /api/auth/me` |
| `login(email, password)` | `Promise<User>` | Логин, устанавливает `user` |
| `logout()` | `Promise<void>` | Логаут, очищает `user` |
| `linkVk(vkUserId)` | `Promise<any>` | Привязывает ВКонтакте |

### features/student-stats

**`useStudentMonthStats(submissions)`** — вычисляет статистику текущего месяца:
- кол-во сдач (`count`)
- средняя оценка (`avg`)
- успеваемость в процентах (`percent` от 100, зажат в 0–100)

### features/submission-filter

**`SubmissionListSearchBar`** — строка поиска + кнопка «Фильтры» для страниц с листингом сдач.  
**`SubmissionFilterSheet`** — боттом-шит / попап с фильтрами по направлению, проекту, курсу.

### features/vk-link

**`VkLinkWidget`** — блок привязки ВКонтакте (показывает статус, кнопка открывает модалку с полем для VK User ID).

### entities/submission

| Тип | Описание |
|---|---|
| `Submission` | Основная сущность сдачи |
| `SubmissionStatus` | `'pending' \| 'graded'` |
| `SubmissionItem` | Вложение (файл или ссылка) |
| `SubmissionGrade` | Оценка: score, comment, graded_at, teacher |

### shared/api

**`axios`** — настроенный инстанс с `baseURL` из `VITE_API_URL`, `withCredentials: true` (для cookie-авторизации).

### shared/ui

| Компонент | Описание |
|---|---|
| `HeroBand` | Полноэкранный hero-блок с фиолетовым градиентом. Variants: `full`, `compact` |
| `ProfileHeader` | Аватар + имя + теги (роль, курс) |
| `ProgressStatsCard` | Карточка с прогресс-барами статистики |

---

## API-интеграция

Все запросы идут на `VITE_API_URL` (дефолт: `http://localhost:8080`).  
Авторизация через **HTTP-only cookie** `auth_token` (устанавливается сервером при логине).

### Используемые эндпоинты

**Auth**
```
POST /api/auth/login          — логин
GET  /api/auth/me             — текущий пользователь
POST /api/auth/logout         — выход
POST /api/auth/vk             — привязать VK User ID
```

**Студент**
```
GET  /api/student/homeworks           — все задания
GET  /api/student/homeworks/todo      — несданные задания
GET  /api/student/submissions         — свои сдачи
GET  /api/student/submissions/:id     — детали сдачи
POST /api/student/submissions         — создать сдачу (multipart/form-data)
PATCH /api/student/submissions/:id    — обновить сдачу
```

**Преподаватель**
```
GET  /api/teacher/submissions                        — все сдачи
GET  /api/teacher/submissions/:id                    — детали сдачи
GET  /api/teacher/submissions/:id/files/:itemId      — скачать файл
POST /api/teacher/submissions/:id/grade              — выставить оценку
```

**Методист**
```
GET  /api/methodist/homeworks         — свои задания
POST /api/methodist/homeworks         — создать задание
GET  /api/methodist/homeworks/:id     — конкретное задание
PUT  /api/methodist/homeworks/:id     — обновить задание
```

---

## Авторизация

- После логина сервер устанавливает **HTTP-only cookie** `auth_token` (JWT, 7 дней).
- При каждом открытии приложения роутер вызывает `authStore.fetchMe()` → проверяет cookie на сервере.
- Если `/api/auth/me` вернул 401 — пользователь редиректится на `/login`.
- Авторизация по роли: роутер сравнивает `user.role` с `meta.role` маршрута.

---

## Переменные окружения

Задаются через `.env` (копируй из `env.example`):

| Переменная | По умолчанию | Описание |
|---|---|---|
| `VITE_API_URL` | `http://localhost:8080` | Базовый URL бекенд-API |

В Docker передаётся через `--build-arg VITE_API_URL=...` при сборке.

---

## Docker

```dockerfile
# Многоэтапная сборка:
# 1. node:20-alpine — npm install + vite build
# 2. nginx:alpine   — раздаёт dist/ на порту 8080
```

Nginx настроен на SPA-режим: все маршруты редиректятся на `index.html`.

```bash
# Сборка образа
docker build --build-arg VITE_API_URL=http://localhost:8080 -t frontend .

# Запуск
docker run -p 5173:8080 frontend
```
