# File Explorer Web App

## Objective
This is a full-stack web application that mimics basic file explorer functionality. Users can manage folder hierarchies and add/view files (.txt and images). The focus is on performance, structured APIs, and an intuitive interface.

---

## Technology Stack

- **Frontend:** Angular 20 (standalone components, Angular signals), PrimeNG
- **Backend:** .NET 8 Core Web API (C#)
- **Database:** PostgreSQL

---

## Features

### Folder Management
- Display folders as a lazy-loaded tree structure (nodes expand on demand for performance)
- Create, rename, and delete folders
- Support for nested folders (multi-level tree)

### File Management
- Upload `.txt` files and image files (`.jpg`, `.png`) into folders
- Display list of files in selected folder
- Delete and rename files
- Basic previews: image thumbnails and text file content

---

## Architecture & Technical Details

- RESTful API endpoints for CRUD operations on folders and files
- Clean architecture principles with CQRS pattern
- EF Core for efficient database interactions
- Folder/file tables are normalized, storing metadata and binary/image data
- Angular signals are used for reactive state management (folder tree, file list)
- Standalone Angular components ensure modularity and clarity

---

## Limitations

Functionality is limited to the scope of the test task:

- No file download or full file content preview
- Files are stored directly in the database for simplicity
- No authentication or authorization (test scope only)
- No validation
- The system allows the use of files or folders with the same name
- The UI/UX design is intentionally kept simple for demonstration purposes
- Requests are not cached, which means each time a request is made, it always goes to the server

---

## UI Overview

- **Left Panel:** Shows folder tree  
  - Hover over folder names to: add new folder, rename folder, upload files,delete folder
- **Right Panel:** Shows folder content  
  - Actions for each file: delete, rename  

> Design is intentionally simple, no excessive animations or styles.

---

## Setup Instructions

### Prerequisites
Before you begin, make sure you have the following installed:
- [Node.js](https://nodejs.org/) (v18 or higher recommended)
- [Angular CLI](https://angular.dev/cli) (latest version)
- [.NET SDK](https://dotnet.microsoft.com/en-us/download) (v8.0 or higher)
- [PostgreSQL](https://www.postgresql.org/download/)
- [Git](https://git-scm.com/)

### Backend
1. Make sure **.NET 8 SDK** is installed.
2. Ensure PostgreSQL is running and accessible.
3. Check connection settings in `appsettings.json` (default):
   ```json
   "Username": "postgres",
   "Password": "postgres",
   "Database": "FileExplorer"
4. Run the backend using `dotnet run`
5. Database migrations will be applied automatically.
6. Swagger UI will be available at: `https://localhost:44391/swagger`

### Frontend
1. Navigate to the frontend folder.
2. Install dependencies using `npm install`
PrimeNG will be installed automatically when running npm install.
3. Run the application using `npm start`
Frontend will be available at: `http://localhost:4200`
API requests are proxied to: `https://localhost:44391`
