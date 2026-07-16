# DiscordVoiceBot

A C# (.NET 9, Discord.Net) Discord bot capable of maintaining a full Voice-to-Voice dialogue in a voice channel. It listens to participants, recognizes speech, generates a response via an LLM, and speaks it out using a cloned voice.

> Note: This project is in active development. Part of the pipeline (playing back the response in the voice channel) is not yet implemented.

## How It Works
Voice channel (Discord) ➔ VoiceCaptureService (RTP/Opus, per-user) ➔ VoiceActivityDetector (silence > 1.2s) ➔ UtteranceCollector (Opus ➔ PCM) ➔ SpeechToTextService (Whisper) ➔ ChatHistoryManager (history) ➔ LlmService (LLM/OpenRouter) ➔ TextToSpeechService (Fish Audio) ➔ VoiceOutputService (streaming PCM to the channel)
## Tech Stack
- .NET 9 / C#
- Discord.Net 3.20.1
- Concentus — decoding Opus → PCM in managed code
- Whisper (STT) and LLMs
## Running the Bot

### Requirements
- .NET 9 SDK
- A Discord bot token with the Message Content Intent enabled, and an invite link with Connect, Speak, Send Messages, and View Channels permissions.

### Environment Variables
Create a .env file in the root of the project:
```
BOT_TOKEN=
OPENROUTER_API_KEY=
LLM_API_ENDPOINT=
STT_API_ENDPOINT=
BOT_SYSTEM_PROMPT=
TTS_API_ENDPOINT=
```
### Chat Commands

- `!join` — the bot joins the voice channel where the message author is currently located.
- `!leave` — the bot leaves the voice channel.

## Known Limitations
- Handling multiple speakers simultaneously (barge-in, response queuing) is currently in development.
## Docker
The project includes a Dockerfile for Linux deployment. To support voice functionality, the container requires the following system libraries: libopus0, libsodium23, and ffmpeg. See the Dockerfile in the project root for details.
