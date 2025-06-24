FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
COPY ["src/DapperSqlConstructor/DapperSqlConstructor.csproj", "src/DapperSqlConstructor/"]
RUN dotnet restore "src/DapperSqlConstructor/DapperSqlConstructor.csproj"

COPY src/DapperSqlConstructor/ src/DapperSqlConstructor/

WORKDIR /app/src/DapperSqlConstructor

RUN dotnet build "DapperSqlConstructor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DapperSqlConstructor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT [ "dotnet",  "DapperSqlConstructor.dll"]
