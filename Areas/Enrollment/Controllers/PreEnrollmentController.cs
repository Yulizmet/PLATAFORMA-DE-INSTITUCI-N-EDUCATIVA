using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Areas.Enrollment.ViewModels;
using SchoolManager.Data;
using SchoolManager.Models;
using SchoolManager.Models.ViewModels;


namespace SchoolManager.Areas.Enrollment.Controllers
{
    [Area("Enrollment")]

    public class PreEnrollmentController : Controller
    {
        private readonly AppDbContext _context;
   

        public PreEnrollmentController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
        }

        // =====================================================================
        // HELPERS PRIVADOS
        // =====================================================================

        private string GenerarMatricula()
        {
            int year = DateTime.Now.Year;
            string yearShort = year.ToString().Substring(2, 2);
            int randomNumber = RandomNumberGenerator.GetInt32(100000, 999999);
            return yearShort + randomNumber.ToString() + yearShort;
        }

        private string GenerarMatriculaUnica()
        {
            string matricula;
            do
            {
                matricula = GenerarMatricula();
            }
            while (_context.PreenrollmentGenerals.Any(x => x.Matricula == matricula));

            return matricula;
        }

        private string GenerarFolio(int idGeneration)
        {
            var generation = _context.PreenrollmentGenerations
                .FirstOrDefault(g => g.IdGeneration == idGeneration);

            if (generation == null)
                throw new Exception("Generación no encontrada.");

            int contador = _context.PreenrollmentGenerals
                .Count(p => p.IdGeneration == idGeneration) + 1;

            return $"{generation.Year}-{contador:D4}";
        }

        private bool preenrollment_generalExists(int id)
        {
            return _context.PreenrollmentGenerals.Any(e => e.IdData == id);
        }


        // =====================================================================
        // DOCUMENTOS (Checklist)
        // =====================================================================

        [HttpPost]
        public async Task<IActionResult> GuardarDocs([FromBody] PreenrollmentDocsDto dto)
        {
            var docs = await _context.PreenrollmentDocs
                .FirstOrDefaultAsync(x => x.IdData == dto.IdData);

            if (docs == null)
            {
                docs = new preenrollment_docs
                {
                    IdData = dto.IdData
                };
                _context.PreenrollmentDocs.Add(docs);
            }

            docs.Fotos = dto.Fotos;
            docs.PagoExamen = dto.PagoExamen;
            docs.ActaNacimiento = dto.ActaNacimiento;
            docs.Curp = dto.Curp;
            docs.Certificados = dto.Certificados;
            docs.ComprobanteDomicilio = dto.ComprobanteDomicilio;
            docs.CartaBuenaConducta = dto.CartaBuenaConducta;

            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }


        // =====================================================================
        // FLUJO 1: REGISTRO INICIAL + GENERACIÓN DE FOLIO
        // El usuario (sin cuenta) llena sus datos y genera un folio de pago.
        // =====================================================================

        // GET: Enrollment/PreEnrollment/Create
       
        [AllowAnonymous]
        public IActionResult Create()
        {
            ViewData["PasoActual"] = 1;

            ViewData["IdCareer"] = new SelectList(
                _context.PreenrollmentCareers, "IdCareer", "name_career");
            ViewData["IdGeneration"] = new SelectList(
                _context.PreenrollmentGenerations, "IdGeneration", "Year");
            return View();
        }


        // GET: Enrollment/PreEnrollment/Aspirantes
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Aspirantes()
        {
            return View();
        }


        // GET: Enrollment/PreEnrollment/AspirantesListas
        [AllowAnonymous]
        [HttpGet]
        public IActionResult AspirantesLista(int idGeneration)
        {
            ViewBag.IdGeneration = idGeneration;
            return View();
        }

        // GET: Enrollment/PreEnrollment/MatriculasAdmin
        [AllowAnonymous]
        [HttpGet]
        public IActionResult MatriculasAdmin()
        {
            return View();
        }

        // GET: Enrollment/PreEnrollment/Configuracion
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Configuracion()
        {
            var careers = await _context.PreenrollmentCareers
                .OrderBy(c => c.name_career)
                .ToListAsync();

            return View(careers);
        }


