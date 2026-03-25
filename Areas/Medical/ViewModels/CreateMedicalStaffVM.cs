namespace SchoolManager.ViewModels
{
    public class CreateMedicalStaffVM
    {
        public int PersonId { get; set; }
        public int RoleId { get; set; }
        public string Shift { get; set; } = string.Empty;

        public bool Ver { get; set; }
        public bool Agregar { get; set; }
        public bool Modificar { get; set; }
        public bool Borrar { get; set; }
    }
}