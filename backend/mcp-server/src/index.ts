import express from 'express';
import cors from 'cors';
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { SSEServerTransport } from "@modelcontextprotocol/sdk/server/sse.js";
import { z } from "zod";

const app = express();
const port = 3001; // Changed to 3001 to avoid conflict if any, though previous was 3000. 
// User said "mcp server that we already have". I'll stick to 3000 if possible, 
// but usually vite uses 5173, BFF 5066. 3000 is fine.

app.use(cors());

// Create MCP Server
const server = new McpServer({
  name: "SkyLab Mission Tools",
  version: "1.0.0"
});

// Helper for backend calls
const backendUrl = "http://localhost:5066/api/mission";

async function callBackend(endpoint: string, body: any) {
    try {
        const res = await fetch(`${backendUrl}/${endpoint}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body)
        });
        if (!res.ok) {
            throw new Error(`Backend returned ${res.status}`);
        }
        return `Command executed successfully: ${endpoint}`;
    } catch (err: any) {
        return `Error executing command: ${err.message}`;
    }
}

async function geocodeLocation(location: string): Promise<{ lat: number, lng: number } | null> {
    try {
        console.log(`[MCP] Geocoding: ${location}`);
        const url = `https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(location)}&format=json&limit=1`;
        console.log(`[MCP] Fetching: ${url}`);
        
        const res = await fetch(url, {
            headers: { 'User-Agent': 'SkyLab-MCP-Server/1.0 (demo-project)' }
        });
        
        console.log(`[MCP] Nominatim Status: ${res.status}`);
        
        if (!res.ok) {
             console.error(`[MCP] Nominatim Error: ${res.statusText}`);
             return null;
        }

        const data = await res.json();
        console.log(`[MCP] Nominatim Data: ${JSON.stringify(data)}`);
        
        if (data && data.length > 0) {
            return { lat: parseFloat(data[0].lat), lng: parseFloat(data[0].lon) };
        }
        return null;
    } catch (err) {
        console.error("Geocoding error:", err);
        return null;
    }
}

// Register Tools
server.tool(
    "navigate_to",
    { location: z.string().describe("The name of the city or location to fly to. Extract only the place name (e.g. 'Paris', 'New York'), ignoring words like 'mission', 'fly', 'over'.") },
    async ({ location }) => {
        const cleanLocation = location.trim();
        console.log(`[MCP] Tool called: navigate_to('${cleanLocation}')`);
        
        // 1. Resolve coordinates
        const coords = await geocodeLocation(cleanLocation);
        if (!coords) {
            return {
                content: [{ type: "text", text: `Failed to find location: ${location}` }]
            };
        }

        // 2. Call Backend with coordinates
        const result = await callBackend("target", { lat: coords.lat, lng: coords.lng });
        return {
            content: [{ type: "text", text: `${result}. Flying to ${location} (${coords.lat}, ${coords.lng}).` }]
        };
    }
);

server.tool(
    "change_speed",
    { speed: z.number().describe("Target speed in knots.") },
    async ({ speed }) => {
        console.log(`[MCP] Tool called: change_speed(${speed})`);
        const result = await callBackend("speed", { speed });
        return {
            content: [{ type: "text", text: result }]
        };
    }
);

server.tool(
    "change_altitude",
    { altitude: z.number().describe("Target altitude in feet.") },
    async ({ altitude }) => {
        console.log(`[MCP] Tool called: change_altitude(${altitude})`);
        const result = await callBackend("altitude", { altitude });
        return {
            content: [{ type: "text", text: result }]
        };
    }
);

// SSE Transport Map
// In a real app with multiple clients, we'd need a map of session -> transport.
// For this single-user demo, we can just instantiate a new transport per connection.
let transport: SSEServerTransport | null = null;

app.get('/sse', async (req, res) => {
    console.log("New SSE connection established");
    transport = new SSEServerTransport("/message", res);
    await server.connect(transport);
});

app.post('/message', async (req, res) => {
    // console.log("Received message on /message");
    if (transport) {
        await transport.handlePostMessage(req, res);
    } else {
        res.status(400).send("No active transport");
    }
});

app.listen(port, () => {
    console.log(`MCP Server (Tools) running on http://localhost:${port}/sse`);
});