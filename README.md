================================================================
  STUDENT MANAGEMENT SYSTEM  —  C# Console + SQLite
================================================================

PROJECT OVERVIEW

A console-based Student Management System built with C# and SQLite, 
providing separate interfaces for administrators and students.

TECHNICAL SPECIFICATIONS

  Language     : C#
  Framework    : .NET 8.0+
  Database     : SQLite (embedded)
  Architecture : Layered (Models, Services, UI)

PROJECT STRUCTURE

SMS/
├── Program.cs              <- Application entry point
├── Models/                 <- Data entity classes
├── Data/                   <- Database connection and seeding
├── Services/               <- Repository pattern implementation
├── Helpers/                <- Console UI utilities
└── UI/                     <- Admin and student menus

DEFAULT ACCESS

  Administrator : username and password provided in code comments
  Students      : sample accounts seeded in database on first run

DATABASE

  The database file (sms.db) is created automatically in the debug 
  folder on first execution.

FEATURES

Administrator:
  - Student management (CRUD operations)
  - Course management
  - Grade assignment
  - Attendance tracking
  - Fee management
  - Report generation
  - Timetable management

Student:
  - Profile management
  - Course viewing
  - Grade checking with CGPA calculation
  - Attendance viewing
  - Fee status checking
  - Timetable viewing

C# CONCEPTS COVERED

  - Classes and object-oriented programming
  - LINQ queries
  - Repository pattern
  - SQLite integration
  - Console I/O with formatting
  - Parameterized SQL queries

================================================================