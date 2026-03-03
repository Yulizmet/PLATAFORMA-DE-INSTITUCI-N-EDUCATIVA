namespace SchoolManager.ViewModels
{
    public class StudentStatisticsVM
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Genero { get; set; } = string.Empty;
        public string Curso { get; set; } = string.Empty;
        public int Semestre { get; set; }
        public double Nota { get; set; }
        public DateTime FechaInscripcion { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
