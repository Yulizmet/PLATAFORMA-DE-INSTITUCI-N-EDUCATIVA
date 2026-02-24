// Areas/Grades/ViewModels/GradeCapture/GradeCaptureViewModel.cs
namespace SchoolManager.Areas.Grades.ViewModels.GradeCapture
{
    public class TeacherClassSelectionViewModel
    {
        public List<TeacherClassViewModel> Classes { get; set; } = new();
    }

    public class TeacherClassViewModel
    {
        public int TeacherSubjectGroupId { get; set; }
        public string GroupName { get; set; } = null!;
        public string SubjectName { get; set; } = null!;
        public string GradeLevelName { get; set; } = null!;
        public int GroupId { get; set; }
        public int SubjectId { get; set; }
    }

    public class UnitSelectionViewModel
    {
        public int TeacherSubjectGroupId { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; } = null!;
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = null!;
        public List<UnitOptionViewModel> Units { get; set; } = new();
    }

    public class UnitOptionViewModel
    {
        public int UnitId { get; set; }
        public int UnitNumber { get; set; }
        public bool IsOpen { get; set; }
        public bool HasGrades { get; set; }
    }

    public class GradeCaptureViewModel
    {
        public int TeacherSubjectGroupId { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; } = null!;
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = null!;
        public int UnitId { get; set; }
        public int UnitNumber { get; set; }
        public List<StudentGradeViewModel> Students { get; set; } = new();
    }

    public class StudentGradeViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = null!;
        public string Matricula { get; set; } = null!;
        public int? GradeId { get; set; }
        public decimal? GradeValue { get; set; }
        public bool HasRecovery { get; set; }
        public decimal? RecoveryValue { get; set; }
        public int? RecoveryId { get; set; }
    }

    public class SaveGradesViewModel
    {
        public int TeacherSubjectGroupId { get; set; }
        public int UnitId { get; set; }
        public List<StudentGradeInputViewModel> Grades { get; set; } = new();
    }

    public class StudentGradeInputViewModel
    {
        public int StudentId { get; set; }
        public decimal? GradeValue { get; set; }
    }
}