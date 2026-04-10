using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Areas.Grades.ViewModels;
using SchoolManager.Areas.Grades.ViewModels.Subjects;
using SchoolManager.Data;
using SchoolManager.Models;

namespace SchoolManager.Areas.Grades.Controllers
{
    [Area("Grades")]
    [Authorize(Roles = "Teacher,Administrator")]

    public class SubjectsController : Controller
    {
        private readonly AppDbContext _context;

        public SubjectsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Subjects
        // GET: Subjects
        public async Task<IActionResult> Index()
        {
            var subjects = await _context.grades_Subjects
                .Include(s => s.GradeLevel)
                .Include(s => s.Units)
                .Select(s => new SubjectViewModel
                {
                    SubjectId = s.SubjectId,
                    Name = s.Name,
                    GradeLevelId = s.GradeLevelId,
                    GradeLevelName = s.GradeLevel.Name,
                    UnitsCount = s.Units.Count,
                    OpenUnitsCount = s.Units.Count(u => u.IsOpen)
                })
                .OrderBy(s => s.GradeLevelName)
                .ThenBy(s => s.Name)
                .ToListAsync();

            ViewBag.GradeLevels = await _context.grades_GradeLevels
                .OrderBy(gl => gl.Name)
                .Select(gl => new { gl.GradeLevelId, gl.Name, gl.IsOpen })
                .ToListAsync();

            return View(subjects);
        }
        // GET: Subjects/Create
        public IActionResult Create(int? gradeLevelId)
        {
            var viewModel = new SubjectViewModel();

            if (gradeLevelId.HasValue)
            {
                viewModel.GradeLevelId = gradeLevelId.Value;
            }

            ViewBag.GradeLevels = _context.grades_GradeLevels
                .Where(gl => gl.IsOpen)
                .OrderBy(gl => gl.Name)
                .Select(gl => new { gl.GradeLevelId, gl.Name })
                .ToList();

            return View(viewModel);
        }

        // POST: Subjects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubjectViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var subject = new grades_subjects
                {
                    Name = viewModel.Name,
                    GradeLevelId = viewModel.GradeLevelId
                };

                _context.Add(subject);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Materia creada exitosamente";
                return RedirectToAction(nameof(Index), new { gradeLevelId = viewModel.GradeLevelId });
            }

            ViewBag.GradeLevels = _context.grades_GradeLevels
                .Where(gl => gl.IsOpen)
                .OrderBy(gl => gl.Name)
                .Select(gl => new { gl.GradeLevelId, gl.Name })
                .ToList();

