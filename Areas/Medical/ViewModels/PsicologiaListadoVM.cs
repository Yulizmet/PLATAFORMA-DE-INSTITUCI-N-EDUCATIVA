#nullable disable

namespace PIBitacoras.ViewModels
{
    public class PsicologiaListadoVM
    {
        public int Id { get; set; }
        public string Folio { get; set; }
        public string Matricula { get; set; }
        public string NombreCompleto { get; set; }
        public string Asistencia { get; set; }
        public DateTime Fecha { get; set; }
    }
}