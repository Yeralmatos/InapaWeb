
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InapaWeb.Data;

namespace InapaWeb.Controllers
{
    public class TecnicoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        // Tamaño máximo permitido por imagen: 5 MB
        private const long TamanoMaximoImagen = 5 * 1024 * 1024;

        private readonly string[] _extensionesPermitidas =
        {
            ".jpg",
            ".jpeg",
            ".png"
        };

        public TecnicoController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // =========================================================
        // VALIDAR SESIÓN DEL TÉCNICO
        // =========================================================
        private bool TecnicoAutenticado()
        {
            string? rol =
                HttpContext.Session.GetString("RolUsuario");

            int? usuarioId =
                HttpContext.Session.GetInt32("UsuarioId");

            return rol == "Técnico" && usuarioId.HasValue;
        }

        // =========================================================
        // OBTENER ID DEL TÉCNICO AUTENTICADO
        // =========================================================
        private int ObtenerIdTecnico()
        {
            return HttpContext.Session
                .GetInt32("UsuarioId")!.Value;
        }

        // =========================================================
        // PANEL PRINCIPAL DEL TÉCNICO
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!TecnicoAutenticado())
            {
                return RedirectToAction("Login", "Acceso");
            }

            int idTecnico = ObtenerIdTecnico();

            ViewBag.NombreUsuario =
                HttpContext.Session.GetString("NombreUsuario")
                ?? "Técnico";

            ViewBag.TotalAsignados =
                await _context.AsignacionesTecnicos.CountAsync(a =>
                    a.IdTecnico == idTecnico &&
                    a.Estado == "Asignado");

            ViewBag.TotalEnProceso =
                await _context.AsignacionesTecnicos.CountAsync(a =>
                    a.IdTecnico == idTecnico &&
                    a.Estado == "En Proceso");

            ViewBag.TotalFinalizados =
                await _context.AsignacionesTecnicos.CountAsync(a =>
                    a.IdTecnico == idTecnico &&
                    a.Estado == "Finalizado");

            ViewBag.TrabajosRecientes =
                await _context.AsignacionesTecnicos
                    .Include(a => a.SolicitudServicio)
                        .ThenInclude(s => s.Cliente)
                            .ThenInclude(c => c.Usuario)
                    .Where(a => a.IdTecnico == idTecnico)
                    .OrderByDescending(a => a.FechaAsignacion)
                    .Take(5)
                    .ToListAsync();

            return View();
        }

        // =========================================================
        // LISTADO DE TRABAJOS DEL TÉCNICO
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> TrabajosAsignados(
            string? estado)
        {
            if (!TecnicoAutenticado())
            {
                return RedirectToAction("Login", "Acceso");
            }

            int idTecnico = ObtenerIdTecnico();

            ViewBag.NombreUsuario =
                HttpContext.Session.GetString("NombreUsuario")
                ?? "Técnico";

            var consulta =
                _context.AsignacionesTecnicos
                    .Include(a => a.SolicitudServicio)
                        .ThenInclude(s => s.Cliente)
                            .ThenInclude(c => c.Usuario)
                    .Where(a => a.IdTecnico == idTecnico)
                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado) &&
                estado != "Todos")
            {
                consulta = consulta.Where(a =>
                    a.Estado == estado);
            }

            ViewBag.EstadoSeleccionado =
                string.IsNullOrWhiteSpace(estado)
                    ? "Todos"
                    : estado;

            ViewBag.TotalTrabajos =
                await _context.AsignacionesTecnicos.CountAsync(a =>
                    a.IdTecnico == idTecnico);

            ViewBag.TotalAsignados =
                await _context.AsignacionesTecnicos.CountAsync(a =>
                    a.IdTecnico == idTecnico &&
                    a.Estado == "Asignado");

            ViewBag.TotalEnProceso =
                await _context.AsignacionesTecnicos.CountAsync(a =>
                    a.IdTecnico == idTecnico &&
                    a.Estado == "En Proceso");

            ViewBag.TotalFinalizados =
                await _context.AsignacionesTecnicos.CountAsync(a =>
                    a.IdTecnico == idTecnico &&
                    a.Estado == "Finalizado");

            var trabajos =
                await consulta
                    .OrderByDescending(a => a.FechaAsignacion)
                    .ToListAsync();

            return View(trabajos);
        }

        // =========================================================
        // DETALLE DEL TRABAJO
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> DetalleTrabajo(int id)
        {
            if (!TecnicoAutenticado())
            {
                return RedirectToAction("Login", "Acceso");
            }

            int idTecnico = ObtenerIdTecnico();

            var trabajo =
                await _context.AsignacionesTecnicos
                    .Include(a => a.SolicitudServicio)
                        .ThenInclude(s => s.Cliente)
                            .ThenInclude(c => c.Usuario)
                    .FirstOrDefaultAsync(a =>
                        a.IdAsignacion == id &&
                        a.IdTecnico == idTecnico);

            if (trabajo == null)
            {
                TempData["Error"] =
                    "El trabajo no existe o no está asignado a este técnico.";

                return RedirectToAction(
                    nameof(TrabajosAsignados)
                );
            }

            ViewBag.NombreUsuario =
                HttpContext.Session.GetString("NombreUsuario")
                ?? "Técnico";

            return View(trabajo);
        }

        // =========================================================
        // COMPATIBILIDAD CON EL BOTÓN ABRIR TRABAJO
        // =========================================================
        [HttpGet]
        public IActionResult AbrirTrabajo(int id)
        {
            return RedirectToAction(
                nameof(DetalleTrabajo),
                new { id }
            );
        }

        // =========================================================
        // INICIAR TRABAJO
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IniciarTrabajo(int id)
        {
            if (!TecnicoAutenticado())
            {
                return RedirectToAction("Login", "Acceso");
            }

            int idTecnico = ObtenerIdTecnico();

            var trabajo =
                await _context.AsignacionesTecnicos
                    .Include(a => a.SolicitudServicio)
                    .FirstOrDefaultAsync(a =>
                        a.IdAsignacion == id &&
                        a.IdTecnico == idTecnico);

            if (trabajo == null)
            {
                TempData["Error"] =
                    "No se encontró el trabajo seleccionado.";

                return RedirectToAction(
                    nameof(TrabajosAsignados)
                );
            }

            if (trabajo.Estado == "Finalizado")
            {
                TempData["Error"] =
                    "Este trabajo ya fue finalizado.";

                return RedirectToAction(
                    nameof(DetalleTrabajo),
                    new { id }
                );
            }

            if (trabajo.Estado == "En Proceso")
            {
                TempData["Error"] =
                    "Este trabajo ya se encuentra en proceso.";

                return RedirectToAction(
                    nameof(DetalleTrabajo),
                    new { id }
                );
            }

            if (trabajo.Estado != "Asignado")
            {
                TempData["Error"] =
                    $"No se puede iniciar un trabajo con estado {trabajo.Estado}.";

                return RedirectToAction(
                    nameof(DetalleTrabajo),
                    new { id }
                );
            }

            trabajo.Estado = "En Proceso";

            if (trabajo.SolicitudServicio != null)
            {
                trabajo.SolicitudServicio.Estado =
                    "Levantamiento en Proceso";
            }

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "El trabajo fue iniciado correctamente.";

            return RedirectToAction(
                nameof(DetalleTrabajo),
                new { id }
            );
        }

        // =========================================================
        // FINALIZAR TRABAJO
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizarTrabajo(
            int id,
            string? observacion,
            string? resultado,
            IFormFile? imagen1,
            IFormFile? imagen2,
            IFormFile? imagen3)
        {
            if (!TecnicoAutenticado())
            {
                return RedirectToAction("Login", "Acceso");
            }

            int idTecnico = ObtenerIdTecnico();

            var trabajo =
                await _context.AsignacionesTecnicos
                    .Include(a => a.SolicitudServicio)
                    .FirstOrDefaultAsync(a =>
                        a.IdAsignacion == id &&
                        a.IdTecnico == idTecnico);

            if (trabajo == null)
            {
                TempData["Error"] =
                    "No se encontró el trabajo seleccionado.";

                return RedirectToAction(
                    nameof(TrabajosAsignados)
                );
            }

            if (trabajo.Estado == "Finalizado")
            {
                TempData["Error"] =
                    "Este trabajo ya fue finalizado.";

                return RedirectToAction(
                    nameof(DetalleTrabajo),
                    new { id }
                );
            }

            if (trabajo.Estado != "En Proceso")
            {
                TempData["Error"] =
                    "Primero debes iniciar el trabajo antes de finalizarlo.";

                return RedirectToAction(
                    nameof(DetalleTrabajo),
                    new { id }
                );
            }

            if (string.IsNullOrWhiteSpace(observacion))
            {
                TempData["Error"] =
                    "Debes escribir la observación del levantamiento.";

                return RedirectToAction(
                    nameof(DetalleTrabajo),
                    new { id }
                );
            }

            if (string.IsNullOrWhiteSpace(resultado))
            {
                TempData["Error"] =
                    "Debes escribir el resultado del levantamiento.";

                return RedirectToAction(
                    nameof(DetalleTrabajo),
                    new { id }
                );
            }

            observacion = observacion.Trim();
            resultado = resultado.Trim();

            if (observacion.Length > 300)
            {
                TempData["Error"] =
                    "La observación no puede superar los 300 caracteres.";

                return RedirectToAction(
                    nameof(DetalleTrabajo),
                    new { id }
                );
            }

            if (resultado.Length > 800)
            {
                TempData["Error"] =
                    "El resultado no puede superar los 800 caracteres.";

                return RedirectToAction(
                    nameof(DetalleTrabajo),
                    new { id }
                );
            }

            // La primera evidencia es obligatoria.
            if (imagen1 == null || imagen1.Length == 0)
            {
                TempData["Error"] =
                    "Debes adjuntar al menos una evidencia fotográfica.";

                return RedirectToAction(
                    nameof(DetalleTrabajo),
                    new { id }
                );
            }

            string? errorImagen1 = ValidarImagen(
                imagen1,
                "la evidencia principal"
            );

            if (errorImagen1 != null)
            {
                TempData["Error"] = errorImagen1;

                return RedirectToAction(
                    nameof(DetalleTrabajo),
                    new { id }
                );
            }

            if (imagen2 != null && imagen2.Length > 0)
            {
                string? errorImagen2 = ValidarImagen(
                    imagen2,
                    "la segunda evidencia"
                );

                if (errorImagen2 != null)
                {
                    TempData["Error"] = errorImagen2;

                    return RedirectToAction(
                        nameof(DetalleTrabajo),
                        new { id }
                    );
                }
            }

            if (imagen3 != null && imagen3.Length > 0)
            {
                string? errorImagen3 = ValidarImagen(
                    imagen3,
                    "la tercera evidencia"
                );

                if (errorImagen3 != null)
                {
                    TempData["Error"] = errorImagen3;

                    return RedirectToAction(
                        nameof(DetalleTrabajo),
                        new { id }
                    );
                }
            }

            var rutasGuardadas = new List<string>();

            try
            {
                string rutaImagen1 =
                    await GuardarImagenAsync(
                        imagen1,
                        id
                    );

                rutasGuardadas.Add(rutaImagen1);

                string? rutaImagen2 = null;
                string? rutaImagen3 = null;

                if (imagen2 != null && imagen2.Length > 0)
                {
                    rutaImagen2 =
                        await GuardarImagenAsync(
                            imagen2,
                            id
                        );

                    rutasGuardadas.Add(rutaImagen2);
                }

                if (imagen3 != null && imagen3.Length > 0)
                {
                    rutaImagen3 =
                        await GuardarImagenAsync(
                            imagen3,
                            id
                        );

                    rutasGuardadas.Add(rutaImagen3);
                }

                trabajo.Observacion = observacion;
                trabajo.Resultado = resultado;
                trabajo.EvidenciaImagen1 = rutaImagen1;
                trabajo.EvidenciaImagen2 = rutaImagen2;
                trabajo.EvidenciaImagen3 = rutaImagen3;
                trabajo.FechaFinalizacion = DateTime.Now;
                trabajo.Estado = "Finalizado";

                if (trabajo.SolicitudServicio != null)
                {
                    trabajo.SolicitudServicio.Estado =
                        "Levantamiento Finalizado";
                }

                await _context.SaveChangesAsync();

                TempData["Exito"] =
                    "El levantamiento fue finalizado correctamente.";

                return RedirectToAction(
                    nameof(DetalleTrabajo),
                    new { id }
                );
            }
            catch
            {
                // Si ocurrió un error, eliminamos las imágenes
                // que alcanzaron a guardarse.
                foreach (string ruta in rutasGuardadas)
                {
                    EliminarImagenFisica(ruta);
                }

                TempData["Error"] =
                    "Ocurrió un error al guardar las evidencias. Inténtalo nuevamente.";

                return RedirectToAction(
                    nameof(DetalleTrabajo),
                    new { id }
                );
            }
        }

        // =========================================================
        // VALIDAR IMAGEN
        // =========================================================
        private string? ValidarImagen(
            IFormFile imagen,
            string nombreCampo)
        {
            if (imagen.Length <= 0)
            {
                return $"El archivo de {nombreCampo} está vacío.";
            }

            if (imagen.Length > TamanoMaximoImagen)
            {
                return $"La imagen de {nombreCampo} no puede superar los 5 MB.";
            }

            string extension =
                Path.GetExtension(imagen.FileName)
                    .ToLowerInvariant();

            if (!_extensionesPermitidas.Contains(extension))
            {
                return $"El archivo de {nombreCampo} debe ser JPG, JPEG o PNG.";
            }

            string tipoContenido =
                imagen.ContentType.ToLowerInvariant();

            bool tipoValido =
                tipoContenido == "image/jpeg" ||
                tipoContenido == "image/jpg" ||
                tipoContenido == "image/png";

            if (!tipoValido)
            {
                return $"El archivo de {nombreCampo} no es una imagen válida.";
            }

            return null;
        }

        // =========================================================
        // GUARDAR IMAGEN EN WWWROOT
        // =========================================================
        private async Task<string> GuardarImagenAsync(
            IFormFile imagen,
            int idAsignacion)
        {
            string carpetaEvidencias =
                Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "evidencias"
                );

            if (!Directory.Exists(carpetaEvidencias))
            {
                Directory.CreateDirectory(carpetaEvidencias);
            }

            string extension =
                Path.GetExtension(imagen.FileName)
                    .ToLowerInvariant();

            string nombreArchivo =
                $"trabajo_{idAsignacion}_{Guid.NewGuid():N}{extension}";

            string rutaFisica =
                Path.Combine(
                    carpetaEvidencias,
                    nombreArchivo
                );

            await using FileStream stream =
                new FileStream(
                    rutaFisica,
                    FileMode.Create
                );

            await imagen.CopyToAsync(stream);

            return $"/uploads/evidencias/{nombreArchivo}";
        }

        // =========================================================
        // ELIMINAR IMAGEN SI OCURRE UN ERROR
        // =========================================================
        private void EliminarImagenFisica(string rutaRelativa)
        {
            if (string.IsNullOrWhiteSpace(rutaRelativa))
            {
                return;
            }

            string rutaLimpia =
                rutaRelativa.TrimStart('/')
                    .Replace(
                        '/',
                        Path.DirectorySeparatorChar
                    );

            string rutaFisica =
                Path.Combine(
                    _environment.WebRootPath,
                    rutaLimpia
                );

            if (System.IO.File.Exists(rutaFisica))
            {
                System.IO.File.Delete(rutaFisica);
            }
        }
    }
}
