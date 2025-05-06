FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Notification.Service/*.csproj ./Notification.Service/
RUN dotnet restore ./Notification.Service/Notification.Service.csproj

COPY . .

WORKDIR /src/Notification.Service
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

COPY Notification.Service/appsettings*.json ./

ENTRYPOINT ["dotnet", "Notification.Service.dll"]
