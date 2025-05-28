using System;
using System.Collections.Generic;

namespace Core.Domain.Entities;

public partial class DeviceThreshold
{
    public string Id { get; set; } = null!;

    public string Metric { get; set; } = null!;

    public decimal WarnMin { get; set; }

    public decimal GoodMin { get; set; }

    public decimal GoodMax { get; set; }

    public decimal WarnMax { get; set; }
}
