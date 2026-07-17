using System;
using System.Collections.Generic;

namespace ClinicSoft.Models;

public partial class Admin
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public string Email { get; set; } = null!;

    public DateOnly Birthday { get; set; }

    public string Phone { get; set; } = null!;

    public int GenderCode { get; set; }

    public virtual Gender GenderCodeNavigation { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
