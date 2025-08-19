namespace MemberCard.Models;

public class Member
{
    public string MemberId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public int Points { get; set; }
}