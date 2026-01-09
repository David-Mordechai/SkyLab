<script setup lang="ts">
import { ref, onMounted, nextTick } from 'vue';
import { signalRService } from '../services/SignalRService';

interface Message {
  user: string;
  text: string;
  isSystem: boolean;
}

const messages = ref<Message[]>([]);
const newMessage = ref('');
const chatContainer = ref<HTMLElement | null>(null);
const isOpen = ref(true);

const scrollToBottom = async () => {
  await nextTick();
  if (chatContainer.value) {
    chatContainer.value.scrollTop = chatContainer.value.scrollHeight;
  }
};

onMounted(() => {
  // Listen for chat messages from SignalR (for synchronization)
  signalRService.onReceiveChatMessage((user: string, text: string) => {
    messages.value.push({
      user,
      text,
      isSystem: user === 'Mission Control'
    });
    scrollToBottom();
  });
});

const sendMessage = async () => {
  if (!newMessage.value.trim()) return;

  const userMsg = newMessage.value;
  newMessage.value = '';

  // 1. Send to MCP Server (Node.js)
  // This will handle LLM logic and trigger the .NET backend tools
  try {
    const response = await fetch('http://localhost:3000/chat', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ message: userMsg })
    });

    const data = await response.json();
    
    // The MCP server handles the response broadcast via its tool execution 
    // and returns the textual reply here.
    // However, to make it feel immediate, we can add the user message locally 
    // or let SignalR handle the roundtrip.
    
    // Let's send the user message to SignalR so others see it too
    await signalRService.sendChatMessage('Commander', userMsg);

    if (data.reply) {
        // The MCP server reply is also broadcasted by the .NET backend when tools are executed,
        // but for non-tool replies (just chat), we might need to handle it.
        // In our current setup, the MCP server doesn't have a SignalR connection, 
        // it just hits the .NET API.
        // So for "non-tool" replies, we should probably send them to the .NET backend to broadcast.
        
        // For now, let's just show the reply if it wasn't a tool execution 
        // (tool executions are broadcasted by the .NET MissionAgent)
        if (!data.tool_executed) {
            await signalRService.sendChatMessage('Mission Control', data.reply);
        }
    }

  } catch (error) {
    console.error("Chat Error:", error);
    messages.value.push({
      user: 'System',
      text: 'Error connecting to MCP Server.',
      isSystem: true
    });
  }
};

const toggleChat = () => {
    isOpen.value = !isOpen.value;
};
</script>

<template>
  <div class="mission-chat" :class="{ 'is-closed': !isOpen }">
    <div class="chat-header" @click="toggleChat">
      <span class="title">MISSION CONTROL</span>
      <span class="toggle-icon">{{ isOpen ? '−' : '+' }}</span>
    </div>
    
    <div v-show="isOpen" class="chat-body">
      <div ref="chatContainer" class="messages-container">
        <div 
          v-for="(msg, index) in messages" 
          :key="index" 
          class="message"
          :class="{ 'system-msg': msg.isSystem, 'user-msg': !msg.isSystem }"
        >
          <span class="user">{{ msg.user }}:</span>
          <span class="text">{{ msg.text }}</span>
        </div>
      </div>
      
      <div class="input-area">
        <input 
          v-model="newMessage" 
          @keyup.enter="sendMessage"
          placeholder="Enter command..."
          type="text"
        />
        <button @click="sendMessage">SEND</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.mission-chat {
  position: absolute;
  bottom: 20px;
  right: 20px;
  width: 320px;
  background: rgba(17, 24, 39, 0.85);
  backdrop-filter: blur(12px);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 12px;
  color: #fff;
  font-family: 'Segoe UI', Roboto, sans-serif;
  z-index: 1000;
  display: flex;
  flex-direction: column;
  transition: all 0.3s ease;
  overflow: hidden;
}

.is-closed {
    height: 40px;
}

.chat-header {
  padding: 10px 16px;
  background: rgba(31, 41, 55, 0.5);
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  display: flex;
  justify-content: space-between;
  align-items: center;
  cursor: pointer;
  font-weight: bold;
  font-size: 0.8rem;
  letter-spacing: 1px;
}

.chat-body {
  height: 400px;
  display: flex;
  flex-direction: column;
}

.messages-container {
  flex: 1;
  padding: 12px;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.message {
  font-size: 0.9rem;
  line-height: 1.4;
}

.user {
  font-weight: bold;
  margin-right: 6px;
  font-size: 0.75rem;
  text-transform: uppercase;
}

.user-msg .user { color: #60A5FA; }
.system-msg .user { color: #10B981; }

.text {
  word-break: break-word;
}

.input-area {
  padding: 12px;
  display: flex;
  gap: 8px;
  background: rgba(0,0,0,0.2);
}

input {
  flex: 1;
  background: rgba(255,255,255,0.05);
  border: 1px solid rgba(255,255,255,0.1);
  border-radius: 4px;
  padding: 6px 10px;
  color: #fff;
  font-size: 0.9rem;
}

input:focus {
  outline: none;
  border-color: #3B82F6;
}

button {
  background: #2563EB;
  border: none;
  border-radius: 4px;
  color: #fff;
  padding: 0 12px;
  cursor: pointer;
  font-weight: bold;
  font-size: 0.75rem;
}

button:hover {
  background: #1D4ED8;
}

/* Scrollbar */
.messages-container::-webkit-scrollbar {
  width: 4px;
}
.messages-container::-webkit-scrollbar-thumb {
  background: rgba(255,255,255,0.1);
  border-radius: 2px;
}
</style>
