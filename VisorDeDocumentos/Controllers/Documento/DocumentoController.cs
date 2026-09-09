using Microsoft.AspNetCore.Mvc;

namespace VisorDeDocumentos.Controllers.Documento
{
    public class DocumentoController : Controller
    {
        [HttpGet]
        public IActionResult Index(string? nodocumento)
        {
            // Si viene vacío en esta prueba, asignamos uno por defecto para visualizar el PDF
            if (string.IsNullOrEmpty(nodocumento))
            {
                nodocumento = "DOC-PRUEBA-001";
            }

            // Ruta hacia la carpeta wwwroot/pdf/
            ViewBag.NoDocumento = nodocumento;
            ViewBag.PdfUrl = "~/pdf/Capitulo 5 Entregable.pdf";

            return View();
        }

        [HttpGet]
        public IActionResult VerPdfLocal()
        {
            // Escribe aquí la ruta exacta de tu computadora
            string rutaAbsoluta = @"C:\Ruta\De\Tu\Archivo\documento.pdf";

            if (!System.IO.File.Exists(rutaAbsoluta))
            {
                return NotFound("El archivo no existe en la ruta especificada.");
            }

            var stream = new FileStream(rutaAbsoluta, FileMode.Open, FileAccess.Read);
            return File(stream, "application/pdf");
        }
    }
}
