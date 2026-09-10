namespace VisorDeDocumentos.Service
{
    public interface IDocumentoService
    {
        Task<byte[]> ObtenerPdfAsync(string codigo);
    }
}
