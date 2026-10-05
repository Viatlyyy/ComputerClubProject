namespace ComputerClubWinForms.Models;

public class Client
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public double TotalHours { get; set; }
    public decimal TotalSpent { get; set; }
    public int DiscountPercent { get; set; }

    public override string ToString() => $"{FullName} ({Phone})";
}
