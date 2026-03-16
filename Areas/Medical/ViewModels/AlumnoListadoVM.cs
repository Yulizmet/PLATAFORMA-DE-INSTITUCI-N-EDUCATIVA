#nullable disable


namespace PIBitacoras.Models
{
    public class AlumnoListadoVM
    {
        public int Id { get; set; }
        public string Matricula { get; set; }
        public string NombreCompleto { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}