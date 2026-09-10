using VisorDeDocumentos.Infrastructure;

namespace VisorDeDocumentos.Service;

public class DocumentoService : IDocumentoService
{
    private readonly DocumentoApiClient _apiClient;

    public DocumentoService(DocumentoApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<byte[]> ObtenerPdfAsync(string codigo)
    {
        var archivoComprimido = await _apiClient.ObtenerDocumentoAsync(codigo);

        // Aqui despues utilizaremos la libreria de la empresa.

        var pdf = Descomprimir(archivoComprimido);

        return pdf;
    }

    private byte[] Descomprimir(byte[] archivoComprimido)
    {
        // pendiente de la libreria.

        throw new NotImplementedException();
    }
}