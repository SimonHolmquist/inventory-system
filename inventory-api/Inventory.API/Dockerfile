FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Inventory.API/*.csproj ./Inventory.API/
RUN dotnet restore ./Inventory.API/Inventory.API.csproj

COPY . .

WORKDIR /src/Inventory.API
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

COPY Inventory.API/appsettings*.json ./

EXPOSE 80

ENTRYPOINT ["dotnet", "Inventory.API.dll"]
