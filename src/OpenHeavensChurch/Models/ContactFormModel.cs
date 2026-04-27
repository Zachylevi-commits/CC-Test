using System.ComponentModel.DataAnnotations;

namespace OpenHeavensChurch.Models;

public class ContactFormModel
{
    [Required(ErrorMessage = "Please tell us your name.")]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please provide an email address so we can reply.")]
    [EmailAddress(ErrorMessage = "That doesn't look like a valid email address.")]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [StringLength(40)]
    public string? Phone { get; set; }

    [StringLength(120)]
    public string? Topic { get; set; }

    [Required(ErrorMessage = "Please add a short message.")]
    [StringLength(4000, MinimumLength = 5, ErrorMessage = "Messages should be between 5 and 4000 characters.")]
    public string Message { get; set; } = string.Empty;

    [Range(typeof(bool), "true", "true", ErrorMessage = "Please confirm that you're happy for us to reply.")]
    public bool Consent { get; set; }

    // Honeypot. Real users never see this field.
    public string? Website { get; set; }

    public string? ReturnUrl { get; set; }
}
