using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Shared;

[EnableCors("CorsPolicy")]
[ApiController]
public class BaseApiController : ControllerBase
{
    public BaseApiController() { }
}