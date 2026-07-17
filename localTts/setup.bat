@echo off
echo Configuration local env XTTSv2

set CURL_CA_BUNDLE=
set REQUESTS_CA_BUNDLE=

py -3.11 -m venv xtts_env
if errorlevel 1 (
    echo [Ошибка] Python 3.11 не найден в системе.
    pause
    exit /b
)

call xtts_env\Scripts\activate
echo Downloading PyTorch with CUDA 12.1
pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu121
echo Downloading XTTS server
pip install xtts-api-server

echo Everything is ready. Execute run.bat
pause