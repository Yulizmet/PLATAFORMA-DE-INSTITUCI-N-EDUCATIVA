namespace SchoolManager.Areas.Grades.ViewModels.StudentGrades
{
    public class StudentDashboardViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = null!;
        public string Matricula { get; set; } = null!;
        public string GradeLevel { get; set; } = null!;
        public string GroupName { get; set; } = null!;
        public List<StudentSubjectGradeViewModel> Subjects { get; set; } = new();
        public ResumenViewModel Resumen { get; set; } = new();
    }

    public class StudentSubjectGradeViewModel
    {
        public string SubjectName { get; set; } = null!;
        public List<UnitGradeViewModel> Units { get; set; } = new();
        public decimal? FinalGrade { get; set; }
        public bool Passed { get; set; }
        public bool HasExtraordinary { get; set; }
        public decimal? ExtraordinaryGrade { get; set; }
        public string Status => GetStatus();

        private string GetStatus()
        {
            if (HasExtraordinary) return "Extraordinario";
            if (Passed) return "Aprobado";
            return "Reprobado";
        }
    }

    public class UnitGradeViewModel
    {
        public int UnitNumber { get; set; }
        public decimal? Grade { get; set; }
        public decimal? Recovery { get; set; }
        public decimal? FinalGrade => Recovery ?? Grade;
        public string DisplayGrade => FinalGrade?.ToString("F2") ?? "—";
    }

    public class ResumenViewModel
    {
        public int TotalMaterias { get; set; }
        public int Aprobadas { get; set; }
        public int Reprobadas { get; set; }
        public int Extraordinarios { get; set; }
        public decimal? PromedioGeneral { get; set; }
    }
}