# DiscordVoiceBot

A C# (.NET 9, Discord.Net) Discord bot capable of maintaining a full Voice-to-Voice dialogue in a voice channel. It listens to participants, recognizes speech, generates a response via an LLM, and speaks it out using a cloned voice.

## How It Works
Voice channel (Discord) ➔ VoiceCaptureService (RTP/Opus, per-user) ➔ VoiceActivityDetector (silence > 0.6s) ➔ UtteranceCollector (Opus ➔ PCM) ➔ SpeechToTextService (Whisper) ➔ ChatHistoryManager (history) ➔ LlmService (LLM/OpenRouter/Ollama) ➔ TextToSpeechService (XTTSv2) ➔ VoiceOutputService (streaming PCM to the channel)
## Tech Stack
- .NET 9 / C#
- Discord.Net 3.20.1
- Concentus - decoding Opus → PCM in managed code
- Whisper (STT)
- Ollama - for local LLM inference.
- Python & XTTS - for local text-to-speech generation.
## Setting Up Local Services
Before running the Discord bot, you need to set up the local AI services for generating text and voice.
### 1. Local LLM (Ollama)
1. Download and install [Ollama](https://ollama.com/).
2. Pull your preferred language model. For example:
   ```bash
   ollama pull llama3
3. Ensure the Ollama server is running (usually on http://localhost:11434).
### 2.Local TTS (Python)
1. Install Python (3.9 to 3.11 recommended).
2. Download the XTTS v2.0.2 model files (model.pth, speakers_xtts.pth, vocab.json, config.json) and place them in the localTts/xtts_models/v2.0.2/ directory. (Note: These files are large and not included in the repository).
3. Run localTts/setup.bat to install all required dependencies from requirements.txt.
4. Run localTts/run.bat to start the TTS server.
## Running the Bot
1. Start **Ollama**.
2. Run `localTts/run.bat` to spin up the TTS server.
3. Build and run the C# .NET project:
   ```bash
   dotnet run
### Requirements
- .NET 9 SDK
- A Discord bot token with the Message Content Intent enabled, and an invite link with Connect, Speak, Send Messages, and View Channels permissions.

### Environment Variables
Create a .env file in the root of the project:
```
BOT_TOKEN=your_discord_bot_token

LLM_API_ENDPOINT=http://localhost:11434/api/generate
TTS_API_ENDPOINT=[http://127.0.0.1:5000/tts](http://127.0.0.1:5000/tts) # Default for the local Python TTS server
STT_API_ENDPOINT=

BOT_SYSTEM_PROMPT=Your persona description here...
```
### Chat Commands

- `!join` — the bot joins the voice channel where the message author is currently located.
- `!leave` — the bot leaves the voice channel.

## Known Limitations
- Handling multiple speakers simultaneously (barge-in, response queuing) is currently in development.
## Docker
The project includes a Dockerfile for Linux deployment. To support voice functionality, the container requires the following system libraries: libopus0, libsodium23, and ffmpeg. See the Dockerfile in the project root for details.
