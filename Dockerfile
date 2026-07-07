# syntax=docker/dockerfile:1

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

FROM mcr.microsoft.com/dotnet/runtime:9.0

WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends \
    ffmpeg \
    curl \
    unzip \
    file \
    ca-certificates \
    libopus0 \
    libsodium23 \
    && rm -rf /var/lib/apt/lists/*

RUN uname -m

RUN curl -L \
    https://github.com/discord/libdave/releases/latest/download/libdave-Linux-X64-boringssl.zip \
    -o /tmp/libdave.zip && \
    unzip -l /tmp/libdave.zip

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "DiscordVoiceBotMark.dll"]