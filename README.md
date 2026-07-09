# DiscordVoiceBot

Discord-бот на C# (.NET 9, Discord.Net), ведущий полноценный голосовой диалог (Voice-to-Voice) в голосовом канале: слушает участников, распознаёт речь, генерирует ответ через LLM и озвучивает его склонированным голосом.

> ⚠️ Проект в активной разработке. Часть пайплайна (воспроизведение ответа в голосовом канале) пока не реализована.

## Как это работает
Голосовой канал (Discord) ➔ VoiceCaptureService (RTP/Opus, per-user) ➔ VoiceActivityDetector (тишина > 1.2с) ➔ UtteranceCollector (Opus ➔ PCM) ➔ SpeechToTextService (Whisper) ➔ ChatHistoryManager (история) ➔ LlmService (LLM/OpenRouter) ➔ TextToSpeechService (Fish Audio) ➔ VoiceOutputService (стриминг PCM в канал)

## Стек

- .NET 9 / C#
- Discord.Net 3.20.1
- Concentus — декодирование Opus → PCM в управляемом коде
- Whisper (STT) и LLM
- Fish Audio API — TTS с клонированным голосом

## Запуск

### Требования
- .NET 9 SDK
- Токен Discord-бота с включённым **Message Content Intent** и правами `Connect`, `Speak`, `Send Messages`, `View Channels` в приглашении

### Переменные окружения (`.env` в корне проекта)

```
BOT_TOKEN=
OPENROUTER_API_KEY=
LLM_API_ENDPOINT=
STT_API_ENDPOINT=
BOT_SYSTEM_PROMPT=
FISH_API_KEY=
TTS_API_ENDPOINT=
VOICE_ID=
```
### Команды в чате

- `!join` — бот заходит в голосовой канал, где сейчас находится автор сообщения
- `!leave` — бот выходит из голосового канала

## Известные ограничения
- Обработка нескольких говорящих одновременно (barge-in, очередь ответов) — в разработке
- Лимит параллельных запросов к Fish Audio (5) пока не реализован

## Docker

В проекте есть `Dockerfile` для деплоя на Linux. Для голосового функционала контейнеру нужны системные библиотеки `libopus0`, `libsodium23` и `ffmpeg` — см. `Dockerfile` в корне проекта.
