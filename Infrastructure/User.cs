using System;
using System.Collections.Generic;

namespace Infrastructure;

public partial class User
{
    public long Id { get; set; }

    public string? Role { get; set; }

    public string? UserName { get; set; }

    public string Email { get; set; } = null!;

    public string? Password { get; set; }

    public string? Token { get; set; }

    public bool IsConfirm { get; set; }

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual ICollection<Driver> Drivers { get; set; } = new List<Driver>();
}
