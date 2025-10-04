using System.IO;
using McDContactManager.data;
using McDContactManager.Model;
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

        // --- deduplikálás memóriában ---
        contacts = contacts
            .GroupBy(c => new { c.Phone, c.Email })
            .Select(g => g.First())
            .ToList();

        using var db = new DatabaseContext("contacts.db");
        db.Database.EnsureCreated();

        int newCount = 0;

        foreach (var c in contacts)
        {
            var exists = db.Contacts.Any(x => x.Phone == c.Phone || x.Email == c.Email);
            if (exists) continue;

            db.Contacts.Add(c);
            newCount++;
        }

        db.SaveChanges();
        return newCount;
    }
}