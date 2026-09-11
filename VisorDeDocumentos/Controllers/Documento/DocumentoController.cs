using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using VisorDeDocumentos.Base;

namespace VisorDeDocumentos.Controllers.Documento
{
    [Route("Documento")]
    public class DocumentoController : Controller
    {
        private readonly string _connectionString;
        private const string nombreTb = "cbs01";

        public DocumentoController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ConexionSIGOB");
        }

        [HttpGet("~/")]
        [HttpGet("~/{codigo?}")]
        [HttpGet("")]
        [HttpGet("{codigo?}")]
        public IActionResult Index(string? codigo)
        {
            if (string.IsNullOrEmpty(codigo))
            {
                ViewBag.NoDocumento = "Ninguno seleccionado";
                ViewBag.Codigo = null;
                ViewBag.PdfUrl = null;
                return View();
            }

            if (int.TryParse(codigo, out int codigoInt))
            {
                ViewBag.NoDocumento = $"DOC-{codigoInt}";
                ViewBag.Codigo = codigoInt;
                ViewBag.PdfUrl = Url.Action("DescargarDocumento", "Documento", new { codigo = codigoInt });
                return View();
            }

            return BadRequest("El código proporcionado debe ser un número entero válido.");
        }

        [HttpGet("DescargarDocumento/{codigo:int}")]
        public IActionResult DescargarDocumento(int codigo)
        {
            byte[]? archivoBytes = ObtenerDocumentoDesdeBD(codigo, nombreTb);

            if (archivoBytes == null || archivoBytes.Length == 0)
            {
                return NotFound("No se encontró el archivo comprimido en la BD.");
            }

            string rutaPdf = DescomprimirArchivo(archivoBytes, codigo);

            if (string.IsNullOrEmpty(rutaPdf) || !System.IO.File.Exists(rutaPdf))
            {
                return NotFound("No se pudo descomprimir el archivo.");
            }

            try
            {
                byte[] pdfBytes = System.IO.File.ReadAllBytes(rutaPdf);
                return File(pdfBytes, "application/pdf");
            }
            finally
            {
                if (System.IO.File.Exists(rutaPdf))
                {
                    System.IO.File.Delete(rutaPdf);
                }
            }
        }

        private byte[]? ObtenerDocumentoDesdeBD(int codigo, string nombreTb)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("[dbo].[usp_buscar_documentos_tramite_compras]", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@nombre_mia", nombreTb);
                    cmd.Parameters.AddWithValue("@codigo", codigo);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read() && reader["document"] != DBNull.Value)
                        {
                            return (byte[])reader["document"];
                        }
                    }
                }
            }
            return null;
        }

        private string DescomprimirArchivo(byte[] archivoComprimido, int codigo)
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "VisorDocumentos");
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            string rutaComprimida = Path.Combine(tempDir, $"Documento_{codigo}.zlib");
            System.IO.File.WriteAllBytes(rutaComprimida, archivoComprimido);

            try
            {
                string rutaDescomprimida = ZLIBSIGOB.DescomprimirArchivoZLIB(rutaComprimida);

                if (System.IO.File.Exists(rutaComprimida))
                {
                    System.IO.File.Delete(rutaComprimida);
                }

                return rutaDescomprimida;
            }
            catch
            {
                if (System.IO.File.Exists(rutaComprimida))
                {
                    System.IO.File.Delete(rutaComprimida);
                }
                throw;
            }
        }
    }
}