# ---------- build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# копируем sln и csproj
COPY Task4.sln .
COPY Task4/Task4.csproj Task4/

# restore зависимостей
RUN dotnet restore Task4/Task4.csproj

# копируем всё остальное
COPY . .

WORKDIR /src/Task4
RUN dotnet publish -c Release -o /app/publish

# ---------- runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/publish .

# Render передает порт через переменную PORT
ENV ASPNETCORE_URLS=http://+:${PORT}

ENTRYPOINT ["dotnet", "Task4.dll"]

