@echo off
cd /d F:\Projects\DiscordVoiceBotMark\localTts

set CURL_CA_BUNDLE=
set REQUESTS_CA_BUNDLE=

echo Starting local XTTSv2 server
call xtts_env\Scripts\activate
python -m xtts_api_server
pause