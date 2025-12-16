# Документація схеми бази даних

## ER-діаграма (ERD)

```
┌─────────────────┐
│     users       │
├─────────────────┤
│ id (PK)         │
│ first_name      │
│ last_name       │
│ email (UQ)      │
│ password_hash   │
│ date_added      │
│ row_version     │
└────────┬────────┘
         │ 1
         │
         │ N
┌────────▼────────┐         ┌─────────────────┐
│   properties    │         │    amenities    │
├─────────────────┤         ├─────────────────┤
│ id (PK)         │         │ id (PK)         │
│ user_id (FK)    │         │ name (UQ)       │
│ title           │         │ description     │
│ description     │         │ row_version     │
│ location        │         └────────┬────────┘
│ property_type   │                  │ N
│ row_version     │                  │
└────────┬────────┘                  │
         │ 1                         │ 1
         │                           │
         │ N                ┌────────▼────────┐
┌────────▼────────┐         │property_amenity │
│    listings     │         ├─────────────────┤
├─────────────────┤         │ id (PK)         │
│ id (PK)         │         │ property_id (FK)│
│ property_id (FK)│◄────────┤ amenity_id (FK) │
│ price           │   N   1 │ row_version     │
│ currency        │         └─────────────────┘
│ min_rental_...  │
│ status          │
│ date_added      │
│ row_version     │
└────────┬────────┘
         │ 1
         │
         │ N
┌────────▼────────┐
│    bookings     │
├─────────────────┤
│ id (PK)         │
│ tenant_id (FK)  │──────┐
│ listing_id (FK) │      │
│ start_date      │      │ N
│ end_date        │      │
│ date_added      │      │ 1
│ row_version     │      │
└─────────────────┘      │
                         │
         ┌───────────────┘
         │
         │
┌────────▼────────┐
│    reviews      │
├─────────────────┤
│ id (PK)         │
│ reviewer_id (FK)├───────► users
│ property_id (FK)├───────► properties
│ rating          │
│ comment         │
│ date_added      │
│ row_version     │
└─────────────────┘
```

---

## Опис таблиць

### Таблиця: `users`

**Призначення:** Зберігає інформацію про облікові записи користувачів (власників нерухомості та орендарів)

**Стовпці:**

| Стовпець      | Тип          | Обмеження                             | Опис                                 |
| ------------- | ------------ | ------------------------------------- | ------------------------------------ |
| id            | INTEGER      | PRIMARY KEY, AUTO_INCREMENT           | Унікальний ідентифікатор користувача |
| first_name    | VARCHAR(100) | NOT NULL, CHECK(LENGTH >= 2)          | Ім'я користувача                     |
| last_name     | VARCHAR(100) | NOT NULL, CHECK(LENGTH >= 2)          | Прізвище користувача                 |
| email         | VARCHAR(100) | UNIQUE, NOT NULL, CHECK(LENGTH >= 7)  | Email для входу та комунікації       |
| password_hash | VARCHAR(255) | NOT NULL, CHECK(LENGTH >= 60)         | Хешований пароль (bcrypt)            |
| date_added    | TIMESTAMP    | NOT NULL, DEFAULT NOW()               | Дата реєстрації                      |
| row_version   | BYTEA        | NOT NULL, DEFAULT gen_random_bytes(8) | Для оптимістичного блокування        |

**Індекси:**

- `PK_users` на `id` (первинний ключ)
- `UQ_users_email` на `email` (унікальність email)

**Зв'язки:**

- Один-до-багатьох з `properties` (користувач може мати багато нерухомості)
- Один-до-багатьох з `bookings` (користувач може мати багато бронювань)
- Один-до-багатьох з `reviews` (користувач може залишити багато відгуків)

---

### Таблиця: `properties`

**Призначення:** Зберігає інформацію про об'єкти нерухомості

**Стовпці:**

| Стовпець      | Тип          | Обмеження                             | Опис                                 |
| ------------- | ------------ | ------------------------------------- | ------------------------------------ |
| id            | INTEGER      | PRIMARY KEY, AUTO_INCREMENT           | Унікальний ідентифікатор нерухомості |
| user_id       | INTEGER      | FOREIGN KEY → users(id), NOT NULL     | Власник нерухомості                  |
| title         | VARCHAR(255) | NOT NULL, CHECK(LENGTH >= 5)          | Назва оголошення                     |
| description   | TEXT         | NOT NULL, CHECK(LENGTH >= 10)         | Детальний опис нерухомості           |
| location      | VARCHAR(500) | NOT NULL, CHECK(LENGTH >= 5)          | Адреса/місцезнаходження              |
| property_type | VARCHAR(50)  | NOT NULL                              | Тип (Apartment/House/Studio/Room)    |
| row_version   | BYTEA        | NOT NULL, DEFAULT gen_random_bytes(8) | Для оптимістичного блокування        |

**Індекси:**

- `PK_properties` на `id` (первинний ключ)
- `IX_properties_user_id` на `user_id` (пошук нерухомості користувача)
- `IX_properties_location` на `location` (пошук за місцезнаходженням)

**Зв'язки:**

