namespace SchoolManager.Areas.Grades.ViewModels.FinalGrades
{
    public class FinalGradeListViewModel
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = null!;
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = null!;
        public string GradeLevelName { get; set; } = null!;
        public decimal MinPassingGradeUsed { get; set; }
        public List<FinalGradeStudentViewModel> Students { get; set; } = new();
    }
}
