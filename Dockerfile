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
FROM mcr.microsoft.com/dotnet/runtime:9.0

WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends \
    ffmpeg \
    curl \
    unzip \
    ca-certificates \
    libopus0 \
    libsodium23 \
    && rm -rf /var/lib/apt/lists/*

RUN uname -m

RUN ln -sf /usr/lib/x86_64-linux-gnu/libopus.so.0 /usr/lib/x86_64-linux-gnu/libopus.so && \
    ln -sf /usr/lib/x86_64-linux-gnu/libsodium.so.23 /usr/lib/x86_64-linux-gnu/libsodium.so

RUN curl -L \
    https://github.com/discord/libdave/releases/latest/download/libdave-Linux-X64-boringssl.zip \
    -o /tmp/libdave.zip && \
    unzip -j /tmp/libdave.zip -d /usr/lib && \
    chmod 755 /usr/lib/libdave.so && \
    ldconfig && \
    rm /tmp/libdave.zip

RUN ls -lah /usr/lib | grep libdave || true && \
    file /usr/lib/libdave.so && \
    ldd /usr/lib/libdave.so || true

ENV LD_LIBRARY_PATH=/usr/lib:/usr/local/lib

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "DiscordVoiceBotMark.dll"]