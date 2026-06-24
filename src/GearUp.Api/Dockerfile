FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/GearUp.Api/GearUp.Api.csproj", "src/GearUp.Api/"]
COPY ["src/GearUp.Application/GearUp.Application.csproj", "src/GearUp.Application/"]
COPY ["src/GearUp.Domain/GearUp.Domain.csproj", "src/GearUp.Domain/"]
COPY ["src/GearUp.Infrastructure/GearUp.Infrastructure.csproj", "src/GearUp.Infrastructure/"]
RUN dotnet restore "./src/GearUp.Api/GearUp.Api.csproj"
COPY . .
WORKDIR "/src/src/GearUp.Api"
RUN dotnet build "./GearUp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GearUp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GearUp.Api.dll"]
