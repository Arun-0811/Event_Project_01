using System.ComponentModel.DataAnnotations;

public class ForgotPasswordModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; }

    [Required(ErrorMessage = "OTP is required")]
    public string Otp { get; set; }
}