        // CRUD DE CARRERAS

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCarrera(string name_career)
        {
            if (string.IsNullOrWhiteSpace(name_career))
            {
                TempData["Error"] = "El nombre de la carrera es obligatorio.";
                return RedirectToAction(nameof(Configuracion));
            }

            name_career = name_career.Trim();

            bool existe = await _context.PreenrollmentCareers
                .AnyAsync(c => c.name_career == name_career);

            if (existe)
            {
                TempData["Error"] = "La carrera ya existe.";
                return RedirectToAction(nameof(Configuracion));
            }

            var career = new preenrollment_careers
            {
                name_career = name_career
            };

            _context.PreenrollmentCareers.Add(career);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Carrera agregada correctamente.";
            return RedirectToAction(nameof(Configuracion));
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarCarrera(int idCareer, string name_career)
        {
            if (string.IsNullOrWhiteSpace(name_career))
            {
                TempData["Error"] = "El nombre de la carrera es obligatorio.";
                return RedirectToAction(nameof(Configuracion));
            }

            var career = await _context.PreenrollmentCareers
                .FirstOrDefaultAsync(c => c.IdCareer == idCareer);

            if (career == null)
            {
                TempData["Error"] = "Carrera no encontrada.";
                return RedirectToAction(nameof(Configuracion));
            }

            name_career = name_career.Trim();

            bool existe = await _context.PreenrollmentCareers
                .AnyAsync(c => c.IdCareer != idCareer && c.name_career == name_career);

            if (existe)
            {
                TempData["Error"] = "Ya existe otra carrera con ese nombre.";
                return RedirectToAction(nameof(Configuracion));
            }

            career.name_career = name_career;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Carrera actualizada correctamente.";
            return RedirectToAction(nameof(Configuracion));
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCarrera(int idCareer)
        {
            var career = await _context.PreenrollmentCareers
                .Include(c => c.preenrollment_general)
                .FirstOrDefaultAsync(c => c.IdCareer == idCareer);

            if (career == null)
            {
                TempData["Error"] = "La carrera no existe.";
                return RedirectToAction(nameof(Configuracion));
            }

            if (career.preenrollment_general != null && career.preenrollment_general.Any())
            {
                TempData["Error"] = "No se puede eliminar la carrera porque ya tiene preinscripciones o inscripciones asociadas.";
                return RedirectToAction(nameof(Configuracion));
            }

            _context.PreenrollmentCareers.Remove(career);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Carrera eliminada correctamente.";
            return RedirectToAction(nameof(Configuracion));
        }


        // POST: Enrollment/PreEnrollment/Create
        // Recibe un ViewModel con los datos del formulario de inscripción.
        // Guarda en preenrollment_general y tablas relacionadas, genera folio y matrícula.
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PreEnrollmentCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewData["PasoActual"] = 1;

                ViewData["IdCareer"] = new SelectList(
                    _context.PreenrollmentCareers, "IdCareer", "name_career", vm.IdCareer);
                ViewData["IdGeneration"] = new SelectList(
                    _context.PreenrollmentGenerations, "IdGeneration", "Year", vm.IdGeneration);
                return View(vm);
            }

            // --- Guardar en preenrollment_general ---
            var general = new preenrollment_general
            {
                IdCareer = vm.IdCareer,
                IdGeneration = vm.IdGeneration,
                BloodType = vm.BloodType,
                MaritalStatus = vm.MaritalStatus,
                Nationality = vm.Nationality,
                Occupation = vm.Occupation,
                Work = vm.Work,
                WorkAddress = vm.WorkAddress,
                WorkPhone = vm.WorkPhone,
                CreateStat = DateTime.Now,
                Matricula = GenerarMatriculaUnica(),
                Folio = GenerarFolio(vm.IdGeneration)
            };

            _context.PreenrollmentGenerals.Add(general);
            await _context.SaveChangesAsync(); // necesitamos el IdData generado

            // --- Guardar domicilio ---
            var address = new preenrollment_addresses
            {
                id_data = general.IdData,
                street = vm.Street,
                exterior_number = vm.ExteriorNumber,
                interior_number = vm.InteriorNumber,
                postal_code = vm.PostalCode,
                neighborhood = vm.Neighborhood,
                state = vm.State,
                city = vm.City,
                //phone = vm.Phone
            };
            _context.PreenrollmentAddresses.Add(address);

            // --- Guardar escuela de procedencia ---
            var school = new preenrollment_schools
            {
                id_data = general.IdData,
                school = vm.School,
                degree = vm.Degree,
                state = vm.SchoolState,
                city = vm.SchoolCity,
                average = vm.Average,
                start_date = vm.StartDate,
                end_date = vm.EndDate,
                study_system = vm.StudySystem,
                high_school_type = vm.HighSchoolType
            };
            _context.PreenrollmentSchools.Add(school);

            // --- Guardar tutor ---
            var tutor = new preenrollment_tutors
            {
                id_data = general.IdData,
                relationship = vm.TutorRelationship,
                paternal_last_name = vm.TutorPaternalLastName,
                maternal_last_name = vm.TutorMaternalLastName,
                name = vm.TutorName,
                home_phone = vm.TutorHomePhone,
                work_phone = vm.TutorWorkPhone
            };
            _context.PreenrollmentTutors.Add(tutor);

            // --- Guardar info adicional ---
            var info = new preenrollment_infos
            {
                id_data = general.IdData,
                beca = vm.Beca,
                comu_indi = vm.ComuIndi,
                lengu_indi = vm.LenguIndi,
                incapa = vm.Incapa,
                disease = vm.Disease,
                comment = vm.Comment
            };
            _context.PreenrollmentInfos.Add(info);

            // --- Guardar documentos (checklist inicial en false) ---
            var docs = new preenrollment_docs
            {
                IdData = general.IdData,
                Fotos = false,
                PagoExamen = false,
                ActaNacimiento = false,
                Curp = false,
                Certificados = false,
                ComprobanteDomicilio = false,
                CartaBuenaConducta = false
            };
            _context.PreenrollmentDocs.Add(docs);

            await _context.SaveChangesAsync();

            // Redirigir a confirmación mostrando el folio generado
            return RedirectToAction(nameof(SubirDocumentos), new { id = general.IdData });
        }


