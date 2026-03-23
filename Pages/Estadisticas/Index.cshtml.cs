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
        public List<PsychologyLogStatisticsVM> PsychologyLogs { get; private set; } = new();
        public List<MedicalLogStatisticsVM> MedicalLogs { get; private set; } = new();

        public string JsonStudents { get; private set; } = "[]";
        public string JsonEmployees { get; private set; } = "[]";
        public string JsonSocialServiceStats { get; private set; } = "[]";
        public string JsonProcedureStats { get; private set; } = "[]";
        public string JsonPsychologyLogs { get; private set; } = "[]";
        public string JsonMedicalLogs { get; private set; } = "[]";

        // Temporal: captura diagnóstico de la carga de Bitácoras
        public string BitacorasDebugInfo { get; private set; } = string.Empty;

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
            LoadPsychologyLogs();
            LoadMedicalLogs();

            JsonStudents = JsonSerializer.Serialize(Students);
            JsonEmployees = JsonSerializer.Serialize(Employees);
            JsonSocialServiceStats = JsonSerializer.Serialize(SocialServiceStats);
            JsonProcedureStats = JsonSerializer.Serialize(ProcedureStats);
            JsonPsychologyLogs = JsonSerializer.Serialize(PsychologyLogs);
            JsonMedicalLogs = JsonSerializer.Serialize(MedicalLogs);
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
                // Step 1: Obtener todas las asignaciones activas
                var assignments = _context.SocialServiceAssignments
                    .Where(a => a.IsActive)
                    .ToList();

                if (!assignments.Any())
                {
                    SocialServiceStats = new List<SocialServiceStatisticsVM>();
                    return;
                }

                var studentIds = assignments.Select(a => a.StudentId).Distinct().ToList();
                var teacherIds = assignments
                    .Where(a => a.TeacherId > 0)
                    .Select(a => a.TeacherId)
                    .Distinct()
                    .ToList();
                var allUserIds = studentIds.Union(teacherIds).Distinct().ToList();

                // Step 2: Batch-load user + person data en una sola consulta
                var userPersonMap = (from u in _context.Users
                                     join p in _context.Persons on u.PersonId equals p.PersonId
                                     where allUserIds.Contains(u.UserId)
                                     select new
                                     {
                                         u.UserId,
                                         FirstName = (string?)p.FirstName,
                                         LastName = (string?)p.LastNamePaternal
                                     })
                                     .ToList()
                                     .ToDictionary(x => x.UserId);

                // Step 3: Batch-aggregate asistencias — una sola consulta para todos los alumnos
                var attendanceRaw = _context.SocialServiceAttendances
                    .Where(a => studentIds.Contains(a.StudentId))
                    .Select(a => new
                    {
                        a.StudentId,
                        a.IsPresent,
                        a.Date,
                        HasNotes = a.Notes != null && a.Notes != ""
                    })
                    .ToList();

                var attendanceByStudent = attendanceRaw
                    .GroupBy(a => a.StudentId)
                    .ToDictionary(g => g.Key, g => new
                    {
                        Total     = g.Count(),
                        Present   = g.Count(a => a.IsPresent),
                        Absent    = g.Count(a => !a.IsPresent && !a.HasNotes),
                        Justified = g.Count(a => !a.IsPresent && a.HasNotes),
                        LastDate  = g.Max(a => (DateTime?)a.Date)
                    });

                // Step 4: Batch-aggregate horas aprobadas — una sola consulta
                var logsRaw = _context.SocialServiceLogs
                    .Where(l => studentIds.Contains(l.StudentId) && l.IsApproved)
                    .Select(l => new
                    {
                        l.StudentId,
                        l.ApprovedHoursPracticas,
                        l.ApprovedHoursServicioSocial,
                        l.CreatedAt
                    })
                    .ToList();

                var logsByStudent = logsRaw
                    .GroupBy(l => l.StudentId)
                    .ToDictionary(g => g.Key, g => new
                    {
                        HoursPracticas      = g.Sum(l => l.ApprovedHoursPracticas),
                        HoursServicioSocial = g.Sum(l => l.ApprovedHoursServicioSocial),
                        LastCreatedAt       = g.Max(l => (DateTime?)l.CreatedAt)
                    });

                // Step 5: Combinar en memoria
                SocialServiceStats = studentIds.Select(studentId =>
                {
                    var assignment = assignments.First(a => a.StudentId == studentId);

                    // Nombre del alumno
                    var studentName = "Sin nombre";
                    if (userPersonMap.TryGetValue(studentId, out var sp))
                    {
                        var full = $"{sp.FirstName ?? ""} {sp.LastName ?? ""}".Trim();
                        if (!string.IsNullOrWhiteSpace(full)) studentName = full;
                    }

                    // Nombre del maestro asesor
                    var teacherName = "Sin asignar";
                    if (assignment.TeacherId > 0 && userPersonMap.TryGetValue(assignment.TeacherId, out var tp))
                    {
                        var full = $"{tp.FirstName ?? ""} {tp.LastName ?? ""}".Trim();
                        if (!string.IsNullOrWhiteSpace(full)) teacherName = full;
                    }

                    var groupName = string.IsNullOrWhiteSpace(assignment.GroupName) ? "Sin asignar" : assignment.GroupName;

                    // Horas de logs
                    var logData = logsByStudent.TryGetValue(studentId, out var ld) ? ld : null;
                    var hoursPracticas      = logData?.HoursPracticas      ?? 0;
                    var hoursServicioSocial = logData?.HoursServicioSocial ?? 0;
                    var totalHours          = hoursPracticas + hoursServicioSocial;

                    // Datos de asistencia
                    var att              = attendanceByStudent.TryGetValue(studentId, out var ad) ? ad : null;
                    var totalAttendances = att?.Total     ?? 0;
                    var totalPresent     = att?.Present   ?? 0;
                    var totalAbsent      = att?.Absent    ?? 0;
                    var totalJustified   = att?.Justified ?? 0;
                    var lastAttendance   = att?.LastDate;

                    double attendanceRate = totalAttendances > 0
                        ? Math.Round((double)totalPresent / totalAttendances * 100.0, 2)
                        : 0.0;

                    // Estado basado en horas acumuladas
                    string status = "Pendiente";
                    if (totalHours >= 120) status = "Completado";
                    else if (totalHours >= 20) status = "En progreso";

                    // Última actualización
                    DateTime? lastUpdate = logData?.LastCreatedAt;
                    if (assignment.AssignedDate > lastUpdate) lastUpdate = assignment.AssignedDate;

                    return new SocialServiceStatisticsVM
                    {
                        StudentId           = studentId,
                        StudentName         = studentName,
                        TeacherName         = teacherName,
                        GroupName           = groupName,
                        HoursPracticas      = hoursPracticas,
                        HoursServicioSocial = hoursServicioSocial,
                        TotalHours          = totalHours,
                        AttendanceRate      = attendanceRate,
                        Status              = status,
                        LastUpdate          = lastUpdate,
                        TotalAttendances    = totalAttendances,
                        TotalPresent        = totalPresent,
                        TotalAbsent         = totalAbsent,
                        TotalJustified      = totalJustified,
                        LastAttendanceDate  = lastAttendance
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

        private void LoadPsychologyLogs()
        {
            var debug = new System.Text.StringBuilder();
            int step = 0;
            try
            {
                // Step 0 — verify table is reachable
                step = 0;
                int tableCount = _context.MedicalPsychology.Count();
                debug.AppendLine($"[OK] Paso 0 — filas en BD: {tableCount}");

                if (tableCount == 0)
                {
                    debug.AppendLine("[INFO] La tabla no contiene filas.");
                    PsychologyLogs = new List<PsychologyLogStatisticsVM>();
                    BitacorasDebugInfo = debug.ToString();
                    return;
                }

                // Step 1 — fetch every appointment row; no joins so no rows are dropped
                step = 1;
                var appts = _context.MedicalPsychology
                    .OrderByDescending(a => a.CreatedAt)
                    .Select(a => new
                    {
                        Id = a.Id,          
                        Folio = a.Fol,      
                        a.PreenrollmentId,
                        a.AttendanceStatus,
                        a.PsychologyObservations,
                        a.AppointmentDatetime,
                        a.CreatedAt
                    })
                    .ToList();
                debug.AppendLine($"[OK] Paso 1 — filas mapeadas: {appts.Count}");

                // Step 2 — fetch preenrollment records only for the IDs that exist
                step = 2;
                var preIds = appts
                    .Select(a => a.PreenrollmentId)
                    .Distinct().ToList();
                debug.AppendLine($"[OK] Paso 2 — preenrollment IDs únicos: {preIds.Count}");

                var preDict = _context.PreenrollmentGenerals
                    .Where(p => preIds.Contains(p.IdData))
                    .Select(p => new { p.IdData, p.UserId, Matricula = (string?)p.Matricula })
                    .ToDictionary(p => p.IdData);
                debug.AppendLine($"[OK] Paso 2 — preenrollments encontrados: {preDict.Count}");

                // Step 3 — fetch person info for the linked users
                step = 3;
                var userIds = preDict.Values
                    .Where(p => p.UserId.HasValue)
                    .Select(p => p.UserId!.Value)
                    .Distinct().ToList();
                debug.AppendLine($"[OK] Paso 3 — userIds a buscar: {userIds.Count}");

                var personDict = (from u in _context.Users
                                  join p in _context.Persons on u.PersonId equals p.PersonId
                                  where userIds.Contains(u.UserId)
                                  select new { u.UserId, p.FirstName, p.LastNamePaternal })
                                 .ToDictionary(x => x.UserId);
                debug.AppendLine($"[OK] Paso 3 — personas encontradas: {personDict.Count}");

                // Step 4 — combine in memory; every appointment survives
                step = 4;
                PsychologyLogs = appts.Select(a =>
                {
                    string? firstName = null, lastName = null, matricula = null;
                    if (preDict.TryGetValue(a.PreenrollmentId, out var pre))
                    {
                        matricula = pre.Matricula;
                        if (pre.UserId.HasValue &&
                            personDict.TryGetValue(pre.UserId.Value, out var person))
                        {
                            firstName = person.FirstName;
                            lastName  = person.LastNamePaternal;
                        }
                    }
                    var name = $"{firstName ?? ""} {lastName ?? ""}".Trim();
                    return new PsychologyLogStatisticsVM
                    {
                        Id = a.Id,
                        Folio                 = string.IsNullOrWhiteSpace(a.Folio) ? "Sin folio" : a.Folio,
                        StudentName           = string.IsNullOrWhiteSpace(name) ? "Sin nombre" : name,
                        EnrollmentOrMatricula = string.IsNullOrWhiteSpace(matricula) ? "Sin matrícula" : matricula,
                        AttendanceStatus      = string.IsNullOrWhiteSpace(a.AttendanceStatus) ? "Sin estado" : a.AttendanceStatus,
                        Observations          = string.IsNullOrWhiteSpace(a.PsychologyObservations) ? "" : a.PsychologyObservations,
                        AppointmentDate       = a.AppointmentDatetime,
                        CreatedAt             = a.CreatedAt
                    };
                }).ToList();
                debug.AppendLine($"[OK] Paso 4 — PsychologyLogs final: {PsychologyLogs.Count}");
            }
            catch (Exception ex)
            {
                string inner = ex.InnerException != null
                    ? $" | Inner → {ex.InnerException.GetType().Name}: {ex.InnerException.Message}"
                    : string.Empty;
                debug.AppendLine($"[ERROR] Paso {step}: {ex.GetType().Name}: {ex.Message}{inner}");
                PsychologyLogs = new List<PsychologyLogStatisticsVM>();
            }
            BitacorasDebugInfo = debug.ToString();
            System.Diagnostics.Debug.WriteLine($"[Psicología Debug]\n{debug}");
        }

        private void LoadMedicalLogs()
        {
            try
            {
                // Step 1 — fetch every medical record row; no joins so no rows are dropped
                var records = _context.MedicalLogbooks
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new
                    {
                        Id = r.Id,                          
                        r.Folio,
                        StudentId = (int?)r.IdAlumno,       
                        ConsultationReason = r.MotivoConsulta,
                        VitalSigns = r.SignosVitales,
                        Observations = r.Observaciones,
                        TreatmentAction = r.Tratamiento,
                        Status = r.Estado,
                        RecordDatetime = (DateTime?)r.FechaHora,
                        r.CreatedAt
                    })
                    .ToList();

                // Step 2 — fetch person info for the linked students
                var studentIds = records
                    .Where(r => r.StudentId.HasValue)
                    .Select(r => r.StudentId!.Value)
                    .Distinct().ToList();

                var personDict = (from u in _context.Users
                                  join p in _context.Persons on u.PersonId equals p.PersonId
                                  where studentIds.Contains(u.UserId)
                                  select new { u.UserId, p.FirstName, p.LastNamePaternal })
                                 .ToDictionary(x => x.UserId);

                // Step 3 — fetch matricula from preenrollment (one per student)
                var matriculaDict = _context.PreenrollmentGenerals
                    .Where(p => p.UserId.HasValue && studentIds.Contains(p.UserId.Value))
                    .Select(p => new { UserId = p.UserId.Value, Matricula = (string?)p.Matricula })
                    .ToList()
                    .GroupBy(p => p.UserId)
                    .ToDictionary(g => g.Key, g => g.First().Matricula);

                // Step 4 — combine in memory; every record survives
                MedicalLogs = records.Select(r =>
                {
                    string? firstName = null, lastName = null, matricula = null;
                    if (r.StudentId.HasValue)
                    {
                        personDict.TryGetValue(r.StudentId.Value, out var person);
                        firstName = person?.FirstName;
                        lastName  = person?.LastNamePaternal;
                        matriculaDict.TryGetValue(r.StudentId.Value, out matricula);
                    }
                    var name = $"{firstName ?? ""} {lastName ?? ""}".Trim();
                    return new MedicalLogStatisticsVM
                    {
                        Id = r.Id,
                        Folio                 = string.IsNullOrWhiteSpace(r.Folio) ? "Sin folio" : r.Folio,
                        StudentName           = string.IsNullOrWhiteSpace(name) ? "Sin nombre" : name,
                        EnrollmentOrMatricula = string.IsNullOrWhiteSpace(matricula) ? "Sin matrícula" : matricula,
                        ConsultationReason    = string.IsNullOrWhiteSpace(r.ConsultationReason) ? "" : r.ConsultationReason,
                        VitalSigns            = string.IsNullOrWhiteSpace(r.VitalSigns) ? "" : r.VitalSigns,
                        Observations          = string.IsNullOrWhiteSpace(r.Observations) ? "" : r.Observations,
                        TreatmentAction       = string.IsNullOrWhiteSpace(r.TreatmentAction) ? "" : r.TreatmentAction,
                        Status                = string.IsNullOrWhiteSpace(r.Status) ? "Sin estado" : r.Status,
                        RecordDate            = r.RecordDatetime ?? r.CreatedAt,
                        CreatedAt             = r.CreatedAt
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando logs de enfermería: {ex.Message}");
                MedicalLogs = new List<MedicalLogStatisticsVM>();
            }
        }
    }
}

