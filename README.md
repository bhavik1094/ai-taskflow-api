# AI TaskFlow 🚀

[![.NET](https://img.shields.io/badge/.NET-Core%20Web%20API-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-Vite%20%2B%20TypeScript-61DAFB?style=for-the-badge&logo=react&logoColor=0A0A0A)](https://react.dev/)
[![SQL Server](https://img.shields.io/badge/Database-SQL%20Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![JWT Auth](https://img.shields.io/badge/Auth-JWT%20Access%20%2B%20Refresh-000000?style=for-the-badge&logo=jsonwebtokens&logoColor=white)](https://jwt.io/)
[![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-1F2937?style=for-the-badge)](#project-structure)
[![Status](https://img.shields.io/badge/Status-Active%20Development-10B981?style=for-the-badge)](#future-improvements)

AI TaskFlow is a modern full-stack SaaS task management platform built to help teams and individuals organize work, track progress, and collaborate through a secure and scalable workflow system.

It exists to solve a common problem: most task tools are either too simple for growing teams or too heavy for day-to-day execution. AI TaskFlow aims to bridge that gap with a clean user experience, secure backend architecture, and a foundation that can evolve into a smart productivity platform with analytics, automation, and AI-driven assistance.

Whether you are managing personal tasks, team projects, or a growing product pipeline, AI TaskFlow is designed to provide clarity, structure, and momentum.

## ✨ Tech Stack

### Backend
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- Clean Architecture
- Service + Repository Pattern

### Frontend
- React
- Vite
- TypeScript
- Tailwind CSS

### Authentication
- JWT Access Tokens
- Refresh Token Flow
- Role-based Authorization

## 🔥 Features

- Secure authentication using JWT access and refresh tokens
- Role-based access control for protected workflows
- Project management with scalable domain-driven structure
- Task management with clean API design and service separation
- Dashboard-ready backend foundation for analytics and reporting
- Clean Architecture with clear separation of API, Application, Domain, and Infrastructure layers
- SQL Server persistence with EF Core migrations support
- Frontend-ready API integration for React SPA clients

## 🧱 Project Structure

The project is designed as a full-stack workspace with clear separation between backend and frontend concerns:

```text
/api   -> ASP.NET Core Web API, business logic, authentication, persistence
/ui    -> React + Vite + TypeScript frontend application
```

In the current backend solution, the API follows a Clean Architecture structure:

```text
src/
  AI.TaskFlow.API              -> Controllers, middleware, app bootstrap
  AI.TaskFlow.Application      -> DTOs, interfaces, response models
  AI.TaskFlow.Domain           -> Entities, enums, core business models
  AI.TaskFlow.Infrastructure   -> DbContext, services, repositories, EF Core
tests/
  AI.TaskFlow.Tests            -> Test project
```

## 🖼️ Screenshots

_Add product screenshots here as the UI evolves._

### Dashboard
> Placeholder for dashboard overview screenshot

### Login
> Placeholder for login screen screenshot

### Projects
> Placeholder for projects management screenshot

### Tasks
> Placeholder for task board / tasks list screenshot

## 🚀 Getting Started

### Prerequisites

Make sure you have the following installed:

- .NET SDK
- Node.js + npm
- SQL Server / LocalDB

## Backend

Run the API:

```bash
dotnet run --project src/AI.TaskFlow.API
```

Default local API URL:

```text
http://localhost:5147
```

## Frontend

Run the React app:

```bash
npm install
npm run dev
```

Typical Vite local URLs:

```text
http://localhost:5173
http://localhost:5174
```

## 🔐 Environment Variables

### Frontend

```env
VITE_API_BASE_URL=http://localhost:5147/api
```

### Backend

Configure your `appsettings.json` / environment values for:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your SQL Server connection string"
  },
  "Jwt": {
    "Key": "Your secret signing key",
    "Issuer": "AI.TaskFlow",
    "Audience": "AI.TaskFlow.Client",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

## 📡 API Endpoints

Core API surface:

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/auth/me`
- `GET /api/projects`
- `POST /api/projects`
- `PUT /api/projects/{id}`
- `DELETE /api/projects/{id}`
- `GET /api/tasks`
- `POST /api/tasks`
- `PUT /api/tasks/{id}`
- `DELETE /api/tasks/{id}`

## 🛡️ Architecture Notes

AI TaskFlow is built with maintainability and scalability in mind:

- Clean separation of domain, application, infrastructure, and API layers
- No business logic inside controllers
- Reusable response models for consistent frontend integration
- JWT-based stateless authentication
- EF Core for data access and migrations
- Extensible service layer for future SaaS features

## 🌱 Future Improvements

- Real-time updates with SignalR
- In-app and email notifications
- Advanced dashboard analytics
- Team collaboration workflows
- Cloud deployment pipeline
- Multi-tenant SaaS capabilities
- AI-based task prioritization and productivity suggestions

## 👨‍💻 Author

**Bhavik Patel**

## 📝 Notes

- This project is structured to be production-friendly and easy to extend
- The repository is intentionally organized to support both backend and frontend growth
- Ideal for showcasing full-stack engineering, architecture, authentication, and SaaS product thinking

---

If you like this project, consider starring the repository and following its progress as AI TaskFlow evolves into a complete intelligent productivity platform.
