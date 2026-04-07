FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Копируем ВСЕ файлы сразу (не только .csproj)
COPY . ./

# Восстанавливаем зависимости, указывая конкретный проект
RUN dotnet restore "ScheduleBotik.csproj"

# Публикуем
RUN dotnet publish "ScheduleBotik.csproj" -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "ScheduleBotik.dll"]
