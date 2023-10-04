using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace ManarEthara
{
    public class Program {
        public static void Main(string[] args)
            => CreateHostBuilder(args)
                .Build()
                .Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureUmbracoDefaults()
               .ConfigureAppConfiguration((context, config) => {
                   var settings = config.Build();
                   var keyVaultEndpoint = settings["AzureKeyVaultEndpoint"];
                   if (!string.IsNullOrEmpty(keyVaultEndpoint) && Uri.TryCreate(keyVaultEndpoint, UriKind.Absolute, out var validUri)) {
                       config.AddAzureKeyVault(validUri, new DefaultAzureCredential());
                   }
               })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStaticWebAssets();
                    webBuilder.UseStartup<Startup>();
                });
    }
}
