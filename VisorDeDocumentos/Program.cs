using VisorDeDocumentos.Infrastructure;
using VisorDeDocumentos.Service;

namespace VisorDeDocumentos
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            builder.Services.AddHttpClient<DocumentoApiClient>((serviceProvider, client) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                var baseUrl = configuration["DocumentoApi:BaseUrl"];

                client.BaseAddress = new Uri(baseUrl!);

                client.Timeout = TimeSpan.FromSeconds(60);
            });

            builder.Services.AddScoped<IDocumentoService, DocumentoService>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Documento/Index");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Documento}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
