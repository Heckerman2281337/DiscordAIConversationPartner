# =========================================================
# Этап 1: Финальный рантайм-образ (минимальный вес)
# =========================================================
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

# Установка системных аудио-зависимостей и утилит
RUN apt-get update && apt-get install -y --no-install-recommends \
    libopus0 \
    libsodium23 \
    ffmpeg \
    && rm -rf /var/lib/apt/lists/*

# Создание символических ссылок для Linux, чтобы Discord.Net распознал нативные библиотеки
RUN ln -sf /usr/lib/x86_64-linux-gnu/libopus.so.0 /usr/lib/x86_64-linux-gnu/libopus.so \
    && ln -sf /usr/lib/x86_64-linux-gnu/libsodium.so.23 /usr/lib/x86_64-linux-gnu/libsodium.so

# =========================================================
# Этап 2: Сборка приложения (SDK)
# =========================================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Кэширование слоев NuGet для ускорения последующих сборок
COPY ["DiscordVoiceBotMark.csproj", "."]
RUN dotnet restore "./DiscordVoiceBotMark.csproj"

# Копирование исходного кода и компиляция
COPY . .
RUN dotnet build "./DiscordVoiceBotMark.csproj" -c $BUILD_CONFIGURATION -o /app/build

# =========================================================
# Этап 3: Публикация приложения
# =========================================================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release

# Публикуем с явным указанием целевой архитектуры linux-x64.
# Это заставит NuGet извлечь все native-библиотеки (включая libdave.so)
RUN dotnet publish "./DiscordVoiceBotMark.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    -r linux-x64 \
    --self-contained false \
    /p:UseAppHost=false

# =========================================================
# Этап 4: Финальный продакшн-слой
# =========================================================
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Безопасное копирование libdave.so в корень рантайма приложения.
# .NET складывает нативные файлы пакетов в runtimes/linux-x64/native/.
# Перенос в корень гарантирует, что Discord.Net мгновенно обнаружит библиотеку.
RUN cp runtimes/linux-x64/native/libdave.so . 2>/dev/null || true

# Установка root-прав, чтобы рантайм C# мог создавать папки (Models) и скачивать туда Whisper веса
USER root

# Запуск приложения
ENTRYPOINT ["dotnet", "DiscordVoiceBotMark.dll"]