- Багато-до-одного з `users` (власник)
- Один-до-багатьох з `listings` (одна нерухомість може мати багато оголошень)
- Багато-до-багатьох з `amenities` (через `property_amenities`)
- Один-до-багатьох з `reviews` (відгуки про нерухомість)

---

### Таблиця: `amenities`

**Призначення:** Довідник зручностей нерухомості (WiFi, Parking тощо)

**Стовпці:**

| Стовпець    | Тип          | Обмеження                             | Опис                               |
| ----------- | ------------ | ------------------------------------- | ---------------------------------- |
| id          | INTEGER      | PRIMARY KEY, AUTO_INCREMENT           | Унікальний ідентифікатор зручності |
| name        | VARCHAR(100) | UNIQUE, NOT NULL, CHECK(LENGTH >= 2)  | Назва зручності                    |
| description | VARCHAR(500) | NOT NULL, CHECK(LENGTH >= 5)          | Опис зручності                     |
| row_version | BYTEA        | NOT NULL, DEFAULT gen_random_bytes(8) | Для оптимістичного блокування      |

**Індекси:**

- `PK_amenities` на `id` (первинний ключ)
- `UQ_amenities_name` на `name` (унікальність назви)

**Зв'язки:**

- Багато-до-багатьох з `properties` (через `property_amenities`)

---

### Таблиця: `property_amenities`

**Призначення:** Зв'язкова таблиця між `properties` та `amenities` (many-to-many)

**Стовпці:**

| Стовпець    | Тип     | Обмеження                              | Опис                             |
| ----------- | ------- | -------------------------------------- | -------------------------------- |
| id          | INTEGER | PRIMARY KEY, AUTO_INCREMENT            | Унікальний ідентифікатор зв'язку |
| property_id | INTEGER | FOREIGN KEY → properties(id), NOT NULL | Посилання на нерухомість         |
| amenity_id  | INTEGER | FOREIGN KEY → amenities(id), NOT NULL  | Посилання на зручність           |
| row_version | BYTEA   | NOT NULL, DEFAULT gen_random_bytes(8)  | Для оптимістичного блокування    |

**Індекси:**

- `PK_property_amenities` на `id` (первинний ключ)
- `UQ_property_amenities_property_amenity` на `(property_id, amenity_id)` (унікальність пари)

**Зв'язки:**

- Багато-до-одного з `properties`
- Багато-до-одного з `amenities`

