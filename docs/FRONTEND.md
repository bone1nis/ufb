# Frontend — U.F.B.

Vue 3 + TypeScript + Vite. Архитектура **Feature-Sliced Design (FSD)**.

## Запуск

```bash
cd frontend
npm install
npm run dev
```

Переменная `VITE_API_URL` (по умолчанию `http://localhost:8080`).  
В Docker фронт: http://localhost:5173

## Правила FSD

Импорты только **снизу вверх**:

```
shared → entities → features → widgets → pages → app
```

| Слой | Назначение |
|------|------------|
| **app** | Роутер, провайдеры, точка входа |
| **pages** | Страницы-композиции (минимум логики и разметки) |
| **widgets** | Крупные блоки UI (кабинет, карусель) |
| **features** | Пользовательские сценарии (auth, фильтр, VK) |
| **entities** | Сущности предметной области (submission, homework, user) |
| **shared** | API, утилиты, переиспользуемый UI |

## Структура `frontend/src/`

```
src/
├── app/router/              # маршруты + role guard
├── pages/
│   ├── login/ui/
│   ├── student/ui/          # Dashboard, Due, Submissions, Submit, Detail
│   ├── teacher/ui/
│   └── methodist/ui/
├── widgets/
│   ├── layout/ui/           # AppLayout (шапка, drawer)
│   └── student-dashboard/ui/
│       ├── StudentDashboardHero.vue
│       └── StudentSubmissionsCarousel.vue
├── features/
│   ├── auth/model/          # useAuthStore (Pinia)
│   ├── vk-link/ui/          # VkLinkWidget
│   ├── submission-filter/ui/
│   │   ├── SubmissionListSearchBar.vue
│   │   └── SubmissionFilterSheet.vue
│   └── student-stats/model/ # useStudentMonthStats
├── entities/
│   ├── user/model/
│   ├── homework/model/
│   └── submission/
│       ├── model/types.ts
│       ├── lib/submissionScore.ts
│       └── ui/
│           ├── SubmissionStatusBadge.vue
│           ├── SubmissionScoreBlock.vue
│           └── SubmissionBriefCard.vue
└── shared/
    ├── api/axios.ts
    ├── lib/date.ts, homeworkGroupTag.ts
    ├── config/submissionFilters.ts
    └── ui/
        ├── HeroBand.vue
        ├── ProfileHeader.vue
        ├── ProgressStatsCard.vue
        ├── CardCarousel.vue
        └── BackLink.vue
```

## Маршруты

| Путь | Роль | Страница |
|------|------|----------|
| `/login` | — | LoginPage |
| `/student` | student | DashboardPage |
| `/student/due` | student | DueHomeworkPage |
| `/student/submissions` | student | SubmissionsPage |
| `/student/submissions/:id` | student | SubmissionDetailPage |
| `/student/submit` | student | SubmitPage |
| `/teacher` | teacher | DashboardPage |
| `/teacher/submissions` | teacher | SubmissionsPage |
| `/teacher/submissions/:id` | teacher | SubmissionDetailPage |
| `/methodist` | methodist | DashboardPage |
| `/methodist/homeworks/new` | methodist | HomeworkNewPage (`?edit=id`) |

## Ключевые компоненты

### shared/ui

- **HeroBand** — фиолетовая шапка профиля (`variant`: `full` | `compact`)
- **ProfileHeader** — аватар, имя, теги роли
- **ProgressStatsCard** — статистика с прогресс-барами
- **CardCarousel** — заголовок + стрелки, `v-model` индекса слайда
- **BackLink** — ссылка «← Назад»

### entities/submission/ui

- **SubmissionStatusBadge** — «Проверено» / «На проверке»
- **SubmissionScoreBlock** — оценка и прогресс 0–100
- **SubmissionBriefCard** — карточка сдачи в карусели кабинета

### widgets/student-dashboard

- **StudentDashboardHero** — профиль + статистика месяца + VK
- **StudentSubmissionsCarousel** — блок «Сдано»

### features/submission-filter

Поиск и bottom-sheet фильтров (направление, проект, курс) — используется на страницах сдач студента и преподавателя.

## Страницы vs компоненты

**pages** только:
- загружают данные (`onMounted`, API);
- собирают widgets/features;
- задают layout страницы (отступы, сетка).

Пример — `pages/student/ui/DashboardPage.vue`:

```vue
<StudentDashboardHero :submissions="submissions" />
<StudentSubmissionsCarousel :submissions="submissions" :loading="loading" />
<RouterLink to="/student/due">Дедлайны ДЗ</RouterLink>
```

## Переменные окружения

Файл `frontend/.env` (пример: `frontend/.env.example`).

| Переменная | Пример | Описание |
|------------|--------|----------|
| `VITE_API_URL` | `http://localhost:8080` | Базовый URL бэкенда. При сборке в Docker передаётся как `--build-arg VITE_API_URL=...` → вшивается в бандл (Vite `import.meta.env`) |
| `VITE_VK_BOT_URL` | `https://vk.com/im?sel=-238684561` | Ссылка «Открыть бот ВКонтакте» на виджете VkLinkWidget. Формат: `https://vk.com/im?sel=-{VK_GROUP_ID}` |

> **Важно:** `VITE_*` переменные вшиваются на этапе `npm run build`, а не в runtime. Если передать неверный URL при сборке Docker-образа — фронт будет стучаться не туда. Всегда передавайте значение через build arg:

```yaml
# docker-compose.dev.yml
frontend:
  build:
    args:
      VITE_API_URL: "http://your-backend-host:8080"
```

## API-клиент

`shared/api/axios.ts`:
- `baseURL` из `VITE_API_URL`
- `withCredentials: true` для cookie JWT

## Стили

- Глобальный градиент: `src/style.css` (`body`)
- Кабинеты: `HeroBand` + контент на градиенте body
- Экраны деталей/«К сдаче»: `gradient-2.png` (локально на странице)

## Сборка

```bash
npm run build   # vue-tsc + vite build
```

Для Docker после изменений бэкенда пересобирайте оба контейнера при необходимости.

## Добавление нового UI

1. Переиспользуемый атом → `shared/ui/`
2. Привязан к сущности → `entities/<name>/ui/`
3. Сценарий (форма, фильтр) → `features/<name>/`
4. Блок из нескольких features → `widgets/<name>/`
5. Маршрут → `pages/...` + запись в `app/router/index.ts`
