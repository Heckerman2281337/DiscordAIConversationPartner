# syntax=docker/dockerfile:1

# =========================
# Build
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY ["DiscordVoiceBotMark.csproj", "./"]

RUN dotnet restore "DiscordVoiceBotMark.csproj"

COPY . .

RUN dotnet publish "DiscordVoiceBotMark.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# =========================
# Runtime
# =========================
FROM mcr.microsoft.com/dotnet/runtime:9.0-noble

WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends \
    ffmpeg \
    ca-certificates \
    libopus0 \
    libsodium23 \
    libstdc++6 \
    && rm -rf /var/lib/apt/lists/*

# Для Discord.Net
RUN ln -sf /usr/lib/x86_64-linux-gnu/libopus.so.0 /usr/lib/x86_64-linux-gnu/libopus.so && \
    ln -sf /usr/lib/x86_64-linux-gnu/libsodium.so.23 /usr/lib/x86_64-linux-gnu/libsodium.so

# Копируем опубликованное приложение
COPY --from=build /app/publish .

# Копируем Linux-версию libdave
COPY nativeLibs/libdave.so ./libdave.so

RUN ls -l /app/libdave.so && \
    ldd /app/libdave.so

ENV LD_LIBRARY_PATH=/app:/usr/lib:/usr/lib/x86_64-linux-gnu

ENTRYPOINT ["dotnet", "DiscordVoiceBotMark.dll"]