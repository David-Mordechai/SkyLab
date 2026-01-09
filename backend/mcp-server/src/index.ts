import express from 'express';
import cors from 'cors';
import bodyParser from 'body-parser';
import { GoogleGenerativeAI, SchemaType } from '@google/generative-ai';
import dotenv from 'dotenv';

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
console.log(`API Key loaded (length: ${apiKey.length})`);

const genAI = new GoogleGenerativeAI(apiKey);

// Define Tools
const navigateToTool = {
  name: "navigate_to",
  description: "Navigate the UAV to a specific city or location.",
  parameters: {
    type: SchemaType.OBJECT,
    properties: {
      location: {
        type: SchemaType.STRING,
        description: "The name of the city or location to fly to (e.g., 'Tel Aviv', 'Haifa')."
      }
    },
    required: ["location"]
  }
};

const model = genAI.getGenerativeModel({
  model: "gemini-flash-latest",
  tools: [{
    functionDeclarations: [navigateToTool as any]
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
        const args = firstCall.args as any;
        const location = args.location;
        console.log(`Tool called: navigate_to(${location})`);

        // Execute Tool: Call .NET Backend
        // Assuming .NET backend is on port 5066 (http)
        const backendUrl = "http://localhost:5066/api/mission/target";
        
        try {
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

  } catch (error: any) {
    console.error("LLM Error:", error.message);
    
    // Fallback: Regex Heuristic
    const { message } = req.body;
    const lowerMsg = message.toLowerCase();
    
    if (lowerMsg.includes("fly to") || lowerMsg.includes("go to") || lowerMsg.includes("over")) {
        let location = "";
        const prefixes = ["fly to ", "go to ", "fly over ", "over "];
        for (const prefix of prefixes) {
            const idx = lowerMsg.indexOf(prefix);
            if (idx !== -1) {
                location = message.substring(idx + prefix.length).trim();
                break;
            }
        }
        
        if (location) {
             console.log(`Fallback: Tool called: navigate_to(${location})`);
             const backendUrl = "http://localhost:5066/api/mission/target";
             try {
                const apiRes = await fetch(backendUrl, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ location: location })
                });

                if (apiRes.ok) {
                    res.json({ reply: `[Fallback] Mission updated! Flying to ${location}.`, tool_executed: true });
                    return;
                }
             } catch (err) {
                 // ignore
             }
        }
    }

    res.status(500).json({ error: error.message || "Internal Server Error", details: error });
  }
});

app.listen(port, () => {
  console.log(`MCP Server running on http://localhost:${port}`);
});
