using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using VisorDeDocumentos.Base;
using VisorDeDocumentos.Models;
using VisorDeDocumentos.Service.Interface;

namespace VisorDeDocumentos.Controllers.Documentos
{
    //[Route("Documento")]
    public class DocumentoController : Controller
    {
        private readonly IDocumentoService _documentoService;
        private const string NombreTabla = "cbs01";
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _turnstileSecretKey;
        private readonly string _turnstileSiteKey;

        public DocumentoController(
            IDocumentoService documentoService,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _documentoService = documentoService;
            _httpClientFactory = httpClientFactory;

            // Leer las claves configuradas en appsettings.json
            _turnstileSiteKey = configuration["CloudflareTurnstile:SiteKey"]
                ?? throw new InvalidOperationException("No se ha configurado 'SiteKey' en appsettings.json.");

            _turnstileSecretKey = configuration["CloudflareTurnstile:SecretKey"]
                ?? throw new InvalidOperationException("No se ha configurado 'SecretKey' en appsettings.json.");
        }

        [HttpGet("")]
        [HttpGet("{codigo?}")]
        [HttpGet("Documento/{codigo?}")]
        public IActionResult Index(string? codigo)
        {
            // Pasar la SiteKey a la vista mediante ViewBag
            ViewBag.TurnstileSiteKey = _turnstileSiteKey;
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

        private async Task<bool> ValidarTurnstileAsync(string token)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                var values = new Dictionary<string, string>
                {
                    { "secret", _turnstileSecretKey },
                    { "response", token }
                };

                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync("https://challenges.cloudflare.com/turnstile/v0/siteverify", content);

                if (!response.IsSuccessStatusCode)
                    return false;

                var jsonString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<TurnstileResponse>(jsonString);

                return result?.Success ?? false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}