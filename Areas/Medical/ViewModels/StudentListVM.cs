#nullable disable


namespace SchoolManager.Models
{
    public class StudentListVM
    {
        public int Id { get; set; }
        public string Matricula { get; set; }
        public string NombreCompleto { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}