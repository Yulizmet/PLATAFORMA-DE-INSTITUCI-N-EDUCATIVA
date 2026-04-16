using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Areas.Procedures.Models;
using SchoolManager.Data;

namespace SchoolManager.Areas.Procedures.Controllers
{
    [Area("Procedures")]
    public class PermissionsController : _ProceduresBaseController
    {
        public PermissionsController(AppDbContext context) : base(context) { }

        public async Task<IActionResult> Index(int? idArea)
        {
            await LoadPermissions("Permisos");
            var areasList = await _context.ProcedureAreas.OrderBy(a => a.Name).ToListAsync();
            ViewBag.Areas = new SelectList(areasList, "Id", "Name", idArea);
            ViewBag.SelectedAreaId = idArea;

            ViewBag.JobCatalog = await _context.ProcedureJobPosition.OrderBy(j => j.Name).ToListAsync();

            if (idArea.HasValue)
            {
                var permissions = await _context.ProcedurePermissions
                    .Include(p => p.JobPosition)
                    .Include(p => p.ModuleCatalog)
                    .Where(p => p.IdArea == idArea.Value)
                    .OrderBy(p => p.IdJobPosition)
                    .ThenBy(p => p.ModuleCatalog!.ModuleName)
                    .ToListAsync();

                return View(permissions);
            }

            return View(new List<procedure_permission>());
        }

