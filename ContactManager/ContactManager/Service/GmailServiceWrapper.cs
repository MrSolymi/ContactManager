using System.IO;
using ContactManager.Model;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;

namespace ContactManager.Service;

public class GmailServiceWrapper
{
    private readonly GmailService _gmail;
    public GmailServiceWrapper(GmailService gmail) => _gmail = gmail;

    public async Task<List<GmailMessageInfo>> SearchEmailsFromAsync(
        string fromEmail,
        int maxMessages = 200,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(fromEmail))
            return new List<GmailMessageInfo>();

        string BuildQuery()
        {
            var q = $"from:{fromEmail} in:anywhere"; // Spam/Trash is látszik
            return q;
        }

        var listReq = _gmail.Users.Messages.List("me");
        listReq.Q = BuildQuery();
        listReq.MaxResults = 100;

        var results = new List<GmailMessageInfo>();
        string? next = null;

        do
        {
            listReq.PageToken = next;
            var page = await listReq.ExecuteAsync(ct);
            var ids = page.Messages ?? new List<Message>();
            if (ids.Count == 0) break;

            foreach (var m in ids)
            {
                if (results.Count >= maxMessages) break;

                var getReq = _gmail.Users.Messages.Get("me", m.Id);
                getReq.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Full;
                // getReq.Fields = "id,internalDate,payload"; // opcionális, de biztonságos

                var full = await getReq.ExecuteAsync(ct);

                string H(string name) =>
                    full.Payload?.Headers?.FirstOrDefault(h => h.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value ?? "";

                DateTime? parsedDate = null;
                if (full.InternalDate.HasValue)
                    parsedDate = DateTimeOffset.FromUnixTimeMilliseconds(full.InternalDate.Value).LocalDateTime;
                else if (DateTime.TryParse(H("Date"), out var d)) parsedDate = d;

                var (hasAtt, count) = DetectAttachments(full);

                results.Add(new GmailMessageInfo
                {
                    Id = full.Id,
                    Date = parsedDate,
                    From = H("From"),
                    Subject = H("Subject"),
                    HasAttachments = hasAtt,
                    AttachmentCount = count
                });
            }


            next = page.NextPageToken;
        }
        while (!string.IsNullOrEmpty(next) && results.Count < maxMessages);

        // rendezés dátum szerint (újak elöl)
        return results.OrderByDescending(r => r.Date ?? DateTime.MinValue).ToList();
    }

    public async Task DownloadAttachmentsAsync(
        IEnumerable<string> messageIds,
        string downloadDir,
        bool onlyOutlookItems = true, // csak .eml/.msg és message/rfc822
        CancellationToken ct = default)
    {
        Directory.CreateDirectory(downloadDir);
        var saved = new List<string>();

        foreach (var id in messageIds.Distinct())
        {
            var getReq = _gmail.Users.Messages.Get("me", id);
            getReq.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Full;
            var full = await getReq.ExecuteAsync(ct);
            if (full?.Payload == null) continue;

            foreach (var p in FlattenParts(full.Payload))
            {
                var mime = p.MimeType ?? "";
                var fname = p.Filename ?? "";
                var attId = p.Body?.AttachmentId;

                bool isRfc822 = string.Equals(mime, "message/rfc822", StringComparison.OrdinalIgnoreCase);
                bool isEmlName = !string.IsNullOrEmpty(fname) && fname.EndsWith(".eml", StringComparison.OrdinalIgnoreCase);
                bool isMsg    = (!string.IsNullOrEmpty(fname) && fname.EndsWith(".msg", StringComparison.OrdinalIgnoreCase))
                                || string.Equals(mime, "application/vnd.ms-outlook", StringComparison.OrdinalIgnoreCase);

                if (attId == null) continue;

                if (onlyOutlookItems && !(isRfc822 || isEmlName || isMsg))
                    continue; // csak outlook-elem (.eml/.msg)

                var att = await _gmail.Users.Messages.Attachments.Get("me", id, attId).ExecuteAsync(ct);
                if (string.IsNullOrEmpty(att.Data)) continue;

                var bytes = Base64UrlDecode(att.Data);

                // fájlnév
                var safeName = !string.IsNullOrWhiteSpace(fname)
                    ? SanitizeFileName(fname)
                    : isRfc822 ? "outlook-item.eml"
                    : isMsg    ? "outlook-item.msg"
                    : "attachment.bin";

                var path = Path.Combine(downloadDir, $"{id}_{p.PartId}_{safeName}");
                await File.WriteAllBytesAsync(path, bytes, ct);
                saved.Add(path);

                Console.WriteLine($"[SAVE] {path}");
            }
        }

        Console.WriteLine($"[ATT] saved files: {saved.Count}");

        // ---- helpers ----
        static byte[] Base64UrlDecode(string input)
        {
            var s = input.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4) { case 2: s += "=="; break; case 3: s += "="; break; }
            return Convert.FromBase64String(s);
        }

        static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            // néha nagyon hosszú subjectből jön a név:
            if (name.Length > 150) name = name.Substring(0, 150);
            return name;
        }
    }

    private static IEnumerable<MessagePart> FlattenParts(MessagePart root)
    {
        var stack = new Stack<MessagePart>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var p = stack.Pop();
            yield return p;
            if (p.Parts != null) foreach (var c in p.Parts) stack.Push(c);
        }
    }

    private static (bool hasAtt, int count) DetectAttachments(Message full)
    {
        if (full?.Payload == null) return (false, 0);

        int n = 0;
        foreach (var p in FlattenParts(full.Payload))
        {
            var mime = p.MimeType ?? "";
            var hasId = !string.IsNullOrEmpty(p.Body?.AttachmentId);
            var hasName = !string.IsNullOrEmpty(p.Filename);
            var isRfc822 = string.Equals(mime, "message/rfc822", StringComparison.OrdinalIgnoreCase);

            if (hasId || hasName || isRfc822) n++;
        }
        return (n > 0, n);
    }

}