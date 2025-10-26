using DbFirst.Services;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace FitnessCentrApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Build configuration from appsettings.json
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var config = builder.Build();
            DatabaseService.Initialize(config);

            // По умолчанию можно использовать SQLite (меньше проблем с сертификатами)
            //DatabaseService.UseDatabase("SQLite");

            // Или если хочешь MSSQL:
            DatabaseService.UseDatabase("MSSQL");
        }
    }

}