        // GET: Enrollment/PreEnrollment/FolioGenerado/5
        // Muestra al usuario su folio de pago recién generado.
        [AllowAnonymous]
        public async Task<IActionResult> FolioGenerado(int id)
        {
            var general = await _context.PreenrollmentGenerals
                .FirstOrDefaultAsync(p => p.IdData == id);

            if (general == null)
                return NotFound();

            return View(general);
        }

        // =====================================================================
        // FLUJO 2: VERIFICACIÓN DE PAGO Y COMPLETAR REGISTRO
        // El admin aprueba el folio. El usuario ingresa su folio, se verifica
        // que esté APPROVED y se le permite completar y activar su cuenta.
        // =====================================================================

        // GET: Enrollment/PreEnrollment/ValidateFolio
        [AllowAnonymous]
        [HttpGet]
        public IActionResult ValidateFolio()
        {
            return View();
        }

        // POST: Enrollment/PreEnrollment/ValidateFolio
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ValidateFolio(string folio)
        {
            if (string.IsNullOrWhiteSpace(folio))
            {
                ModelState.AddModelError("", "Debes ingresar un folio.");
                return View();
            }

            var pre = await _context.PreenrollmentGenerals
                .Include(p => p.ProcedureRequest)
                    .ThenInclude(pr => pr.ProcedureFlow)
                        .ThenInclude(pf => pf.ProcedureStatus)
                .FirstOrDefaultAsync(p => p.Folio == folio);

            if (pre == null)
            {
                ModelState.AddModelError("", "Folio no encontrado.");
                return View();
            }

            if (pre.ProcedureRequest == null)
            {
                ModelState.AddModelError("", "No hay un trámite asociado a este folio.");
                return View();
            }

            if (pre.ProcedureRequest.ProcedureFlow?.ProcedureStatus?.InternalCode != "APPROVED")
            {
                ModelState.AddModelError("", "Tu pago aún no ha sido aprobado. Intenta más tarde.");
                return View();
            }

            if (pre.UserId != null)
            {
                ModelState.AddModelError("", "Este folio ya tiene una cuenta asociada.");
                return View();
            }

            return RedirectToAction(nameof(CrearUsuarioAlumno), new { id = pre.IdData });
        }

        // GET: Enrollment/PreEnrollment/CrearUsuarioAlumno
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> CrearUsuarioAlumno(int id)
        {
            var pre = await _context.PreenrollmentGenerals
                .Include(p => p.Person)
                .Include(p => p.Career)
                .Include(p => p.ProcedureRequest)
                    .ThenInclude(pr => pr.ProcedureFlow)
                        .ThenInclude(pf => pf.ProcedureStatus)
                .FirstOrDefaultAsync(p => p.IdData == id);

            if (pre == null)
                return NotFound();

            if (pre.ProcedureRequest?.ProcedureFlow?.ProcedureStatus?.InternalCode != "APPROVED")
                return RedirectToAction(nameof(ValidateFolio));

            if (pre.UserId != null)
                return RedirectToAction(nameof(ValidateFolio));

            if (pre.Person == null)
                return RedirectToAction(nameof(ValidateFolio));

            var vm = new CreateStudentUserViewModel
            {
                IdData = pre.IdData,
                PersonId = pre.PersonId,
                Folio = pre.Folio ?? "",
                Matricula = pre.Matricula ?? "",
               
                Carrera = pre.Career?.name_career ?? "",
                NombreCompleto = $"{pre.Person.FirstName} {pre.Person.LastNamePaternal} {pre.Person.LastNameMaternal}",
                Email = pre.Person.Email ?? ""
            };

            return View(vm);
        }

