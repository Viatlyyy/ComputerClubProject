using System;
using System.Linq;

namespace ComputerClubWinForms.Models;

public class Client
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public double TotalHours { get; set; }
    public decimal TotalSpent { get; set; }
    public int DiscountPercent { get; set; }

    internal static string GetPhoneDigits(string text)
    {
        if (text.Skip(text.StartsWith('+') ? 1 : 0).Any(c =>
            !char.IsAsciiDigit(c) && !char.IsWhiteSpace(c) && c is not ('(' or ')' or '-' or '–' or '—' or '.')))
            return string.Empty;
        return new string(text.Where(char.IsAsciiDigit).ToArray());
    }

    internal static bool TryGetRussianPhoneDigits(string phone, out string localDigits)
    {
        localDigits = string.Empty;
        var text = phone.Trim();
        var hasPlus = text.StartsWith('+');
        var digits = GetPhoneDigits(text);
        if (digits.Length == 11 && (hasPlus ? digits[0] == '7' : digits[0] is '7' or '8'))
        {
            localDigits = digits[1..];
            return true;
        }
        if (!hasPlus && digits.Length == 10)
        {
            localDigits = digits;
            return true;
        }
        return false;
    }

    public override string ToString() => $"{FullName} ({Phone})";
}
