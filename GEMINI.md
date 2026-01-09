# SkyLab Project Context

## Project Overview
SkyLab is a web application designed for **controlling and monitoring UAV (Unmanned Aerial Vehicle) flights**.

## System Architecture
The project follows a split architecture structure:
- **Frontend:** Located in the `frontend` directory, built with Vue 3, TypeScript, and Vite.
- **Backend:** Located in the `backend` directory, built with ASP.NET Core and SignalR.
- **Communication:** Real-time bidirectional communication via **SignalR** (`/flighthub`).

## Functional Requirements
- **Map Interface:** The frontend must implement an **open-source free map engine** (e.g., Leaflet, OpenLayers) to visualize flight paths and UAV locations.

## Technology Stack

### Frontend (`frontend/`)
- **Framework:** Vue 3
- **Language:** TypeScript
- **Build Tool:** Vite
- **Dependencies:**
  - `vue` (^3.5.24)
  - `leaflet` (^1.9.4)
  - `@microsoft/signalr` (^8.0.7)
- **Dev Dependencies:**
  - `typescript` (~5.9.3)
  - `vite` (^7.2.4)
  - `vue-tsc` (^3.1.4)
### Backend (`backend/`)
- **Framework:** ASP.NET Core (.NET 10.0)
- **Language:** C#
- **Communication:** SignalR (Real-time)
- **Key Components:**
  - `FlightHub`: Handles real-time flight data communication.
  - `FlightSimulationWorker`: Background service simulating a UAV flight over Ashdod.

## Development Setup & Commands

### Backend
Navigate to the backend directory to run these commands:
`cd backend`

| Action | Command | Description |
| :--- | :--- | :--- |
| **Build** | `dotnet build` | Builds the project. |
| **Run** | `dotnet run` | Runs the backend server. |
| **Watch Run** | `dotnet watch run` | Runs with hot reload. |

### Frontend
Navigate to the frontend directory to run these commands:
`cd frontend`

| Action | Command | Description |
| :--- | :--- | :--- |
| **Install Dependencies** | `npm install` | Installs project dependencies. |
| **Start Dev Server** | `npm run dev` | Starts the Vite development server. |
| **Build for Production** | `npm run build` | Runs type checks (`vue-tsc`) and builds the project. |
| **Preview Build** | `npm run preview` | Previews the production build locally. |

## Project Structure
```text
C:\Development\SkyLab\
├── backend/          # Backend code (currently empty)
└── frontend/         # Frontend application (Vue 3 + Vite)
    ├── src/          # Source code
    ├── public/       # Static assets
    ├── vite.config.ts # Vite configuration
    └── package.json  # Project manifest
```

## Current Status & Next Steps

- The frontend is initialized with a Vue 3 + TypeScript template.

- **Map Integration:** Leaflet has been installed and integrated into the frontend with a full-screen `MapComponent`.

- The backend directory is created but empty.



## Recent Changes



- Renamed `fronted` to `frontend`.



- Installed `leaflet` and `@types/leaflet`.



- Created `MapComponent.vue` using Leaflet and OpenStreetMap tiles.



- Updated `App.vue` and `style.css` to accommodate a full-screen map.



- **Verification:** Frontend build (`npm run build`) passed successfully.



- **Backend Init:** Created `SkyLab.Backend` (ASP.NET Core Web API).



- **SignalR:** Implemented `FlightHub` and configured CORS for `localhost:5173`.



- **Git:** Created root `.gitignore` with standard .NET ignore patterns.



- **Frontend SignalR:** Created `SignalRService.ts` with auto-reconnect and infinite initial retry.



- **Backend Simulation:** Implemented `FlightSimulationWorker` to simulate a UAV flying over Ashdod.



- **Map Update:** Configured `MapComponent` to center on Israel and display the simulated UAV in real-time.







## Immediate Action Items



1.  **UAV Logic:** Expand data structures to include altitude, speed, and heading.








