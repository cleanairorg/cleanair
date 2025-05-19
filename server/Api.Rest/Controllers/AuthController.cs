using Api.Rest.Extensions;
using Application.Interfaces;
using Application.Interfaces.Infrastructure.Logging;
using Application.Models.Dtos.RestDtos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest.Controllers;

[ApiController]
public class AuthController(ISecurityService securityService, ILoggingService logger) : ControllerBase
{
    public const string ControllerRoute = "api/auth/";

    public const string LoginRoute = ControllerRoute + nameof(Login);


    public const string RegisterRoute = ControllerRoute + nameof(Register);
    
    
    public const string GetUserInfoRoute = ControllerRoute + nameof(GetUserInfo);


    public const string SecuredRoute = ControllerRoute + nameof(Secured);


    [Route(LoginRoute)]
    [HttpPost]
    public ActionResult<AuthResponseDto> Login([FromBody] AuthLoginRequestDto dto)
    {
        logger.LogInformation($"Login request: {dto}");
        return Ok(securityService.Login(dto));
        
    }

    [Route(RegisterRoute)]
    [HttpPost]
    public ActionResult<AuthResponseDto> Register([FromBody] AuthRegisterRequestDto dto)
    {
        logger.LogInformation($"Register request: {dto}");
        return Ok(securityService.Register(dto));
    }

    [Route(GetUserInfoRoute)]
    [HttpGet]
    public ActionResult<AuthGetUserInfoDto> GetUserInfo(string email)
    {
        return Ok(securityService.GetUserInfo(email));
    }

    [Route(SecuredRoute)]
    [HttpGet]
    public ActionResult Secured()
    {
        securityService.VerifyJwtOrThrow(HttpContext.GetJwt());
        logger.LogInformation("Secured route requested");
        return Ok("You are authorized to see this message");
    }
}