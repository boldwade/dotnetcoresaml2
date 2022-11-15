using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sustainsys.Saml2.DemoAppOne.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorizationController : ControllerBase
    {
        [HttpGet]
        public ActionResult Get()
        {
            //var jwt = HttpContext.Session.GetString("JWT");
            HttpContext.Request.Cookies.TryGetValue("JWT", out var jwt);

            if (string.IsNullOrEmpty(jwt))
                return Unauthorized();

            return Ok(jwt);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("TestAuthorization")]
        public ActionResult TestAuthorization()
        {
            return Ok("Congratulations, you are authorized.");
        }
    }
}
