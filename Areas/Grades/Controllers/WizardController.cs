// Areas/Grades/Controllers/SchoolCycleWizardController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Areas.Grades.ViewModels.Wizard;
using SchoolManager.Data;
using SchoolManager.Models;
using System.Text.Json;

namespace SchoolManager.Areas.Grades.Controllers
{
    [Area("Grades")]
    public class SchoolCycleWizardController : Controller
    {
        private readonly AppDbContext _context;

        public SchoolCycleWizardController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Grades/SchoolCycleWizard
        public IActionResult Index()
        {
            // Landing page with options: Start from zero, continue draft, etc.
            return View();
        }

        // GET: /Grades/SchoolCycleWizard/Step1
        public IActionResult Step1()
        {
            var model = new SchoolCycleWizardViewModel();
            return View(model);
        }

        // POST: /Grades/SchoolCycleWizard/Step1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Step1(SchoolCycleWizardViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Save to session
                TempData["WizardData"] = JsonSerializer.Serialize(model);
                return RedirectToAction("Step2");
            }
            return View(model);
        }

        // GET: /Grades/SchoolCycleWizard/Step2
        public IActionResult Step2()
        {
            var model = GetModelFromTempData();
            model.CurrentStep = 2;
            return View(model);
        }

        // POST: /Grades/SchoolCycleWizard/Step2
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Step2(SchoolCycleWizardViewModel model)
        {
            System.Diagnostics.Debug.WriteLine($"=== STEP2 POST ===");
            System.Diagnostics.Debug.WriteLine($"Materias recibidas: {model.Subjects.Count}");

            // Limpiar materias vacías
            model.Subjects = model.Subjects.Where(s => !string.IsNullOrWhiteSpace(s.Name)).ToList();

            System.Diagnostics.Debug.WriteLine($"Materias guardadas: {model.Subjects.Count}");

            TempData["WizardData"] = JsonSerializer.Serialize(model);
            return RedirectToAction("Step3");
        }

        // AJAX: Add a new subject
        [HttpPost]
        public IActionResult AddSubject([FromBody] string subjectName)
        {
            var model = GetModelFromTempData();
            var newSubject = new SubjectWizardViewModel
            {
                TempId = (model.Subjects.Count + 1) * -1, // Negative temporary ID
                Name = subjectName
            };
            model.Subjects.Add(newSubject);
            TempData["WizardData"] = JsonSerializer.Serialize(model);
            return Json(new { success = true, subject = newSubject });
        }

        // AJAX: Add a unit to a subject
        [HttpPost]
        public IActionResult AddUnit(int subjectTempId)
        {
            var model = GetModelFromTempData();
            var subject = model.Subjects.FirstOrDefault(s => s.TempId == subjectTempId);
            if (subject != null)
            {
                var newUnit = new UnitWizardViewModel
                {
                    Number = subject.Units.Count + 1
                };
                subject.Units.Add(newUnit);
                TempData["WizardData"] = JsonSerializer.Serialize(model);
                return Json(new { success = true, unit = newUnit });
            }
            return Json(new { success = false });
        }

        // GET: /Grades/SchoolCycleWizard/Step3
        // GET: /Grades/SchoolCycleWizard/Step3
        public IActionResult Step3()
        {
            var model = GetModelFromTempData();

            // 👇 DEPURACIÓN
            System.Diagnostics.Debug.WriteLine($"=== STEP3 GET ===");
            System.Diagnostics.Debug.WriteLine($"Materias recuperadas: {model.Subjects.Count}");
            System.Diagnostics.Debug.WriteLine($"Grupos: {model.Groups?.Count ?? 0}");

            model.CurrentStep = 3;
            return View(model);
        }

        // POST: /Grades/SchoolCycleWizard/Step3
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Step3(SchoolCycleWizardViewModel model)
        {
            System.Diagnostics.Debug.WriteLine($"=== STEP3 POST ===");
            System.Diagnostics.Debug.WriteLine($"model.Subjects (del form): {model.Subjects.Count}");

            // NO llames a GetModelFromTempData aquí, porque TempData ya expiró

            // Limpiar grupos vacíos
            model.Groups = model.Groups.Where(g => !string.IsNullOrWhiteSpace(g.Name)).ToList();

            System.Diagnostics.Debug.WriteLine($"Grupos guardados: {model.Groups.Count}");
            System.Diagnostics.Debug.WriteLine($"Materias conservadas: {model.Subjects.Count}");

            TempData["WizardData"] = JsonSerializer.Serialize(model);
            return RedirectToAction("Step4");
        }

        // GET: /Grades/SchoolCycleWizard/Step4
        public async Task<IActionResult> Step4()
        {
            var model = GetModelFromTempData();
            System.Diagnostics.Debug.WriteLine($"=== STEP4 GET ===");
            System.Diagnostics.Debug.WriteLine($"Materias: {model.Subjects.Count}");
            System.Diagnostics.Debug.WriteLine($"Grupos: {model.Groups?.Count ?? 0}");
            model.CurrentStep = 4;
            var gruposEnModelo = model.Groups?.Count ?? 0;

            // 👇 IMPORTANTE: Asegurar que los grupos existen
            if (model.Groups == null || !model.Groups.Any())
            {
                TempData["Error"] = "Debes crear al menos un grupo antes de asignar profesores.";
                return RedirectToAction("Step3");
            }

            // Load teachers for dropdowns
            ViewBag.Teachers = await _context.Users
                .Include(u => u.Person)
                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Teacher")) // Adjust role name
                .Select(u => new SelectListItem
                {
                    Value = u.UserId.ToString(),
                    Text = u.Person.FirstName + " " + u.Person.LastNamePaternal
                })
                .ToListAsync();

            // Map subjects to assignments
            model.Assignments = model.Subjects.Select(s => new AssignmentWizardViewModel
            {
                SubjectTempId = s.TempId,
                SubjectName = s.Name,
                SelectedGroupIds = new List<int>()
            }).ToList();

            return View(model);
        }

        // POST: /Grades/SchoolCycleWizard/Step4
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Step4(SchoolCycleWizardViewModel model)
        {
            // Validar que los valores requeridos existan
            if (!model.StartDate.HasValue)
            {
                TempData["Error"] = "La fecha de inicio es requerida";
                return RedirectToAction("Step1");
            }

            if (!model.EndDate.HasValue)
            {
                TempData["Error"] = "La fecha de fin es requerida";
                return RedirectToAction("Step1");
            }

            if (!model.MinPassingGrade.HasValue)
            {
                TempData["Error"] = "La calificación mínima es requerida";
                return RedirectToAction("Step1");
            }

            if (string.IsNullOrEmpty(model.LevelName))
            {
                TempData["Error"] = "El nombre del nivel es requerido";
                return RedirectToAction("Step1");
            }

            // --- SAVE EVERYTHING TO DATABASE ---
            // 1. Create GradeLevel
            TempData["WizardData"] = JsonSerializer.Serialize(model);

            var newLevel = new grades_grade_level
            {
                Name = model.LevelName!,
                StartDate = DateOnly.FromDateTime(model.StartDate.Value), // Ya validado
                EndDate = DateOnly.FromDateTime(model.EndDate.Value),     // Ya validado
                IsOpen = true,
                MinPassingGrade = model.MinPassingGrade.Value             // Ya validado
            };
            _context.grades_GradeLevels.Add(newLevel);
            await _context.SaveChangesAsync();

            // 2. Create Subjects and Units
            var subjectIdMap = new Dictionary<int, int>(); // Maps TempId to real ID
            foreach (var subjectVm in model.Subjects)
            {
                var newSubject = new grades_subjects
                {
                    Name = subjectVm.Name!,
                    GradeLevelId = newLevel.GradeLevelId
                };
                _context.grades_Subjects.Add(newSubject);
                await _context.SaveChangesAsync();
                subjectIdMap[subjectVm.TempId] = newSubject.SubjectId;

                // Create Units
                foreach (var unit in subjectVm.Units)
                {
                    var newUnit = new grades_subject_unit
                    {
                        SubjectId = newSubject.SubjectId,
                        UnitNumber = unit.Number,
                        IsOpen = unit.IsOpen
                    };
                    _context.grades_SubjectUnits.Add(newUnit);
                }
            }
            await _context.SaveChangesAsync();

            // 3. Create Groups
            var groupIdMap = new Dictionary<int, int>(); // Maps TempId to real ID
            foreach (var groupVm in model.Groups)
            {
                var newGroup = new grades_group
                {
                    Name = groupVm.Name!,
                    GradeLevelId = newLevel.GradeLevelId
                };
                _context.grades_GradeGroups.Add(newGroup);
                await _context.SaveChangesAsync();
                groupIdMap[groupVm.TempId] = newGroup.GroupId;
            }

            // 4. Create Assignments (TeacherSubject and TeacherSubjectGroup)
            foreach (var assignmentVm in model.Assignments)
            {
                if (assignmentVm.TeacherId.HasValue && assignmentVm.TeacherId > 0)
                {
                    // Find or create TeacherSubject
                    var teacherSubject = await _context.grades_TeacherSubjects
                        .FirstOrDefaultAsync(ts => ts.TeacherId == assignmentVm.TeacherId && ts.SubjectId == subjectIdMap[assignmentVm.SubjectTempId]);

                    if (teacherSubject == null)
                    {
                        teacherSubject = new grades_teacher_subject
                        {
                            TeacherId = assignmentVm.TeacherId.Value,
                            SubjectId = subjectIdMap[assignmentVm.SubjectTempId]
                        };
                        _context.grades_TeacherSubjects.Add(teacherSubject);
                        await _context.SaveChangesAsync();
                    }

                    // Assign selected groups
                    foreach (var groupTempId in assignmentVm.SelectedGroupIds)
                    {
                        var teacherSubjectGroup = new grades_teacher_subject_group
                        {
                            TeacherSubjectId = teacherSubject.TeacherSubjectId,
                            GroupId = groupIdMap[groupTempId]
                        };
                        _context.grades_TeacherSubjectGroups.Add(teacherSubjectGroup);
                    }
                }
            }
            await _context.SaveChangesAsync();

            // Clear session and redirect to summary
            TempData.Remove("WizardData");
            TempData["Success"] = "School cycle configured successfully.";
            return RedirectToAction("Summary", new { levelId = newLevel.GradeLevelId });
        }

        // GET: /Grades/SchoolCycleWizard/Summary/5
        public async Task<IActionResult> Summary(int levelId)
        {
            var level = await _context.grades_GradeLevels
                .Include(l => l.Subjects)
                    .ThenInclude(s => s.Units)
                .Include(l => l.Groups)
                .FirstOrDefaultAsync(l => l.GradeLevelId == levelId);

            if (level == null) return NotFound();

            return View(level);
        }

        // Helper to get model from session
        private SchoolCycleWizardViewModel GetModelFromTempData()
        {
            if (TempData["WizardData"] == null)
            {
                System.Diagnostics.Debug.WriteLine("*** TempData WIZARDDATA es NULL, creando nuevo modelo");
                return new SchoolCycleWizardViewModel();
            }

            var data = TempData["WizardData"]?.ToString();

            if (string.IsNullOrEmpty(data))
            {
                System.Diagnostics.Debug.WriteLine("*** TempData WIZARDDATA está vacío");
                return new SchoolCycleWizardViewModel();
            }

            try
            {
                var model = JsonSerializer.Deserialize<SchoolCycleWizardViewModel>(data);
                System.Diagnostics.Debug.WriteLine($"*** Modelo recuperado: Subjects={model?.Subjects?.Count ?? 0}, Groups={model?.Groups?.Count ?? 0}");
                return model ?? new SchoolCycleWizardViewModel();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"*** Error deserializando: {ex.Message}");
                return new SchoolCycleWizardViewModel();
            }
        }
    }
}