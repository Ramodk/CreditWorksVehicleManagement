using System;

namespace CreditWorks.Domain.Entities;

public class Vehicle
{
 public int Id { get; set; }

    public string Owner { get; set; } = string.Empty;

    public int ManufacturerId { get; set; }

    public Manufacturer Manufacturer { get; set; } = null!;

    public int Year { get; set; }

    public decimal Weight { get; set; }
}
