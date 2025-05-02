using System;
using System.Collections.Generic;

namespace Core.Domain.Entities;

public partial class Devicelog
{
    public string Id { get; set; } = null!;

    public string Deviceid { get; set; } = null!;

    public decimal Value { get; set; }

    public string Unit { get; set; } = null!;

    public DateTime Timestamp { get; set; }
}
