namespace VinhUni_Educator_API.Models
{
    public class TimeTableSyncModel
    {
        public List<TimeTableItem> lstExportTkbRowData { get; set; } = null!;
    }
    public class TimeTableItem
    {
        public string maHp { get; set; } = null!;
        public string tenLopHp { get; set; } = null!;
        public int idHocKy { get; set; }
        public int idLopHocPhan { get; set; }
        public string maCanBo { get; set; } = null!;
    }
}