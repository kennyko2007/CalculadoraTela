# 1. ETAPA DE COMPILACIÓN (SDK de .NET 10.0)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar archivo del proyecto y restaurar dependencias
COPY ["CalculadoraTela.csproj", "./"]
RUN dotnet restore "CalculadoraTela.csproj"

# Copiar todo el código fuente y publicar en modo Release
COPY . .
RUN dotnet publish "CalculadoraTela.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. ETAPA FINAL DE EJECUCIÓN (Runtime de ASP.NET Core 10.0)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copiar la aplicación compilada
COPY --from=build /app/publish .

# Configurar puertos y entorno para Render
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "CalculadoraTela.dll"]
