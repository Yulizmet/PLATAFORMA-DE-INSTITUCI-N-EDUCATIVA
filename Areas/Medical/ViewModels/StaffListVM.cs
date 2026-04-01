namespace SchoolManager.ViewModels
{
    public class StaffListVM
    {
        public int Id { get; set; }
        public string? NombreCompleto { get; set; }
        public string? Rol { get; set; }
        public int RoleId { get; set; }
        public string? Curp { get; set; }
        public string? Correo { get; set; }
        public string? Turno { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}