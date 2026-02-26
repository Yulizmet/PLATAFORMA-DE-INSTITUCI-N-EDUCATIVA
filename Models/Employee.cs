namespace SchoolManager.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Departamento { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string Genero { get; set; } = string.Empty;
        public DateTime FechaContratacion { get; set; }
        public int ActividadesHoy { get; set; }
    }
}
