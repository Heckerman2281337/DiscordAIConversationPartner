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

# Runtime-зависимости для Discord voice / audio / native libs
RUN apt-get update && apt-get install -y --no-install-recommends \
    ca-certificates \
    libopus0 \
    libsodium23 \
    ffmpeg \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Некоторые библиотеки ожидают имена без версии
RUN ln -sf /usr/lib/x86_64-linux-gnu/libopus.so.0 /usr/lib/x86_64-linux-gnu/libopus.so \
    && ln -sf /usr/lib/x86_64-linux-gnu/libsodium.so.23 /usr/lib/x86_64-linux-gnu/libsodium.so

# libdave.so для Discord.Net
RUN curl -fsSL https://github.com/discord-net/Discord.Net/releases/download/3.20.1/libdave.so -o /app/libdave.so \
    || curl -fsSL https://github.com/discord-net/Discord.Net/releases/download/3.17.0/libdave.so -o /app/libdave.so \
    && chmod 755 /app/libdave.so

COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "DiscordVoiceBotMark.dll"]