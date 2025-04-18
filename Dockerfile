FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["GameManagement.API/GameManagement.API.csproj", "GameManagement.API/"]
COPY ["GameManagement.Application/GameManagement.Application.csproj", "GameManagement.Application/"]
COPY ["GameManagement.Domain/GameManagement.Domain.csproj", "GameManagement.Domain/"]
COPY ["GameManagement.Infrastructure/GameManagement.Infrastructure.csproj", "GameManagement.Infrastructure/"]
RUN dotnet restore "GameManagement.API/GameManagement.API.csproj"
COPY . .
WORKDIR "/src/GameManagement.API"
RUN dotnet build "GameManagement.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GameManagement.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GameManagement.API.dll"] 