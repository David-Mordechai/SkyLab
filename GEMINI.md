# SkyLab Project Context

## Project Overview
SkyLab is a web application designed for **controlling and monitoring UAV (Unmanned Aerial Vehicle) flights**. It features an AI-driven Mission Control system using the Model Context Protocol (MCP) pattern, allowing users to command drones using natural language.

## System Architecture
The project follows a modern distributed architecture:
- **Frontend:** Vue 3 (Vite), TypeScript, Leaflet.
- **BFF (Backend for Frontend):** ASP.NET Core 10, SignalR. Manages the high-frequency flight simulation (20Hz), physics engine, and geocoding.
- **MCP Server:** Node.js, TypeScript, Gemini AI. Acts as the "Brain," interpreting natural language commands and orchestrating mission tools.
- **Communication:** 
    - **Telemetry:** Real-time bidirectional via **SignalR** (`/flighthub`).
    - **Mission Control:** REST API between MCP Server and BFF (`/api/mission`).

## Functional Requirements
- **Map Interface:** Real-time visualization of UAV position, heading, and altitude using Leaflet.
- **AI Mission Control:** Natural language chat interface (e.g., "Fly over Tel Aviv") that resolves locations and updates flight paths.
- **Advanced Physics:** Smooth transitions between linear `Transit` movement and circular `Orbit` patterns.
- **Geocoding:** Automatic resolution of city/location names to coordinates via OpenStreetMap Nominatim.

## Technology Stack

### Frontend (`frontend/`)
- **Framework:** Vue 3, Composition API.
- **Mapping:** Leaflet with custom SVG icons and dynamic CSS scaling.
- **Communication:** `@microsoft/signalr` for live telemetry.

### BFF (`backend/bff/`)
- **Framework:** ASP.NET Core (.NET 10.0).
- **Physics Engine:** Vector-based movement with 50ms update intervals.
- **Geocoding:** `HttpClient` integration with Nominatim API.

### MCP Server (`backend/mcp-server/`)
- **Runtime:** Node.js (TypeScript).
- **AI Engine:** Google Gemini (`gemini-flash-latest`).
- **Capabilities:** Tool calling (`navigate_to`) with a regex-based fallback for high reliability.

## Development Setup & Commands

### 1. BFF (Simulation & API)
`cd backend/bff && dotnet watch run`

### 2. MCP Server (AI Brain)
`cd backend/mcp-server && npm start`
*Requires `GEMINI_API_KEY` in `.env`.*

### 3. Frontend (User Interface)
`cd frontend && npm run dev`

## Project Structure
```text
C:\Development\SkyLab\
├── backend/
│   ├── bff/              # .NET BFF (Simulation, SignalR, Geocoding)
│   └── mcp-server/       # Node.js MCP (Gemini AI, Mission Tools)
└── frontend/             # Vue 3 Application (Map, Chat, Telemetry)
```

## Current Status & Next Steps
- **AI Mission Control:** Fully functional. Gemini successfully calls the `navigate_to` tool to change drone destinations.
- **Physics Engine V2:** Successfully handles transit vectors and orbital transitions.
- **UI/UX:** Consistent glassmorphism design across all overlays (Chat & Telemetry).

## Recent Changes
- **Architecture Migration:** Separated backend into `bff` and `mcp-server`.
- **AI Integration:** Implemented Gemini 2.0/Flash integration with tool-calling capabilities.
- **Mission Chat:** Added `MissionChat.vue` with fixed-positioning and real-time SignalR synchronization.
- **Diagnostics:** Added `list-models.ts` to debug Gemini API capabilities.
- **Stability:** Implemented fallback parsing in the MCP server to handle AI API failures gracefully.

## Immediate Action Items
1.  **Path Visualization:** Draw the flight path trail on the map.
2.  **Multi-Drone Support:** Extend the `FlightStateService` to manage an array of UAVs.
3.  **Altitude Control:** Add AI tools to change flight altitude (e.g., "Climb to 5000ft").