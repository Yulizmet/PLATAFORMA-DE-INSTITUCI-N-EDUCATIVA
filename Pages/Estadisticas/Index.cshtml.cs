using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SchoolManager.Data;
using SchoolManager.ViewModels;

namespace SchoolManager.Pages.Estadisticas
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }
        // Propiedades para almacenar los datos de estudiantes y empleados

        public List<StudentStatisticsVM> Students { get; private set; } = new();
        public List<EmployeeStatisticsVM> Employees { get; private set; } = new();
        public List<SocialServiceStatisticsVM> SocialServiceStats { get; private set; } = new();
        public List<ProcedureStatisticsVM> ProcedureStats { get; private set; } = new();

        public string JsonStudents { get; private set; } = "[]";
        public string JsonEmployees { get; private set; } = "[]";
        public string JsonSocialServiceStats { get; private set; } = "[]";
        public string JsonProcedureStats { get; private set; } = "[]";

        public int Total => Students.Count;
        public int CountInscrito => Students.Count(s => s.Estado == "Inscrito");
        public int CountCursando => Students.Count(s => s.Estado == "Cursando");
        public int CountAprobado => Students.Count(s => s.Estado == "Aprobado");
        public int CountReprobado => Students.Count(s => s.Estado == "Reprobado");

        public int EmployeesTotal => Employees.Count;

        public void OnGet()
        {
            LoadStudents();
            LoadEmployees();
            LoadSocialServiceStatistics();
            LoadProcedureStatistics();

            JsonStudents = JsonSerializer.Serialize(Students);
            JsonEmployees = JsonSerializer.Serialize(Employees);
            JsonSocialServiceStats = JsonSerializer.Serialize(SocialServiceStats);
            JsonProcedureStats = JsonSerializer.Serialize(ProcedureStats);
        }

        private void LoadStudents()
        {
            try
            {
                var raw = (from user in _context.Users
                           join person in _context.Persons on user.PersonId equals person.PersonId into personJoin
                           from person in personJoin.DefaultIfEmpty()
                           join userRole in _context.UserRoles on user.UserId equals userRole.UserId into roleJoin
                           from userRole in roleJoin.DefaultIfEmpty()
                           join role in _context.Roles on userRole.RoleId equals role.RoleId into roleDetailJoin
                           from role in roleDetailJoin.DefaultIfEmpty()
                           join finalGrade in _context.grades_FinalGrades on user.UserId equals finalGrade.StudentId into gradeJoin
                           from finalGrade in gradeJoin.DefaultIfEmpty()
                           join subject in _context.grades_Subjects on (finalGrade != null ? finalGrade.SubjectId : -1) equals subject.SubjectId into subjectJoin
                           from subject in subjectJoin.DefaultIfEmpty()
                           where role != null && role.Name == "Student"
                           select new
                           {
                               user.UserId,
                               FirstName = (string?)person.FirstName,
                               LastName = (string?)person.LastNamePaternal,
                               Genero = (string?)person.Gender,
                               Curso = (string?)subject.Name,
                               Semestre = finalGrade != null ? finalGrade.GroupId : 0,
                               Nota = finalGrade != null ? (double)finalGrade.Value : 0.0,
                               FechaInscripcion = user.CreatedDate,
                               HasGrade = finalGrade != null,
                               Passed = finalGrade != null && finalGrade.Passed
                           })
                           .Distinct()
                           .ToList();

                Students = raw
                    .GroupBy(x => x.UserId)
                    .Select(g =>
                    {
                        var s = g.First();
                        var nombre = $"{s.FirstName ?? string.Empty} {s.LastName ?? string.Empty}".Trim();
                        return new StudentStatisticsVM
                        {
                            Id = s.UserId,
                            Nombre = string.IsNullOrWhiteSpace(nombre) ? "Sin nombre" : nombre,
                            Genero = string.IsNullOrWhiteSpace(s.Genero) ? "N/A" : s.Genero,
                            Curso = string.IsNullOrWhiteSpace(s.Curso) ? "Sin asignar" : s.Curso,
                            Semestre = s.Semestre,
                            Nota = s.Nota,
                            FechaInscripcion = s.FechaInscripcion,
                            Estado = !s.HasGrade ? "Inscrito"
                                             : s.Passed ? "Aprobado"
                                             : "Reprobado"
                        };
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando estudiantes: {ex.Message}");
                Students = new List<StudentStatisticsVM>();
            }
        }

        private void LoadEmployees()
        {
            try
            {
                var raw = (from user in _context.Users
                           join person in _context.Persons on user.PersonId equals person.PersonId into personJoin
                           from person in personJoin.DefaultIfEmpty()
                           join userRole in _context.UserRoles on user.UserId equals userRole.UserId into roleJoin
                           from userRole in roleJoin.DefaultIfEmpty()
                           join role in _context.Roles on userRole.RoleId equals role.RoleId into roleDetailJoin
                           from role in roleDetailJoin.DefaultIfEmpty()
                           where role != null && role.Name == "Teacher"
                           select new
                           {
                               user.UserId,
                               FirstName = (string?)person.FirstName,
                               LastName = (string?)person.LastNamePaternal,
                               Genero = (string?)person.Gender,
                               Departamento = (string?)role.Name,
                               Rol = (string?)role.Name,
                               FechaContratacion = (DateTime?)userRole.CreatedDate
                           })
                           .Distinct()
                           .ToList();

                Employees = raw
                    .GroupBy(x => x.UserId)
                    .Select(g =>
                    {
                        var e = g.First();
                        var nombre = $"{e.FirstName ?? string.Empty} {e.LastName ?? string.Empty}".Trim();
                        return new EmployeeStatisticsVM
                        {
                            Id = e.UserId,
                            Nombre = string.IsNullOrWhiteSpace(nombre) ? "Sin nombre" : nombre,
                            Genero = string.IsNullOrWhiteSpace(e.Genero) ? "N/A" : e.Genero,
                            Departamento = string.IsNullOrWhiteSpace(e.Departamento) ? "Sin asignar" : e.Departamento,
                            Rol = string.IsNullOrWhiteSpace(e.Rol) ? "Sin rol" : e.Rol,
                            ActividadesHoy = 0,
                            FechaContratacion = e.FechaContratacion ?? DateTime.Now
                        };
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando empleados: {ex.Message}");
                Employees = new List<EmployeeStatisticsVM>();
            }
        }

        private void LoadSocialServiceStatistics()
        {
            try
            {
                // Obtener todas las asignaciones activas de servicio social
                var assignments = _context.SocialServiceAssignments
                    .Where(a => a.IsActive)
                    .ToList();

                if (!assignments.Any())
                {
                    SocialServiceStats = new List<SocialServiceStatisticsVM>();
                    return;
                }

                var studentIds = assignments.Select(a => a.StudentId).Distinct().ToList();

                // Para cada estudiante, agregar datos de logs, asistencia y nombres
                SocialServiceStats = studentIds.Select(studentId =>
                {
                    var assignment = assignments.FirstOrDefault(a => a.StudentId == studentId);

                    // Obtener información del estudiante
                    var studentUser = _context.Users.FirstOrDefault(u => u.UserId == studentId);
                    var studentPerson = studentUser != null 
                        ? _context.Persons.FirstOrDefault(p => p.PersonId == studentUser.PersonId)
                        : null;

                    var studentName = "";
                    if (studentPerson != null)
                    {
                        var firstName = (string?)studentPerson.FirstName ?? "";
                        var lastName = (string?)studentPerson.LastNamePaternal ?? "";
                        studentName = $"{firstName} {lastName}".Trim();
                    }
                    if (string.IsNullOrWhiteSpace(studentName))
                    {
                        studentName = "Sin nombre";
                    }

                    // Obtener información del maestro asesor
                    var teacherName = "Sin asignar";
                    if (assignment?.TeacherId > 0)
                    {
                        var teacherUser = _context.Users.FirstOrDefault(u => u.UserId == assignment.TeacherId);
                        var teacherPerson = teacherUser != null
                            ? _context.Persons.FirstOrDefault(p => p.PersonId == teacherUser.PersonId)
                            : null;

                        if (teacherPerson != null)
                        {
                            var teacherFirstName = (string?)teacherPerson.FirstName ?? "";
                            var teacherLastName = (string?)teacherPerson.LastNamePaternal ?? "";
                            teacherName = $"{teacherFirstName} {teacherLastName}".Trim();
                        }
                        if (string.IsNullOrWhiteSpace(teacherName))
                        {
                            teacherName = "Sin asignar";
                        }
                    }

                    // Obtener nombre del grupo
                    var groupName = assignment?.GroupName ?? "Sin asignar";
                    if (string.IsNullOrWhiteSpace(groupName))
                    {
                        groupName = "Sin asignar";
                    }

                    // Agregar horas aprobadas de los logs
                    var logs = _context.SocialServiceLogs
                        .Where(l => l.StudentId == studentId && l.IsApproved)
                        .ToList();

                    var hoursPracticas = logs.Sum(l => l.ApprovedHoursPracticas);
                    var hoursServicioSocial = logs.Sum(l => l.ApprovedHoursServicioSocial);
                    var totalHours = hoursPracticas + hoursServicioSocial;

                    // Calcular tasa de asistencia
                    var attendanceRecords = _context.SocialServiceAttendances
                        .Where(a => a.StudentId == studentId)
                        .ToList();

                    double attendanceRate = 0.0;
                    if (attendanceRecords.Any())
                    {
                        var presentCount = attendanceRecords.Count(a => a.IsPresent);
                        attendanceRate = (double)presentCount / attendanceRecords.Count * 100.0;
                    }

                    // Determinar estado basado en horas
                    // Umbrales: 120 horas para completado, 20 horas para en progreso
                    string status = "Pendiente";
                    if (totalHours >= 120)
                    {
                        status = "Completado";
                    }
                    else if (totalHours >= 20)
                    {
                        status = "En progreso";
                    }

                    // Obtener última actualización
                    DateTime? lastUpdate = null;
                    var latestLog = logs.OrderByDescending(l => l.CreatedAt).FirstOrDefault();
                    if (latestLog != null)
                    {
                        lastUpdate = latestLog.CreatedAt;
                    }
                    if (assignment?.AssignedDate > lastUpdate)
                    {
                        lastUpdate = assignment.AssignedDate;
                    }

                    return new SocialServiceStatisticsVM
                    {
                        StudentId = studentId,
                        StudentName = studentName,
                        TeacherName = teacherName,
                        GroupName = groupName,
                        HoursPracticas = hoursPracticas,
                        HoursServicioSocial = hoursServicioSocial,
                        TotalHours = totalHours,
                        AttendanceRate = Math.Round(attendanceRate, 2),
                        Status = status,
                        LastUpdate = lastUpdate
                    };
                })
                .OrderBy(s => s.StudentName)
                .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando estadísticas de servicios sociales: {ex.Message}");
                SocialServiceStats = new List<SocialServiceStatisticsVM>();
            }
        }

        private void LoadProcedureStatistics()
        {
            try
            {
                // Obtener todas las solicitudes de procedimiento (inscripción/preinscripción)
                var now = DateTime.Now;

                var rawProcedures = (from req in _context.ProcedureRequest
                                     join procType in _context.ProcedureTypes on req.IdTypeProcedure equals procType.Id
                                     join procFlow in _context.ProcedureFlow on req.IdProcedureFlow equals procFlow.Id
                                     join procStatus in _context.ProcedureStatus on procFlow.IdStatus equals procStatus.Id
                                     join procArea in _context.ProcedureAreas on procType.IdArea equals procArea.Id
                                     join user in _context.Users on req.IdUser equals user.UserId into userJoin
                                     from user in userJoin.DefaultIfEmpty()
                                     join person in _context.Persons on (user != null ? user.PersonId : -1) equals person.PersonId into personJoin
                                     from person in personJoin.DefaultIfEmpty()
                                     where procType.Name.Contains("Preinscripción") || procType.Name.Contains("Inscripción")
                                     orderby req.DateUpdated descending
                                     select new
                                     {
                                         req.Id,
                                         req.Folio,
                                         FirstName = (string?)person.FirstName,
                                         LastName = (string?)person.LastNamePaternal,
                                         ProcedureTypeName = (string?)procType.Name,
                                         AreaName = (string?)procArea.Name,
                                         StatusName = (string?)procStatus.Name,
                                         InternalCode = (string?)procStatus.InternalCode,
                                         req.DateCreated,
                                         req.DateUpdated
                                     })
                                     .ToList();

                ProcedureStats = rawProcedures
                    .Select(p =>
                    {
                        var studentName = "";
                        if (!string.IsNullOrWhiteSpace(p.FirstName) || !string.IsNullOrWhiteSpace(p.LastName))
                        {
                            var firstName = (p.FirstName ?? "").Trim();
                            var lastName = (p.LastName ?? "").Trim();
                            studentName = $"{firstName} {lastName}".Trim();
                        }
                        if (string.IsNullOrWhiteSpace(studentName))
                        {
                            studentName = "Sin nombre";
                        }

                        var daysElapsed = (int)(now - p.DateCreated).TotalDays;

                        return new ProcedureStatisticsVM
                        {
                            Id = p.Id,
                            Folio = string.IsNullOrWhiteSpace(p.Folio) ? "Sin folio" : p.Folio,
                            StudentName = studentName,
                            ProcedureType = string.IsNullOrWhiteSpace(p.ProcedureTypeName) ? "Sin tipo" : p.ProcedureTypeName,
                            AreaName = string.IsNullOrWhiteSpace(p.AreaName) ? "Sin área" : p.AreaName,
                            StatusName = string.IsNullOrWhiteSpace(p.StatusName) ? "Desconocido" : p.StatusName,
                            InternalCode = string.IsNullOrWhiteSpace(p.InternalCode) ? "UNKNOWN" : p.InternalCode,
                            DateCreated = p.DateCreated,
                            DateUpdated = p.DateUpdated,
                            DaysElapsed = daysElapsed
                        };
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando estadísticas de procedimientos: {ex.Message}");
                ProcedureStats = new List<ProcedureStatisticsVM>();
            }
        }
    }
}

