using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Models;

namespace SchoolManager.Areas.MainScreen.Controllers
{
    [Area("MainScreen")]
    public class ForoController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ForoController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // LISTADO ADMIN
        public async Task<IActionResult> Index()
        {
            try
            {
                var ahora = DateTime.Now;

                var expiradas = await _context.ForoPublicaciones
                    .Where(f => f.FechaFin != null && f.FechaFin < ahora && f.Activo)
                    .ToListAsync();

                if (expiradas.Any())
                {
                    foreach (var item in expiradas)
                    {
                        item.Activo = false;
                        item.UpdatedDate = ahora;
                    }

                    await _context.SaveChangesAsync();
                }

                var publicaciones = await _context.ForoPublicaciones
                    .Include(f => f.Usuario)
                    .Include(f => f.Imagenes)
                    .OrderByDescending(f => f.CreatedDate)
                    .ToListAsync();

                ViewBag.Total = publicaciones.Count;

                return View("~/Areas/MainScreen/Views/MainScreen/Foro.cshtml", publicaciones);
            }
            catch (Exception ex)
            {
                ViewBag.Total = 0;
                ViewBag.Errores = new List<string>
                {
                    "Ocurrió un error al cargar las publicaciones.",
                    ex.Message
                };

                return View("~/Areas/MainScreen/Views/MainScreen/Foro.cshtml", new List<ForoPublicacion>());
            }
        }

        // FORO PÚBLICO
        [AllowAnonymous]
        public async Task<IActionResult> ForoPublico()
        {
            var ahora = DateTime.Now;

            var publicaciones = await _context.ForoPublicaciones
                .Include(f => f.Imagenes)
                .Where(f =>
                    f.Activo &&
                    f.FechaInicio <= ahora &&
                    (f.FechaFin == null || f.FechaFin >= ahora))
                .OrderByDescending(f => f.CreatedDate)
                .ToListAsync();

            return View("~/Areas/MainScreen/Views/MainScreen/ForoPublico.cshtml", publicaciones);
        }

        // CREAR - GET
        public IActionResult Create()
        {
            var model = new ForoPublicacion
            {
                EstiloVisual = "clasico",
                Activo = true,
                FechaInicio = DateTime.Now
            };

            return View("~/Areas/MainScreen/Views/MainScreen/ForoCreate.cshtml", model);
        }

        // CREAR - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ForoPublicacion model, IFormFile? imagenArchivo)
        {
            try
            {
                ModelState.Remove("Usuario");
                ModelState.Remove("Imagenes");

                if (!ModelState.IsValid)
                {
                    return View("~/Areas/MainScreen/Views/MainScreen/ForoCreate.cshtml", model);
                }

                var usuarioExistente = await _context.Users.FirstOrDefaultAsync();
                if (usuarioExistente == null)
                {
                    ModelState.AddModelError("", "No existe ningún usuario en la tabla users_user.");
                    return View("~/Areas/MainScreen/Views/MainScreen/ForoCreate.cshtml", model);
                }

                model.UsuarioId = usuarioExistente.UserId;
                model.CreatedDate = DateTime.Now;
                model.UpdatedDate = null;

                _context.ForoPublicaciones.Add(model);
                await _context.SaveChangesAsync();

                if (imagenArchivo != null && imagenArchivo.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "foro");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string fileName = Guid.NewGuid() + Path.GetExtension(imagenArchivo.FileName);
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imagenArchivo.CopyToAsync(stream);
                    }

                    var imagen = new ForoImagen
                    {
                        PublicacionId = model.PublicacionId,
                        UrlImagen = "/uploads/foro/" + fileName,
                        CreatedDate = DateTime.Now
                    };

                    _context.ForoImagenes.Add(imagen);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al guardar: " + ex.Message);
                return View("~/Areas/MainScreen/Views/MainScreen/ForoCreate.cshtml", model);
            }
        }

        // DETALLE
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var publicacion = await _context.ForoPublicaciones
                .Include(f => f.Usuario)
                .Include(f => f.Imagenes)
                .FirstOrDefaultAsync(f => f.PublicacionId == id);

            if (publicacion == null) return NotFound();

            return View("~/Areas/MainScreen/Views/MainScreen/ForoDetails.cshtml", publicacion);
        }

        // EDITAR - GET
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var publicacion = await _context.ForoPublicaciones
                .Include(f => f.Imagenes)
                .FirstOrDefaultAsync(f => f.PublicacionId == id);

            if (publicacion == null) return NotFound();

            return View("~/Areas/MainScreen/Views/MainScreen/ForoEdit.cshtml", publicacion);
        }

        // EDITAR - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ForoPublicacion model, IFormFile? imagenArchivo)
        {
            if (id != model.PublicacionId) return NotFound();

            ModelState.Remove("Usuario");
            ModelState.Remove("Imagenes");

            if (!ModelState.IsValid)
            {
                return View("~/Areas/MainScreen/Views/MainScreen/ForoEdit.cshtml", model);
            }

            var publicacionDb = await _context.ForoPublicaciones
                .Include(f => f.Imagenes)
                .FirstOrDefaultAsync(f => f.PublicacionId == id);

            if (publicacionDb == null) return NotFound();

            publicacionDb.Titulo = model.Titulo;
            publicacionDb.Descripcion = model.Descripcion;
            publicacionDb.EstiloVisual = model.EstiloVisual;
            publicacionDb.LinkExterno = model.LinkExterno;
            publicacionDb.FechaInicio = model.FechaInicio;
            publicacionDb.FechaFin = model.FechaFin;
            publicacionDb.Activo = model.Activo;
            publicacionDb.UpdatedDate = DateTime.Now;

            if (imagenArchivo != null && imagenArchivo.Length > 0)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "foro");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string fileName = Guid.NewGuid() + Path.GetExtension(imagenArchivo.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imagenArchivo.CopyToAsync(stream);
                }

                var imagenExistente = publicacionDb.Imagenes?.FirstOrDefault();

                if (imagenExistente != null)
                {
                    imagenExistente.UrlImagen = "/uploads/foro/" + fileName;
                }
                else
                {
                    _context.ForoImagenes.Add(new ForoImagen
                    {
                        PublicacionId = publicacionDb.PublicacionId,
                        UrlImagen = "/uploads/foro/" + fileName,
                        CreatedDate = DateTime.Now
                    });
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ELIMINAR - GET
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var publicacion = await _context.ForoPublicaciones
                .Include(f => f.Imagenes)
                .FirstOrDefaultAsync(f => f.PublicacionId == id);

            if (publicacion == null) return NotFound();

            return View("~/Areas/MainScreen/Views/MainScreen/ForoDelete.cshtml", publicacion);
        }

        // ELIMINAR - POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var publicacion = await _context.ForoPublicaciones
                .Include(f => f.Imagenes)
                .FirstOrDefaultAsync(f => f.PublicacionId == id);

            if (publicacion != null)
            {
                _context.ForoPublicaciones.Remove(publicacion);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}