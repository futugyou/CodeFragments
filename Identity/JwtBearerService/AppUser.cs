using Microsoft.AspNetCore.Identity;

namespace JwtBearerService;

public class AppUser : IdentityUser
{
    public string Address { get; set; }
}