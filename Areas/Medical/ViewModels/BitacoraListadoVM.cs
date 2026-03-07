#nullable disable


namespace PIBitacoras.Models
{
    public class BitacoraListadoVM
    {
        public int Id { get; set; }
        public string Folio { get; set; }
        public string Matricula { get; set; }
        public string NombreCompleto { get; set; }
        public string Motivo { get; set; }
        public string Estado { get; set; }
        public DateTime Fecha { get; set; }
    }
}