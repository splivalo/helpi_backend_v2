namespace Helpi.Application.DTOs.Minimax;

public sealed record MinimaxAttachment
{
    public int AttachmentId { get; init; }
    public string AttachmentData { get; init; } = string.Empty;
    public DateTime AttachmentDate { get; init; }
    public string AttachmentFileName { get; init; } = string.Empty;
    public string AttachmentMimeType { get; init; } = string.Empty;
}
