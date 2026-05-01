using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace DraftLite.Api.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    private readonly ILogger<BaseController> _logger;

    public BaseController(ILogger<BaseController> logger)
    {
        _logger = logger;
    }

    protected string? googleId { 
        get { 
             var sub = User.Claims.FirstOrDefault(e=> e.Type == ClaimTypes.NameIdentifier, new Claim("", "")).Value;
             return sub;
            } 
        }         // Google user ID
    protected string? email { 
        get { 
            return User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email"); 
            } 
        }

    protected string? name
    {
        get
        {
            return User.FindFirstValue(ClaimTypes.Name)
                  ?? User.FindFirstValue("name");
        }
    }
    protected string? picture { 
        get { 
            return User.FindFirstValue("picture"); 
            } 
        }
    protected string? CurrentUserId
    {
        get
        {
            var sub = User.Claims.FirstOrDefault(e=> e.Type == ClaimTypes.NameIdentifier, new Claim("", "")).Value;
            return sub;
        }
    }
}
