// Models/ApiWrapper.cs
namespace MemberCard.Models;

public class ApiWrapper<T> where T : class
{
    public int? code { get; set; }
    public string? message { get; set; }
    public T? data { get; set; }
}
