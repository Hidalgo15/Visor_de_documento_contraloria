namespace VisorDeDocumentos.Service.Interface
{
    public interface IDocumentoService
    {
        Task<byte[]?> ObtenerDocumentoDesdeBDAsync(int codigo, string nombreTabla);
        Task<byte[]> DescomprimirDocumentoAsync(byte[] archivoComprimido, int codigo);
    }
}
