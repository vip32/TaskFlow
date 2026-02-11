FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["TaskFlow.slnx", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["src/TaskFlow.Domain/TaskFlow.Domain.csproj", "src/TaskFlow.Domain/"]
COPY ["src/TaskFlow.Application/TaskFlow.Application.csproj", "src/TaskFlow.Application/"]
COPY ["src/TaskFlow.Infrastructure/TaskFlow.Infrastructure.csproj", "src/TaskFlow.Infrastructure/"]
COPY ["src/TaskFlow.Presentation/TaskFlow.Presentation.csproj", "src/TaskFlow.Presentation/"]
COPY ["tests/TaskFlow.UnitTests/TaskFlow.UnitTests.csproj", "tests/TaskFlow.UnitTests/"]

RUN dotnet restore "TaskFlow.slnx"

COPY . .
RUN dotnet publish "src/TaskFlow.Presentation/TaskFlow.Presentation.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "TaskFlow.Presentation.dll"]
