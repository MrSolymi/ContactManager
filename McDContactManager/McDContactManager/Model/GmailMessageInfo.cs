namespace McDContactManager.Model;

public class GmailMessageInfo
{
    public string Id { get; init; } = "";
    public string ThreadId { get; init; } = "";
    public DateTime? Date { get; init; }
    public string From { get; init; } = "";
    public string To { get; init; } = "";
    public string Subject { get; init; } = "";
    public string Snippet { get; init; } = "";
    public bool HasAttachments { get; init; }
    public int AttachmentCount { get; init; }
}