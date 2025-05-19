using System;
using System.Collections.Generic;

namespace Core.Domain.Entities;

public partial class Devicelog
{
    public string Deviceid { get; set; } = null!;

    public string Id { get; set; } = null!;

    public string Unit { get; set; } = null!;

    public DateTime Timestamp { get; set; }

    public decimal Temperature { get; set; }

    public decimal Humidity { get; set; }

    public decimal Pressure { get; set; }

    public int Airquality { get; set; }
}
