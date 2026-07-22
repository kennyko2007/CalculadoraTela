# 1. Imagen para la compilación (SDK de .NET 10.0)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar el archivo del proyecto y restaurar las dependencias
COPY ["CalculadoraTela.csproj", "./"]
RUN dotnet restore "CalculadoraTela.csproj"

# Copiar el resto del código fuente y compilar la aplicación en modo Release
COPY . .
RUN dotnet publish "CalculadoraTela.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Imagen final para ejecución en producción (Runtime de ASP.NET 10.0)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Puerto por defecto expuesto en contenedores ASP.NET Core
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "CalculadoraTela.dll"]
