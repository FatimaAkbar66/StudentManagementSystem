# 🎓 Student Management System (SMS)

![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![SQLite](https://img.shields.io/badge/sqlite-%2307405e.svg?style=for-the-badge&logo=sqlite&logoColor=white)
![Docker](https://img.shields.io/badge/docker-%230db7ed.svg?style=for-the-badge&logo=docker&logoColor=white)

A professional C# Console Application integrated with **SQLite** for persistent data storage. This project demonstrates a clean, modular architecture (Models-Services-UI) designed for scalability, security, and cross-platform deployment.

---

## 🛠️ Tech Stack & Tools
* **Language:** C# (.NET 10.0)
* **Database:** SQLite (using `Microsoft.Data.Sqlite`)
* **Environment:** Visual Studio 2026
* **Containerization:** Docker (Multi-stage builds)
* **Data Handling:** LINQ for advanced data querying and Parameterized SQL for security.

## 📂 Project Architecture
The project is organized into distinct layers to ensure a clean separation of concerns:
* **Models:** Core data structures (Student, Course, Admin, etc.).
* **Data:** Database connection helpers and initial schema seeding.
* **Services:** Repository layer handling all CRUD operations and SQL logic.
* **UI:** Organized menus and interactive screens for both Admin and Student roles.
* **Helpers:** Formatting tools for a polished Console UI experience.

## ✨ Key Features

### 🔐 Secure Authentication
* Role-based login for Admin and Student accounts.
* Secret Signup Key protection for new Admin registrations.
* Profile management and password update functionality.

### 🛡️ Admin Capabilities
* **Record Management:** Full CRUD operations for Students and Courses.
* **Academic Control:** Enroll students, assign grades, and manage course schedules.
* **Attendance Tracking:** Log daily attendance with automated "Shortage Reports" for students below 75%.
* **Financial Management:** Track fee payments, manage pending balances, and generate revenue summaries.
* **Advanced Analytics:** Generate reports for top performers, enrollment statistics, and departmental summaries.

### 🎓 Student Features
* **Academic Dashboard:** View enrolled courses, personal grades, and calculated CGPA.
* **Attendance Log:** Transparent access to personal attendance history.
* **Finance Tracking:** View total tuition fees, paid history, and outstanding balance.
* **Timetable:** Access personalized weekly class schedules.

## 🚀 Getting Started

### Prerequisites
* [.NET 10.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
* Visual Studio Code with the **C# Dev Kit** extension.

### Installation
1.  **Open Terminal** in the project root directory.
2.  **Restore Packages:**
```bash
    dotnet restore
```
3.  **Run the Project:**
```bash
    dotnet run
```
    *The `sms.db` SQLite database will be automatically created on the first run.*

## 🐳 Docker

The project is fully containerized using a **multi-stage Dockerfile** — Stage 1 compiles the C# project using the .NET SDK, Stage 2 runs it on a lightweight runtime image. The SQLite database is stored in a Docker volume so data persists between container restarts.

### Run with Docker
```bash
# Build the image
docker build -t sms-console .

# Run the container
docker run -it -v sms-data:/app/data sms-console
```


## 💡 C# Concepts Demonstrated
* **Object-Oriented Programming:** Encapsulation through Namespaces and Classes.
* **Data Security:** Implementation of Parameterized Queries to prevent SQL Injection.
* **Modern Syntax:** Switch expressions, String interpolation, and Nullable types.
* **Functional Programming:** Use of `Action<T>` delegates and LINQ.
* **Resource Management:** Efficient database connection handling using `using` blocks.
* **Containerization:** Multi-stage Dockerfile with Docker volume for persistent storage.

---
*git clone [https://github.com/FatimaAkbar66/StudentManagementSystem.git](https://github.com/FatimaAkbar66/StudentManagementSystem.git)
*Developed as part of the CS curriculum to master C# logic and Database Integration.*

