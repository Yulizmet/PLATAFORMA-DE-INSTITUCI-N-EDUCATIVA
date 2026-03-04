//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using PIBitacoras.Data;
//using PIBitacoras.Models;
//using PIBitacoras.ViewModels;
//#nullable disable


//namespace PIBitacoras.Controllers
//{
//    public class PsicologiaController : Controller
//    {
//        private readonly ApplicationDbContext _context;

//        public PsicologiaController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        // ================= LISTA =================
//        public async Task<IActionResult> Index()
//        {
//            var lista = await (
//                from p in _context.Psicologia

//                join pre in _context.PreenrollmentGeneral
//                    on p.PreenrollmentId equals pre.IdData

//                join per in _context.Personas
//                    on pre.UserId equals per.Id

//                orderby p.AppointmentDatetime descending

//                select new PsicologiaListadoVM
//                {
//                    Id = p.Id,
//                    Folio = p.Fol,
//                    Matricula = pre.Matricula,
//                    NombreCompleto = per.Nombre + " " + per.ApellidoPaterno + " " + per.ApellidoMaterno,
//                    Asistencia = p.AttendanceStatus,
//                    Fecha = p.AppointmentDatetime
//                }

//            ).ToListAsync();

//            return View(lista);
//        }

//        // ================= DETAILS =================
//        public async Task<IActionResult> Details(int id)
//        {
//            var data = await (
//                from p in _context.Psicologia

//                join pre in _context.PreenrollmentGeneral
//                    on p.PreenrollmentId equals pre.IdData

//                join per in _context.Personas
//                    on pre.UserId equals per.Id

//                where p.Id == id

//                select new Psicologia
//                {
//                    Id = p.Id,
//                    Fol = p.Fol,
//                    AppointmentDatetime = p.AppointmentDatetime,
//                    AttendanceStatus = p.AttendanceStatus,
//                    PsychologyObservations = p.PsychologyObservations,
//                    PreenrollmentId = p.PreenrollmentId,

//                    // Datos visuales
//                    MatriculaTemp = pre.Matricula,
//                    NombreCompletoTemp = per.Nombre + " " + per.ApellidoPaterno + " " + per.ApellidoMaterno
//                }

//            ).FirstOrDefaultAsync();

//            if (data == null)
//                return NotFound();

//            return View(data);
//        }

//        // ================= CREATE GET =================
//        public IActionResult Create()
//        {
//            return View();
//        }

//        // ================= CREATE POST =================
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(Psicologia model)
//        {
//            if (ModelState.IsValid)
//            {
//                // ===== GENERAR FOLIO AUTOMÁTICO =====
//                var ultimoFol = await _context.Psicologia
//                    .OrderByDescending(x => x.Id)
//                    .Select(x => x.Fol)
//                    .FirstOrDefaultAsync();

//                int nuevo = 1;

//                if (!string.IsNullOrEmpty(ultimoFol))
//                {
//                    int.TryParse(ultimoFol, out nuevo);
//                    nuevo++;
//                }

//                model.Fol = nuevo.ToString("D6");

//                // ===== FECHAS AUTOMÁTICAS =====
//                model.AppointmentDatetime = DateTime.Now;
//                model.CreatedAt = DateTime.Now;

//                // Staff NULL por ahora
//                model.StaffId = null;

//                _context.Psicologia.Add(model);
//                await _context.SaveChangesAsync();

//                return RedirectToAction(nameof(Index));
//            }

//            return View(model);
//        }

//        // ================= EDIT GET =================
//        public async Task<IActionResult> Edit(int id)
//        {
//            var cita = await _context.Psicologia.FindAsync(id);
//            if (cita == null) return NotFound();

//            return View(cita);
//        }

//        // ================= EDIT POST =================
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, Psicologia model)
//        {
//            if (id != model.Id)
//                return NotFound();

//            if (ModelState.IsValid)
//            {
//                _context.Update(model);
//                await _context.SaveChangesAsync();
//                return RedirectToAction(nameof(Index));
//            }

//            return View(model);
//        }

//        // ================= DELETE GET =================
//        public async Task<IActionResult> Delete(int id)
//        {
//            var cita = await _context.Psicologia
//                .FirstOrDefaultAsync(x => x.Id == id);

//            if (cita == null)
//                return NotFound();

//            return View(cita);
//        }

//        // ================= DELETE POST =================
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            var cita = await _context.Psicologia.FindAsync(id);

//            if (cita != null)
//            {
//                _context.Psicologia.Remove(cita);
//                await _context.SaveChangesAsync();
//            }

//            return RedirectToAction(nameof(Index));
//        }

//        // ================= BUSCAR POR MATRICULA =================
//        [HttpGet]
//        public async Task<IActionResult> BuscarPorMatricula(string matricula)
//        {
//            var data = await (
//                from pre in _context.PreenrollmentGeneral
//                join per in _context.Personas
//                    on pre.UserId equals per.Id
//                where pre.Matricula == matricula

//                select new
//                {
//                    preId = pre.IdData,
//                    nombre = per.Nombre,
//                    paterno = per.ApellidoPaterno,
//                    materno = per.ApellidoMaterno
//                }
//            ).FirstOrDefaultAsync();

//            if (data == null)
//                return Json(null);

//            return Json(data);
//        }
//    }
//}