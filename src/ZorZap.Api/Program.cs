// 1. Déclarer les "using" nécessaires en haut du fichier
using Microsoft.EntityFrameworkCore;
using ZorZap.Core.Interfaces;
using ZorZap.Infrastructure.Persistence;
using ZorZap.Infrastructure.Repositories;
using ZorZap.Application.Interfaces;
using Hangfire;
using Hangfire.PostgreSql; // <-- USING AJOUTÉ

// 2. Créer l'application Web
// C'est le point de départ de notre application.
var builder = WebApplication.CreateBuilder(args);

// --- DÉBUT DE LA MODIFICATION ---

// On récupère la chaîne de connexion depuis appsettings.json UNE SEULE FOIS.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Configurer les services (l'injection de dépendances)
// On dit à notre application quels "outils" elle peut utiliser.

// On enregistre le DbContext et on lui dit d'utiliser PostgreSQL.
// L'ancienne ligne "UseInMemoryDatabase" est supprimée.
builder.Services.AddDbContext<ZorZapDbContext>(options => 
    options.UseNpgsql(connectionString));

// On fait le lien entre l'interface et sa classe concrète.
// Quand un contrôleur demandera un "IScanReportRepository", le système lui donnera un "ScanReportRepository".
builder.Services.AddScoped<IScanReportRepository, ScanReportRepository>();
// On enregistre le service de scan ZAP.
// Quand un contrôleur demandera un "IZapScanService", le système lui donnera un "ZapScanService".
builder.Services.AddScoped<IZapScanService, ZorZap.Infrastructure.Services.ZapScanService>();

// On configure Hangfire pour qu'il utilise PostgreSQL comme stockage persistant.
// L'ancienne ligne "UseInMemoryStorage" est remplacée.
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180) // Bonne pratique pour la compatibilité
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString))); // On utilise la même connexion

builder.Services.AddHangfireServer();

// --- FIN DE LA MODIFICATION ---

// On ajoute le service de configuration pour lire les paramètres de l'application.
// On ajoute le support pour les contrôleurs d'API.
builder.Services.AddControllers();

// On ajoute les services pour générer la documentation Swagger.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// On ajoute la prise en charge de CORS pour permettre aux applications front-end d'accéder à l'API.

var app = builder.Build();

// 3. Configurer le pipeline de requêtes HTTP
// On définit dans quel ordre les requêtes sont traitées.

// En mode développement, on active l'interface visuelle de Swagger.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHangfireDashboard();
// On active CORS pour permettre aux applications front-end d'accéder à l'API.

// Redirige les requêtes HTTP vers HTTPS.
app.UseHttpsRedirection();

// Indique à l'application d'utiliser les routes définies dans vos contrôleurs.
// C'est cette ligne qui permet à "/api/scans" de fonctionner.
app.MapControllers();

app.Run();
// 4. Démarrer l'application
