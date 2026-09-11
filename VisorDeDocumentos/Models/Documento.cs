namespace VisorDeDocumentos.Models
{
    public class Documento
    {
        public string NoDocumento { get; set; } = "Ninguno seleccionado";
        public int Codigo { get; set; }
        public string? PdfUrl { get; set; }
    }
}
