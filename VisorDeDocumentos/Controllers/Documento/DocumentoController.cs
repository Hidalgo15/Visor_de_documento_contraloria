using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.IO;
using VisorDeDocumentos.Base;

namespace VisorDeDocumentos.Controllers.Documento
{
    public class DocumentoController : Controller
    {
        private readonly string _connectionString;

        public DocumentoController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ConexionSIGOB");
        }

        [HttpGet]
        
        public IActionResult Index(string? nodocumento, string nombreTb = "cbs01", int codigo = 1645)
        {
            if (string.IsNullOrEmpty(nodocumento))
            {
                nodocumento = "DOC-PRUEBA-001";
            }

            ViewBag.NoDocumento = nodocumento;

            // Apuntamos ViewBag.PdfUrl hacia el endpoint que obtiene, descomprime y retorna el PDF de la BD
            ViewBag.PdfUrl = Url.Action("DescargarDocumento", "Documento", new { nombreTb = nombreTb, codigo = codigo });

            return View();
        }

        [HttpGet]
        public IActionResult VerPdfLocal()
        {
            string rutaAbsoluta = @"C:\Ruta\De\Tu\Archivo\documento.pdf";

            if (!System.IO.File.Exists(rutaAbsoluta))
            {
                return NotFound("El archivo no existe en la ruta especificada.");
            }

            var stream = new FileStream(rutaAbsoluta, FileMode.Open, FileAccess.Read);
            return File(stream, "application/pdf");
        }

        [HttpGet]
        public IActionResult DescargarDocumento(string nombreTb = "cbs01", int codigo = 1645)
        {
            byte[] archivoBytes = null;

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
                        if (reader.Read())
                        {
                            if (reader["document"] != DBNull.Value)
                            {
                                archivoBytes = (byte[])reader["document"];
                            }
                        }
                    }
                }
            }

            if (archivoBytes == null || archivoBytes.Length == 0)
            {
                return NotFound("No se encontró el archivo comprimido en la BD.");
            }

            // Descomprimir usando ZLIBSIGOB
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
                // Limpiar archivo temporal descomprimido
                if (System.IO.File.Exists(rutaPdf))
                {
                    System.IO.File.Delete(rutaPdf);
                }
            }
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
                // Llamada a tu clase ZLIBSIGOB
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