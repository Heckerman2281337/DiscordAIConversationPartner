# DiscordVoiceBot
Pet project that I created to practice something new. Also to have fun with friends.

## How It Works
Voice channel (Discord) ➔ VoiceCaptureService (RTP/Opus, per-user) ➔ VoiceActivityDetector (silence > 0.6s) ➔ UtteranceCollector (Opus ➔ PCM) ➔ SpeechToTextService (Whisper) ➔ ChatHistoryManager (history) ➔ LlmService (Ollama) ➔ TextToSpeechService (XTTSv2) ➔ VoiceOutputService (streaming PCM to the channel)

## Tech Stack
### Backend
- C#
- Python
- .NET 9
### Infrastructure
- Docker/Docker compose
### Other
- Opus
- Libdave
- Libsodium
- Discord.Net 3.20.1
- Whisper
- Ollama
- XTTS 


## How to run
You can run this proj either with Docker compose or set all dependencies locally (but who will do all dat?)
### Option 1: Docker Compose 
1. Install Docker and Docker Compose. > GPU acceleration requires NVIDIA Container Toolkit.
2. Download the XTTS v2.0.2 model files and place them in the `localTts/models/` directory.
3. Start Ollama locally or in a separate container.
4. Create a `.env` file based on `.env.example`. Make sure your TTS endpoint points to the Docker service:
   `TTS_API_ENDPOINT=http://tts-server:8020/tts_to_audio/`
5. Create a `system prompt.txt` file in the project root to define the bots personality (btw some bots has censor on, but default model here is uncensored one).
6. Build and run:
   ```bash
   docker compose up --build
### Option 2: Local Setup
#### 1. Local LLM (Ollama)
1. Download and install [Ollama](https://ollama.com/).
2. Pull your preferred language model (remember that many has censor on). For example:
   ```bash
   ollama pull llama3
3. Make sure the Ollama server is running (usually on http://localhost:11434).
#### 2.Local TTS (Python)
1. Install Python (3.9 to 3.11 recommended).
2. Download the XTTS v2.0.2 model files (model.pth, speakers_xtts.pth, vocab.json, config.json) and place them in the localTts/xtts_models/v2.0.2/ directory. (Note: These files are large and not included in the repository).
3. Run localTts/setup.bat to install all required dependencies from requirements.txt.
4. Run localTts/run.bat to start the TTS server.

#### Running the Bot
1. Start Ollama.
2. Run `localTts/run.bat` to spin up the TTS server (!!!DO NOT CLOSE THE BAT WINDOW WHILE USING BOT!!!).
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
# Use http://tts-server:8020/tts_to_audio/ for Docker Compose
# Use [http://127.0.0.1:5000/tts](http://127.0.0.1:5000/tts) for local Python setup
TTS_API_ENDPOINT=http://127.0.0.1:5000/tts # Default for the local Python TTS server

```
For bot prompt create a system prompt.txt and write bot prompt in it.
### Chat Commands
- `!join` — the bot joins the voice channel where the message author is currently located.
- `!leave` — the bot leaves the voice channel.

## Known Limitations
- Handling multiple speakers simultaneously (barge-in, response queuing) is currently in development.
The project includes a Dockerfile for Linux deployment. To support voice functionality, the container requires the following system libraries: libopus0, libsodium23, and ffmpeg. See the Dockerfile in the project root for details.
