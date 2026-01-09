import express from 'express';
import cors from 'cors';
import bodyParser from 'body-parser';
import { GoogleGenerativeAI, FunctionDeclarationSchemaType } from '@google/generative-ai';
import dotenv from 'dotenv';
import fetch from 'node-fetch'; // Need to install node-fetch if using Node < 18, but Node 18+ has fetch built-in. I'll assume Node 18+.

dotenv.config();

const app = express();
const port = 3000;

app.use(cors());
app.use(bodyParser.json());

const apiKey = process.env.GEMINI_API_KEY;
if (!apiKey) {
  console.error("GEMINI_API_KEY is missing!");
  process.exit(1);
}

const genAI = new GoogleGenerativeAI(apiKey);

// Define Tools
const navigateToTool = {
  name: "navigate_to",
  description: "Navigate the UAV to a specific city or location.",
  parameters: {
    type: FunctionDeclarationSchemaType.OBJECT,
    properties: {
      location: {
        type: FunctionDeclarationSchemaType.STRING,
        description: "The name of the city or location to fly to (e.g., 'Tel Aviv', 'Haifa')."
      }
    },
    required: ["location"]
  }
};

const model = genAI.getGenerativeModel({
  model: "gemini-pro", // or gemini-1.5-flash
  tools: [{
    functionDeclarations: [navigateToTool]
  }]
});

app.post('/chat', async (req, res) => {
  try {
    const { message } = req.body;
    console.log(`Received message: ${message}`);

    const chat = model.startChat();
    const result = await chat.sendMessage(message);
    const response = await result.response;
    const call = response.functionCalls();

    if (call && call.length > 0) {
      const firstCall = call[0];
      if (firstCall.name === "navigate_to") {
        const location = firstCall.args["location"];
        console.log(`Tool called: navigate_to(${location})`);

        // Execute Tool: Call .NET Backend
        // Assuming .NET backend is on port 5066 (http)
        const backendUrl = "http://localhost:5066/api/mission/target";
        
        try {
            // Note: In Node 18+, fetch is global. If older, might need node-fetch.
            const apiRes = await fetch(backendUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ location: location })
            });

            if (apiRes.ok) {
                const data = await apiRes.json();
                res.json({ reply: `Mission updated! Flying to ${location}.`, tool_executed: true });
            } else {
                res.json({ reply: `Failed to update mission. Backend returned ${apiRes.status}.`, tool_executed: false });
            }
        } catch (err) {
            console.error("Error calling backend:", err);
            res.json({ reply: "Failed to communicate with Flight Control System.", tool_executed: false });
        }
        return;
      }
    }

    // Standard text response if no tool called
    res.json({ reply: response.text(), tool_executed: false });

  } catch (error) {
    console.error("Error processing chat:", error);
    res.status(500).json({ error: "Internal Server Error" });
  }
});

app.listen(port, () => {
  console.log(`MCP Server running on http://localhost:${port}`);
});
