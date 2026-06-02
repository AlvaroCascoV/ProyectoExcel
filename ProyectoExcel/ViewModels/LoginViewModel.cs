using System.ComponentModel.DataAnnotations;

namespace MvcProyectoExcel.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email is required.")]
    [Display(Name = "Email")]
    public string EmailLocalPart { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
