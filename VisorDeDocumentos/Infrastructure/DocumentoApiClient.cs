namespace VisorDeDocumentos.Infrastructure;

public class DocumentoApiClient
{
    private readonly HttpClient _httpClient;

    public DocumentoApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<byte[]> ObtenerDocumentoAsync(string codigo)
    {
        var response = await _httpClient.GetAsync(
            $"documentos/{Uri.EscapeDataString(codigo)}"
        );

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsByteArrayAsync();
    }
}