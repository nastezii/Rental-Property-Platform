# Rental Property Platform

## Опис проєкту

**Rental Property Platform** — це API для платформи оренди житла, що дозволяє власникам виставляти нерухомість на оренду, а орендарям — знаходити та бронювати житло. Система керує повним життєвим циклом оренди: від створення оголошення до укладання брогювання та відгуків.

### Предметна область

Платформа охоплює:

- Управління нерухомістю (квартири, будинки, готелі)
- Оголошення про оренду
- Система бронювання
- Система відгуків
- Пошуки

### Склад команди:

- Анастасія

---

## Технологічний стек

| Технологія                | Версія | Призначення                  |
| ------------------------- | ------ | ---------------------------- |
| **C#**                    | 12     | Мова програмування           |
| **.NET**                  | 8      | Runtime та фреймворк         |
| **PostgreSQL**            | 18     | Реляційна база даних         |
| **Entity Framework Core** | 8      | ORM для роботи з БД          |
| **xUnit**                 | 2.5.3  | Фреймворк тестування         |
| **FluentAssertions**      | 8.8.0  | Assertion library для тестів |

---

## Інструкції з налаштування

### Передумови

> [!IMPORTANT]
> Переконайтеся, що у вас встановлений [Docker Desktop](https://www.docker.com/get-started) (версія 20.10+)

### Крок 1: Клонування репозиторію

```git clone https://github.com/nastezii/Rental-Property-Platform.git
cd Rental-Property-Platform

### Крок 2: Запуск через Docker Compose

```bash
# Збірка та запуск всіх сервісів
docker-compose up -d --build

# Перевірка статусу контейнерів
docker-compose ps
```

Сервіси будуть доступні:

- **API**: http://localhost:8080
- **Swagger UI**: http://localhost:8080/swagger/index.html
- **PostgreSQL**: localhost:5432

### Крок 3: Застосування міграцій

Міграції застосовуються автоматично при запуску API. Якщо потрібно виконати вручну:

```bash
cd ../WebApplication1/Infrastructure
dotnet ef database update
```

---

## Запуск додатку

### Запуск через Docker Compose

```bash
# Запуск у фоновому режимі
docker-compose up -d --build

# Зупинка
docker-compose down

# Зупинка з видаленням volumes (очистить БД)
docker-compose down -v
```

### Запуск через Docker Compose за допомогою IDE

- Обрати конфігурацію проекту Docker Compose Deployment
- Run Docker Compose Deployment

---

## Запуск тестів

> [!IMPORTANT]  
> Переконайтесь, що контейнер з БД запущений

### Запуск всіх тестів

```bash
dotnet test
```

### Запуск конкретного тестового файлу

```bash
# Інтеграційні тести
dotnet test --filter "FullyQualifiedName~IntegrationTests"

# Юніт тести
dotnet test --filter "FullyQualifiedName~UnitTests"

# Тести сценаріїв збоїв
dotnet test --filter "FullyQualifiedName~FailureScenarioTests"
```

### Запуск з покриттям коду та генерація звіту покриття

```bash
dotnet test --collect:"XPlat Code Coverage"
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:Tests/TestResults/**/coverage.cobertura.xml -targetdir:coveragereport -reporttypes:Html
cd ../WebApplication1/coveragereport
```

### Запуск з покриттям коду та генерація звіту покриття через IDE

- Відкрити вікно тестів
- Cover Unit Tests

---

## Структура проекту

```
├── docker-compose.yml           # Конфігурація Docker Compose
├── Dockerfile                   # Dockerfile для API
├── README.md                    # Основна документація
│
├── Domain/                      # Доменний шар
│   ├── Entities/                # Сутності бази даних
│   ├── Dtos/                    # Data Transfer Objects
│   └── Enums/                   # Енами
│
├── Application/                 # Бізнес-логіка
│
├── Infrastructure/              # Інфраструктурний шар
│   ├── Configurations/          # Конфігурації зв'язків між сутностями
│   ├── Migrations/              # Міграції БД
│   ├── Repositories/            # Репозиторії для операцій з БД
│
├── WebApplication1/             # API шар
│   ├── Controllers/             # REST API endpoints
│   ├── Program.cs               # Точка входу
|   ├── MappingProfiles.cs       # AutoMapper профілі
|   ├── Dockerfile               # Dockerfile для API
│   └── appsettings.json         # Конфігурація
|
├── Tests/                       # Тести
│   ├── IntegrationTests         # Інтеграційні тести
│   ├── UnitTests.cs             # Юніт тести
|   ├── FailureScenarioTests.cs  # Тести сценаріїв збоїв
|
└── docs/                        # Документація
    ├── schema.md                # Схема бази даних з ERD
    └── queries.md               # Пояснення складних запитів
```

### Опис шарів

- **Domain**: Сутності, енами та DTO. Не залежить від інших шарів
- **Infrastructure**: Робота з БД, міграції, конфігурації EF Core. Залежить від Domain
- **Application**: Бізнес-логіка, сервіси. Залежить від Domain та Infrastructure
- **WebApplication1**: REST API, контролери, middleware. Залежить від усіх шарів вище
- **Tests**: Інтеграційні, юніт тести та сценарії збоїв, Залежить від усіх шарів вище

---

## Приклади використання API

### 1. Створення користувача з оголошенням

**POST** `/api`

```json
{
  "user": {
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "password": "SecurePass123!"
  },
  "property": {
    "title": "Cozy 2-Bedroom Apartment",
    "description": "Beautiful apartment in the city center with modern amenities",
    "location": "Kyiv, Shevchenko District",
    "propertyType": "Apartments",
    "amenities": [1]
  },
  "listing": {
    "price": 15000.0,
    "currency": "UAH",
    "minRentalPeriodInMonths": 6
  }
}
```

**Response 200 OK:**

```json
{
  "id": 1,
  "propertyId": 1,
  "price": 15000.0,
  "currency": "UAH",
  "minRentalPeriodInMonths": 6,
  "status": "Active",
  "property": {
    "id": 1,
    "userId": 1,
    "description": "Beautiful apartment in the city center with modern amenities",
    "title": "Cozy 2-Bedroom Apartment",
    "location": "Kyiv, Shevchenko District",
    "propertyType": "Apartments",
    "rating": 0,
    "amenities": [
      {
        "id": 1,
        "name": "Test",
        "description": "TestTestTest"
      }
    ]
  }
}
```

### 2. Оновлення нерухомості

**PATCH** `/api?id=1`

```json
{
  "title": "Updated Title",
  "description": "Updated description",
  "location": "Kyiv, Pechersk District",
  "propertyType": "Apartments",
  "amenities": [1]
}
```

**Response 200 OK**

```json
{
  "id": 1,
  "userId": 1,
  "title": "Updated Title",
  "description": "Updated description",
  "location": "Kyiv, Pechersk District",
  "propertyType": "Apartments",
  "rating": 4,
  "amenities": [
    {
      "id": 1,
      "name": "Test",
      "description": "TestTestTest"
    }
  ]
}
```

### 3. Видалення нерухомості

**DELETE** `/api?id=1`

**Response 200 OK**

### 4. Отримання посортованої нерухомості за рейтингом

**GET** `/api/properties`

**Response 200 OK:**

```json
[
  {
    "id": 1,
    "userId": 1,
    "title": "string",
    "description": "string string",
    "location": "string",
    "propertyType": "Apartments",
    "rating": 5,
    "amenities": [
      {
        "id": 1,
        "name": "Test",
        "description": "TestTestTest"
      }
    ]
  },
  {
    "id": 2,
    "userId": 2,
    "title": "strin2",
    "description": "string strin2",
    "location": "strin2",
    "propertyType": "Hotel",
    "rating": 4,
    "amenities": [
      {
        "id": 2,
        "name": "Test2",
        "description": "TestTestTest2"
      }
    ]
  }
]
```

### 5. Отримання активних оголошень (з пагінацією)

**GET** `/api/listings?page=1&pageSize=1`

**Response 200 OK:**

```json
[
  {
    "id": 1,
    "propertyId": 2,
    "price": 20,
    "currency": "USD",
    "minRentalPeriodInMonths": 6,
    "status": "Active",
    "property": {
      "id": 1,
      "userId": 1,
      "title": "string",
      "description": "stringstring",
      "location": "string",
      "propertyType": "Apartments",
      "rating": 5,
      "amenities": [
        {
          "id": 1,
          "name": "Test",
          "description": "TestTestTest"
        }
      ]
    }
  }
]
```

### 6. Отримання аналітики

**GET** `/api/analytics`

**Response 200 OK:**

```json
[
  {
    "propertyId": 1,
    "propertyTitle": "Test Property",
    "totalBookings": 2,
    "avgDuration": 136.5,
    "minDuration": 91,
    "maxDuration": 182,
    "revenues": [
      {
        "currency": "EUR",
        "totalRevenue": 1365
      },
      {
        "currency": "USD",
        "totalRevenue": 3640
      }
    ]
  },
  {
    "propertyId": 2,
    "propertyTitle": "Test Property 2",
    "totalBookings": 1,
    "avgDuration": 121,
    "minDuration": 121,
    "maxDuration": 121,
    "revenues": [
      {
        "currency": "USD",
        "totalRevenue": 4840
      }
    ]
  }
]
```
