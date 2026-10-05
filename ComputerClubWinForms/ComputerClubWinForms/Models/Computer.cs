namespace ComputerClubWinForms.Models;

public class Computer
{
    public int Number { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public override string ToString() => $"Компьютер №{Number}";
}
