namespace SchoolManager.Models
{
    public class EntrevistaViewModel
    {
        // 👇 ¡ESTA ES LA LÍNEA QUE TE FALTA Y CAUSA EL ERROR! 👇
        public int UserId { get; set; }

        // I. Aspectos Personales
        public string EstadoCivil { get; set; }
        public string TieneHijos { get; set; }
        public string Trabaja { get; set; }
        public string Transporte { get; set; }
        public string Hermanos { get; set; }
        public string ViveCon { get; set; }
        public string SituacionFamiliar { get; set; }

        // II. Aspectos Académicos
        public string PromedioPrepa { get; set; }
        public string ReproboAnterior { get; set; }
        public string EquipoComputo { get; set; }
        public string Internet { get; set; }
        public string MotivoCarrera { get; set; }
    }
}