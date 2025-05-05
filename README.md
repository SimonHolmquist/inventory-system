# Technical Challenge – Inventory Notification System

This project is a solution for the technical challenge proposed for the Backend .NET Developer position.

## Description

A simple system for managing inventory updates between two microservices, using RabbitMQ as the messaging middleware.

The goal is to demonstrate skills in REST API development, asynchronous messaging, error handling, resiliency patterns, and software development best practices.

## Microservices involved

- **Inventory.API**: Responsible for the CRUD operations of products. Publishes events to RabbitMQ when changes occur.
- **Notification.Service**: Consumes the published events and stores a local record of the updates.

## Technologies used

- .NET 6+
- RabbitMQ
- Entity Framework Core
- SQLite
- Docker / Docker Compose

---

This file will be updated with setup instructions, API documentation, and an architecture diagram.
