#nullable disable


namespace SchoolManager.Models
{
    public class StudentDetailVM
    {
        public int Id { get; set; }
        public string Matricula { get; set; }

        public string Nombre { get; set; }
        public string Paterno { get; set; }
        public string Materno { get; set; }

        public string Sangre { get; set; }

        public decimal? Peso { get; set; }
        public string Alergias { get; set; }
        public string CondicionesCronicas { get; set; }

        public DateTime FechaCreacion { get; set; }
    }
}