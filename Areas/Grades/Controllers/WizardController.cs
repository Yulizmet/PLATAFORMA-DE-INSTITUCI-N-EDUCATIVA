// Areas/Grades/Controllers/SchoolCycleWizardController.cs
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "Administrator")]

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
            return View();
        }

        // GET: /Grades/SchoolCycleWizard/Step1
        public IActionResult Step1()
        {
            // Si hay datos en TempData (venimos de regresar), los usamos
            var model = GetModelFromTempData();
            return View(model);
        }

        // POST: /Grades/SchoolCycleWizard/Step1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Step1(SchoolCycleWizardViewModel model)
        {
            if (ModelState.IsValid)
            {
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

        // POST: /Grades/SchoolCycleWizard/Step2 (Continuar)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Step2(SchoolCycleWizardViewModel model)
        {
            model.Subjects = model.Subjects.Where(s => !string.IsNullOrWhiteSpace(s.Name)).ToList();
            TempData["WizardData"] = JsonSerializer.Serialize(model);
            return RedirectToAction("Step3");
        }

        // POST: /Grades/SchoolCycleWizard/Step2Back (Regresar a Step1)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Step2Back(SchoolCycleWizardViewModel model)
        {
            model.Subjects = model.Subjects.Where(s => !string.IsNullOrWhiteSpace(s.Name)).ToList();
            TempData["WizardData"] = JsonSerializer.Serialize(model);
            return RedirectToAction("Step1");
        }

        // GET: /Grades/SchoolCycleWizard/Step3
        public IActionResult Step3()
        {
            var model = GetModelFromTempData();
            model.CurrentStep = 3;
            return View(model);
        }

        // POST: /Grades/SchoolCycleWizard/Step3 (Continuar)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Step3(SchoolCycleWizardViewModel model)
        {
            model.Groups = model.Groups.Where(g => !string.IsNullOrWhiteSpace(g.Name)).ToList();
            TempData["WizardData"] = JsonSerializer.Serialize(model);
            return RedirectToAction("Step4");
        }

        // POST: /Grades/SchoolCycleWizard/Step3Back (Regresar a Step2)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Step3Back(SchoolCycleWizardViewModel model)
        {
            model.Groups = model.Groups.Where(g => !string.IsNullOrWhiteSpace(g.Name)).ToList();
            TempData["WizardData"] = JsonSerializer.Serialize(model);
            return RedirectToAction("Step2");
        }

        // GET: /Grades/SchoolCycleWizard/Step4
        public async Task<IActionResult> Step4()
        {
            var model = GetModelFromTempData();
            model.CurrentStep = 4;

            if (model.Groups == null || !model.Groups.Any())
            {
                TempData["Error"] = "Debes crear al menos un grupo antes de asignar profesores.";
                return RedirectToAction("Step3");
            }

            ViewBag.Teachers = await GetTeachersSelectList();

            model.Assignments = model.Subjects.Select(s => new AssignmentWizardViewModel
            {
                SubjectTempId = s.TempId,
                SubjectName = s.Name,
                TeacherAssignments = new List<TeacherGroupAssignment>
                {
                    new TeacherGroupAssignment()
                }
            }).ToList();

            return View(model);
        }

        // POST: /Grades/SchoolCycleWizard/Step4Back (Regresar a Step3)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Step4Back(SchoolCycleWizardViewModel model)
        {
            TempData["WizardData"] = JsonSerializer.Serialize(model);
            return RedirectToAction("Step3");
        }

        // POST: /Grades/SchoolCycleWizard/Step4 (Finalizar y guardar)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Step4(SchoolCycleWizardViewModel model)
        {
            if (!model.StartDate.HasValue)
            { TempData["Error"] = "La fecha de inicio es requerida"; return RedirectToAction("Step1"); }

            if (!model.EndDate.HasValue)
            { TempData["Error"] = "La fecha de fin es requerida"; return RedirectToAction("Step1"); }

            if (!model.MinPassingGrade.HasValue)
            { TempData["Error"] = "La calificación mínima es requerida"; return RedirectToAction("Step1"); }

            if (string.IsNullOrEmpty(model.LevelName))
            { TempData["Error"] = "El nombre del nivel es requerido"; return RedirectToAction("Step1"); }

            // 1. Crear GradeLevel
            var newLevel = new grades_grade_level
            {
                Name = model.LevelName!,
                StartDate = DateOnly.FromDateTime(model.StartDate.Value),
                EndDate = DateOnly.FromDateTime(model.EndDate.Value),
                IsOpen = true,
                MinPassingGrade = model.MinPassingGrade.Value
            };
            _context.grades_GradeLevels.Add(newLevel);
            await _context.SaveChangesAsync();

            // 2. Crear Materias y Unidades
            var subjectIdMap = new Dictionary<int, int>();
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

                foreach (var unit in subjectVm.Units)
                {
                    _context.grades_SubjectUnits.Add(new grades_subject_unit
                    {
                        SubjectId = newSubject.SubjectId,
                        UnitNumber = unit.Number,
                        IsOpen = unit.IsOpen
                    });
                }
            }
            await _context.SaveChangesAsync();

            // 3. Crear Grupos
            var groupIdMap = new Dictionary<int, int>();
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

            // 4. Crear asignaciones profe → materia → grupos
            foreach (var assignmentVm in model.Assignments)
            {
                if (!subjectIdMap.ContainsKey(assignmentVm.SubjectTempId)) continue;
                var realSubjectId = subjectIdMap[assignmentVm.SubjectTempId];

                foreach (var ta in assignmentVm.TeacherAssignments)
                {
                    if (!ta.TeacherId.HasValue || ta.TeacherId <= 0 || !ta.SelectedGroupIds.Any())
                        continue;

                    var teacherSubject = await _context.grades_TeacherSubjects
                        .FirstOrDefaultAsync(ts => ts.TeacherId == ta.TeacherId && ts.SubjectId == realSubjectId);

                    if (teacherSubject == null)
                    {
                        teacherSubject = new grades_teacher_subject
                        {
                            TeacherId = ta.TeacherId.Value,
                            SubjectId = realSubjectId
                        };
                        _context.grades_TeacherSubjects.Add(teacherSubject);
                        await _context.SaveChangesAsync();
                    }

                    foreach (var groupTempId in ta.SelectedGroupIds)
                    {
                        if (!groupIdMap.ContainsKey(groupTempId)) continue;

                        _context.grades_TeacherSubjectGroups.Add(new grades_teacher_subject_group
                        {
                            TeacherSubjectId = teacherSubject.TeacherSubjectId,
                            GroupId = groupIdMap[groupTempId]
                        });
                    }
                }
            }
            await _context.SaveChangesAsync();

            TempData.Remove("WizardData");
            TempData["Success"] = "Ciclo escolar configurado exitosamente.";
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

        // ── Helpers ────────────────────────────────────────────────────────────

        private async Task<List<SelectListItem>> GetTeachersSelectList()
        {
            return await _context.Users
                .Include(u => u.Person)
                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Teacher"))
                .Select(u => new SelectListItem
                {
                    Value = u.UserId.ToString(),
                    Text = u.Person.FirstName + " " + u.Person.LastNamePaternal
                })
                .ToListAsync();
        }

        private SchoolCycleWizardViewModel GetModelFromTempData()
        {
            var data = TempData["WizardData"]?.ToString();
            if (string.IsNullOrEmpty(data))
                return new SchoolCycleWizardViewModel();

            try
            {
                return JsonSerializer.Deserialize<SchoolCycleWizardViewModel>(data)
                       ?? new SchoolCycleWizardViewModel();
            }
            catch
            {
                return new SchoolCycleWizardViewModel();
            }
        }
    }
}
