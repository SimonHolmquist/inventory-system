# Inventory System - .NET Technical Challenge

This project implements an inventory notification system using two microservices built with **.NET 8** and communicating via **RabbitMQ**.

## 🧩 Architecture

```
+-------------------+        RabbitMQ         +-------------------------+
|                   |  --------------------> |                         |
|  Inventory.API    |  [inventory_exchange]   |  Notification.Service   |
|  (Producer)       |                        |  (Consumer)             |
+-------------------+                        +-------------------------+
```

- **Inventory.API**: Exposes a RESTful API to manage products.
- **Notification.Service**: Listens to product events and logs them to a local database.

---

## 🚀 Services

### 📦 Inventory.API

RESTful API for inventory product management.

**Endpoints:**
- `GET /api/products` - List all products
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create new product
- `PUT /api/products/{id}` - Update existing product
- `DELETE /api/products/{id}` - Delete product

**Product model:**
```json
{
  "id": 1,
  "name": "Product A",
  "description": "Product description",
  "price": 100.00,
  "stock": 50,
  "category": "Electronics"
}
```

Each action emits a corresponding event to RabbitMQ.

---

### 📬 Notification.Service

This service consumes events from RabbitMQ:

- Subscribed to:
  - `product.created.queue`
  - `product.updated.queue`
  - `product.deleted.queue`
- Saves all received events into a local SQLite database.
- Implements basic error handling and validation.

---

## ⚙️ Technologies Used

- .NET 8
- RabbitMQ
- Entity Framework Core
- SQLite
- Docker / Docker Compose
- Swagger (for Inventory.API)

---

## 🐳 Running with Docker

1. Clone the repository:

```bash
git clone https://github.com/SimonHolmquist/inventory-system.git
cd inventory-system
```

2. Start with Docker Compose:

```bash
docker-compose up --build
```

3. Access via browser:

- Inventory API: [http://localhost:5274/swagger](http://localhost:5274/swagger)
- RabbitMQ UI: [http://localhost:15672](http://localhost:15672) (user/password: guest/guest)

---

## 🗂️ Project Structure

```
inventory-system/
│
├── Inventory.API/              # Producer microservice
│   ├── Controllers/
│   ├── Data/
│   ├── Models/
│   ├── Messaging/               # Publisher and config
│   ├── DTOs/
│   ├── Migrations/
│
├── Notification.Service/       # Consumer microservice
│   ├── Data/
│   ├── Models/
│   ├── Data/
│   ├── Resilience/
│   ├── Messaging/               # Consumer and config
|   ├── Migrations/
│
├── docker-compose.yml
└── README.md
```

---

## 📌 Notes

- ✅ A custom Circuit Breaker was implemented in Notification.Service without Polly.

- It opens after 3 consecutive message processing failures and remains open for 15 seconds.

- During this time, new messages are requeued, and processing is paused.

- The circuit automatically resets after the cooldown period, ensuring resiliency.

---

## 📧 Contact

Developed by **Simón Holmquist**  
[GitHub](https://github.com/SimonHolmquist)