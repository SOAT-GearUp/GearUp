FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/GearUp.Api/GearUp.Api.csproj", "src/GearUp.Api/"]
COPY ["src/GearUp.Application/GearUp.Application.csproj", "src/GearUp.Application/"]
COPY ["src/GearUp.Domain/GearUp.Domain.csproj", "src/GearUp.Domain/"]
COPY ["src/GearUp.Infrastructure/GearUp.Infrastructure.csproj", "src/GearUp.Infrastructure/"]
RUN dotnet restore "./src/GearUp.Api/GearUp.Api.csproj"
COPY . .
WORKDIR /src/src/GearUp.Api
RUN dotnet publish "./GearUp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "GearUp.Api.dll"]
