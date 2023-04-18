using System.ComponentModel.DataAnnotations;
// ReSharper disable UnusedAutoPropertyAccessor.Global

#pragma warning disable CS8618
namespace Application.User.Model;

public class CreateAdminAndOrganizationRequest
{
    // User
    [EmailAddress]
    [Required(ErrorMessage = "Email is a mandatory field")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Password field is mandatory")]
    public string Password { get; set; }
    
    [Required(ErrorMessage = "Name is mandatory field")]
    public string FullName { get; set; }
    
    // Organization
    public string OrganizationName { get; set; }
    public string Country { get; set; }
    public string Address { get; set; }
}