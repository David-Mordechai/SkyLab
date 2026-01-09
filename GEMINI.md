# SkyLab Project Context

## Project Overview
SkyLab is a web application designed for **controlling and monitoring UAV (Unmanned Aerial Vehicle) flights**.

## System Architecture
The project follows a split architecture structure:
- **Frontend:** Located in the `frontend` directory, built with Vue 3, TypeScript, and Vite.
- **Backend:** Located in the `backend` directory, built with ASP.NET Core and SignalR.
- **Communication:** Real-time bidirectional communication via **SignalR** (`/flighthub`).

## Functional Requirements
- **Map Interface:** The frontend implements **Leaflet** to visualize flight paths and UAV locations.
- **Real-time Monitoring:** Displays live UAV telemetry (Position, Altitude, Speed, Heading).

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
  - `@types/leaflet` (^1.9.12)

### Backend (`backend/`)
- **Framework:** ASP.NET Core (.NET 10.0)
- **Language:** C#
- **Communication:** SignalR (Real-time)
- **Key Components:**
  - `FlightHub`: Handles real-time flight data communication.
  - `FlightSimulationWorker`: Background service simulating a UAV flight over Ashdod with high-frequency updates (20Hz).

## Development Setup & Commands

### Backend
Navigate to the backend directory to run these commands:
`cd backend`

| Action | Command | Description |
| :--- | :--- | :--- |
| **Build** | `dotnet build` | Builds the project. |
| **Run** | `dotnet run` | Runs the backend server. |
| **Watch Run** | `dotnet watch run` | Runs with hot reload (Recommended). |

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
├── backend/
│   ├── Hubs/             # SignalR Hubs (FlightHub.cs)
│   └── Workers/          # Background Services (FlightSimulationWorker.cs)
└── frontend/
    ├── public/           # Static assets (uav.svg)
    ├── src/
    │   ├── components/   # Vue components (MapComponent.vue, FlightDataOverlay.vue)
    │   └── services/     # Services (SignalRService.ts)
    └── vite.config.ts    # Vite configuration
```

## Current Status & Next Steps

- **Frontend:**
  - Map integrated with Leaflet.
  - `MapComponent`: Handles map rendering, UAV marker management, and smooth transitions.
  - `FlightDataOverlay`: Displays real-time telemetry (ID, Lat/Lng, Alt, Speed, Hdg) in a glassmorphism card.
  - `SignalRService`: Robust connection handling with auto-reconnect.
  - Assets: Custom military-style UAV icon (`uav.svg`) with dynamic scaling based on altitude.

- **Backend:**
  - `FlightSimulationWorker`: Simulates a UAV flying over Ashdod at ~4000ft, ~105kts, with 20Hz updates.
  - Broadcasts: Lat, Lng, Heading, Altitude, Speed.

## Recent Changes

- **UI/UX Polish:**
  - Created `FlightDataOverlay` to replace simple tooltips.
  - Implemented smooth CSS transitions for UAV movement.
  - Designed custom military UAV icon (Reaper style).
  - Implemented dynamic icon scaling based on altitude (reference 3000ft).

- **Simulation Engine:**
  - Increased update frequency to 20Hz (50ms) for smooth animation.
  - Added Speed and Heading calculations.
  - Simulating altitude variations around 4000ft.

## Immediate Action Items
1.  **Multiple UAVs:** Expand simulation to handle multiple drones simultaneously.
2.  **Flight Path History:** Visualize the trail of the UAV on the map.
3.  **Control Interface:** Add frontend controls to modify simulation parameters (e.g., change target altitude/speed).