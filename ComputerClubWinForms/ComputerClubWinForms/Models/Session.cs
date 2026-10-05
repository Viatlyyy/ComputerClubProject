using System;

namespace ComputerClubWinForms.Models;

public class Session
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public int ComputerNumber { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime PlannedEndTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TariffPerHour { get; set; }
    public decimal OneTimeDiscountPercent { get; set; }
    public decimal PersonalDiscountPercent { get; set; }
    public bool IsCompleted { get; set; }

    public TimeSpan RemainingTime => PlannedEndTime - DateTime.Now;
}
