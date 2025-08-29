namespace MemberCard.Models;

public class UserUpsertRequest
{
    public string Email { get; set; } = default!;
    public bool EmailConfirmed { get; set; }
}
