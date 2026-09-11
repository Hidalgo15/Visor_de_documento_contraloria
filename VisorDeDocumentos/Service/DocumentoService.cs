using Microsoft.Data.SqlClient;
using System.Data;
using VisorDeDocumentos.Base;
using VisorDeDocumentos.Service.Interface;

namespace VisorDeDocumentos.Service;

public class DocumentoService : IDocumentoService
{
    private readonly string _connectionString;

    public DocumentoService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ConexionSIGOB")
            ?? throw new InvalidOperationException("La cadena de conexión 'ConexionSIGOB' no existe.");
    }

    public async Task<byte[]?> ObtenerDocumentoDesdeBDAsync(int codigo, string nombreTabla)
    {
        await using var conn = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand("[dbo].[usp_buscar_documentos_tramite_compras]", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.AddWithValue("@nombre_mia", nombreTabla);
        cmd.Parameters.AddWithValue("@codigo", codigo);

        await conn.OpenAsync();
        await using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync() && !reader.IsDBNull(reader.GetOrdinal("document")))
        {
            return (byte[])reader["document"];
        }

        return null;
    }

    public async Task<byte[]> DescomprimirDocumentoAsync(byte[] archivoComprimido, int codigo)
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "VisorDocumentos");
        Directory.CreateDirectory(tempDir);

        string rutaComprimida = Path.Combine(tempDir, $"Documento_{codigo}.zlib");
        string? rutaDescomprimida = null;

        try
        {
            await File.WriteAllBytesAsync(rutaComprimida, archivoComprimido);
            rutaDescomprimida = ZLIBSIGOB.DescomprimirArchivoZLIB(rutaComprimida);

            if (string.IsNullOrEmpty(rutaDescomprimida) || !File.Exists(rutaDescomprimida))
            {
                throw new FileNotFoundException("No se generó el archivo descomprimido.");
            }

            return await File.ReadAllBytesAsync(rutaDescomprimida);
        }
        finally
        {
            if (File.Exists(rutaComprimida)) File.Delete(rutaComprimida);
            if (!string.IsNullOrEmpty(rutaDescomprimida) && File.Exists(rutaDescomprimida)) File.Delete(rutaDescomprimida);
        }
    }
}