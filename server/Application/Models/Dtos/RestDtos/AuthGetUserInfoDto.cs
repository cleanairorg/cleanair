namespace Application.Models.Dtos.RestDtos;

public class AuthGetUserInfoDto
{
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
}