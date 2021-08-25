﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controller;
[Route("api/[controller]")]
[ApiController]
public class LocalApiController : ControllerBase
{
    [Route("/local/identity")]
    [Authorize]
    public async Task<IActionResult> GetAsync()
    {
        var token = await HttpContext.GetUserAccessTokenAsync();
        var name = User.FindFirst("name")?.Value ?? User.FindFirst("sub")?.Value;
        return new JsonResult(new { message = "Local API Success!", user = name });
    }
}
