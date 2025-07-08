using ZorZap.Core.Entities;

namespace ZorZap.Core.Interfaces;

public interface IScanReportRepository
{
    // Méthode pour ajouter un nouveau rapport de scan
    // Cette méthode est utilisée pour enregistrer un nouveau rapport de scan dans la base de données.
    // Elle prend un objet ScanReport en paramètre et retourne une tâche asynchrone
    Task AddAsync(ScanReport report); // <-- AJOUTÉ
}
