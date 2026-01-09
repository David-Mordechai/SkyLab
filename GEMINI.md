# SkyLab Project Context

## Project Overview
SkyLab is a web application designed for **controlling and monitoring UAV (Unmanned Aerial Vehicle) flights**. It features an AI-driven Mission Control system using the Model Context Protocol (MCP).

## System Architecture
The project follows a modular architecture:
- **Frontend:** Vue 3, TypeScript, Leaflet.
- **BFF (Backend for Frontend):** ASP.NET Core, SignalR. Manages the flight simulation and physics.
- **MCP Server:** Node.js, TypeScript, Gemini AI. Interprets natural language commands and orchestrates mission tools.
- **Communication:** 
    - Real-time telemetry via **SignalR** (`/flighthub`).
    - AI Mission Control via **MCP Server** (`http://localhost:3000`).

## Functional Requirements
- **Map Interface:** Visualize flight paths and UAV locations using Leaflet.
- **Mission Control:** natural language interface to command UAVs (e.g., "Fly over Tel Aviv").
- **Dynamic Physics:** Smooth transition between linear transit and circular orbiting.

## Technology Stack

### Frontend (`frontend/`)
- **Framework:** Vue 3 (Vite)
- **Mapping:** Leaflet
- **Communication:** `@microsoft/signalr`

### BFF (`backend/bff/`)
- **Framework:** ASP.NET Core (.NET 10.0)
- **Features:** SignalR Hubs, Background Workers, Geocoding (Nominatim).

### MCP Server (`backend/mcp-server/`)
- **Runtime:** Node.js (TypeScript)
- **AI Engine:** Google Gemini (Generative AI)
- **Tools:** `navigate_to(location)`

## Development Setup & Commands

### 1. BFF (Simulation)
`cd backend/bff && dotnet watch run`

### 2. MCP Server (AI Brain)
`cd backend/mcp-server && npm start`
*Requires `GEMINI_API_KEY` in `.env`.*

### 3. Frontend (UI)
`cd frontend && npm run dev`

## Project Structure
```text
C:\Development\SkyLab\
├── backend/
│   ├── bff/              # .NET Backend (Simulation & Tools)
│   └── mcp-server/       # Node.js MCP Server (AI Agent)
└── frontend/             # Vue 3 Application
```

## Current Status & Next Steps
- **AI Mission Control:** Implemented full pipeline from Chat UI -> MCP Server -> Gemini -> BFF Simulation.
- **Physics Engine:** Supports `Transit` and `Orbit` modes with smooth transitions.
- **Geocoding:** Integrated OpenStreetMap Nominatim for location resolution.

## Recent Changes
- **Reorganization:** Moved .NET project to `backend/bff` and created `backend/mcp-server`.
- **MCP Implementation:** Created Node.js server with Gemini function calling capabilities.
- **Mission UI:** Added `MissionChat.vue` and updated `SignalRService` for bidirectional communication.
- **Physics V2:** Refactored simulation worker for vector-based transit movement.
