using Microsoft.AspNetCore.Mvc;

namespace InapaWeb.Controllers
{
    public class ReclamacionesController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }


        // Reclamación Individual
        public IActionResult Individual()
        {
            return View();
        }


        // Reclamación Colectiva
        public IActionResult Colectiva()
        {
            return View();
        }


        // Reclamación Virtual
        public IActionResult RegistrarVirtual()
        {
            return View();
        }


        // Reclamación Presencial
        public IActionResult RegistrarPresencial()
        {
            return View();
        }


        // Reclamación para una vivienda
        public IActionResult ReclamacionVivienda()
        {
            return View();
        }


        // Reclamación para barrio o sector
        public IActionResult ReclamacionSector()
        {
            return View();
        }


        // Estados y seguimiento
        public IActionResult Seguimiento()
        {
            return View();
        }


        // Detalle y evidencias
        public IActionResult Detalle()
        {
            return View();
        }


        // Atención de reclamación
        public IActionResult Atencion()
        {
            return View();
        }


        // Cierre de reclamación
        public IActionResult Cerrar()
        {
            return View();
        }

    }
}