namespace SchoolManager.Areas.Grades.ViewModels.BulkGrade
{
    public class ColumnMappingInput
    {
        public int ColumnIndex { get; set; }
        public string Type { get; set; } = null!; // "nombre" o "calificacion"
        public int? UnitNumber { get; set; } // Solo para calificaciones
    }



    public class ApplyMappingRequest
    {
        public int TeacherSubjectGroupId { get; set; }
        public int UnitId { get; set; }
        public int GroupId { get; set; }
        public int SubjectId { get; set; }
        public List<ColumnMappingInput> ColumnMappings { get; set; } = new();
    }
}
