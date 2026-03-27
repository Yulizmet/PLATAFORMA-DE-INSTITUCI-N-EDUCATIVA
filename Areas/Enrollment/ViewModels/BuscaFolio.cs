using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models.ViewModels
{
    public class BuscarFolioViewModel
    {
        [Required(ErrorMessage = "Ingresa el folio")]
        public string Folio { get; set; } = string.Empty;
    }
}