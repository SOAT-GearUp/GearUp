# Imagens no Docker Hub (evita dependência do MCR em redes que bloqueiam mcr.microsoft.com).
FROM bitnami/dotnet-sdk:latest AS build
USER root
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

FROM bitnami/aspnet-core:latest AS final
WORKDIR /app
COPY --from=build --chown=1001:root /app/publish .
USER 1001
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "GearUp.Api.dll"]
