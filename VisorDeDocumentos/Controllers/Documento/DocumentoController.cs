using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
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
        public IActionResult DescargarDocumento(string nombreTb = "cbs01", int codigo = 1645)
        {
            byte[] archivoBytes = null;
            string nombreArchivo = $"Documento_{codigo}.zip";

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

            return File(archivoBytes, "application/zip", nombreArchivo);
        }

        //--------------------------------------------------------------
        //Ejemplo de descompresion de archivo zlib
        private void unificadomentostre(int codigo)
        {
            string tempPath = @"W:\";

            //var docGrouped = db_CGR.T_DOCUMENTOS_TRE.Take(3).ToList();
            //string[] docGrouped = db_CGR.Database.SqlQuery<string>("SELECT codigo_tramite FROM T_DOCUMENTOS_TRE GROUP BY codigo_tramite order by codigo_tramite ").ToArray();
            //var docGrouped = db_CGR.T_DOCUMENTOS_TRE.Take(10).GroupBy(a => a.numero_libramiento).ToList();
            //var query = people.DistinctBy(p => p.Id);
            //foreach (var noLibramiento in docGrouped)
            //{
            //string currentFolder = tempPath;
            //string ruta = docGroup.Key;
            string currentFolder = tempPath + 56565;
            //var nol = db_CGR.T_DOCUMENTOS_TRE.Take(15).Where(c => c.codigo_caso == d.codigo_caso).ToList();
            System.IO.Directory.CreateDirectory(currentFolder);

            //var dotre = db_CGR.v_documentos_tre.Where(c => c.codigo_tramite == noLibramiento).ToList();
            int conteo = 1;

            string noDoc = "nombre";
            string docName = String.Format("{0}\\{1}", currentFolder, noDoc.ToString());
            System.IO.File.WriteAllBytes(docName + ".zlib", System.IO.File.ReadAllBytes(noDoc));
            var Archivodescomprimido = ZLIBSIGOB.DescomprimirArchivoZLIB(docName + ".zlib");
            System.IO.File.Move(Archivodescomprimido, docName + "-" + conteo.ToString() + new System.IO.FileInfo(Archivodescomprimido).Extension); //
            System.IO.File.Delete(docName + ".zlib");
            conteo++;

        }

    }
}