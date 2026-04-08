using System.ComponentModel.DataAnnotations;
namespace SchoolManager.Areas.Grades.ViewModels.Subjects
{
    public class SubjectViewModel
    {
        public int SubjectId { get; set; }
        public string Name { get; set; } = null!;
        public int GradeLevelId { get; set; }
        public string GradeLevelName { get; set; } = null!;
        public int UnitsCount { get; set; }
        public int OpenUnitsCount { get; set; }

        [Display(Name = "Calificación mínima (opcional)")]
        public decimal? MinPassingGrade { get; set; }
    }

    public class UnitViewModel
    {
        public int UnitId { get; set; }
        public int UnitNumber { get; set; }
        public bool IsOpen { get; set; }
        public bool HasGrades { get; set; }
    }

    // ── Profesores asignados a esta materia ──────────────────────────────────

    /// <summary>Un grupo dentro de la asignación profe→materia</summary>
    public class SubjectTeacherGroupViewModel
    {
        public int TeacherSubjectGroupId { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; } = null!;
    }

    /// <summary>Una fila en la tabla de profesores de la materia</summary>
    public class SubjectTeacherViewModel
    {
        public int TeacherSubjectId { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = null!;
        /// <summary>Grupos que este profe imparte en esta materia</summary>
        public List<SubjectTeacherGroupViewModel> Groups { get; set; } = new();
    }

    /// <summary>Un grupo del nivel (para el selector al asignar)</summary>
    public class SubjectGroupOptionViewModel
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = null!;
        /// <summary>true si ya hay otro profe asignado a este grupo en esta materia</summary>
        public bool IsTaken { get; set; }
        /// <summary>UserId del profe que ya lo tiene (null si libre)</summary>
        public int? TakenByTeacherId { get; set; }
        public string? TakenByTeacherName { get; set; }
    }

    // ── ViewModel principal ──────────────────────────────────────────────────

    public class SubjectDetailsViewModel
    {
        public int SubjectId { get; set; }
        public string Name { get; set; } = null!;
        public int GradeLevelId { get; set; }
        public string GradeLevelName { get; set; } = null!;

        public decimal? MinPassingGrade { get; set; }


        public List<UnitViewModel> Units { get; set; } = new();

        /// <summary>Profesores actualmente asignados a esta materia</summary>
        public List<SubjectTeacherViewModel> Teachers { get; set; } = new();

        /// <summary>Todos los grupos del nivel, con info de si están ocupados</summary>
        public List<SubjectGroupOptionViewModel> GroupOptions { get; set; } = new();
    }
}
