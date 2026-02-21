using SchoolManager.Models;
using System.Collections.Generic;

namespace SchoolManager.Areas.Procedures.ViewModels
{
    public class BulkUpdateViewModel
    {
        public List<int> Ids { get; set; } = new List<int>();
        public string? Name { get; set; }
        public int IdArea { get; set; }
        public List<procedure_type_requirements> RequirementsList { get; set; } = new List<procedure_type_requirements>();
        public List<procedure_flow> FlowList { get; set; } = new List<procedure_flow>();
    }
}