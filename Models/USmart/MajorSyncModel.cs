namespace VinhUni_Educator_API.Models
{
    public class MajorSyncModel
    {
        public int id { get; set; }
        public string nganH_Ma { get; set; } = null!;
        public string nganH_Ten { get; set; } = null!;
        public float? nganH_ThoiGianToiThieu { get; set; } = null;
        public float? nganH_ThoiGianToiDa { get; set; } = null;
    }
}