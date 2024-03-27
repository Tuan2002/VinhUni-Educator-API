
using System.ComponentModel.DataAnnotations;

namespace VinhUni_Educator_API.Models
{
    public class UploadProfileImage
    {
        [Required]
        public string ImageURL { get; set; } = null!;
    }
}