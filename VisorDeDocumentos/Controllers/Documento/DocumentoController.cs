using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
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
                            if (reader[0] != DBNull.Value)
                            {
                                archivoBytes = (byte[])reader[0];
                            }
                        }
                    }
                }
            }

            if (archivoBytes == null || archivoBytes.Length == 0)
            {
                return NotFound("No se encontró el archivo comprimido.");
            }

            // Descomprimir y mostrar el PDF
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
                // Limpiar archivo temporal
                if (System.IO.File.Exists(rutaPdf))
                {
                    System.IO.File.Delete(rutaPdf);
                }
            }
        }

        /// <summary>
        /// Descomprime el archivo ZLIB en memoria y devuelve la ruta del PDF descomprimido
        /// </summary>
        private string DescomprimirArchivo(byte[] archivoComprimido, int codigo)
        {
            // Crear carpeta temporal
            string tempDir = Path.Combine(Path.GetTempPath(), "VisorDocumentos");
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            // Guardar archivo comprimido temporalmente
            string rutaComprimida = Path.Combine(tempDir, $"Documento_{codigo}.zlib");
            System.IO.File.WriteAllBytes(rutaComprimida, archivoComprimido);

            try
            {
                // Descomprimir usando ZLIBSIGOB
                string rutaDescomprimida = ZLIBSIGOB.DescomprimirArchivoZLIB(rutaComprimida);

                // Limpiar archivo comprimido temporal
                if (System.IO.File.Exists(rutaComprimida))
                {
                    System.IO.File.Delete(rutaComprimida);
                }

                return rutaDescomprimida;
            }
            catch
            {
                // En caso de error, limpiar
                if (System.IO.File.Exists(rutaComprimida))
                {
                    System.IO.File.Delete(rutaComprimida);
                }
                throw;
            }
        }
    }
}