            return View(viewModel);
        }

        // GET: Subjects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var subject = await _context.grades_Subjects
                .Where(s => s.SubjectId == id)
                .Select(s => new SubjectViewModel
                {
                    SubjectId = s.SubjectId,
                    Name = s.Name,
                    GradeLevelId = s.GradeLevelId
                })
                .FirstOrDefaultAsync();

            if (subject == null) return NotFound();

            ViewBag.GradeLevels = _context.grades_GradeLevels
                .Where(gl => gl.IsOpen)
                .OrderBy(gl => gl.Name)
                .Select(gl => new { gl.GradeLevelId, gl.Name })
                .ToList();

            return View(subject);
        }

        // POST: Subjects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubjectViewModel viewModel)
        {
            if (id != viewModel.SubjectId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var subject = await _context.grades_Subjects.FindAsync(id);
                    if (subject == null) return NotFound();

                    subject.Name = viewModel.Name;
                    subject.GradeLevelId = viewModel.GradeLevelId;

                    _context.Update(subject);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Materia actualizada exitosamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubjectExists(viewModel.SubjectId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index), new { gradeLevelId = viewModel.GradeLevelId });
            }

            ViewBag.GradeLevels = _context.grades_GradeLevels
                .Where(gl => gl.IsOpen)
                .OrderBy(gl => gl.Name)
                .Select(gl => new { gl.GradeLevelId, gl.Name })
                .ToList();

            return View(viewModel);
        }

        // GET: Subjects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var subject = await _context.grades_Subjects
                .Include(s => s.GradeLevel)
                .Include(s => s.Units)
                .FirstOrDefaultAsync(s => s.SubjectId == id);

            if (subject == null) return NotFound();

            // Profesores ya asignados a esta materia con sus grupos
            var teacherSubjects = await _context.grades_TeacherSubjects
                .Include(ts => ts.Teacher).ThenInclude(t => t.Person)
                .Include(ts => ts.TeacherSubjectGroups).ThenInclude(tsg => tsg.Group)
                .Where(ts => ts.SubjectId == id)
                .ToListAsync();

            // Todos los grupos del mismo nivel
            var allGroups = await _context.grades_GradeGroups
                .Where(g => g.GradeLevelId == subject.GradeLevelId)
                .OrderBy(g => g.Name)
                .ToListAsync();

            // Mapa groupId → profe que lo tiene en esta materia
            var takenMap = teacherSubjects
                .SelectMany(ts => ts.TeacherSubjectGroups.Select(tsg => new
                {
                    tsg.GroupId,
                    ts.TeacherId,
                    TeacherName = ts.Teacher.Person.FirstName + " " + ts.Teacher.Person.LastNamePaternal
                }))
                .ToDictionary(x => x.GroupId, x => x);

            var viewModel = new SubjectDetailsViewModel
            {
                SubjectId = subject.SubjectId,
                Name = subject.Name,
                GradeLevelId = subject.GradeLevelId,
                GradeLevelName = subject.GradeLevel.Name,
                Units = subject.Units
                    .OrderBy(u => u.UnitNumber)
                    .Select(u => new UnitViewModel
                    {
                        UnitId = u.UnitId,
                        UnitNumber = u.UnitNumber,
                        IsOpen = u.IsOpen,
                        HasGrades = _context.grades_Grades.Any(g => g.SubjectUnitId == u.UnitId)
                    }).ToList(),
                Teachers = teacherSubjects.Select(ts => new SubjectTeacherViewModel
                {
                    TeacherSubjectId = ts.TeacherSubjectId,
                    TeacherId = ts.TeacherId,
                    TeacherName = ts.Teacher.Person.FirstName + " " +
                                  ts.Teacher.Person.LastNamePaternal + " " +
                                  ts.Teacher.Person.LastNameMaternal,
                    Groups = ts.TeacherSubjectGroups.Select(tsg => new SubjectTeacherGroupViewModel
                    {
                        TeacherSubjectGroupId = tsg.TeacherSubjectGroupId,
                        GroupId = tsg.GroupId,
                        GroupName = tsg.Group.Name
                    }).OrderBy(g => g.GroupName).ToList()
                }).OrderBy(t => t.TeacherName).ToList(),
                GroupOptions = allGroups.Select(g => new SubjectGroupOptionViewModel
                {
                    GroupId = g.GroupId,
                    GroupName = g.Name,
                    IsTaken = takenMap.ContainsKey(g.GroupId),
                    TakenByTeacherId = takenMap.ContainsKey(g.GroupId) ? takenMap[g.GroupId].TeacherId : null,
                    TakenByTeacherName = takenMap.ContainsKey(g.GroupId) ? takenMap[g.GroupId].TeacherName : null
                }).ToList()
            };

            ViewBag.Teachers = await GetTeachersForSubjectAsync();
            return View(viewModel);
        }

        // POST: Subjects/AssignTeacher
        // Asigna un profe a esta materia con los grupos seleccionados
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AssignTeacher(int subjectId, int teacherId, List<int> groupIds)
        {
            if (!groupIds.Any())
            {
                TempData["Error"] = "Debes seleccionar al menos un grupo";
                return RedirectToAction(nameof(Details), new { id = subjectId });
            }

            // Verificar que ningún grupo seleccionado ya tenga otro profe en esta materia
            var conflictingGroups = await _context.grades_TeacherSubjectGroups
                .Include(tsg => tsg.TeacherSubject)
                .Include(tsg => tsg.Group)
                .Where(tsg => tsg.TeacherSubject.SubjectId == subjectId
                           && groupIds.Contains(tsg.GroupId)
                           && tsg.TeacherSubject.TeacherId != teacherId)
                .ToListAsync();

            if (conflictingGroups.Any())
            {
                var names = string.Join(", ", conflictingGroups.Select(c => c.Group.Name));
                TempData["Error"] = $"Los siguientes grupos ya tienen un profesor asignado en esta materia: {names}";
                return RedirectToAction(nameof(Details), new { id = subjectId });
            }

            // Obtener o crear el TeacherSubject
            var teacherSubject = await _context.grades_TeacherSubjects
                .FirstOrDefaultAsync(ts => ts.TeacherId == teacherId && ts.SubjectId == subjectId);

            if (teacherSubject == null)
            {
                teacherSubject = new grades_teacher_subject
                {
                    TeacherId = teacherId,
                    SubjectId = subjectId
                };
                _context.grades_TeacherSubjects.Add(teacherSubject);
                await _context.SaveChangesAsync();
            }

            // Agregar solo los grupos nuevos (sin duplicar los que ya tenga este profe)
            var existingGroupIds = await _context.grades_TeacherSubjectGroups
                .Where(tsg => tsg.TeacherSubjectId == teacherSubject.TeacherSubjectId)
                .Select(tsg => tsg.GroupId)
                .ToListAsync();

            foreach (var gId in groupIds.Where(g => !existingGroupIds.Contains(g)))
            {
                _context.grades_TeacherSubjectGroups.Add(new grades_teacher_subject_group
                {
                    TeacherSubjectId = teacherSubject.TeacherSubjectId,
                    GroupId = gId
                });
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Profesor asignado exitosamente";
            return RedirectToAction(nameof(Details), new { id = subjectId });
        }

        // POST: Subjects/RemoveTeacherGroup
        // Quita un grupo específico de un profe en esta materia
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> RemoveTeacherGroup(int teacherSubjectGroupId, int subjectId)
        {
            var tsg = await _context.grades_TeacherSubjectGroups.FindAsync(teacherSubjectGroupId);
            if (tsg == null) return NotFound();

            var teacherSubjectId = tsg.TeacherSubjectId;
            _context.grades_TeacherSubjectGroups.Remove(tsg);
            await _context.SaveChangesAsync();

            // Si el profe ya no tiene ningún grupo en esta materia, limpiar también el TeacherSubject
            var remaining = await _context.grades_TeacherSubjectGroups
                .CountAsync(x => x.TeacherSubjectId == teacherSubjectId);

            if (remaining == 0)
            {
                var ts = await _context.grades_TeacherSubjects.FindAsync(teacherSubjectId);
                if (ts != null)
                {
                    _context.grades_TeacherSubjects.Remove(ts);
                    await _context.SaveChangesAsync();
                }
            }

            TempData["Success"] = "Grupo removido del profesor";
            return RedirectToAction(nameof(Details), new { id = subjectId });
        }

        // GET: Subjects/ManageUnits/5
        public async Task<IActionResult> ManageUnits(int? subjectId)
        {
            if (subjectId == null) return NotFound();

            var subject = await _context.grades_Subjects
                .Include(s => s.GradeLevel)
                .Include(s => s.Units)
                .FirstOrDefaultAsync(s => s.SubjectId == subjectId);

            if (subject == null) return NotFound();

            var viewModel = new ManageUnitsViewModel
            {
                SubjectId = subject.SubjectId,
                SubjectName = subject.Name,
                GradeLevelName = subject.GradeLevel.Name,
                Units = subject.Units
                    .OrderBy(u => u.UnitNumber)
                    .Select(u => new UnitViewModel
                    {
                        UnitId = u.UnitId,
                        UnitNumber = u.UnitNumber,
                        IsOpen = u.IsOpen,
                        HasGrades = _context.grades_Grades.Any(g => g.SubjectUnitId == u.UnitId)
                    }).ToList()
            };

            return View(viewModel);
        }

        // POST: Subjects/AddUnit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUnit(int subjectId, int unitNumber)
        {
            var subject = await _context.grades_Subjects
                .Include(s => s.Units)
                .FirstOrDefaultAsync(s => s.SubjectId == subjectId);

            if (subject == null) return NotFound();

            if (subject.Units.Any(u => u.UnitNumber == unitNumber))
            {
                TempData["Error"] = $"La unidad {unitNumber} ya existe";
                return RedirectToAction(nameof(ManageUnits), new { subjectId });
            }

            var unit = new grades_subject_unit
            {
                SubjectId = subjectId,
                UnitNumber = unitNumber,
                IsOpen = true
            };

            _context.grades_SubjectUnits.Add(unit);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Unidad {unitNumber} agregada exitosamente";
            return RedirectToAction(nameof(ManageUnits), new { subjectId });
        }

        // POST: Subjects/ToggleUnit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUnit(int unitId)
        {
            var unit = await _context.grades_SubjectUnits.FindAsync(unitId);
            if (unit == null) return NotFound();

            unit.IsOpen = !unit.IsOpen;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Unidad {(unit.IsOpen ? "abierta" : "cerrada")} exitosamente";
            return RedirectToAction(nameof(ManageUnits), new { subjectId = unit.SubjectId });
        }

        // POST: Subjects/DeleteUnit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUnit(int unitId)
        {
            var unit = await _context.grades_SubjectUnits
                .Include(u => u.Subject)
                .FirstOrDefaultAsync(u => u.UnitId == unitId);

            if (unit == null) return NotFound();

            var hasGrades = await _context.grades_Grades.AnyAsync(g => g.SubjectUnitId == unitId);
            if (hasGrades)
            {
                TempData["Error"] = "No se puede eliminar la unidad porque tiene calificaciones asociadas";
                return RedirectToAction(nameof(ManageUnits), new { subjectId = unit.SubjectId });
            }

            _context.grades_SubjectUnits.Remove(unit);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Unidad eliminada exitosamente";
            return RedirectToAction(nameof(ManageUnits), new { subjectId = unit.SubjectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var subject = await _context.grades_Subjects
                .Include(s => s.Units)
                .FirstOrDefaultAsync(s => s.SubjectId == id);

            if (subject == null) return NotFound();

            var hasGrades = await _context.grades_Grades
                .AnyAsync(g => subject.Units.Select(u => u.UnitId).Contains(g.SubjectUnitId));

            if (hasGrades)
            {
                TempData["Error"] = "No se puede eliminar la materia porque tiene calificaciones asociadas";
                return RedirectToAction(nameof(Index));
            }

            // Eliminar asignaciones de profesores
            var teacherSubjects = await _context.grades_TeacherSubjects
                .Include(ts => ts.TeacherSubjectGroups)
                .Where(ts => ts.SubjectId == id)
                .ToListAsync();

            foreach (var ts in teacherSubjects)
                _context.grades_TeacherSubjectGroups.RemoveRange(ts.TeacherSubjectGroups);

            _context.grades_TeacherSubjects.RemoveRange(teacherSubjects);

            // Eliminar unidades y materia
            if (subject.Units.Any())
                _context.grades_SubjectUnits.RemoveRange(subject.Units);

            _context.grades_Subjects.Remove(subject);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Materia eliminada exitosamente";
            return RedirectToAction(nameof(Index));
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        private bool SubjectExists(int id)
        {
            return _context.grades_Subjects.Any(e => e.SubjectId == id);
        }

        /// <summary>Usuarios con rol Teacher/Profesor para los selects de asignación</summary>
        private async Task<List<dynamic>> GetTeachersForSubjectAsync()
        {
            var teacherRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name.ToLower() == "profesor"
                                       || r.Name.ToLower() == "teacher"
                                       || r.Name.ToLower() == "docente");

            if (teacherRole == null)
            {
                return await _context.Users
                    .Include(u => u.Person)
                    .Where(u => u.IsActive)
                    .Select(u => new {
                        u.UserId,
                        FullName = u.Person.FirstName + " " +
                                   u.Person.LastNamePaternal + " " +
                                   u.Person.LastNameMaternal
                    })
                    .OrderBy(u => u.FullName)
                    .ToListAsync<dynamic>();
            }

            return await _context.UserRoles
                .Include(ur => ur.User).ThenInclude(u => u!.Person)
                .Where(ur => ur.RoleId == teacherRole.RoleId && ur.IsActive)
                .Select(ur => new {
                    ur.User!.UserId,
                    FullName = ur.User.Person.FirstName + " " +
                               ur.User.Person.LastNamePaternal + " " +
                               ur.User.Person.LastNameMaternal
                })
                .OrderBy(u => u.FullName)
                .ToListAsync<dynamic>();
        }
    }
}
