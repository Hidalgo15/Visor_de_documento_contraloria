using Microsoft.AspNetCore.Mvc;
using VisorDeDocumentos.Models;
using VisorDeDocumentos.Service.Interface;

namespace VisorDeDocumentos.Controllers.Documentos
{
    [Route("Documento")]
    public class DocumentoController : Controller
    {
        private readonly IDocumentoService _documentoService;
        private const string NombreTabla = "cbs01";

        public DocumentoController(IDocumentoService documentoService)
        {
            _documentoService = documentoService;
        }

        [HttpGet("~/")]
        [HttpGet("~/{codigo?}")]
        [HttpGet("")]
        [HttpGet("{codigo?}")]
        public IActionResult Index(string? codigo)
        {
            var model = new Documento();

            if (string.IsNullOrEmpty(codigo))
            {
                return View(model);
            }

            if (int.TryParse(codigo, out int codigoInt))
            {
                model.NoDocumento = $"DOC-{codigoInt}";
                model.Codigo = codigoInt;

                // Genera la URL resolviendo el parámetro directo en la plantilla de ruta
                ViewBag.PdfUrl = Url.Action("DescargarDocumento", "Documento", new { codigo = codigoInt });
                return View();
            }

            return BadRequest("El código proporcionado debe ser un número entero válido.");
        }

        [HttpGet("DescargarDocumento/{codigo:int}")]
        public async Task<IActionResult> DescargarDocumento(int codigo)
        {
            if (codigo <= 0)
            {
                return BadRequest("Código de documento inválido.");
            }

            byte[]? archivoBytes = await _documentoService.ObtenerDocumentoDesdeBDAsync(codigo, NombreTabla);

            if (archivoBytes == null || archivoBytes.Length == 0)
            {
                return NotFound("No se encontró el archivo comprimido en la BD.");
            }

            try
            {
                byte[] pdfBytes = await _documentoService.DescomprimirDocumentoAsync(archivoBytes, codigo);
                return File(pdfBytes, "application/pdf");
            }
            catch (Exception)
            {
                return NotFound("No se pudo procesar o descomprimir el archivo.");
            }
        }
    }
}