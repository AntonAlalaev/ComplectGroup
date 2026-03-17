namespace ComplectGroup.Application.DTOs;

public class ReceiptTransactionDto
{
    public int Id { get; set; }

    public PartDto Part { get; set; } = default!;

    public int Quantity { get; set; }

    public DateTime ReceiptDate { get; set; }

    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// ID пользователя, выполнившего приёмку
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Имя пользователя, выполнившего приёмку
    /// </summary>
    public string UserName { get; set; } = string.Empty;
}
