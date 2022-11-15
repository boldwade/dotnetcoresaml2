using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Sustainsys.Saml2.AspNetCore2;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Sustainsys.Saml2.DemoAppOne.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Saml2Controller : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public Saml2Controller(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        [AllowAnonymous]
        [HttpGet("InitiateSingleSignOn")]
        public IActionResult InitiateSingleSignOn(string? returnUrl)
        {
            return new ChallengeResult(
                Saml2Defaults.Scheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action(nameof(LoginCallback), new { returnUrl }),
                });
        }

        [AllowAnonymous]
        [HttpGet("callback")]
        public async Task<IActionResult> LoginCallback(String? returnUrl)
        {
            //var authenticateResult = await HttpContext.AuthenticateAsync(ApplicationSamlConstants.External);
            var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
            {
                return Unauthorized();
            }

            var token = CreateJwtSecurityToken(authenticateResult);
            var tokenHandler = new JwtSecurityTokenHandler().WriteToken(token);
            Response.Cookies.Append("JWT", tokenHandler, new CookieOptions { SameSite = SameSiteMode.Lax });
            //HttpContext.Session.SetString("JWT", tokenHandler);

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);

            }

            return Ok(tokenHandler);
        }

        private JwtSecurityToken CreateJwtSecurityToken(AuthenticateResult authenticateResult)
        {
            var claimsIdentity = new ClaimsIdentity(ApplicationSamlConstants.Application);
            var firstPrincipal = authenticateResult.Principal?.FindFirst(ClaimTypes.NameIdentifier);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            if (firstPrincipal != null)
            {
                claimsIdentity.AddClaim(firstPrincipal);
                var username = firstPrincipal.Value.ToString();
                claims.Add(new Claim(JwtRegisteredClaimNames.Sub, username));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            return new JwtSecurityToken(
                _configuration["JWT:Issuer"],
                _configuration["JWT:Issuer"],
                claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
            );
        }
    }
}
