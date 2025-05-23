# Inventory System - .NET Microservices Challenge

This project implements a distributed inventory notification system using microservices built with **.NET 8** and communicating asynchronously through **RabbitMQ**.

## 🔧 Overview

The system is composed of two independent microservices:

- **Inventory.API** (Producer): Exposes a RESTful API for product management. Each operation publishes an event to RabbitMQ.
- **Notification.Service** (Consumer): Listens to RabbitMQ events and logs inventory changes to a local database.

This technical challenge provided an opportunity to apply modern design practices, implement resiliency in distributed systems, and automate the environment setup with Docker.

---

## 🧩 System Components

### Inventory.API

Responsible for managing inventory products.

- **Framework**: ASP.NET Core 8
- **Endpoints**:
  - `GET /api/products`
  - `GET /api/products/{id}`
  - `POST /api/products`
  - `PUT /api/products/{id}`
  - `DELETE /api/products/{id}`
- **Data Model**: ID, Name, Description, Price, Stock, Category
- **Messaging**: Publishes events to RabbitMQ after each action
- **API Documentation**: Available via Swagger

### Notification.Service

Consumes inventory-related events from RabbitMQ.

- **Subscriptions**:
  - `product.created.queue`
  - `product.updated.queue`
  - `product.deleted.queue`
- **Persistence**: SQLite local database
- **Error Handling**: Custom Circuit Breaker implementation
- **Validation**: Business and message validation logic included

---

## 🛠️ Technologies Used

- .NET 8
- RabbitMQ
- Entity Framework Core
- SQLite
- Docker / Docker Compose
- Swagger
- FluentValidation

---

## 🐳 Running with Docker

1. Clone the repository:

```bash
git clone https://github.com/SimonHolmquist/inventory-system.git
cd inventory-system
```

2. Start the services:

```bash
docker-compose up --build
```

3. Access:

- Swagger (Inventory API): [http://localhost:5274/swagger](http://localhost:5274/swagger)
- RabbitMQ UI: [http://localhost:15672](http://localhost:15672)  
  (username/password: guest / guest)

---

## 🧠 Key Concepts

- Microservices architecture
- Asynchronous communication with routing via event type
- Decoupled responsibilities
- Resilient processing with a custom Circuit Breaker (without Polly)
- Domain and DTO validation
- Environment automation with Docker Compose

---

## 👤 Author

Developed by **Simón Holmquist**  
