# 🧪 Backend .NET Challenge — Inventory Notification System

This repository contains the implementation of a technical challenge for a Backend .NET Developer role.  
The goal was to design and implement an event-driven microservice system for inventory updates using RabbitMQ as the messaging middleware.

---

## 📦 Project Overview

This solution is composed of two microservices:

| Project               | Description                                                        |
|------------------------|--------------------------------------------------------------------|
| `Inventory.API`        | A RESTful API to manage products. It acts as the event **producer** |
| `Notification.Service` | A background service that listens to events and **persists them**   |

---

## 🎯 Requirements Implemented

- ✅ Product CRUD API (`GET`, `POST`, `PUT`, `DELETE`) in `Inventory.API`
- ✅ Event publishing on product creation, update, deletion
- ✅ RabbitMQ integration with a direct exchange: `inventory_exchange`
- ✅ Queues: `product.created.queue`, `product.updated.queue`, `product.deleted.queue`
- ✅ Consumer running as a hosted background service (`Notification.Service`)
- ✅ Events stored in a local SQLite database (`ProductEvents` table)
- ✅ Error handling and resilience mechanisms in consumer

---

## 🚀 How to Run the Project

### Prerequisites

- [.NET SDK 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

---

### Step 1 – Start RabbitMQ

```bash
docker compose up -d
```

> 📍 RabbitMQ Management UI: [http://localhost:15672](http://localhost:15672)  
> Default credentials: `guest` / `guest`

---

### Step 2 – Run the Inventory API

```bash
cd inventory-api/Inventory.API
dotnet run
```

> API available at: `https://localhost:5274/swagger`  
> Test product creation, updates, and deletion via Swagger

---

### Step 3 – Run the Notification Service

```bash
cd notification-service/Notification.Service
dotnet run
```

> This service subscribes to product events and stores them in `notification-dev.db`

---

## 📂 Architecture Diagram

```txt
[Inventory.API] ---> [RabbitMQ] ---> [Notification.Service] ---> [SQLite]
     (POST/PUT/DEL)     (Events)         (Consumes events)         (Persists)
```

---

## 🔧 Configuration

Both projects use:

- `appsettings.json` and `appsettings.Development.json`
- SQLite connection string: `"Data Source=inventory-dev.db"` / `"notification-dev.db"`
- `launchSettings.json` to set `DOTNET_ENVIRONMENT`

---

## 📄 Technologies Used

- [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- ASP.NET Core Web API
- RabbitMQ (with Docker)
- Entity Framework Core (SQLite)
- Hosted BackgroundService
- Docker Compose

---

## 📁 Deliverables Checklist

- ✅ Code for both services (Inventory + Notifications)
- ✅ Swagger UI for API testing
- ✅ Docker Compose for RabbitMQ
- ✅ Architecture diagram (in README)
- ✅ Event-driven flow with RabbitMQ
- ✅ Local DB persistence

---

## 👨‍💻 Author

Challenge implemented by [Simón Holmquist](https://www.linkedin.com/in/simonholmquist)
