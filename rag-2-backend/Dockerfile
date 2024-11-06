FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY ["rag-2-backend/rag-2-backend.csproj", "./"]
RUN dotnet restore "rag-2-backend.csproj"
COPY ["rag-2-backend/", "."]
WORKDIR "/src/"
RUN dotnet build "rag-2-backend.csproj" -c --no-launch-profile -o /app/build

FROM build AS publish
ARG PROFILE=Development
RUN dotnet publish "rag-2-backend.csproj" -c --no-launch-profile -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "rag-2-backend.dll"]
