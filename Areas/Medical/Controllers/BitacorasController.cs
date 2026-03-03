//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using PIBitacoras.Data;
//using PIBitacoras.Models;
//#nullable disable


//namespace PIBitacoras.Controllers
//{
//    public class BitacorasController : Controller
//    {
//        private readonly ApplicationDbContext _context;

//        public BitacorasController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        // LISTA GENERAL
//        // ================= LISTA =================
//        public async Task<IActionResult> Index()
//        {
//            var lista = await (
//                from b in _context.Bitacoras

//                join a in _context.Alumnos
//                    on b.IdAlumno equals a.Id

//                join pre in _context.PreenrollmentGeneral
//                    on a.PreenrollmentId equals pre.IdData

//                join per in _context.Personas
//                    on pre.UserId equals per.Id

//                orderby b.Id descending

//                select new BitacoraListadoVM
//                {
//                    Id = b.Id,
//                    Folio = b.Folio,
//                    Matricula = pre.Matricula,
//                    NombreCompleto = per.Nombre + " " +
//                                     per.ApellidoPaterno + " " +
//                                     per.ApellidoMaterno,
//                    Motivo = b.MotivoConsulta,
//                    Estado = b.Estado,
//                    Fecha = b.FechaHora
//                }
//            ).ToListAsync();

//            return View(lista);
//        }

//        // DETAILS
//        public async Task<IActionResult> Details(int id)
//        {
//            var bitacora = await (
//                from b in _context.Bitacoras

//                join a in _context.Alumnos
//                    on b.IdAlumno equals a.Id

//                join pre in _context.PreenrollmentGeneral
//                    on a.PreenrollmentId equals pre.IdData

//                join per in _context.Personas
//                    on pre.UserId equals per.Id

//                where b.Id == id

//                select new Bitacora
//                {
//                    Id = b.Id,
//                    Folio = b.Folio,
//                    FechaHora = b.FechaHora,
//                    Estado = b.Estado,
//                    MotivoConsulta = b.MotivoConsulta,
//                    SignosVitales = b.SignosVitales,
//                    Observaciones = b.Observaciones,
//                    Tratamiento = b.Tratamiento,

//                    // Datos visuales
//                    MatriculaTemp = pre.Matricula,
//                    NombreCompletoTemp = per.Nombre + " " + per.ApellidoPaterno + " " + per.ApellidoMaterno
//                }

//            ).FirstOrDefaultAsync();

//            if (bitacora == null)
//                return NotFound();

//            return View(bitacora);
//        }

//        // ================= EDIT GET =================
//        public async Task<IActionResult> Edit(int id)
//        {
//            var bitacora = await _context.Bitacoras.FindAsync(id);
//            if (bitacora == null) return NotFound();

//            return View(bitacora);
//        }

//        // ================= EDIT POST =================
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, Bitacora bitacora)
//        {
//            if (id != bitacora.Id) return NotFound();

//            if (ModelState.IsValid)
//            {
//                _context.Update(bitacora);
//                await _context.SaveChangesAsync();
//                return RedirectToAction(nameof(Index));
//            }

//            return View(bitacora);
//        }

//        // CREATE GET
//        public IActionResult Create()
//        {
//            return View();
//        }

//        // CREATE POST
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(Bitacora bitacora)
//        {
//            if (!ModelState.IsValid)
//            {
//                return View(bitacora);
//            }

//            // validar alumno
//            if (bitacora.IdAlumno == 0)
//            {
//                ModelState.AddModelError("", "Debe buscar un alumno válido con la matrícula");
//                return View(bitacora);
//            }

//            // GENERAR FOLIO AUTOMATICO
//            var ultimoFol = await _context.Bitacoras
//                .OrderByDescending(x => x.Id)
//                .Select(x => x.Folio)
//                .FirstOrDefaultAsync();

//            int nuevoNumero = 1;

//            if (!string.IsNullOrEmpty(ultimoFol))
//            {
//                int.TryParse(ultimoFol, out nuevoNumero);
//                nuevoNumero++;
//            }

//            bitacora.Folio = nuevoNumero.ToString("D6");

//            // fechas automáticas
//            bitacora.FechaHora = DateTime.Now;
//            bitacora.CreatedAt = DateTime.Now;

//            // NO usar staff por ahora
//            bitacora.IdPersonal = null;

//            _context.Bitacoras.Add(bitacora);
//            await _context.SaveChangesAsync();

//            return RedirectToAction(nameof(Index));
//        }

//        // DELETE GET
//        public async Task<IActionResult> Delete(int id)
//        {
//            var bitacora = await _context.Bitacoras.FindAsync(id);
//            if (bitacora == null)
//                return NotFound();

//            return View(bitacora);
//        }

//        // DELETE POST
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            var bitacora = await _context.Bitacoras.FindAsync(id);

//            if (bitacora != null)
//            {
//                _context.Bitacoras.Remove(bitacora);
//                await _context.SaveChangesAsync();
//            }

//            return RedirectToAction(nameof(Index));
//        }

//        // BUSCAR ALUMNO POR MATRICULA (AJAX)
//        [HttpGet]
//        public async Task<IActionResult> BuscarPorMatricula(string matricula)
//        {
//            if (string.IsNullOrEmpty(matricula))
//                return Json(null);

//            var data = await (
//                from m in _context.Alumnos

//                join pre in _context.PreenrollmentGeneral
//                    on m.PreenrollmentId equals pre.IdData

//                join per in _context.Personas
//                    on pre.UserId equals per.Id

//                where pre.Matricula == matricula

//                select new
//                {
//                    alumnoId = m.Id,
//                    nombre = per.Nombre,
//                    paterno = per.ApellidoPaterno,
//                    materno = per.ApellidoMaterno,
//                    sangre = pre.BloodType,
//                    peso = m.Peso,
//                    alergias = m.Alergias,
//                    condiciones = m.CondicionesCronicas
//                }
//            ).FirstOrDefaultAsync();

//            if (data == null)
//                return Json(null);

//            return Json(data);
//        }
//    }
//}