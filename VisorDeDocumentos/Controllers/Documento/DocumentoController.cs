using Microsoft.AspNetCore.Mvc;

namespace VisorDeDocumentos.Controllers.Documento
{
    public class DocumentoController : Controller
    {
        [HttpGet]
        public IActionResult Index(string? nodocumento)
        {
            if (string.IsNullOrEmpty(nodocumento))
            {
                ViewBag.Error = "No se ha proporcionado un número de documento válido.";
                return View();
            }

            // Simulación: En el futuro aquí harás el Fetch a la API con nodocumento
            // Por el momento asignamos la ruta del PDF directamente para visualización
            string rutaPdf = $"/pdf/{nodocumento}.pdf";

            ViewBag.NoDocumento = nodocumento;
            ViewBag.PdfUrl = rutaPdf;

            return View();
        }
    }
}
