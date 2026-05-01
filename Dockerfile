FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["AI.TaskFlow.slnx", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["NuGet.Config", "./"]
COPY ["src/AI.TaskFlow.API/AI.TaskFlow.API.csproj", "src/AI.TaskFlow.API/"]
COPY ["src/AI.TaskFlow.Application/AI.TaskFlow.Application.csproj", "src/AI.TaskFlow.Application/"]
COPY ["src/AI.TaskFlow.Domain/AI.TaskFlow.Domain.csproj", "src/AI.TaskFlow.Domain/"]
COPY ["src/AI.TaskFlow.Infrastructure/AI.TaskFlow.Infrastructure.csproj", "src/AI.TaskFlow.Infrastructure/"]

RUN dotnet restore "src/AI.TaskFlow.API/AI.TaskFlow.API.csproj" --configfile NuGet.Config

COPY . .
RUN dotnet publish "src/AI.TaskFlow.API/AI.TaskFlow.API.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

CMD ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} dotnet AI.TaskFlow.API.dll"]
