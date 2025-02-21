using System.ComponentModel.DataAnnotations;

public class UserRegisterDto
{
    [Required]
    public string Name { get; set; }

    [Required]
    public string Surname { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MinLength(8)] // Minimum 8 karakter
    public string Password { get; set; }
}