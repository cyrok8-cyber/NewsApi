using System.ComponentModel.DataAnnotations;

namespace NewsApi.DTOs.Requests
{
    public class CreateCommentRequest
    {
        [Required, MaxLength(300)]
        public string Text { get; set; } = string.Empty;
    }
}