        [HttpPost]
        public async Task<IActionResult> InitJobPermissions(int idArea, int? idJobPosition = null)
        {
            await LoadPermissions("Permisos");
            try
            {
                if (idArea <= 0) return Json(new { success = false, message = "Área no válida." });

                int targetJobId = 0;

                if (idJobPosition == null || idJobPosition <= 0)
                {
                    var adminJob = await _context.ProcedureJobPosition
                        .FirstOrDefaultAsync(j => j.Name.ToLower() == "administrador");

                    if (adminJob == null)
                        return Json(new { success = false, message = "No se encontró el puesto 'Administrador'." });

                    targetJobId = adminJob.Id;
                }
                else
                {
                    targetJobId = idJobPosition.Value;
                }

                var moduleCatalog = await _context.ProcedureModuleCatalog.ToListAsync();

                if (!moduleCatalog.Any())
                    return Json(new { success = false, message = "El catálogo de módulos está vacío." });

                var jobInfo = await _context.ProcedureJobPosition.FindAsync(targetJobId);
                bool isFullAccess = jobInfo?.Name.ToLower() == "administrador";

                int nuevosRegistros = 0;

                foreach (var item in moduleCatalog)
                {
                    var exists = await _context.ProcedurePermissions.AnyAsync(p =>
                        p.IdArea == idArea &&
                        p.IdJobPosition == targetJobId &&
                        p.IdModuleCatalog == item.Id);

                    if (!exists)
                    {
                        _context.ProcedurePermissions.Add(new procedure_permission
                        {
                            IdArea = idArea,
                            IdJobPosition = targetJobId,
                            IdModuleCatalog = item.Id,
                            CanView = isFullAccess
                        });
                        nuevosRegistros++;
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"Se generaron {nuevosRegistros} reglas de acceso para {jobInfo?.Name}."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> InitModules(int idArea)
        {
            await LoadPermissions("Permisos");
            try
            {
                if (idArea <= 0) return Json(new { success = false, message = "Área no válida." });

                var adminJob = await _context.ProcedureJobPosition
                    .FirstOrDefaultAsync(j => j.Name.ToLower() == "administrador");

                if (adminJob == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No se encontró un puesto llamado 'Administrador' en el catálogo de puestos."
                    });
                }

                var catalogItems = await _context.ProcedureModuleCatalog.ToListAsync();

                if (!catalogItems.Any())
                {
                    return Json(new { success = false, message = "El catálogo de módulos está vacío." });
                }

                int nuevosRegistros = 0;

                foreach (var item in catalogItems)
                {
                    bool exists = await _context.ProcedurePermissions.AnyAsync(p =>
                        p.IdArea == idArea &&
                        p.IdJobPosition == adminJob.Id &&
                        p.IdModuleCatalog == item.Id);

                    if (!exists)
                    {
                        _context.ProcedurePermissions.Add(new procedure_permission
                        {
                            IdArea = idArea,
                            IdJobPosition = adminJob.Id,
                            IdModuleCatalog = item.Id,
                            CanView = true
                        });
                        nuevosRegistros++;
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"Se han generado {nuevosRegistros} reglas de acceso para el perfil Administrador."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al inicializar: " + ex.Message });
            }
        }

        //GET MODALS
        [HttpGet]
        public async Task<IActionResult> GetDeleteModal(int id, string type)
        {
            await LoadPermissions("Permisos");

            ViewBag.ItemId = id;
            ViewBag.IsModuleGroup = false;

            string normalizedType = type?.Trim().ToLower() ?? "";

            if (normalizedType == "module")
            {
                var item = await _context.ProcedureModuleCatalog.FindAsync(id);
                ViewBag.ItemName = item?.ModuleName ?? "Módulo Desconocido";
                ViewBag.ActionUrl = Url.Action("DeleteCatalogModule", "Permissions");
                ViewBag.IsModuleGroup = true;
                ViewBag.ItemType = "módulo";
            }
            else if (normalizedType == "button")
            {
                var item = await _context.ProcedureModuleCatalog.FindAsync(id);
                ViewBag.ItemName = item?.ButtonName ?? "Botón Desconocido";
                ViewBag.ActionUrl = Url.Action("DeleteCatalogItem", "Permissions");
                ViewBag.ItemType = "botón";
            }
            else
            {
                var item = await _context.ProcedureJobPosition.FindAsync(id);
                ViewBag.ItemName = item?.Name ?? "Puesto Desconocido";
                ViewBag.ActionUrl = Url.Action("DeleteJobPosition", "Permissions");
                ViewBag.ItemType = "puesto de trabajo";
            }

            return PartialView("_DeleteModal");
        }

        [HttpGet]
        public async Task<IActionResult> GetModuleModal(int id, string? moduleName = null)
        {
            await LoadPermissions("Permisos");
            procedure_module_catalog? model;

            if (id > 0)
            {
                model = await _context.ProcedureModuleCatalog.FindAsync(id);
                if (model != null && string.IsNullOrEmpty(model.ModuleName))
                {
                    model.ModuleName = "Módulo sin nombre";
                }
            }
            else
            {
                model = new procedure_module_catalog
                {
                    ModuleName = moduleName ?? "",
                    ButtonName = ""
                };
            }

            if (model == null) return NotFound();
            return PartialView("Modules/_ModuleModal", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetCatalogTable()
        {
            await LoadPermissions("Permisos");
            var catalog = await _context.ProcedureModuleCatalog
                .OrderBy(m => m.ModuleName)
                .ThenBy(m => m.ButtonName)
                .ToListAsync();
            return PartialView("_CatalogTable", catalog);
        }

        [HttpGet]
        public async Task<IActionResult> GetJobsTable()
        {
            await LoadPermissions("Permisos");
            var jobs = await _context.ProcedureJobPosition
                .OrderBy(j => j.Name)
                .ToListAsync();
            return PartialView("_JobsTable", jobs);
        }

        [HttpGet]
        public async Task<IActionResult> GetJobModal(int id)
        {
            await LoadPermissions("Permisos");
            var model = id > 0
                ? await _context.ProcedureJobPosition.FindAsync(id)
                : new procedure_job_position();

            return PartialView("JobPosition/_AddJobModal", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetAssignJobModal(int idArea)
        {
            await LoadPermissions("Permisos");
            ViewBag.IdArea = idArea;
            var existingJobIds = await _context.ProcedurePermissions
                .Where(p => p.IdArea == idArea)
                .Select(p => p.IdJobPosition)
                .Distinct()
                .ToListAsync();

            ViewBag.Jobs = await _context.ProcedureJobPosition
                .Where(j => !existingJobIds.Contains(j.Id))
                .ToListAsync();

            return PartialView("JobPosition/_AssignJobModal");
        }

        // SAVINGS
        [HttpPost]
        public async Task<IActionResult> SyncCatalog(int idArea)
        {
            await LoadPermissions("Permisos");
            var currentJobs = await _context.ProcedurePermissions
                .Where(p => p.IdArea == idArea)
                .Select(p => p.IdJobPosition)
                .Distinct()
                .ToListAsync();

            foreach (var jobId in currentJobs)
            {
                await InitJobPermissions(idArea, jobId);
            }

            return RedirectToAction(nameof(Index), new { idArea });
        }

        [HttpPost]
        public async Task<IActionResult> SaveModuleToCatalog(procedure_module_catalog model)
        {
            await LoadPermissions("Permisos");
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                bool isNew = model.Id == 0;

                if (!isNew)
                {
                    var dbEntry = await _context.ProcedureModuleCatalog.FindAsync(model.Id);
                    if (dbEntry == null) return Json(new { success = false, message = "No existe el registro." });

                    string oldModuleName = dbEntry.ModuleName;

                    if (string.IsNullOrEmpty(dbEntry.ButtonName) && oldModuleName != model.ModuleName)
                    {
                        var children = await _context.ProcedureModuleCatalog
                            .Where(m => m.ModuleName == oldModuleName && m.Id != model.Id)
                            .ToListAsync();

                        foreach (var child in children)
                        {
                            child.ModuleName = model.ModuleName;
                        }
                    }

                    dbEntry.ModuleName = model.ModuleName;
                    dbEntry.ButtonName = model.ButtonName;
                    dbEntry.Route = model.Route;

                    _context.ProcedureModuleCatalog.Update(dbEntry);
                }
                else
                {
                    _context.ProcedureModuleCatalog.Add(model);
                }

                await _context.SaveChangesAsync();

                if (isNew)
                {
                    var existingCombinations = await _context.ProcedurePermissions
                        .Select(p => new { p.IdArea, p.IdJobPosition })
                        .Distinct()
                        .ToListAsync();

                    foreach (var combo in existingCombinations)
                    {
                        _context.ProcedurePermissions.Add(new procedure_permission
                        {
                            IdArea = combo.IdArea,
                            IdJobPosition = combo.IdJobPosition,
                            IdModuleCatalog = model.Id,
                            CanView = false
                        });
                    }

                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return Json(new { success = true, message = isNew ? "Módulo inyectado en todos los perfiles correctamente." : "Cambios guardados." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Error: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveJobPosition(procedure_job_position model)
        {
            await LoadPermissions("Permisos");
            if (model.Id > 0) _context.Update(model);
            else _context.Add(model);

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Puesto guardado con éxito." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePermissions(List<procedure_permission> permissions)
        {
            await LoadPermissions("Permisos");
            try
            {
                foreach (var item in permissions)
                {
                    var existing = await _context.ProcedurePermissions
                        .FirstOrDefaultAsync(p => p.Id == item.Id);

                    if (existing != null)
                    {
                        existing.CanView = item.CanView;
                        _context.ProcedurePermissions.Update(existing);
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Matriz actualizada correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        //DELETIONS

        [HttpPost]
        public async Task<IActionResult> DeleteCatalogItem(int id)
        {
            await LoadPermissions("Permisos");
            var item = await _context.ProcedureModuleCatalog.FindAsync(id);
            if (item == null) return Json(new { success = false });

            _context.ProcedureModuleCatalog.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCatalogModule(string moduleName)
        {
            await LoadPermissions("Permisos");
            var items = await _context.ProcedureModuleCatalog.Where(m => m.ModuleName == moduleName).ToListAsync();
            _context.ProcedureModuleCatalog.RemoveRange(items);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteJobPosition(int id)
        {
            await LoadPermissions("Permisos");
            try
            {
                var job = await _context.ProcedureJobPosition.FindAsync(id);
                if (job == null) return Json(new { success = false, message = "Puesto no encontrado." });

                _context.ProcedureJobPosition.Remove(job);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Puesto eliminado correctamente." });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Json(new { success = false, message = "No se puede eliminar: el puesto tiene permisos asignados en áreas." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> BulkDeleteJobs(List<int> ids)
        {
            await LoadPermissions("Permisos");

            if (ids == null || !ids.Any())
            {
                return Json(new { success = false, message = "No se seleccionaron registros." });
            }

            try
            {
                var jobs = await _context.ProcedureJobPosition
                    .Where(j => ids.Contains(j.Id))
                    .ToListAsync();

                if (!jobs.Any())
                {
                    return Json(new { success = false, message = "Los puestos ya no existen." });
                }

                _context.ProcedureJobPosition.RemoveRange(jobs);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"{jobs.Count} puestos eliminados correctamente." });
            }
            catch (Exception)
            {
                return Json(new
                {
                    success = false,
                    message = "Uno o más puestos no se pueden eliminar porque tienen permisos asignados o están en uso administrativo."
                });
            }
        }
    }
}