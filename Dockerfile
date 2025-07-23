# --- Étape 1: Build ---
# Utilise l'image du SDK.NET 6 pour la compilation. Nommée 'build-env' pour référence ultérieure.
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copie les fichiers.csproj et.sln et restaure les dépendances en premier.
# Cela tire parti de la mise en cache des couches Docker : cette étape ne sera ré-exécutée
# que si les fichiers de projet changent.
COPY *.sln .
COPY src/ZorZap.Api/*.csproj ./src/ZorZap.Api/
COPY src/ZorZap.Application/*.csproj ./src/ZorZap.Application/
COPY src/ZorZap.Core/*.csproj ./src/ZorZap.Core/
COPY src/ZorZap.Infrastructure/*.csproj ./src/ZorZap.Infrastructure/
RUN dotnet restore "ZorZap.sln"

# Copie le reste du code source.
COPY . .
WORKDIR /app/src/ZorZap.Api
RUN dotnet publish -c Release -o /app/publish

# --- Étape 2: Production ---
# Utilise l'image de runtime ASP.NET, beaucoup plus légère que l'image du SDK.
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app

# Copie les artéfacts publiés de l'étape de build ('build-env') vers l'image finale.
COPY --from=build-env /app/publish .

# Expose le port 80, le port par défaut pour les applications web.
EXPOSE 80

# Définit le point d'entrée pour exécuter l'application lorsque le conteneur démarre.
ENTRYPOINT ["dotnet", "ZorZap.Api.dll"]
