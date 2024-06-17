namespace VinhUni_Educator_API.Models
{
    public class UpdateQuestionKitModel
    {
        public string KitName { get; set; } = null!;
        public string KitDescription { get; set; } = null!;
        public string Tag { get; set; } = null!;
        public string CategoryId { get; set; } = null!;
    }
}