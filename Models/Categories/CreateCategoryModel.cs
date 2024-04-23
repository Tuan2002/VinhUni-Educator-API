using System.ComponentModel.DataAnnotations;

namespace VinhUni_Educator_API.Models
{
    public class CreateCategoryModel
    {
        [Required]
        public string CategoryName { get; set; } = null!;
        [Required]
        public string Description { get; set; } = null!;
    }
}