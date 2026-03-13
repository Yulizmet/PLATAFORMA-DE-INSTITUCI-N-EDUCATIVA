namespace SchoolManager.Areas.MainScreen.ViewModels
{
    public class PanelAdministrativoViewModel
    {
        public List<AsignacionResumenViewModel> Asignaciones { get; set; } = new();
        public List<GrupoResumenViewModel> Grupos { get; set; } = new();
    }

    public class AsignacionResumenViewModel
    {
        public int TeacherSubjectId { get; set; }
        public string TeacherName { get; set; } = null!;
        public string SubjectName { get; set; } = null!;
        public string GradeLevelName { get; set; } = null!;
    }

    public class GrupoResumenViewModel
    {
        public int GroupId { get; set; }
        public string Name { get; set; } = null!;
        public string GradeLevelName { get; set; } = null!;
    }
}
