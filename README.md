# GearUp
Projeto FIAP SOAT Fase 1 - Oficina Mecânica

Este documento descreve como outra pessoa pode executar esta solução localmente.

Pré-requisitos
- .NET SDK 10 (https://dotnet.microsoft.com)
- Visual Studio 2026 (opcional) ou VS Code/Outro editor

Clonar o repositório
1. git clone https://github.com/JoseHenriqueRG/GearUp.git
2. cd GearUp

Restaurar dependências e compilar
dotnet restore
dotnet build --configuration Release

Executar a API via linha de comando
dotnet run --project src/GearUp.Api/GearUp.Api.csproj --configuration Release

Executar no Visual Studio
1. Abra GearUp.slnx no Visual Studio 2026.
2. Defina o projeto src/GearUp.Api como projeto de inicialização (Set as Startup Project).
3. Pressione F5 para depurar ou Ctrl+F5 para executar sem depuração.

Executar testes
dotnet test

Configurações adicionais
- Se necessário, ajuste as connection strings e variáveis de ambiente em src/GearUp.Api/appsettings.json ou forneça as variáveis de ambiente antes de executar a aplicação.


Opção: subir o banco SQL em um container (Docker)
Se preferir executar um SQL Server em um container Docker (útil para desenvolvimento), siga uma das opções abaixo.

Usando docker run
1. docker pull mcr.microsoft.com/mssql/server:2022-latest
2. docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Your_strong!Pass123" -p 14333:1433 --name gearup-sql -d mcr.microsoft.com/mssql/server:2022-latest

Usando docker-compose (exemplo)
1. Crie um arquivo docker-compose.yml com o conteúdo:

version: '3.8'
services:
  sqlserver:
	image: mcr.microsoft.com/mssql/server:2022-latest
	environment:
	  - ACCEPT_EULA=Y
	  - SA_PASSWORD=Your_strong!Pass123
	ports:
	  - "14333:1433"
	healthcheck:
	  test: ["CMD-SHELL", "/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $${SA_PASSWORD} -Q \"SELECT 1\""]
	  interval: 10s
	  timeout: 5s
	  retries: 12

2. docker-compose up -d

Notas importantes
- A connection string padrão no projeto aponta para Server=tcp:localhost,14333;Database=GearUp;User Id=sa;Password=Your_strong!Pass123;TrustServerCertificate=True; — se modificar a porta ou senha, atualize src/GearUp.Api/appsettings.json ou a variável de ambiente ConnectionStrings__GearUpDatabase.
- Aguarde o SQL Server iniciar antes de executar a aplicação. Você pode verificar o status com docker ps ou docker-compose ps.
- Se precisar aplicar migrações do Entity Framework, execute (requer dotnet-ef):
  dotnet ef database update --project src/GearUp.Infrastructure --startup-project src/GearUp.Api
