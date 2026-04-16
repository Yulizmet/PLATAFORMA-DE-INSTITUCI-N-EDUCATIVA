// Areas/Grades/ViewModels/Kardex/KardexViewModel.cs
namespace SchoolManager.Areas.Grades.ViewModels.Kardex
{
    public class KardexViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = null!;
        public string Matricula { get; set; } = "S/N";
        public string Email { get; set; } = null!;

        public List<KardexSemesterViewModel> Semesters { get; set; } = new();

        // Resumen general
        public decimal PromedioGeneral { get; set; }
        public int TotalMaterias { get; set; }
        public int MateriasAprobadas { get; set; }
        public int MateriasReprobadas { get; set; }
        public int MateriasConExtraordinario { get; set; }
    }

    public class KardexSemesterViewModel
    {
        public int GradeLevelId { get; set; }
        public string GradeLevelName { get; set; } = null!;
        public string GroupName { get; set; } = null!;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public bool IsOpen { get; set; }

        public List<KardexSubjectViewModel> Subjects { get; set; } = new();
        public decimal PromedioSemestre { get; set; }
    }

    public class KardexSubjectViewModel
    {
        public string SubjectName { get; set; } = null!;
        public List<KardexUnitGradeViewModel> Units { get; set; } = new();
        public decimal? FinalGrade { get; set; }
        public bool Passed { get; set; }
        public bool HasExtraordinary { get; set; }
        public decimal? ExtraordinaryGrade { get; set; }
    }

    public class KardexUnitGradeViewModel
    {
        public int UnitNumber { get; set; }
        public decimal? Grade { get; set; }
        public decimal? Recovery { get; set; }
        public decimal? EffectiveGrade =>
            (Grade.HasValue && Recovery.HasValue)
                ? Math.Max(Grade.Value, Recovery.Value)
                : Grade ?? Recovery;
    }

    // Para la búsqueda
    public class KardexStudentSearchViewModel
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = null!;
        public string Matricula { get; set; } = "S/N";
        public string Email { get; set; } = null!;
        public string? CurrentGroup { get; set; }
        public string? CurrentGradeLevel { get; set; }
    }
}