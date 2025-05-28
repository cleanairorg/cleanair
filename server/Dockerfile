# server/Dockerfile

# === BUILD STAGE ===
# Bruger .NET SDK image til at bygge applikationen
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

# Sætter arbejdsmappe for build processen
WORKDIR /src

# Kopierer kun projekt-filen først for at cache restore-processen
COPY ["server/Startup/Startup.csproj", "server/Startup/"]

# Henter NuGet dependencies (cacheable hvis .csproj ikke ændres)
RUN dotnet restore "server/Startup/Startup.csproj"

# Kopierer hele server mappen med alt kildekode
COPY ["server/", "server/"]

# Add this line before your RUN dotnet publish command
RUN find server -type f -name "stryker-config.json" -delete

# Kompilerer og publisher applikationen
# --runtime: Specificerer target runtime (Alpine Linux)
# --self-contained: Inkluderer .NET runtime i output
# /p:PublishSingleFile: Laver én executable fil i stedet for mange DLLs
RUN dotnet publish "server/Startup/Startup.csproj" \
    -c Release \
    -o /app/publish \
    --runtime linux-musl-x64 \
    --self-contained true \
    /p:PublishSingleFile=true

# === RUNTIME STAGE ===
# Bruger minimal runtime image (ikke hele SDK)
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine

# Sætter arbejdsmappe for runtime
WORKDIR /app

# Kopierer den kompilerede applikation fra build stage
COPY --from=build /app/publish .

# Sætter miljøvariabel for hvilket port applikationen lytter på
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

# Eksponerer port 8080 (informativt for Docker)
EXPOSE 8080

# Definerer hvordan applikationen startes
ENTRYPOINT ["./Startup"]