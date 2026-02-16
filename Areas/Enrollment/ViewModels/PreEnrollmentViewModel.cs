using SchoolManager.Models;

namespace SchoolManager.Areas.Enrollment.ViewModels
{
    public class PreEnrollmentViewModel
    {
        public preenrollment_general DatosGenerales { get; set; } = new();
        public preenrollment_schools DatosEscolares { get; set; } = new();
        public preenrollment_addresses Domicilio { get; set; } = new();
        public preenrollment_tutors Tutor { get; set; } = new();
        public preenrollment_infos Otros { get; set; } = new();
    }
}
