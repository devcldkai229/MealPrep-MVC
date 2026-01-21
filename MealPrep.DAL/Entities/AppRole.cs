using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MealPrep.DAL.Entities;

public class AppRole
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}

