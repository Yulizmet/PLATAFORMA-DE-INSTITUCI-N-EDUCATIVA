namespace SchoolManager.ViewModels
{
    public class EditMedicalStaffVM
    {
        public int Id { get; set; }          
        public string? NombreCompleto { get; set; }
        public string? Curp { get; set; }
        public int RoleId { get; set; }
        public string? Shift { get; set; }

        // Permisos
        public bool Ver { get; set; }
        public bool Agregar { get; set; }
        public bool Modificar { get; set; }
        public bool Borrar { get; set; }
    }
}