namespace MemberCard.Models;

public class TransactionItem
{
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public int PointsDelta { get; set; } // + earn / - redeem
}