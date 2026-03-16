// Areas/Grades/ViewModels/Wizard/SchoolCycleWizardViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Areas.Grades.ViewModels.Wizard
{
    public class SchoolCycleWizardViewModel
    {
        // Current step (1-5)
        public int CurrentStep { get; set; } = 1;

        // --- STEP 1: LEVEL DATA ---
        [Required(ErrorMessage = "The level name is required")]
        [Display(Name = "Level Name")]
        public string? LevelName { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Required(ErrorMessage = "Minimum passing grade is required")]
        [Display(Name = "Minimum Passing Grade")]
        [Range(0, 10, ErrorMessage = "Grade must be between 0 and 10")]
        public decimal? MinPassingGrade { get; set; } = 6.0m;

        // --- STEP 2: SUBJECTS DATA ---
        public List<SubjectWizardViewModel> Subjects { get; set; } = new List<SubjectWizardViewModel>();

        // --- STEP 3: GROUPS DATA ---
        public List<GroupWizardViewModel> Groups { get; set; } = new List<GroupWizardViewModel>();

        // --- STEP 4: ASSIGNMENTS DATA ---
        public List<AssignmentWizardViewModel> Assignments { get; set; } = new List<AssignmentWizardViewModel>();

        // --- FLOW CONTROL ---
        public bool IsDraft { get; set; } = false;
    }

    public class SubjectWizardViewModel
    {
        public int TempId { get; set; } // Temporary ID for the view
        public string? Name { get; set; }
        public List<UnitWizardViewModel> Units { get; set; } = new List<UnitWizardViewModel>();
        public bool ShowUnits { get; set; } = false; // To toggle unit panel
    }

    public class UnitWizardViewModel
    {
        public int Number { get; set; }
        public bool IsOpen { get; set; } = true;
    }

    public class GroupWizardViewModel
    {
        public int TempId { get; set; }
        public string? Name { get; set; }
    }

    public class AssignmentWizardViewModel
    {
        public int SubjectTempId { get; set; }
        public string? SubjectName { get; set; }
        public List<TeacherGroupAssignment> TeacherAssignments { get; set; } = new()
            {
                new TeacherGroupAssignment() // Siempre empieza con 1 fila
            };
        public string? TeacherName { get; set; }
    }
    public class TeacherGroupAssignment
    {
        public int? TeacherId { get; set; }
        public string? TeacherName { get; set; } // ← necesaria para el resumen

        public List<int> SelectedGroupIds { get; set; } = new();
    }
}