FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

# Добавляем curl для прямого скачивания файла
RUN apt-get update && apt-get install -y --no-install-recommends \
    libopus0 \
    libsodium23 \
    ffmpeg \
    curl \
    && rm -rf /var/lib/apt/lists/*

RUN ln -sf /usr/lib/x86_64-linux-gnu/libopus.so.0 /usr/lib/x86_64-linux-gnu/libopus.so \
    && ln -sf /usr/lib/x86_64-linux-gnu/libsodium.so.23 /usr/lib/x86_64-linux-gnu/libsodium.so

RUN curl -L -f https://github.com/discord-net/Discord.Net/releases/download/3.20.1/libdave.so -o /app/libdave.so || \
    curl -L -f https://github.com/discord-net/Discord.Net/releases/download/3.17.0/libdave.so -o /app/libdave.so

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DiscordVoiceBotMark.csproj", "."]
RUN dotnet restore "./DiscordVoiceBotMark.csproj"
COPY . .
RUN dotnet build "./DiscordVoiceBotMark.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DiscordVoiceBotMark.csproj" -c $BUILD_CONFIGURATION -o /app/publish /r linux-x64 --self-contained false /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER root
ENTRYPOINT ["dotnet", "DiscordVoiceBotMark.dll"]