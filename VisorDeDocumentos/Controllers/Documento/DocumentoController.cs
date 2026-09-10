using Microsoft.AspNetCore.Mvc;
using VisorDeDocumentos.Base;

namespace VisorDeDocumentos.Controllers.Documento
{
    public class DocumentoController : Controller
    {
        private const string UrlBase = "https://sistema.com/documentos/";

        private static readonly HttpClient HttpClient = new HttpClient();

        [HttpGet]
        public IActionResult Index(int? nodocumento)
        {
            ViewBag.NoDocumento = nodocumento;

            if (nodocumento.HasValue)
            {
                ViewBag.PdfUrl = Url.Action(
                    "Pdf",
                    "Documento",
                    new { codigo = nodocumento.Value }
                );
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Pdf(int codigo)
        {
            try
            {
                string url = $"{UrlBase}{codigo}";

                using var httpClient = new HttpClient();

                byte[] archivoComprimido =
                    await HttpClient.GetByteArrayAsync(url);

                // descompresion del archivo comprimido usando zlib

                byte[] pdf = DescomprimirArchivo(archivoComprimido);

                return File(pdf, "application/pdf");
            }
            catch (HttpRequestException)
            {
                return NotFound(
                    "No fue posible obtener el documento."
                );
            }
            catch (Exception)
            {
                return StatusCode(
                    500,
                    "Ocurrió un error procesando el documento."
                );
            }
        }

        private byte[] DescomprimirArchivo(byte[] archivoComprimido)
        {
            // Aqui va zlib.

            throw new NotImplementedException();
        }

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