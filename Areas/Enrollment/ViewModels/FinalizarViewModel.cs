namespace SchoolManager.Areas.Enrollment.ViewModels
{
    public class FinalizarViewModel
    {
        public string Folio { get; set; } = string.Empty;
        public DateTime FechaEnvio { get; set; }
        public string NombreAspirante { get; set; } = string.Empty;
        public string Especialidad { get; set; } = string.Empty;

        public string Genero { get; set; } = string.Empty;
        public DateTime? FechaNacimiento { get; set; }
        public string Curp { get; set; } = string.Empty;
        public string TipoSangre { get; set; } = string.Empty;

        public string SecundariaProcedencia { get; set; } = string.Empty;
        public decimal? Promedio { get; set; }
        public DateTime? FechaFinSecundaria { get; set; }

        public string Calle { get; set; } = string.Empty;
        public string NumeroExterior { get; set; } = string.Empty;
        public string NumeroInterior { get; set; } = string.Empty;
        public string Colonia { get; set; } = string.Empty;
        public string CodigoPostal { get; set; } = string.Empty;
        public string Municipio { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;

        public string Generacion { get; set; } = string.Empty;
    }
}