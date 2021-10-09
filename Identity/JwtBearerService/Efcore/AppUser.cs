using Microsoft.AspNetCore.Identity;

namespace JwtBearerService;

public class AppUser : IdentityUser<int>
{
    public string Address { get; set; }
}