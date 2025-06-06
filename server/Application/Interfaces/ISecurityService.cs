using Application.Models;
using Application.Models.Dtos.RestDtos;

namespace Application.Interfaces;

public interface ISecurityService
{
    public string HashPassword(string password);
    public void VerifyPasswordOrThrow(string password, string hashedPassword);
    public string GenerateSalt();
    public string GenerateJwt(JwtClaims claims);
    public AuthResponseDto Login(AuthLoginRequestDto dto);
    public AuthResponseDto Register(AuthRegisterRequestDto dto);
    public AuthGetUserInfoDto GetUserInfo(string email);
    public JwtClaims VerifyJwtOrThrow(string jwt);
}