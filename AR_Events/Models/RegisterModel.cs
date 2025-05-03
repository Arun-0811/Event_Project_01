using System.ComponentModel.DataAnnotations;

public class RegisterModel
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    [Display(Name = "Mobile No")]
    public string MobileNo { get; set; } = string.Empty;

    [Required]
    public string City { get; set; } = string.Empty;

    [Required]
    public string State { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;


    [Display(Name = "Profile Photo")]
    public IFormFile PhotoPath { get; set; }


}
