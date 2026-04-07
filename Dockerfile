
# Этап 1: Сборка
FROM ://microsoft.com AS build
WORKDIR /app

# Копируем проект и восстанавливаем зависимости
COPY *.csproj ./
RUN dotnet restore

# Копируем всё остальное и собираем
COPY . ./
RUN dotnet publish -c Release -o out

# Этап 2: Запуск
FROM ://microsoft.com
WORKDIR /app
COPY --from=build /app/out .

# Запуск бота
ENTRYPOINT ["dotnet", "ScheduleBotik.dll"]

