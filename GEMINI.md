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
- **AI Mission Control:** Natural language chat interface (e.g., "Climb to 5000ft") that resolves intent and updates flight parameters.
- **Advanced Physics:** Smooth transitions between linear `Transit` movement, circular `Orbit` patterns, and dynamic changes in speed/altitude.
- **Geocoding:** Automatic resolution of city/location names to coordinates via OpenStreetMap Nominatim.

## Technology Stack

### Frontend (`frontend/`)
- **Framework:** Vue 3, Composition API.
- **Mapping:** Leaflet with custom SVG icons and dynamic CSS scaling.
- **Communication:** `@microsoft/signalr` for live telemetry.

### BFF (`backend/bff/`)
- **Framework:** ASP.NET Core (.NET 10.0).
- **Physics Engine:** Vector-based movement with 50ms update intervals and smooth telemetry interpolation (V3).
- **Geocoding:** `HttpClient` integration with Nominatim API.

### MCP Server (`backend/mcp-server/`)
- **Runtime:** Node.js (TypeScript).
- **AI Engine:** Google Gemini (`gemini-flash-latest`).
- **Tools:** `navigate_to(location)`, `change_speed(speed)`, `change_altitude(altitude)`. Includes a comprehensive regex-based fallback system.

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
- **AI Mission Control:** Fully functional with navigation, speed, and altitude tools.
- **Physics Engine V3:** Implemented smooth transitions for all telemetry data (linear speed changes and altitude climbs/descents).
- **Architecture Cleanup:** Unified intelligence in the MCP server; `FlightHub` now serves as a clean communication pipe.

## Recent Changes
- **Dynamic Telemetry:** Upgraded physics engine to handle target speed and altitude with smooth interpolation.
- **Expanded Toolset:** Added `change_speed` and `change_altitude` tools to the MCP server.
- **Architectural Refactoring:** Removed legacy `MissionAgent` from BFF to eliminate command conflicts.
- **UI Integration:** Matched chat styling with telemetry overlay for a cohesive glassmorphism aesthetic.

## Immediate Action Items
1.  **Path Visualization:** Draw the flight path trail on the map.
2.  **Multi-Drone Support:** Extend the `FlightStateService` to manage an array of UAVs.
3.  **Collision Avoidance:** Implement basic logic to prevent drones from occupying the same space if multiple are added.
