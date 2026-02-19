namespace SchoolManager.Areas.Grades.ViewModels
{
    public class SetupWizardViewModel
    {
        // PASO 1: Datos del Ciclo Escolar
        public string SchoolCycleName { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        // PASO 2: Niveles/Grados
        public List<string> GradeLevelNames { get; set; } = new();
        public string NewGradeLevel { get; set; }

        // PASO 3: Materias
        public List<string> SubjectNames { get; set; } = new();
        public string NewSubject { get; set; }

        // Control del wizard
        public int CurrentStep { get; set; } = 1;
    }
}
