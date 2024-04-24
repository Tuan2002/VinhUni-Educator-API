using System.ComponentModel.DataAnnotations;

namespace VinhUni_Educator_API.Models
{
    public class CreateQuestionKitModel
    {
        [Required]
        public string KitName { get; set; } = null!;
        public string? KitDescription { get; set; }
        public string? Tag { get; set; }
        [Required]
        public string CategoryId { get; set; } = null!;
    }
}