using System;

namespace WebApp.Domain.Parsers;

public class AccountItem
{
    public DateTime? Date { get; set; }
    public double? Amount { get; set; }
    public string? Payee { get; set; }
}