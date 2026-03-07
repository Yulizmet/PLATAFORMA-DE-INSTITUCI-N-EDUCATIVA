//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using PIBitacoras.Data;
//using PIBitacoras.Models;
//#nullable disable


//namespace PIBitacoras.Controllers
//{
//    public class AlumnosController : Controller
//    {
//        private readonly ApplicationDbContext _context;

//        public AlumnosController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        // LISTA GENERAL DE ALUMNOS
//        public async Task<IActionResult> Index()
//        {
//            var alumnos = await (
//                from m in _context.Alumnos

//                join pre in _context.PreenrollmentGeneral
//                    on m.PreenrollmentId equals pre.IdData

//                join per in _context.Personas
//                    on pre.UserId equals per.Id

//                select new AlumnoListadoVM
//                {
//                    Id = m.Id,
//                    Matricula = pre.Matricula,
//                    NombreCompleto = per.Nombre + " " +
//                                     per.ApellidoPaterno + " " +
//                                     per.ApellidoMaterno,
//                    FechaCreacion = m.FechaCreacion   
//                }
//            ).ToListAsync();

//            return View(alumnos);
//        }

//        // CREATE GET
//        public IActionResult Create()
//        {
//            return View();
//        }

//        // BUSCAR POR MATRICULA (AJAX)
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
//                    nombre = per.Nombre,
//                    paterno = per.ApellidoPaterno,
//                    materno = per.ApellidoMaterno,
//                    sangre = pre.BloodType,
//                    preenrollmentId = pre.IdData
//                }
//            ).FirstOrDefaultAsync();

//            if (data == null)
//                return Json(null);

//            return Json(data);
//        }

//        // CREATE POST
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(Alumno alumno)
//        {
//            if (alumno.PreenrollmentId == 0)
//            {
//                ModelState.AddModelError("", "Debe buscar una matrícula válida");
//                return View(alumno);
//            }

//            bool existe = await _context.Alumnos
//                .AnyAsync(a => a.PreenrollmentId == alumno.PreenrollmentId);

//            if (existe)
//            {
//                ModelState.AddModelError("", "Este alumno ya tiene registro médico");
//                return View(alumno);
//            }

//            alumno.FechaCreacion = DateTime.Now;

//            _context.Alumnos.Add(alumno);
//            await _context.SaveChangesAsync();

//            return RedirectToAction(nameof(Index));
//        }

//        // DETAILS
//        public async Task<IActionResult> Details(int id)
//        {
//            var alumno = await (
//                from m in _context.Alumnos

//                join pre in _context.PreenrollmentGeneral
//                    on m.PreenrollmentId equals pre.IdData

//                join per in _context.Personas
//                    on pre.UserId equals per.Id

//                where m.Id == id

//                select new AlumnoDetalleVM
//                {
//                    Id = m.Id,
//                    Matricula = pre.Matricula,
//                    Nombre = per.Nombre,
//                    Paterno = per.ApellidoPaterno,
//                    Materno = per.ApellidoMaterno,
//                    Sangre = pre.BloodType,
//                    Peso = m.Peso,
//                    Alergias = m.Alergias,
//                    CondicionesCronicas = m.CondicionesCronicas,
//                    FechaCreacion = m.FechaCreacion
//                }
//            ).FirstOrDefaultAsync();

//            if (alumno == null)
//                return NotFound();

//            return View(alumno);
//        }

//        // EDIT GET
//        public async Task<IActionResult> Edit(int id)
//        {
//            var alumno = await _context.Alumnos.FindAsync(id);
//            if (alumno == null)
//                return NotFound();

//            return View(alumno);
//        }

//        // EDIT POST
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, Alumno alumno)
//        {
//            if (id != alumno.Id)
//                return NotFound();

//            if (!ModelState.IsValid)
//                return View(alumno);

//            // Obtener registro original de la BD
//            var alumnoBD = await _context.Alumnos
//                .AsNoTracking()
//                .FirstOrDefaultAsync(a => a.Id == id);

//            if (alumnoBD == null)
//                return NotFound();

//            // MANTENER fecha original (evita error datetime)
//            alumno.FechaCreacion = alumnoBD.FechaCreacion;

//            try
//            {
//                _context.Update(alumno);
//                await _context.SaveChangesAsync();
//            }
//            catch (Exception)
//            {
//                throw;
//            }

//            return RedirectToAction(nameof(Index));
//        }

//        // DELETE GET
//        public async Task<IActionResult> Delete(int id)
//        {
//            var alumno = await _context.Alumnos.FindAsync(id);
//            if (alumno == null)
//                return NotFound();

//            return View(alumno);
//        }

//        // DELETE POST
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            var alumno = await _context.Alumnos.FindAsync(id);

//            if (alumno != null)
//            {
//                _context.Alumnos.Remove(alumno);
//                await _context.SaveChangesAsync();
//            }

//            return RedirectToAction(nameof(Index));
//        }
//    }
//}