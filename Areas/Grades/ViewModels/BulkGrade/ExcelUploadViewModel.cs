namespace SchoolManager.Areas.Grades.ViewModels.BulkGrade
{
    public class ExcelUploadViewModel
    {
        public int GroupId { get; set; }
        public int SubjectId { get; set; }
        public string GroupName { get; set; } = null!;
        public string SubjectName { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public List<string> Headers { get; set; } = new();
        public List<Dictionary<string, object>> PreviewRows { get; set; } = new();
        public ColumnMappingViewModel Mapping { get; set; } = new();
    }

    public class ColumnMappingViewModel
    {
        public int NombreColumnIndex { get; set; } = -1;
        public List<UnitColumnMapping> UnitMappings { get; set; } = new();
    }

    public class UnitColumnMapping
    {
        public int UnitNumber { get; set; }
        public int ColumnIndex { get; set; } = -1;
        public string ColumnName { get; set; } = null!;
    }

    public class ProcessMappingViewModel
    {
        public int GroupId { get; set; }
        public int SubjectId { get; set; }
        public int NombreColumnIndex { get; set; }
        public Dictionary<int, int> UnitColumns { get; set; } = new(); // Key: UnitNumber, Value: ColumnIndex
    }
}