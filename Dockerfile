# Вказуємо базовий образ .NET SDK (для збірки)
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# Встановлюємо робочу директорію
WORKDIR /app

# Копіюємо .sln та всі проєкти
COPY . ./

# Вказуємо проєкт, який будемо збирати (заміни на потрібний, якщо інший)
WORKDIR /app/Lampac

# Збірка проєкту
RUN dotnet publish -c Release -o /app/publish

# ------------------

# Створюємо фінальний образ на базі .NET Runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0

WORKDIR /app

# Копіюємо опубліковану збірку з етапу build
COPY --from=build /app/publish .

# Вказуємо порт (заміни, якщо в тебе інший у Lampac)
EXPOSE 9118

# Команда запуску
ENTRYPOINT ["dotnet", "Lampac.dll"]
