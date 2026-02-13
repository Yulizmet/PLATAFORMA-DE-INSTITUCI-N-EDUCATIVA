using Microsoft.AspNetCore.Mvc;
using SchoolManager.Areas.Grades.ViewModels;
using SchoolManager.Data;
using SchoolManager.Models;

namespace SchoolManager.Areas.Grades.Controllers
{
    [Area("Grades")]
    public class SetupController : Controller
    {
        private readonly AppDbContext _context;
        // GET: Setup
 

        public SetupController(AppDbContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {
            // Verificar si ya hay configuración
            var hayCiclo = _context.Set<grades_school_cycle>().Any();
            var hayNiveles = _context.Set<grades_grade_level>().Any();
            var hayMaterias = _context.Set<grades_subjects>().Any();

            if (hayCiclo && hayNiveles && hayMaterias)
            {
                TempData["Info"] = "El sistema ya está configurado";
                return RedirectToAction("Index", "Home");
            }

            return View(new SetupWizardViewModel { CurrentStep = 1 });
        }
        [HttpGet]
        public IActionResult Test()
        {
            return Content("FUNCIONA");
        }


        // POST: Setup/Step1
        [HttpPost]
        public IActionResult Step1(SetupWizardViewModel model)
        {
            model.CurrentStep = 1;

            if (string.IsNullOrEmpty(model.SchoolCycleName))
            {
                ModelState.AddModelError("SchoolCycleName", "El nombre del ciclo es requerido");
                return View("Index", model);
            }

            TempData["SchoolCycleName"] = model.SchoolCycleName;
            TempData["StartDate"] = model.StartDate.ToString("yyyy-MM-dd");
            TempData["EndDate"] = model.EndDate.ToString("yyyy-MM-dd");

            model.CurrentStep = 2;
            return View("Index", model);
        }
        public IActionResult TestPost()
        {
            return Content("POST funciona");
        }

        // POST: Setup/Step2
        [HttpPost]
        public IActionResult Step2(SetupWizardViewModel model)
        {
            model.CurrentStep = 2;

            if (TempData["SchoolCycleName"] != null)
                model.SchoolCycleName = TempData["SchoolCycleName"].ToString();

            if (!string.IsNullOrEmpty(model.NewGradeLevel))
            {
                model.GradeLevelNames.Add(model.NewGradeLevel);
                model.NewGradeLevel = "";
            }

            if (Request.Form["action"] == "next" && model.GradeLevelNames.Count == 0)
            {
                ModelState.AddModelError("", "Debes agregar al menos un nivel");
                return View("Index", model);
            }

            if (Request.Form["action"] == "next")
            {
                TempData["GradeLevelNames"] = string.Join(",", model.GradeLevelNames);
                model.CurrentStep = 3;
            }

            if (Request.Form["action"] == "prev")
            {
                model.CurrentStep = 1;
            }

            return View("Index", model);
        }

        // POST: Setup/Step3
        [HttpPost]
        public async Task<IActionResult> Step3(SetupWizardViewModel model)
        {
            model.CurrentStep = 3;

            if (TempData["SchoolCycleName"] != null)
                model.SchoolCycleName = TempData["SchoolCycleName"].ToString();

            if (TempData["GradeLevelNames"] != null)
                model.GradeLevelNames = TempData["GradeLevelNames"].ToString().Split(',').ToList();

            if (!string.IsNullOrEmpty(model.NewSubject))
            {
                model.SubjectNames.Add(model.NewSubject);
                model.NewSubject = "";
                return View("Index", model);
            }

            if (Request.Form["action"] == "finish")
            {
                if (model.SubjectNames.Count == 0)
                {
                    ModelState.AddModelError("", "Debes agregar al menos una materia");
                    return View("Index", model);
                }

                // 1. Guardar ciclo escolar
                var ciclo = new grades_school_cycle
                {
                    Name = model.SchoolCycleName,
                    StartDate = DateOnly.Parse(TempData["StartDate"].ToString()),
                    EndDate = DateOnly.Parse(TempData["EndDate"].ToString()),
                    IsOpen = true
                };
                _context.Add(ciclo);
                await _context.SaveChangesAsync();

                // 2. Guardar niveles
                foreach (var nivel in model.GradeLevelNames)
                {
                    _context.Add(new grades_grade_level { Name = nivel });
                }
                await _context.SaveChangesAsync();

                // 3. Guardar materias
                foreach (var materia in model.SubjectNames)
                {
                    _context.Add(new grades_subjects { Name = materia });
                }
                await _context.SaveChangesAsync();

                TempData["Success"] = "Configuración completada exitosamente";
                return RedirectToAction("Index", "Home");
            }

            if (Request.Form["action"] == "prev")
            {
                model.CurrentStep = 2;
            }

            return View("Index", model);
        }

        // GET: Setup/Reset (Opcional - para reiniciar configuración)
        public async Task<IActionResult> Reset()
        {
            // Eliminar datos de configuración (solo para desarrollo)
            _context.Set<grades_school_cycle>().RemoveRange(_context.Set<grades_school_cycle>());
            _context.Set<grades_grade_level>().RemoveRange(_context.Set<grades_grade_level>());
            _context.Set<grades_subjects>().RemoveRange(_context.Set<grades_subjects>());
            await _context.SaveChangesAsync();

            TempData["Warning"] = "Configuración reiniciada";
            return RedirectToAction(nameof(Index));
        }
    }
}
