using System;
using System.Collections.Generic;

namespace Infrastructure;

public partial class Driver
{
    public long Id { get; set; }

    public long? CabId { get; set; }

    public DateOnly? Dob { get; set; }

    public double? LocationLatitude { get; set; }

    public double? LocationLongitude { get; set; }

    public byte[] CreateAt { get; set; } = null!;

    public long? UserId { get; set; }

    public virtual Cab? Cab { get; set; }

    public virtual ICollection<Cab> Cabs { get; set; } = new List<Cab>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();

    public virtual User? User { get; set; }
}
