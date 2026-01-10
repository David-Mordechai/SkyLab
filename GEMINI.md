# SkyLab Project Context

## Project Overview
SkyLab is a web application designed for **controlling and monitoring UAV (Unmanned Aerial Vehicle) flights**. It features an AI-driven Mission Control system using the Model Context Protocol (MCP) pattern, allowing users to command drones using natural language.

## System Architecture
The project follows a modern distributed architecture with a standardized MCP implementation:
- **Frontend:** Vue 3 (Vite), TypeScript, Leaflet.
- **BFF (Backend for Frontend):** ASP.NET Core 10. Acts as the **MCP Client** (using official C# SDK) and manages the flight simulation (20Hz), physics engine, and SignalR broadcasting.
- **MCP Server:** Node.js, TypeScript. Acts as the **Tool Host**, exposing `navigate_to`, `change_speed`, etc., via **Server-Sent Events (SSE)**.
- **Communication:** 
    - **Telemetry:** Real-time bidirectional via **SignalR** (`/flighthub`).
    - **MCP Protocol:** Standardized SSE transport (`http://127.0.0.1:3001/sse`) between BFF and MCP Server.
    - **AI Processing:** BFF orchestrates the conversation with Google Gemini, using tools discovered from the MCP Server.

## Functional Requirements
- **Map Interface:** Real-time visualization of UAV position, heading, altitude, **historical path**, and **projected flight plan**.
- **AI Mission Control:** Natural language chat interface (e.g., "Climb to 5000ft") that resolves intent via Gemini and executes standardized MCP tools.
- **Advanced Physics:** Smooth transitions between linear `Transit` movement, circular `Orbit` patterns, and dynamic changes in speed/altitude.
- **Geocoding:** Automatic resolution of city/location names to coordinates via OpenStreetMap Nominatim.

## Technology Stack

### Frontend (`frontend/`)
- **Framework:** Vue 3, Composition API.
- **Mapping:** Leaflet with custom SVG icons, dynamic CSS scaling, and **Polyline** visualization.
- **Communication:** `@microsoft/signalr` for live telemetry.

### BFF (`backend/bff/`)
- **Framework:** ASP.NET Core (.NET 10.0).
- **MCP Client:** `ModelContextProtocol` (NuGet) with custom `SseClientTransport`.
- **AI Integration:** Google Gemini API (`gemini-flash-latest`).
- **Physics Engine:** Vector-based movement (V3) with smooth interpolation.

### MCP Server (`backend/mcp-server/`)
- **Runtime:** Node.js (TypeScript).
- **SDK:** `@modelcontextprotocol/sdk`.
- **Tools:** `navigate_to(location)`, `change_speed(speed)`, `change_altitude(altitude)`. Exposes tools via SSE transport.

## Development Setup & Commands

### 1. BFF (Simulation & API)
`cd backend/bff && dotnet watch run`
*Requires `GEMINI_API_KEY` in `appsettings.Development.json`.*

### 2. MCP Server (Tool Host)
`cd backend/mcp-server && npm start`

### 3. Frontend (User Interface)
`cd frontend && npm run dev`

## Project Structure
```text
C:\Development\SkyLab\
├── backend/
│   ├── bff/              # .NET BFF (MCP Client, Simulation, SignalR)
│   └── mcp-server/       # Node.js MCP Server (Tool Definitions)
└── frontend/             # Vue 3 Application (Map, Chat, Telemetry)
```

## Current Status & Next Steps
- **Standardized MCP Architecture:** The system now uses the official `ModelContextProtocol` SDK for C# and Node.js, ensuring a robust and extensible tool ecosystem.
- **Path Visualization:** The map displays a solid blue trail (history) and a dashed light-blue line (projected path to target).
- **Robustness:** Fixed build warnings, improved error handling, and ensured strict null safety in the backend services.

## Recent Changes
- **MCP SDK Integration:** Refactored BFF to use `ModelContextProtocol` NuGet package. Implemented `SseClientTransport` to consume SSE streams from the Node.js server.
- **Path Visualization:** Added `Polyline` rendering in Leaflet to show where the drone has been and where it is going.
- **Tool Schema Handling:** Solved interoperability issues between C# SDK `McpTool` and Gemini's JSON schema requirements by implementing dynamic property extraction.
- **Cleanup:** Removed legacy `McpClientService` and redundant dependencies.

## Immediate Action Items
1.  **Multi-Drone Support:** Extend the `FlightStateService` to manage an array of UAVs.
2.  **Collision Avoidance:** Implement basic logic to prevent drones from occupying the same space if multiple are added.