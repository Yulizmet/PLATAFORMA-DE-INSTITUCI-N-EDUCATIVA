using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Models;
using System.Security.Cryptography;
using SchoolManager.Models.ViewModels;

namespace SchoolManager.Areas.Enrollment.Controllers
{
    [Area("Enrollment")]
    public class PreEnrollmentController : Controller
    {
        private readonly AppDbContext _context;

        public PreEnrollmentController(AppDbContext context)
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
            var generation = _context.Generations
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
        // FLUJO 1: REGISTRO INICIAL + GENERACIÓN DE FOLIO
        // El usuario (sin cuenta) llena sus datos y genera un folio de pago.
        // =====================================================================

        // GET: Enrollment/PreEnrollment/Create
        public IActionResult Create()
        {
            ViewData["IdCareer"] = new SelectList(
                _context.PreenrollmentCareers, "IdCareer", "name_career");
            ViewData["IdGeneration"] = new SelectList(
                _context.Generations, "IdGeneration", "Year");
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
                ViewData["IdCareer"] = new SelectList(
                    _context.PreenrollmentCareers, "IdCareer", "name_career", vm.IdCareer);
                ViewData["IdGeneration"] = new SelectList(
                    _context.Generations, "IdGeneration", "Year", vm.IdGeneration);
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
                phone = vm.Phone
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
            return RedirectToAction(nameof(FolioGenerado), new { id = general.IdData });
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

        // GET: Enrollment/PreEnrollment
        public async Task<IActionResult> Index()
        {
            var generales = await _context.PreenrollmentGenerals
                .Include(p => p.Career)
                .Include(p => p.Generation)
                .ToListAsync();
            return View(generales);
        }

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
                _context.Generations, "IdGeneration", "Year", general.IdGeneration);

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
                _context.Generations, "IdGeneration", "Year", general.IdGeneration);

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