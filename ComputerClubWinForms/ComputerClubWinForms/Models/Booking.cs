using System;

namespace ComputerClubWinForms.Models;

public class Booking
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public int ComputerNumber { get; set; }
    public DateTime PlannedStart { get; set; }
    public TimeSpan Duration { get; set; }
    public string Status { get; set; } = "active";

    public DateTime PlannedEnd => PlannedStart.Add(Duration);
}