        // POST: Enrollment/PreEnrollment/CrearUsuarioAlumno
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearUsuarioAlumno(CreateStudentUserViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var pre = await _context.PreenrollmentGenerals
                .Include(p => p.Person)
                .Include(p => p.Career)
                .Include(p => p.ProcedureRequest)
                    .ThenInclude(pr => pr.ProcedureFlow)
                        .ThenInclude(pf => pf.ProcedureStatus)
                .FirstOrDefaultAsync(p => p.IdData == vm.IdData);

            if (pre == null)
                return NotFound();

            if (pre.ProcedureRequest?.ProcedureFlow?.ProcedureStatus?.InternalCode != "APPROVED")
            {
                ModelState.AddModelError("", "Tu pago aún no ha sido aprobado.");
                return View(vm);
            }

            if (pre.UserId != null)
            {
                ModelState.AddModelError("", "Este folio ya tiene una cuenta asociada.");
                return View(vm);
            }

            if (pre.Person == null)
            {
                ModelState.AddModelError("", "No existe la persona asociada a esta preinscripción.");
                return View(vm);
            }

            bool existeUsername = await _context.Users.AnyAsync(u => u.Username == vm.Username);
            if (existeUsername)
            {
                ModelState.AddModelError("", "La matrícula ya está registrada como usuario.");
                return View(vm);
            }

            bool existeCorreo = await _context.Users.AnyAsync(u => u.Email == vm.Email);
            if (existeCorreo)
            {
                ModelState.AddModelError("Email", "Ese correo ya está registrado.");
                return View(vm);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                pre.Person.Email = vm.Email;

                var user = new users_user
                {
                    PersonId = pre.Person.PersonId,
                    Username = pre.Matricula ?? vm.Username,
                    Email = vm.Email,
                    IsLocked = false,
                    LockReason = "",
                    LastLoginDate = null,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.Password);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var userRole = new users_userrole
                {
                    UserId = user.UserId,
                    RoleId = 1,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                _context.UserRoles.Add(userRole);
                await _context.SaveChangesAsync();

                pre.UserId = user.UserId;
                pre.PersonId = pre.Person.PersonId;

                if (pre.ProcedureRequest != null)
                {
                    pre.ProcedureRequest.IdUser = user.UserId;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                ModelState.Clear();

                vm.Username = user.Username;
                vm.Matricula = pre.Matricula ?? "";
                vm.NombreCompleto = $"{pre.Person.FirstName} {pre.Person.LastNamePaternal} {pre.Person.LastNameMaternal}";
                vm.Carrera = pre.Career?.name_career ?? "";
                vm.Email = user.Email;
                vm.Password = "";
                vm.ConfirmPassword = "";

                ViewBag.UserCreated = true;
                ViewBag.CreatedPassword = "La contraseña es la que capturaste antes de crear el usuario.";

                return View(vm);
            }
            catch
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Ocurrió un error al crear el usuario.");
                return View(vm);
            }
        }

        // =====================================================================
        // CRUD ADMIN (Index, Details, Edit, Delete)
        // Para que el admin pueda gestionar las preinscripciones.
        // =====================================================================


        // =====================================================================
        // Documentos
        // =====================================================================

        // GET: Enrollment/PreEnrollment/SubirDocumentos
        // =====================================================================
        // Documentos
        // =====================================================================

        // GET: Enrollment/PreEnrollment/SubirDocumentos
        [AllowAnonymous]
        [HttpGet]
        public IActionResult SubirDocumentos(int id)
        {
            ViewData["PasoActual"] = 2;
            ViewBag.IdData = id;
            return View();
        }

        public class PreenrollmentDocsDto
        {
            public int IdData { get; set; }
            public bool Fotos { get; set; }
            public bool PagoExamen { get; set; }
            public bool ActaNacimiento { get; set; }
            public bool Curp { get; set; }
            public bool Certificados { get; set; }
            public bool ComprobanteDomicilio { get; set; }
            public bool CartaBuenaConducta { get; set; }
        }


        // =====================================================================
        // Confirmar informacion
        // =====================================================================

        // GET: Enrollment/PreEnrollment/ConfirmarPreinscripcion
        [AllowAnonymous]
        [HttpGet]
        public IActionResult ConfirmarPreinscripcion()
        {
            ViewData["PasoActual"] = 3;

            ViewData["IdCareer"] = new SelectList(
                _context.PreenrollmentCareers, "IdCareer", "name_career"
            );

            return View();
        }



        // POST: Enrollment/PreEnrollment/ConfirmarPreinscripcion
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarPreinscripcion(PreEnrollmentViewModel vm)
        {
            Console.WriteLine("=== ENTRO AL POST ConfirmarPreinscripcion ===");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("=== MODELSTATE INVALIDO ===");

                foreach (var item in ModelState)
                {
                    if (item.Value.Errors.Count > 0)
                    {
                        Console.WriteLine($"Campo: {item.Key}");
                        foreach (var error in item.Value.Errors)
                        {
                            Console.WriteLine($" - Error: {error.ErrorMessage}");
                            if (error.Exception != null)
                                Console.WriteLine($" - Exception: {error.Exception.Message}");
                        }
                    }
                }

                ViewData["PasoActual"] = 3;
                ViewData["IdCareer"] = new SelectList(
                    _context.PreenrollmentCareers, "IdCareer", "name_career", vm?.DatosGenerales?.IdCareer
                );
                ViewData["ModoConfirmacion"] = true;

                return View(vm);
            }

            var generation = await _context.PreenrollmentGenerations
                .OrderByDescending(g => g.Year)
                .FirstOrDefaultAsync();

            if (generation == null)
            {
                Console.WriteLine("=== NO HAY GENERACION ===");

                ModelState.AddModelError("", "No hay generaciones registradas en el sistema.");
                ViewData["PasoActual"] = 3;
                ViewData["IdCareer"] = new SelectList(
                    _context.PreenrollmentCareers, "IdCareer", "name_career", vm?.DatosGenerales?.IdCareer
                );
                ViewData["ModoConfirmacion"] = true;

                return View(vm);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                Console.WriteLine("=== VA A GUARDAR PERSON ===");

                var person = new users_person
                {
                    FirstName = vm.Persona.FirstName,
                    LastNamePaternal = vm.Persona.LastNamePaternal,
                    LastNameMaternal = vm.Persona.LastNameMaternal,
                    BirthDate = vm.Persona.BirthDate,
                    Gender = vm.Persona.Gender,
                    Curp = vm.Persona.Curp,
                    Email = vm.Persona.Email,
                    Phone = vm.Persona.Phone,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                _context.Persons.Add(person);
                await _context.SaveChangesAsync();

                Console.WriteLine("=== PERSON GUARDADA ===");

                var general = new preenrollment_general
                {
                    IdCareer = vm.DatosGenerales.IdCareer,
                    IdGeneration = generation.IdGeneration,
                    PersonId = person.PersonId,
                    BloodType = vm.DatosGenerales.BloodType,
                    MaritalStatus = vm.DatosGenerales.MaritalStatus,
                    Nationality = vm.DatosGenerales.Nationality,
                    Occupation = vm.DatosGenerales.Occupation,
                    Work = vm.DatosGenerales.Work,
                    WorkAddress = vm.DatosGenerales.WorkAddress,
                    WorkPhone = vm.DatosGenerales.WorkPhone,
                    CreateStat = DateTime.Now,
                    Folio = GenerarFolio(generation.IdGeneration),
                    Matricula = GenerarMatriculaUnica()
                };

                _context.PreenrollmentGenerals.Add(general);
                await _context.SaveChangesAsync();

                Console.WriteLine("=== GENERAL GUARDADA CON ID: " + general.IdData + " ===");

                var address = new preenrollment_addresses
                {
                    id_data = general.IdData,
                    street = vm.Domicilio.street,
                    exterior_number = vm.Domicilio.exterior_number,
                    interior_number = vm.Domicilio.interior_number,
                    postal_code = vm.Domicilio.postal_code,
                    neighborhood = vm.Domicilio.neighborhood,
                    state = vm.Domicilio.state,
                    city = vm.Domicilio.city,
                };
                _context.PreenrollmentAddresses.Add(address);

                var school = new preenrollment_schools
                {
                    id_data = general.IdData,
                    school = vm.DatosEscolares.school,
                    degree = vm.DatosEscolares.degree,
                    state = vm.DatosEscolares.state,
                    city = vm.DatosEscolares.city,
                    average = vm.DatosEscolares.average,
                    start_date = vm.DatosEscolares.start_date,
                    end_date = vm.DatosEscolares.end_date,
                    study_system = vm.DatosEscolares.study_system,
                    high_school_type = vm.DatosEscolares.high_school_type
                };
                _context.PreenrollmentSchools.Add(school);

                var tutor = new preenrollment_tutors
                {
                    id_data = general.IdData,
                    relationship = vm.Tutor.relationship,
                    paternal_last_name = vm.Tutor.paternal_last_name,
                    maternal_last_name = vm.Tutor.maternal_last_name,
                    name = vm.Tutor.name,
                    home_phone = vm.Tutor.home_phone,
                    work_phone = vm.Tutor.work_phone
                };
                _context.PreenrollmentTutors.Add(tutor);

                var info = new preenrollment_infos
                {
                    id_data = general.IdData,
                    beca = string.IsNullOrWhiteSpace(vm.Otros?.beca) ? "No" : vm.Otros.beca,
                    comu_indi = vm.Otros?.comu_indi ?? false,
                    lengu_indi = vm.Otros?.lengu_indi ?? false,
                    incapa = vm.Otros?.incapa ?? false,
                    disease = vm.Otros?.disease ?? false,
                    comment = vm.Otros?.comment
                };
                _context.PreenrollmentInfos.Add(info);

                var docs = new preenrollment_docs
                {
                    IdData = general.IdData,
                    Fotos = false,
                    PagoExamen = false,
                    ActaNacimiento = false,
                    Curp = false,
                    Certificados = false,
                    ComprobanteDomicilio = false,
                    CartaBuenaConducta = false
                };
                _context.PreenrollmentDocs.Add(docs);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                Console.WriteLine("=== TODO GUARDADO, REDIRIGIENDO A FINALIZAR ===");

                return RedirectToAction(nameof(Finalizar), new { id = general.IdData });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                Console.WriteLine("=== ERROR EN CATCH ===");
                Console.WriteLine(ex.ToString());

                ViewData["PasoActual"] = 3;
                ViewData["IdCareer"] = new SelectList(
                    _context.PreenrollmentCareers, "IdCareer", "name_career", vm?.DatosGenerales?.IdCareer
                );
                ViewData["ModoConfirmacion"] = true;

                throw;
            }
        }
        // vista de finalizar
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Finalizar(int id)
        {
            ViewData["PasoActual"] = 4;

            var general = await _context.PreenrollmentGenerals
                .Include(g => g.Career)
                .Include(g => g.Person)
                .Include(g => g.Generation)
                .FirstOrDefaultAsync(g => g.IdData == id);

            if (general == null)
                return NotFound();

            var domicilio = await _context.PreenrollmentAddresses
                .FirstOrDefaultAsync(x => x.id_data == id);

            var escuela = await _context.PreenrollmentSchools
                .FirstOrDefaultAsync(x => x.id_data == id);

            var vm = new FinalizarViewModel
            {
                Folio = general.Folio,
                FechaEnvio = general.CreateStat ?? DateTime.Now,
                NombreAspirante = general.Person == null
                    ? ""
                    : $"{general.Person.FirstName} {general.Person.LastNamePaternal} {general.Person.LastNameMaternal}",
                Especialidad = general.Career?.name_career ?? "",

                Genero = general.Person?.Gender ?? "",
                FechaNacimiento = general.Person?.BirthDate,
                Curp = general.Person?.Curp ?? "",
                TipoSangre = general.BloodType ?? "",

                SecundariaProcedencia = escuela?.school ?? "",
                Promedio = escuela?.average,
                FechaFinSecundaria = escuela?.end_date,

                Calle = domicilio?.street ?? "",
                NumeroExterior = domicilio?.exterior_number ?? "",
                NumeroInterior = domicilio?.interior_number ?? "",
                Colonia = domicilio?.neighborhood ?? "",
                CodigoPostal = domicilio?.postal_code ?? "",
                Municipio = domicilio?.city ?? "",
                Estado = domicilio?.state ?? "",

                Generacion = general.Generation != null ? general.Generation.Year.ToString() : ""
            };

            return View(vm);
        }

        // GET: Enrollment/PreEnrollment
        [AllowAnonymous]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Create));
        }

        // public async Task<IActionResult> Index()
        //{
        //   var generales = await _context.PreenrollmentGenerals
        //     .Include(p => p.Career)
        //    .Include(p => p.Generation)
        //    .ToListAsync();
        // return View(generales);
        //} 

        // GET: Enrollment/PreEnrollment/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var general = await _context.PreenrollmentGenerals
                .Include(p => p.Career)
                .Include(p => p.Generation)
                .Include(p => p.Addresses)
                .Include(p => p.Schools)
                .Include(p => p.Tutors)
                .Include(p => p.Infos)
                .FirstOrDefaultAsync(m => m.IdData == id);

            if (general == null)
                return NotFound();

            return View(general);
        }

        // GET: Enrollment/PreEnrollment/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var general = await _context.PreenrollmentGenerals.FindAsync(id);
            if (general == null)
                return NotFound();

            ViewData["IdCareer"] = new SelectList(
                _context.PreenrollmentCareers, "IdCareer", "name_career", general.IdCareer);
            ViewData["IdGeneration"] = new SelectList(
                _context.PreenrollmentGenerations, "IdGeneration", "Year", general.IdGeneration);

            return View(general);
        }

        // POST: Enrollment/PreEnrollment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, preenrollment_general general)
        {
            if (id != general.IdData)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(general);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!preenrollment_generalExists(general.IdData))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["IdCareer"] = new SelectList(
                _context.PreenrollmentCareers, "IdCareer", "name_career", general.IdCareer);
            ViewData["IdGeneration"] = new SelectList(
                _context.PreenrollmentGenerations, "IdGeneration", "Year", general.IdGeneration);

            return View(general);
        }

        // GET: Enrollment/PreEnrollment/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var general = await _context.PreenrollmentGenerals
                .Include(p => p.Career)
                .FirstOrDefaultAsync(m => m.IdData == id);

            if (general == null)
                return NotFound();

            return View(general);
        }

        // POST: Enrollment/PreEnrollment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var general = await _context.PreenrollmentGenerals.FindAsync(id);
            if (general != null)
            {
                _context.PreenrollmentGenerals.Remove(general);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        [HttpGet]
        [EnableCors("AllowAdmin")]
        public async Task<IActionResult> GetGeneraciones()
        {
            var generaciones = await _context.PreenrollmentGenerations
                .OrderByDescending(g => g.Year)
                .Select(g => new
                {
                    g.IdGeneration,
                    g.Year
                })
                .ToListAsync();

            return Json(generaciones);
        }

        // GET: Enrollment/PreEnrollment/GetAspirantesPorGeneracion?idGeneration=4
        [AllowAnonymous]
        [HttpGet]
        [EnableCors("AllowAdmin")]
        public async Task<IActionResult> GetAspirantesPorGeneracion(int idGeneration)
        {
            var generation = await _context.PreenrollmentGenerations
                .FirstOrDefaultAsync(g => g.IdGeneration == idGeneration);

            if (generation == null)
                return NotFound(new { message = "Generación no encontrada." });

            var aspirantes = await _context.PreenrollmentGenerals
                .Include(g => g.Person)
                .Where(g => g.IdGeneration == idGeneration)
                .Select(g => new
                {
                    g.IdData,
                    g.Folio,
                    g.Matricula,
                    NombreCompleto = g.Person != null
                        ? $"{g.Person.FirstName} {g.Person.LastNamePaternal} {g.Person.LastNameMaternal}"
                        : "Sin datos personales",
                    Email = g.Person != null ? g.Person.Email : "",
                    g.IdGeneration
                })
                .ToListAsync();

            return Json(aspirantes);
        }

        // GET: Enrollment/PreEnrollment/GetAspiranteDetalle?idData=5
        [AllowAnonymous]
        [HttpGet]
        [EnableCors("AllowAdmin")]
        public async Task<IActionResult> GetAspiranteDetalle(int idData)
        {
            var general = await _context.PreenrollmentGenerals
                .Include(g => g.Person)
                .Include(g => g.Career)
                .Include(g => g.Generation)
                .Include(g => g.Addresses)
                .Include(g => g.Schools)
                .Include(g => g.Tutors)
                .Include(g => g.Infos)
                .FirstOrDefaultAsync(g => g.IdData == idData);

            if (general == null)
                return NotFound(new { message = "Aspirante no encontrado." });

            var address = general.Addresses.FirstOrDefault();
            var school = general.Schools.FirstOrDefault();
            var tutor = general.Tutors.FirstOrDefault();
            var info = general.Infos.FirstOrDefault();

            var result = new
            {
                // --- Persona ---
                nombre = general.Person?.FirstName ?? "",
                apellidoPaterno = general.Person?.LastNamePaternal ?? "",
                apellidoMaterno = general.Person?.LastNameMaternal ?? "",
                email = general.Person?.Email ?? "",
                telefono = general.Person?.Phone ?? "",
                genero = general.Person?.Gender ?? "",
                fechaNacimiento = general.Person?.BirthDate,
                curp = general.Person?.Curp ?? "",

                // --- General ---
                folio = general.Folio,
                matricula = general.Matricula,
                carrera = general.Career?.name_career ?? "",
                generacion = general.Generation?.Year.ToString() ?? "",
                tipoSangre = general.BloodType ?? "",
                estadoCivil = general.MaritalStatus,
                nacionalidad = general.Nationality,
                ocupacion = general.Occupation ?? "",
                trabaja = general.Work,
                domicilioTrabajo = general.WorkAddress ?? "",
                telefonoTrabajo = general.WorkPhone ?? "",

                // --- Domicilio ---
                calle = address?.street ?? "",
                numExterior = address?.exterior_number ?? "",
                numInterior = address?.interior_number ?? "",
                colonia = address?.neighborhood ?? "",
                codigoPostal = address?.postal_code ?? "",
                ciudad = address?.city ?? "",
                estado = address?.state ?? "",

                // --- Escuela ---
                escuela = school?.school ?? "",
                grado = school?.degree ?? "",
                escuelaEstado = school?.state ?? "",
                escuelaCiudad = school?.city ?? "",
                promedio = school?.average,
                fechaInicioEsc = school?.start_date,
                fechaFinEsc = school?.end_date,
                sistemaEstudio = school?.study_system ?? "",
                tipoBachillerato = school?.high_school_type ?? "",

                // --- Info adicional ---
                beca = info?.beca ?? "",
                comunidadIndi = info?.comu_indi ?? false,
                lenguaIndi = info?.lengu_indi ?? false,
                discapacidad = info?.incapa ?? false,
                enfermedad = info?.disease ?? false,
                comentario = info?.comment ?? ""
            };

            return Json(result);
        }

        // GET: Enrollment/PreEnrollment/GetMatriculas
        [AllowAnonymous]
        [HttpGet]
        [EnableCors("AllowAdmin")]
        public async Task<IActionResult> GetMatriculas()
        {
            var matriculas = await _context.PreenrollmentGenerals
                .Include(g => g.Person)
                .Where(g =>
                    !string.IsNullOrEmpty(g.Matricula) &&
                    g.Person != null &&
                    g.Person.IsActive == true   // 🔥 FILTRO CLAVE
                )
                .Select(g => new
                {
                    g.IdData,
                    g.Folio,
                    g.Matricula,
                    NombreCompleto = g.Person != null
                        ? $"{g.Person.FirstName} {g.Person.LastNamePaternal} {g.Person.LastNameMaternal}"
                        : "Sin nombre",
                    Email = g.Person != null ? g.Person.Email : "",
                    IsActive = g.Person != null ? g.Person.IsActive : false
                })
                .OrderBy(g => g.Matricula)
                .ToListAsync();

            return Json(matriculas);
        }


        [AllowAnonymous]
        [HttpPut]
        [EnableCors("AllowAdmin")]
        public async Task<IActionResult> UpdateAspirante([FromBody] UpdateAspiranteDto dto)
        {
            var general = await _context.PreenrollmentGenerals
                .Include(g => g.Person)
                .Include(g => g.Addresses)
                .Include(g => g.Schools)
                .Include(g => g.Infos)
                .FirstOrDefaultAsync(g => g.IdData == dto.IdData);

            if (general == null)
                return NotFound(new { message = "Aspirante no encontrado" });

            var address = general.Addresses.FirstOrDefault();
            var school = general.Schools.FirstOrDefault();
            var info = general.Infos.FirstOrDefault();

            // ========================
            // PERSON
            // ========================
            if (general.Person != null)
            {
                general.Person.FirstName = dto.Nombre;
                general.Person.LastNamePaternal = dto.ApellidoPaterno;
                general.Person.LastNameMaternal = dto.ApellidoMaterno;
                general.Person.Email = dto.Email;
                general.Person.Phone = dto.Telefono;
                general.Person.Gender = dto.Genero;
                general.Person.Curp = dto.Curp;
            }

            // ========================
            // GENERAL
            // ========================
            general.Nationality = dto.Nacionalidad;
            general.MaritalStatus = dto.EstadoCivil;
            general.BloodType = dto.TipoSangre;

            // ========================
            // ADDRESS
            // ========================
            if (address != null)
            {
                address.street = dto.Calle;
                address.exterior_number = dto.NumExt;
                address.interior_number = dto.NumInt;
                address.neighborhood = dto.Colonia;
                address.city = dto.Ciudad;
                address.state = dto.Estado;
                address.postal_code = dto.CP;
            }

            // ========================
            // SCHOOL
            // ========================
            if (school != null)
            {
                school.school = dto.Escuela;
                school.average = dto.Promedio;
                school.city = dto.EscuelaCiudad;
                school.state = dto.EscuelaEstado;
            }

            // ========================
            // INFO
            // ========================
            if (info != null)
            {
                info.beca = dto.Beca;
                info.comu_indi = dto.ComunidadIndi;
                info.lengu_indi = dto.LenguaIndi;
                info.incapa = dto.Discapacidad;
                info.disease = dto.Enfermedad;
                info.comment = dto.Comentario;
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        public class UpdateAspiranteDto
        {
            public int IdData { get; set; }

            // PERSON
            public string Nombre { get; set; }
            public string ApellidoPaterno { get; set; }
            public string ApellidoMaterno { get; set; }
            public string Email { get; set; }
            public string Telefono { get; set; }
            public string Genero { get; set; }
            public string Curp { get; set; }

            // GENERAL
            public string Nacionalidad { get; set; }
            public string EstadoCivil { get; set; }
            public string TipoSangre { get; set; }

            // ADDRESS
            public string Calle { get; set; }
            public string NumExt { get; set; }
            public string NumInt { get; set; }
            public string Colonia { get; set; }
            public string Ciudad { get; set; }
            public string Estado { get; set; }
            public string CP { get; set; }

            // SCHOOL
            public string Escuela { get; set; }
            public decimal? Promedio { get; set; }
            public string EscuelaCiudad { get; set; }
            public string EscuelaEstado { get; set; }

            // INFO
            public string Beca { get; set; }
            public bool ComunidadIndi { get; set; }
            public bool LenguaIndi { get; set; }
            public bool Discapacidad { get; set; }
            public bool Enfermedad { get; set; }
            public string Comentario { get; set; }
        }
    }
}