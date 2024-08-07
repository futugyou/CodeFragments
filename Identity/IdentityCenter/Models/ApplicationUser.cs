﻿
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace IdentityCenter.Models;
public class ApplicationUser : IdentityUser
{
    [Required]
    [PersonalData]
    public string Card { get; set; } = Guid.NewGuid().ToString();
}
