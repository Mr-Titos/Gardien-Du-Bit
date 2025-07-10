# 🔐 Gardien du Bit (Guardian Bit)

A secure password manager developed with **Blazor WebAssembly** and **ASP.NET Core**.

## 👥 Development Team

- **Arthur TITOS**
- **Kévin QUIERCELIN**

## 📋 Table of Contents

- [Overview](#-overview)
- [Features](#-features)
- [Architecture](#-architecture)
- [Technologies Used](#-technologies-used)
- [Installation](#-installation)

## 🎯 Overview

Bit Guardian is a secure web application for managing and storing passwords in an encrypted way. The app uses a modern architecture with Blazor WebAssembly for the frontend and ASP.NET Core for the backend API.

## ✨ Features

### 🔐 Advanced Security
- **TOTP Authentication** with QR Code for enhanced security
- **Data encryption** with robust algorithms
- **API route protection** against injection attacks

### 👤 User Experience
- **Persistent session** - Keeps the session even after closing the browser
- **Secure storage** of information in localStorage
- **Modern and intuitive interface**

### 🤝 Sharing and Collaboration
- **Vault sharing** via secure links
- **Access permission management**

## 🏗️ Architecture

The application follows a 3-layer architecture: Backend, Frontend, and a Shared Library.

### 1. `api-gardienbit` (Backend)
This project is the main API, handling business logic, data access, and exposing endpoints.

**Project structure:**
- **Controllers:** Contains API controllers (C#) exposing HTTP routes (e.g., GET, POST) for the frontend.
- **DAL (Data Access Layer):** Data access layer, often for custom database operations.
- **Fixtures:** For seeding data, useful in development or for test datasets.
- **Migrations:** Entity Framework Core files for managing database schema changes.
- **Models:** Represents database entities (e.g., User, Vault, etc.).
- **Repositories:** Implements the Repository pattern to encapsulate data access logic.
- **Services:** Contains business logic, used by controllers.
- **Utils:** Reusable utility functions (e.g., encryption, validation, token generation).
- **Program.cs:** Application entry point (ASP.NET Core configuration).
- **appsettings.json:** API configuration (connections, keys, options, etc.).

### 2. `common-gardienbit` (Shared Library)
A common project used by both the API and WebAssembly. Used to share type definitions, such as DTOs, enums, and utility classes.

**Project structure:**
- **DTO (Data Transfer Objects):** Lightweight and secure representations of data exchanged between client and server.
- **Enum:** Shared enumerations (e.g., roles, statuses, etc.).
- **Utils:** Utility methods reused across all projects (e.g., cryptographic helpers, extensions, etc.).

### 3. `IIABlazorWebAssembly` (Frontend)
This is the SPA (Single Page Application) frontend, developed with Blazor WebAssembly.

**Project structure:**
- **Connected Services:** References to remote services (e.g., Swagger, Microsoft APIs, etc.).
- **wwwroot:** Static files (CSS, images, JS).
- **Layout:** Layout components (MainLayout, NavMenu, etc.).
- **Models:** Data models specific to the frontend (sometimes linked to DTOs).
- **Pages:** Blazor .razor components representing views/pages.
- **Services:** Classes managing client-side logic (e.g., API calls via HttpClient, local storage management, etc.).
- **Shared:** Reusable Blazor components (buttons, dialogs, progress bars, etc.).
- **App.razor:** Defines the app structure and routes.
- **Program.cs:** Configures Blazor WebAssembly (services, dependency injection, etc.).

### 🔧 Backend - `api-gardienbit`
**ASP.NET Core Web API** - Handles business logic and data access

```
Project structure
├── Controllers/     # API controllers (HTTP endpoints)
├── DAL/            # Data access layer
├── Fixtures/       # Test data and seeding
├── Migrations/     # Entity Framework migrations
├── Models/         # Database entities
├── Repositories/   # Repository pattern
├── Services/       # Business logic
├── Utils/          # Utility functions
├── Program.cs      # Application entry point
└── appsettings.json # Configuration
```

### 📦 Shared Library - `common-gardienbit`
**.NET Library** - Shares types and utilities between frontend and backend

```
Project structure
├── DTO/            # Data Transfer Objects
├── Enum/           # Shared enumerations
└── Utils/          # Common utility methods
```

### 🌐 Frontend - `IIABlazorWebAssembly`
**Blazor WebAssembly SPA** - Modern and responsive user interface

```
Project structure
├── Connected Services/ # Remote services (Swagger, etc.)
├── wwwroot/           # Static files (CSS, images, JS)
├── Layout/            # Layout components
├── Models/            # Frontend models
├── Pages/             # Blazor pages (.razor)
├── Services/          # Client-side services
├── Shared/            # Reusable components
├── App.razor          # App structure
└── Program.cs         # Blazor configuration
```

## 🛠️ Technologies Used

### Backend
- **ASP.NET Core 8.0** - Web framework
- **Entity Framework Core** - ORM for the database
- **JWT Authentication** - Token-based authentication
- **TOTP** - Two-factor authentication

### Frontend
- **Blazor WebAssembly** - SPA framework
- **MudBlazor** - CSS framework for Blazor

### Database
- **SQL Server** - Relational database

## 🚀 Installation

### Prerequisites
- **.NET 8.0 SDK** or newer
- **SQL Server** (or SQL Server LocalDB)
- **Visual Studio 2022** or **Visual Studio Code**

### Installation Steps

1. **Clone the repository**
   ```bash
   git clone [repository-url]
   cd gardien-du-bit
   ```

2. **Set up the database**
   ```bash
   cd api-gardienbit
   dotnet ef database update
   ```

3. **Run the API**
   ```bash
   dotnet run
   ```

4. **Run the frontend**
   ```bash
   cd ../IIABlazorWebAssembly
   dotnet run
   ```

## 📄 License

This project is licensed under the MIT License. See the `LICENSE` file for details.



