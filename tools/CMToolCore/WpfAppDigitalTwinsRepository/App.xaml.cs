using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace WpfAppDigitalTwinsRepository
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public string SettingFilePath { get => settingFilePath; }
        public App()
        {

            var settingDir = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KnowledgeAndExperience", "DigitalTwinsRepsitory");
            if (!Directory.Exists(settingDir))
            {
                Directory.CreateDirectory(settingDir);
            }
            settingFilePath = System.IO.Path.Combine(settingDir, "appsettings.json");

            var builder = Host.CreateDefaultBuilder(Array.Empty<string>())
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddDebug();
                })
                .ConfigureAppConfiguration((ctx, config) =>
                {
                    config.AddJsonFile(settingFilePath, optional: true, reloadOnChange: false);
                }).ConfigureServices((ctx, services) =>
                {
                    services.AddLogging(builder =>
                    {
                        builder.AddConsole();
                    });
                    services.AddSingleton<MainWindow>();
                }).Build();
            host = builder;
        }

        public void UpdateSettingFile(string itemName, string itemValue)
        {
            if (!File.Exists(settingFilePath))
            {
                var initial = new JObject();
                File.WriteAllText(settingFilePath, initial.ToString());

            }
            var json = JObject.Parse(File.ReadAllText(settingFilePath));
            json[itemName] = itemValue;
            File.WriteAllText(settingFilePath, json.ToString());
        }

        private string settingFilePath = "";
        private IHost host;

        protected override async void OnStartup(StartupEventArgs e)
        {
            await host.StartAsync();
            var window = host.Services.GetRequiredService<MainWindow>();
            window.Show();

            base.OnStartup(e);
        }
    }

}
