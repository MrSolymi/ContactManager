using System.Net;
using System.Text.RegularExpressions;

namespace McDContactManager.Service;

public class EmailParser
{
    public static (string Name, string Phone, string Email) Parse(string htmlBody)
    {
        string name = "";
        string phone = "";
        string email = "";

        // HTML entity decode
        var text = WebUtility.HtmlDecode(htmlBody);

        // Normalize whitespace
        text = Regex.Replace(text, @"\s+", " ");

        // Név
        var nameMatch = Regex.Match(text, @"Név[:：]?\s*<b>([^<]+)</b>", RegexOptions.IgnoreCase);
        if (nameMatch.Success)
            name = nameMatch.Groups[1].Value.Trim();

        // Telefonszám
        var phoneMatch = Regex.Match(text, @"Telefon[:：]?\s*<b>([\d\+\s]+)</b>", RegexOptions.IgnoreCase);
        if (phoneMatch.Success)
            phone = phoneMatch.Groups[1].Value.Replace(" ", "").Trim();

        // Email
        var emailMatch = Regex.Match(text, @"Email[:：]?\s*(.*?)<b>@<a[^>]*>([^<]+)</a></b>", RegexOptions.IgnoreCase);
        if (emailMatch.Success)
        {
            string user = Regex.Replace(emailMatch.Groups[1].Value, @"<[^>]*>", "").Trim();
            string domain = emailMatch.Groups[2].Value.Trim();
            email = $"{user}@{domain}";
        }

        return (name, phone, email);
    }
}