// ============================================================================
// ARCHIVO 1 — CREAR NUEVO
// Ruta: Areas/Grades/ViewModels/GradeCapture/ExcelImportViewModel.cs
// ============================================================================

namespace SchoolManager.Areas.Grades.ViewModels.GradeCapture
{
    /// <summary>
    /// Modelo para el endpoint ApplyExcelMapping.
    /// Solo necesita columna de nombres y columna de calificaciones
    /// (la unidad ya se conoce desde la vista de captura).
    /// </summary>
    public class ExcelImportViewModel
    {
        public int TeacherSubjectGroupId { get; set; }
        public int UnitId { get; set; }
        public int GroupId { get; set; }
        public string FileId { get; set; } = null!;
        public int NombreColumnIndex { get; set; } = -1;
        public int CalificacionColumnIndex { get; set; } = -1;
        public bool HasHeaderRow { get; set; }
    }

    /// <summary>
    /// Resultado individual de importación para serializar a JSON.
    /// </summary>
    public class ImportedGradeResult
    {
        public int StudentId { get; set; }
        public decimal Grade { get; set; }
    }
}
