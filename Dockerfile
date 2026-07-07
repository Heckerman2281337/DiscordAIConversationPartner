# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY ["DiscordVoiceBotMark.csproj", "./"]

RUN dotnet restore "./DiscordVoiceBotMark.csproj" --runtime linux-x64

COPY . .

RUN dotnet publish "./DiscordVoiceBotMark.csproj" \
    -c "$BUILD_CONFIGURATION" \
    -o /app/publish \
    --runtime linux-x64 \
    --self-contained false \
    /p:UseAppHost=false \
    --no-restore


FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final

WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends \
    ca-certificates \
    libopus0 \
    libsodium23 \
    ffmpeg \
    curl \
    unzip \
    && rm -rf /var/lib/apt/lists/*

RUN ln -sf /usr/lib/x86_64-linux-gnu/libopus.so.0 /usr/lib/x86_64-linux-gnu/libopus.so \
    && ln -sf /usr/lib/x86_64-linux-gnu/libsodium.so.23 /usr/lib/x86_64-linux-gnu/libsodium.so

RUN set -eux; \
    curl -fsSL "https://github.com/discord/libdave/releases/latest/download/libdave-Linux-X64-boringssl.zip" -o /tmp/libdave.zip; \
    unzip -j /tmp/libdave.zip "libdave.so" -d /app || unzip -j /tmp/libdave.zip "*/libdave.so" -d /app; \
    chmod 755 /app/libdave.so; \
    rm -f /tmp/libdave.zip; \
    test -f /app/libdave.so

COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "DiscordVoiceBotMark.dll"]