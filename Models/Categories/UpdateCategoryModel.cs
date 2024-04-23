using System.ComponentModel.DataAnnotations;

namespace VinhUni_Educator_API.Models
{
    public class UpdateCategoryModel
    {
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
    }
}