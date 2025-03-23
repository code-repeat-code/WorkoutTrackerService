# 🏋️‍♂️ Workout Tracker API

A full-stack **Workout Tracking API** built with **.NET 8**, **PostgreSQL (Supabase)**, and **JWT-based authentication**. Designed to allow users to manage personalized workout plans, schedule sessions, and track progress — all through a clean, RESTful interface.

---

## 🚀 Features

- 🧑‍💻 User authentication (sign up, login, JWT secured)
- 📋 Create & manage workout plans
- 🏃 Schedule workouts for specific dates & times
- 📈 Generate progress reports from past sessions
- ✅ Soft delete support
- 🌐 Swagger UI for API testing

---

## 🛠️ Tech Stack

- **Backend**: ASP.NET Core 8.0 Web API
- **Database**: PostgreSQL (Supabase)
- **Security**: JWT Auth
- **ORM**: Entity Framework Core
- **Containerized**: Docker, Docker Hub

---

## 🧱 Database Design

The database schema includes:

- `Users`
- `Exercise` (meta-data)
- `Workout` (user-created)
- `WorkoutExercise` (many-to-many mapping)
- `WorkoutSchedule` (scheduling with status)

➡️ DB Schema
<img width="1298" alt="image" src="https://github.com/user-attachments/assets/c4c54a43-0406-4a9b-8853-be9944c22466" />



---

## 📦 Setup Locally

```bash
git clone https://github.com/your-username/WorkoutTracker.git
cd WorkoutTracker

# Create .env with your DB + JWT config
cp .env.example .env

# Run via Docker
docker-compose --env-file .env up --build
