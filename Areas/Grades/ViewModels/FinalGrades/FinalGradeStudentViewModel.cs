namespace SchoolManager.Areas.Grades.ViewModels.FinalGrades
{
    public class FinalGradeStudentViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = null!;
        public string Matricula { get; set; } = null!;
        public decimal? FinalGrade { get; set; }
        public bool Passed { get; set; }
        public int? FinalGradeId { get; set; }
        public bool HasExtraordinary { get; set; }
        public decimal? ExtraordinaryGrade { get; set; }
        public int? ExtraordinaryGradeId { get; set; }
        public List<UnitGradeSummary> UnitGrades { get; set; } = new();
    }

    public class UnitGradeSummary
    {
        public int UnitNumber { get; set; }
        public decimal? Grade { get; set; }
        public decimal? Recovery { get; set; }
        public decimal? FinalUnitGrade { get; set; }
    }
}
