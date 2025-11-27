using Microsoft.EntityFrameworkCore;

namespace ContactManager.data;

public class DatabaseMigrator
{
    public static void EnsureIsForeignColumn(DatabaseContext db)
    {
        try
        {
            // Régi adatbázisnál ez sikeresen lefut, és hozzáadja a mezőt.
            // Új adatbázisnál (ahol már benne van) "duplicate column name" hibát dob,
            // amit elkapunk és figyelmen kívül hagyunk.
            db.Database.ExecuteSqlRaw(
                "ALTER TABLE Contacts ADD COLUMN IsForeign INTEGER NOT NULL DEFAULT 0;");
        }
        catch (Exception ex)
        {
            // Itt általában "duplicate column name: IsForeign" lesz,
            // ami nekünk teljesen oké.
            Console.WriteLine($"[DB MIGRATION] IsForeign oszlop ellenőrzése: {ex.Message}");
        }
    }
}