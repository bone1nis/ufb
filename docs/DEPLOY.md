# Деплой — переменные для DevOps (team-1)

Публичный фронт: **https://team-1.hack.kam-dev.ru**

> API обычно на отдельном поддомене: **https://api.team-1.hack.kam-dev.ru** — уточните у платформы, если другой URL.

---

## Backend (контейнер `backend`)

Передавать как **environment variables** (или `backend/.env` + `env_file` в compose).

### Обязательные

| Переменная | Значение для team-1 | Комментарий |
|------------|---------------------|-------------|
| `DB_HOST` | `postgres` | Имя сервиса БД в Docker / хост PostgreSQL |
| `DB_PORT` | `5432` | |
| `DB_NAME` | `hack_backend` | Или как заведено на стенде |
| `DB_USER` | `hack_user` | |
| `DB_PASS` | *секрет* | Пароль PostgreSQL (**обязателен**, иначе пустая строка в connection string) |
| `JWT_SECRET` | *секрет* | Минимум **32 символа**, свой на проде |
| `JWT_EXPIRES_HOURS` | `24` | Срок жизни JWT в часах |
| `FRONTEND_ORIGIN` | `https://team-1.hack.kam-dev.ru` | CORS + ссылки в VK-боте. **Без слэша в конце** |
| `UPLOADS_PATH` | `/app/data/uploads` | Путь **внутри** контейнера |

### VK (чтобы работали уведомления и бот)

| Переменная | Значение | Комментарий |
|------------|----------|-------------|
| `VK_GROUP_ACCESS_TOKEN` | *masked secret* | Токен сообщества VK |
| `VK_GROUP_ID` | `238684561` | ID группы (число) |
| `VK_MESSAGES_INCLUDE_GROUP_ID` | `1` | Как в рабочем `.env` |

Без VK приложение работает, но бот и push в VK отключены.

### Опционально (если нет `FRONTEND_ORIGIN`)

| Переменная | Значение |
|------------|----------|
| `TEAM_SLUG` | `team-1` |
| `DOMAIN` | `hack.kam-dev.ru` |

Бэкенд сам соберёт origin: `https://team-1.hack.kam-dev.ru`.

### Volume для файлов сдач

`UPLOADS_PATH` должен указывать на каталог, **смонтированный как volume**, иначе файлы пропадут при перезапуске:

```yaml
services:
  backend:
    environment:
      UPLOADS_PATH: /app/data/uploads
    volumes:
      - uploads:/app/data/uploads
      # или на проде:
      # - /srv/team-1/uploads:/app/data/uploads

volumes:
  uploads:
```

### Пример блока для docker-compose (как у вас локально, но для прода)

```yaml
environment:
  DB_HOST: postgres
  DB_PORT: 5432
  DB_NAME: hack_backend
  DB_USER: hack_user
  DB_PASS: <секрет>
  JWT_SECRET: <секрет-32+-символов>
  JWT_EXPIRES_HOURS: "24"
  FRONTEND_ORIGIN: "https://team-1.hack.kam-dev.ru"
  UPLOADS_PATH: /app/data/uploads
  VK_GROUP_ACCESS_TOKEN: <masked>
  VK_GROUP_ID: "238684561"
  VK_MESSAGES_INCLUDE_GROUP_ID: "1"
```

Плюс `env_file: ./backend/.env` — если секреты лежат в файле на сервере.

---

## Frontend (сборка образа)

Переменные **`VITE_*` задаются только при `npm run build`**, в runtime контейнера nginx их уже нет.

### Build-args (Docker / GitLab CI)

| Переменная | Значение для team-1 | Комментарий |
|------------|---------------------|-------------|
| `VITE_API_URL` | `https://api.team-1.hack.kam-dev.ru` | URL бэкенда в браузере (axios, скачивание файлов) |
| `VITE_VK_BOT_URL` | `https://vk.com/im?sel=-238684561` | Кнопка «Открыть бот ВК» (`sel=-{VK_GROUP_ID}`) |

### GitLab CI (платформа)

```yaml
EXTRA_BUILD_ARGS: "VITE_API_URL=https://api.team-1.hack.kam-dev.ru VITE_VK_BOT_URL=https://vk.com/im?sel=-238684561"
```

Или отдельно `API_BASE_URL` → в пайплайне мапится в `VITE_API_URL` (см. `frontend/.gitlab-ci.yml`).

### Локально (для справки)

`frontend/.env`:

```env
VITE_API_URL=http://localhost:8080
VITE_VK_BOT_URL=https://vk.com/im?sel=-238684561
```

---

## Чеклист «всё работает»

- [ ] `FRONTEND_ORIGIN` = `https://team-1.hack.kam-dev.ru` (совпадает с URL в браузере)
- [ ] `VITE_API_URL` при сборке фронта = публичный URL API (HTTPS)
- [ ] CORS: бэкенд видит origin фронта
- [ ] Cookie `auth_token`: фронт и API на одном «сайте» по политике браузера, или настроен proxy / SameSite
- [ ] Volume на `/app/data/uploads` смонтирован
- [ ] `JWT_SECRET` не дефолтный на проде
- [ ] После смены API в фронте — **пересборка** образа frontend (не только restart)

---

## Проверка

| Что | URL |
|-----|-----|
| Фронт | https://team-1.hack.kam-dev.ru |
| API health | https://api.team-1.hack.kam-dev.ru/health |
| Swagger | https://api.team-1.hack.kam-dev.ru/swagger |
| VK diag (если включён) | https://api.team-1.hack.kam-dev.ru/api/vk/diag |

Демо-логин: `student@demo.ru` / `teacher@demo.ru` / `methodist@demo.ru`, пароль `password`.
