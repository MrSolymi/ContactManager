using System.IO;
using McDContactManager.data;
using McDContactManager.Model;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace McDContactManager.Service;

public static class EmlProcessorService
{
    public static List<Contact> ProcessEmlFolder(string folderPath)
    {
        var results = new List<Contact>();
        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine($"[EML] Mappa nem található: {folderPath}");
            return results;
        }

        var files = Directory.GetFiles(folderPath, "*.eml", SearchOption.TopDirectoryOnly);
        Console.WriteLine($"[EML] {files.Length} fájl feldolgozása...");

        foreach (var file in files)
        {
            try
            {
                var msg = MimeMessage.Load(file);

                // dátum: ha nincs, akkor fallback a fájl create date
                var assignedDate = msg.Date != DateTimeOffset.MinValue
                    ? msg.Date.LocalDateTime
                    : File.GetCreationTime(file);

                // HTML body keresése
                string? html = msg.HtmlBody;
                if (string.IsNullOrWhiteSpace(html) && msg.TextBody != null)
                    html = msg.TextBody;

                if (string.IsNullOrWhiteSpace(html))
                {
                    Console.WriteLine($"[EML] Üres body: {file}");
                    continue;
                }

                var (name, phone, email) = EmailParser.Parse(html);

                if (string.IsNullOrWhiteSpace(name) ||
                    string.IsNullOrWhiteSpace(phone) ||
                    string.IsNullOrWhiteSpace(email))
                {
                    Console.WriteLine($"[EML] Nem találtam elég adatot: {Path.GetFileName(file)}");
                    continue;
                }

                var contact = new Contact(name, phone, email, assignedDate);
                results.Add(contact);

                Console.WriteLine($"[EML] {contact}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EML] Hiba '{file}' feldolgozásakor: {ex.Message}");
            }
        }

        return results;
    }

    public static int SaveContactsToDatabase(List<Contact> contacts)
    {
        if (contacts.Count == 0)
            return 0;

        // --- normalizáló függvények ---
        static string NormName(string? s)  => (s ?? "").Trim();
        static string NormEmail(string? s) => (s ?? "").Trim().ToLowerInvariant();
        static string NormPhone(string? s) => new string((s ?? "").Where(char.IsDigit).ToArray()); // csak számjegyek
        
        // --- memóriabeli deduplikálás: Név+Telefon+Email ---
        var deduped = contacts
            .Select(c => new
            {
                Original = c,
                Key = new
                {
                    Name = NormName(c.Name),
                    Phone = NormPhone(c.Phone),
                    Email = NormEmail(c.Email)
                }
            })
            .GroupBy(x => x.Key)         // teljes hármas kulcs
            .Select(g =>
            {
                // ha több azonos kulcsú érkezik, vedd az elsőt (vagy itt dönthetsz, melyiket preferálod)
                var first = g.First().Original;
                // fontos: a normalizált értékeket írd vissza, hogy konszisztensek legyenek az adatbázisban
                first.Name  = NormName(first.Name);
                first.Phone = NormPhone(first.Phone);
                first.Email = NormEmail(first.Email);
                return first;
            })
            .ToList();


        using var db = new DatabaseContext("contacts.db");
        db.Database.EnsureCreated();

        // --- létező rekordok kulcsainak beolvasása egyszerre ---
        var existingKeys = db.Contacts
            .Select(x => new
            {
                Name  = x.Name,
                Phone = x.Phone,
                Email = x.Email
            })
            .AsEnumerable() // SQLite miatt oké; alternatíva: ToList()
            .Select(k => $"{k.Name}||{k.Phone}||{k.Email}")
            .ToHashSet(StringComparer.Ordinal);

        // --- új rekordok kiválogatása: akkor új, ha Név+Telefon+Email nincs az adatbázisban ---
        var toInsert = deduped
            .Where(c =>
            {
                var key = $"{c.Name}||{c.Phone}||{c.Email}";
                return !existingKeys.Contains(key);
            })
            .ToList();

        if (toInsert.Count == 0)
            return 0;

        db.Contacts.AddRange(toInsert);
        try
        {
            db.SaveChanges();
            return toInsert.Count;
        }
        catch (DbUpdateException)
        {
            // Ha párhuzamos beszúrás/versenyhelyzet miatt belefut a unique indexbe,
            // itt fel lehet fogni és visszaadni a biztosan beszúrtak számát.
            // Egy egyszerű “best-effort” megoldás:
            // újratöltöd az existingKeys-et és kiszámolod, mennyi került be ténylegesen.
            return db.ChangeTracker.Entries<Contact>().Count(e => e.State == EntityState.Unchanged);
        }
    }
}