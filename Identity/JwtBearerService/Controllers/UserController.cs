using Microsoft.AspNetCore.Mvc;

namespace JwtBearerService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _userService.RegisterAsync(request.UserName, request.Password, request.Address);
        if (!result.Success)
        {
            return BadRequest(new FailedResponse()
            {
                Errors = result.Errors
            });
        }
        return Ok(new TokenResponse
        {
            AccessToken = result.AccessToken,
            TokenType = result.TokenType
        });
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _userService.LoginAsync(request.UserName, request.Password);
        if (!result.Success)
        {
            return Unauthorized(new FailedResponse()
            {
                Errors = result.Errors
            });
        }
        return Ok(new TokenResponse
        {
            AccessToken = result.AccessToken,
            TokenType = result.TokenType
        });
    }

    [HttpPost("RefreshToken")]
    public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
    {
        var result = await _userService.RefreshTokenAsync(request.AccessToken, request.RefreshToken);
        if (!result.Success)
        {
            return Unauthorized(new FailedResponse()
            {
                Errors = result.Errors
            });
        }

        return Ok(new TokenResponse
        {
            AccessToken = result.AccessToken,
            TokenType = result.TokenType,
            ExpiresIn = result.ExpiresIn,
            RefreshToken = result.RefreshToken
        });
    }
}