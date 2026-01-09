import { GoogleGenerativeAI } from '@google/generative-ai';
import dotenv from 'dotenv';
dotenv.config();

const apiKey = process.env.GEMINI_API_KEY;
if (!apiKey) {
    console.error("No API KEY found");
    process.exit(1);
}

console.log(`Checking models for Key: ${apiKey.substring(0, 5)}...`);

// We can't list models via the SDK easily in v1beta without an error if the key is restricted, 
// but let's try a direct REST call which is often more informative.
async function check() {
    try {
        const url = `https://generativelanguage.googleapis.com/v1beta/models?key=${apiKey}`;
        const res = await fetch(url);
        const data = await res.json();
        if (data.models) {
            console.log("Available Models:");
            data.models.forEach((m: any) => console.log(` - ${m.name}`));
        } else {
            console.error("Error listing models:", JSON.stringify(data, null, 2));
        }
    } catch (e) {
        console.error("Fetch error:", e);
    }
}

check();
