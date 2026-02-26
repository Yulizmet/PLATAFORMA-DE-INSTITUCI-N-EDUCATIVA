namespace SchoolManager.ViewModels
{
    public class EmployeeStatisticsVM
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Genero { get; set; } = string.Empty;
        public string Departamento { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public int ActividadesHoy { get; set; }
        public DateTime FechaContratacion { get; set; }
    }
}
