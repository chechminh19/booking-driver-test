using System;
using System.Collections.Generic;

namespace Infrastructure;

public partial class Cab
{
    public long Id { get; set; }

    public long? DriverId { get; set; }

    public string? Type { get; set; }

    public string? RegNo { get; set; }

    public virtual Driver? Driver { get; set; }

    public virtual ICollection<Driver> Drivers { get; set; } = new List<Driver>();
}