**ON DELETE:** CASCADE (при видаленні нерухомості або зручності - видаляється зв'язок)

---

### Таблиця: `listings`

**Призначення:** Оголошення про оренду конкретної нерухомості з ціною та умовами

**Стовпці:**

| Стовпець                    | Тип           | Обмеження                              | Опис                                |
| --------------------------- | ------------- | -------------------------------------- | ----------------------------------- |
| id                          | INTEGER       | PRIMARY KEY, AUTO_INCREMENT            | Унікальний ідентифікатор оголошення |
| property_id                 | INTEGER       | FOREIGN KEY → properties(id), NOT NULL | Посилання на нерухомість            |
| price                       | DECIMAL(10,2) | NOT NULL, CHECK(price > 0)             | Ціна оренди за місяць               |
| currency                    | VARCHAR(3)    | NOT NULL                               | Валюта (UAH, USD, EUR)              |
| min_rental_period_in_months | INTEGER       | NOT NULL, CHECK(> 0)                   | Мінімальний термін оренди           |
| status                      | VARCHAR(50)   | NOT NULL                               | Active/Inactive/Rented              |
| date_added                  | TIMESTAMP     | NOT NULL, DEFAULT NOW()                | Дата створення оголошення           |
| row_version                 | BYTEA         | NOT NULL, DEFAULT gen_random_bytes(8)  | Оптимістичне блокування             |

**Індекси:**

- `PK_listings` на `id` (первинний ключ)
- `IX_listings_property_id` на `property_id` (пошук оголошень нерухомості)
- `IX_listings_status_price` на `(status, price)` (пошук активних з сортуванням за ціною)

**Зв'язки:**

- Багато-до-одного з `properties`
- Один-до-багатьох з `bookings`

---

### Таблиця: `bookings`

**Призначення:** Бронювання/оренда нерухомості орендарем

**Стовпці:**

| Стовпець    | Тип       | Обмеження                              | Опис                                |
| ----------- | --------- | -------------------------------------- | ----------------------------------- |
| id          | INTEGER   | PRIMARY KEY, AUTO_INCREMENT            | Унікальний ідентифікатор бронювання |
| tenant_id   | INTEGER   | FOREIGN KEY → users(id), NOT NULL      | Орендар                             |
| listing_id  | INTEGER   | FOREIGN KEY → listings(id), NOT NULL   | Оголошення                          |
| start_date  | TIMESTAMP | NOT NULL                               | Дата початку оренди                 |
| end_date    | TIMESTAMP | NOT NULL, CHECK(end_date > start_date) | Дата закінчення оренди              |
| date_added  | TIMESTAMP | NOT NULL, DEFAULT NOW()                | Дата створення бронювання           |
| row_version | BYTEA     | NOT NULL, DEFAULT gen_random_bytes(8)  | Оптимістичне блокування             |

**Індекси:**

- `PK_bookings` на `id` (первинний ключ)
- `IX_bookings_tenant_id` на `tenant_id` (бронювання користувача)
- `IX_bookings_listing_id` на `listing_id` (бронювання оголошення)
- `IX_bookings_start_date_end_date` на `(start_date, end_date)` (пошук за датами)

**Зв'язки:**

- Багато-до-одного з `users` (орендар)
- Багато-до-одного з `listings`

**ON DELETE:** CASCADE (при видаленні користувача або оголошення)

---

### Таблиця: `reviews`

**Призначення:** Відгуки користувачів про нерухомість після оренди

**Стовпці:**

| Стовпець    | Тип       | Обмеження                                    | Опис                             |
| ----------- | --------- | -------------------------------------------- | -------------------------------- |
| id          | INTEGER   | PRIMARY KEY, AUTO_INCREMENT                  | Унікальний ідентифікатор відгуку |
| reviewer_id | INTEGER   | FOREIGN KEY → users(id), NOT NULL            | Автор відгуку                    |
| property_id | INTEGER   | FOREIGN KEY → properties(id), NOT NULL       | Нерухомість                      |
| rating      | INTEGER   | NOT NULL, CHECK(rating >= 1 AND rating <= 5) | Оцінка від 1 до 5                |
| comment     | TEXT      | NOT NULL, CHECK(LENGTH >= 10)                | Текст відгуку                    |
| date_added  | TIMESTAMP | NOT NULL, DEFAULT NOW()                      | Дата публікації                  |
| row_version | BYTEA     | NOT NULL, DEFAULT gen_random_bytes(8)        | Оптимістичне блокування          |

**Індекси:**

- `PK_reviews` на `id` (первинний ключ)
- `IX_reviews_property_id` на `property_id` (відгуки нерухомості)
- `IX_reviews_reviewer_id` на `reviewer_id` (відгуки користувача)
- `IX_reviews_rating` на `rating` (фільтрація за рейтингом)

**Зв'язки:**

- Багато-до-одного з `users` (автор)
- Багато-до-одного з `properties`

**ON DELETE:** CASCADE

---

## Рішення щодо дизайну

### Чому обрана саме ця структура схеми?

1. **Розділення відповідальності:**

   - `users` - управління користувачами
   - `properties` - опис нерухомості (незмінні характеристики)
   - `listings` - умови оренди (ціна, статус - можуть змінюватися)
   - Це дозволяє одну нерухомість здавати за різними умовами (ціна, валюта, мінімальний термін)

2. **Багато-до-багатьох через проміжну таблицю:**

   - `property_amenities` дозволяє гнучко керувати зручностями
   - Легко додавати/видаляти зручності без зміни структури
   - Можливість розширити функціонал (наприклад, додати `is_included_in_price`)

3. **Відокремлення бронювань від договорів:**
   - `bookings` - факт оренди з датами
   - Зберігається історія всіх бронювань

### Досягнутий рівень нормалізації

** Третя нормальна форма (3NF)**

- **1НФ:** Всі атрибути атомарні, немає повторюваних груп
- **2НФ:** Немає часткових залежностей від складених ключів
- **3НФ:** Немає транзитивних залежностей

### Зроблені компроміси

1. **Денормалізація для продуктивності:**

   - НЕ додавали `average_rating` в `properties` (буде обчислюватися динамічно)
   - ПРИЧИНА: Дані часто змінюються, синхронізація складна

2. **Мультивалютність:**
   - Валюта зберігається як VARCHAR(3), а не FK на таблицю `currencies`
   - ПРИЧИНА: Набір валют обмежений та стабільний (UAH, USD, EUR)

### Стратегія індексування

#### Первинні індекси (автоматичні)

- Всі `id` (PRIMARY KEY) автоматично індексуються

#### Унікальні індекси

- `users.email` - запобігає дублям, прискорює login
- `amenities.name` - унікальність зручностей
- `property_amenities(property_id, amenity_id)` - запобігає дублям зв'язків

#### Індекси для пошуку та JOIN

```sql
-- Пошук нерухомості за локацією
CREATE INDEX idx_properties_location ON properties(location);

-- Пошук оголошень за власником через property
CREATE INDEX idx_properties_user_id ON properties(user_id);

-- Пошук активних оголошень з сортуванням за ціною
CREATE INDEX idx_listings_status_price ON listings(status, price);

-- Бронювання користувача
CREATE INDEX idx_bookings_tenant_id ON bookings(tenant_id);

-- Перевірка перекриття дат бронювань
CREATE INDEX idx_bookings_dates ON bookings(start_date, end_date);

-- Відгуки нерухомості для розрахунку рейтингу
CREATE INDEX idx_reviews_property_rating ON reviews(property_id, rating);
```

### Оптимістичне блокування

**row_version (BYTEA)** у всіх таблицях:

```sl
UPDATE listings
SET status = 'Rented', row_version = gen_random_bytes(8)
WHERE id = 1 AND row_version = @old_version;
```

Це запобігає **race conditions** при одночасному оновленні (наприклад, два користувачі бронюють одне оголошення).

---
