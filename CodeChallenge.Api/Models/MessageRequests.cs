namespace CodeChallenge.Api.Models;
using System.ComponentModel.DataAnnotations;

public class CreateMessageRequest
{
    [Required, StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;
    [Required, StringLength(1000, MinimumLength = 10)]
    public string Content { get; set; } = string.Empty;
}

public class UpdateMessageRequest
{
    [Required, StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;
    [Required, StringLength(1000, MinimumLength = 10)]
    public string Content { get; set; } = string.Empty;
    
    public bool IsActive { get; set; }
}

