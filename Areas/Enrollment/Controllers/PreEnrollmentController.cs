using System.Security.Cryptography;
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
        public IActionResult Create()
        {
            ViewData["PasoActual"] = 1;

            ViewData["IdCareer"] = new SelectList(
                _context.PreenrollmentCareers, "IdCareer", "name_career");
            ViewData["IdGeneration"] = new SelectList(
                _context.PreenrollmentGenerations, "IdGeneration", "Year");
            return View();
        }

        // POST: Enrollment/PreEnrollment/Create
        // Recibe un ViewModel con los datos del formulario de inscripción.
        // Guarda en preenrollment_general y tablas relacionadas, genera folio y matrícula.
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
        public IActionResult ValidateFolio()
        {
            return View();
        }

        // POST: Enrollment/PreEnrollment/ValidateFolio
        // El usuario escribe su folio. Se verifica que exista y esté aprobado.
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

            // Si ya tiene cuenta creada, no permitir duplicar
            if (pre.UserId != null)
            {
                ModelState.AddModelError("", "Este folio ya tiene una cuenta asociada.");
                return View();
            }

            // Todo bien: redirigir a completar cuenta
            return RedirectToAction(nameof(CreateAccount), new { id = pre.IdData });
        }

        // GET: Enrollment/PreEnrollment/CreateAccount/5
        // Muestra el formulario para que el usuario establezca su contraseña
        // y confirme sus datos. Se pre-llenan con los datos del preenrollment.
        public async Task<IActionResult> CreateAccount(int id)
        {
            var pre = await _context.PreenrollmentGenerals
                .Include(p => p.ProcedureRequest)
                    .ThenInclude(pr => pr.ProcedureFlow)
                        .ThenInclude(pf => pf.ProcedureStatus)
                .FirstOrDefaultAsync(p => p.IdData == id);

            if (pre == null)
                return NotFound();

            // Doble verificación: no se puede acceder directo por URL sin aprobación
            if (pre.ProcedureRequest?.ProcedureFlow?.ProcedureStatus?.InternalCode != "APPROVED")
                return RedirectToAction(nameof(ValidateFolio));

            if (pre.UserId != null)
                return RedirectToAction(nameof(ValidateFolio));

            // Pasar los datos precargados a la vista para mostrarlos al usuario
            var vm = new CompleteRegistrationViewModel
            {
                IdData = pre.IdData,
                Matricula = pre.Matricula,
                Folio = pre.Folio
            };

            return View(vm);
        }

        // POST: Enrollment/PreEnrollment/CreateAccount
        // Crea el users_person y users_user con los datos del preenrollment.
        // El usuario solo elige su contraseña en este paso.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAccount(CompleteRegistrationViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var pre = await _context.PreenrollmentGenerals
                .Include(p => p.ProcedureRequest)
                    .ThenInclude(pr => pr.ProcedureFlow)
                        .ThenInclude(pf => pf.ProcedureStatus)
                .FirstOrDefaultAsync(p => p.IdData == vm.IdData);

            if (pre == null)
                return NotFound();

            // Triple verificación de seguridad
            if (pre.ProcedureRequest?.ProcedureFlow?.ProcedureStatus?.InternalCode != "APPROVED")
                return RedirectToAction(nameof(ValidateFolio));

            if (pre.UserId != null)
                return RedirectToAction(nameof(ValidateFolio));

            // Crear users_person (los datos de nombre/apellidos
            // vienen del ViewModel que el usuario confirmó)
            var person = new users_person
            {
                // Llenar con los campos que users_person tenga definidos
                // según el modelo de tu compañero
            };

            _context.Persons.Add(person);
            await _context.SaveChangesAsync();

            // Crear users_user
            var user = new users_user
            {
                PersonId = person.PersonId,
                Username = pre.Matricula,
                Email = vm.Email,
                IsActive = true,
                CreatedDate = DateTime.Now,
                // Aquí aplica el hash de la contraseña que defina tu compañero
                // PasswordHash = HashPassword(vm.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Ligar el usuario creado al preenrollment
            pre.UserId = user.UserId;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Success));
        }

        // GET: Enrollment/PreEnrollment/Success
        public IActionResult Success()
        {
            return View();
        }

        // =====================================================================
        // CRUD ADMIN (Index, Details, Edit, Delete)
        // Para que el admin pueda gestionar las preinscripciones.
        // =====================================================================


        // =====================================================================
        // Documentos
        // =====================================================================

        // GET: Enrollment/PreEnrollment/SubirDocumentos
        [HttpGet]
        public IActionResult SubirDocumentos()
        {
            ViewData["PasoActual"] = 2;
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
    }
}