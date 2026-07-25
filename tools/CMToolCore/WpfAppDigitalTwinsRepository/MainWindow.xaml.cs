using KAE.CMTools.Core;
using KAE.CMTools.Generator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System;
using KAE.CMTools.Repository.OnMemory;

namespace WpfAppDigitalTwinsRepository
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IConfiguration _configuration;
        public MainWindow(IConfiguration configuration)
        {
            InitializeComponent();

            var services = new ServiceCollection();
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddProvider(new TextBoxLoggerProvider(tbLogging, svLogging, this.Dispatcher));
                builder.SetMinimumLevel(LogLevel.Information);
            });
            var provider = services.BuildServiceProvider();
            logger = provider.GetRequiredService<ILogger<MainWindow>>();
            _configuration = configuration;


            this.Loaded += MainWindow_Loaded;
        }

        readonly string cimSchemaFilePathKey = "cim_schema_filepath";
        readonly string cimDescripFilePathKey = "cim_description_filepath";
        readonly string instanceSchemaFilePathKey = "instance_schema_filepath";

        bool hasCIMSchemaReadAndParsed = false;

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string? cimSchemaFilePath = _configuration[cimSchemaFilePathKey];
            string? cimDescripFilePath = _configuration[cimDescripFilePathKey];
            string? instanceSchemaFilePath = _configuration[instanceSchemaFilePathKey];
            if (!string.IsNullOrEmpty(cimSchemaFilePath))
            {
                tbCIMSchemaFilePath.Text = cimSchemaFilePath;
                AddOperationHistory($"Selected CIM Schema - {cimSchemaFilePath}");
            }
            if (!string.IsNullOrEmpty(cimDescripFilePath))
            {
                tbCIMDefFilePath.Text = cimDescripFilePath;
                AddOperationHistory($"Selected CIM Descrip - {cimDescripFilePath}");
            }
            if (!string.IsNullOrEmpty(instanceSchemaFilePath))
            {
                tbInstSchemaFilePath.Text = instanceSchemaFilePath;
                AddOperationHistory($"Selected Instance Schema - {instanceSchemaFilePath}");
            }

            if (!string.IsNullOrEmpty(tbCIMSchemaFilePath.Text) && !string.IsNullOrEmpty(tbCIMDefFilePath.Text))
            {
                buttonReadCIMDef.IsEnabled = true;
            }
            DataContext = this;
        }

        public void AddOperationHistory(string message)
        {
            if (!string.IsNullOrEmpty(tbOpHistory.Text))
            {
                tbOpHistory.Text += System.Environment.NewLine;
            }
            tbOpHistory.Text += $"{DateTime.Now:HH:mm:s} : {message}";
        }

        private ILogger<MainWindow> logger = null;

        private void buttonSelectCIMSchemaFilePath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog() { Filter = "YAML File (*.yaml)|*.yaml" };
            if (dialog.ShowDialog() == true)
            {
                tbCIMSchemaFilePath.Text = dialog.FileName;
                AddOperationHistory($"Selected CIM Schema - {tbCIMSchemaFilePath.Text}");
                hasCIMSchemaReadAndParsed = false;
                if (!string.IsNullOrEmpty(tbCIMSchemaFilePath.Text) && !string.IsNullOrEmpty(tbCIMDefFilePath.Text))
                {
                    buttonReadCIMDef.IsEnabled = true;
                }
            }
        }

        private void buttonSelectCIMDefFilePath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog() { Filter = "YAML File (*.yaml)|*.yaml" };
            if (dialog.ShowDialog() == true)
            {
                tbCIMDefFilePath.Text = dialog.FileName;
                AddOperationHistory($"Selected CIM Descrip - {tbCIMDefFilePath.Text}");
                hasCIMSchemaReadAndParsed = false;
            }
        }

        InstanceRepository repository;
        private void buttonReadCIMDef_Click(object sender, RoutedEventArgs e)
        {
            if (repository == null)
            {
                repository = new InstanceRepositoryImpl();
            }
            if (!string.IsNullOrEmpty(tbCIMDefFilePath.Text) && !string.IsNullOrEmpty(tbCIMSchemaFilePath.Text))
            {
                var reader = new YamlSchemaReader() { Logger = logger };
                using (var schemaStream = File.OpenRead(tbCIMSchemaFilePath.Text))
                {
                    using (var defStream = File.OpenRead(tbCIMDefFilePath.Text))
                    {
                        try
                        {
                            if (reader.Read(schemaStream, defStream))
                            {
                                reader.Parse(repository);
                                bool validated = true;
                                if (cbIsValidation.IsChecked == true)
                                {
                                    AddOperationHistory($"Read and Parsed - CIM schema description.");
                                    if (validated = reader.Validate())
                                    {
                                        AddOperationHistory($"Validated - CIM schema description.");
                                        var app = (App)Application.Current;
                                        app.UpdateSettingFile(cimSchemaFilePathKey, tbCIMSchemaFilePath.Text);
                                        app.UpdateSettingFile(cimDescripFilePathKey, tbCIMDefFilePath.Text);
                                        hasCIMSchemaReadAndParsed = true;

                                        parsedDomains.Clear();
                                        foreach(var domainName in repository.ConceptualDomains.Keys)
                                        {
                                            parsedDomains.Add(repository.ConceptualDomains[domainName]);
                                        }
                                    }
                                }
                                if (validated && cbIsGenerateInstSchema.IsChecked == true)
                                {
                                    GenerateInstanceSchema();
                                }
                                buttonSetInstSchemaFilePath.IsEnabled = true;
                                if (!string.IsNullOrEmpty(tbInstSchemaFilePath.Text))
                                {
                                    buttonSelectInstanceModel.IsEnabled = true;
                                }
                                buttonReadCIMDef.IsEnabled = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogCritical(ex, "While Reading");
                        }
                    }
                }
            }
        }

        private void GenerateInstanceSchema()
        {
            foreach (var domainName in repository.ConceptualDomains.Keys)
            {
                MessageBox.Show($"Generate Instance Schema of '{domainName}'");
                var generator = new DomainSchemaGenerator() { Logger = logger };
                using (var writer = new StreamWriter(File.OpenWrite(tbInstSchemaFilePath.Text)))
                {
                    try
                    {
                        generator.Generate(domainName, repository, writer);
                        AddOperationHistory($"Generated Instance Schema - {tbInstSchemaFilePath.Text}");
                        var app = (App)Application.Current;
                        app.UpdateSettingFile(instanceSchemaFilePathKey, tbInstSchemaFilePath.Text);
                    }
                    catch (Exception ex)
                    {
                        logger.LogCritical(ex, "Generating Instance Schema");
                    }
                }
                break;
            }
        }

        private void buttonSetInstSchemaFilePath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                CheckFileExists = false,
                CheckPathExists = true,
                Filter = "YAML File (*.yaml)|*.yaml"
            };
            if (dialog.ShowDialog() == true)
            {
                tbInstSchemaFilePath.Text = dialog.FileName;
                AddOperationHistory($"Selected Instance Schema - {tbInstSchemaFilePath.Text}");
                buttonRead.IsEnabled = true;
            }
        }

        private void cbIsGenerateInstSchema_Checked(object sender, RoutedEventArgs e)
        {
            if (cbIsGenerateInstSchema.IsChecked == true)
            {
                if (string.IsNullOrEmpty(tbInstSchemaFilePath.Text))
                {
                    MessageBox.Show("Please Set or Select Instance Schema File Path!");
                    cbIsGenerateInstSchema.IsChecked = false;
                }
                else
                {
                    if (hasCIMSchemaReadAndParsed)
                    {
                        GenerateInstanceSchema();
                    }
                    else
                    {
                        MessageBox.Show("Please Read and Parse Instance Schema!");
                    }
                }
            }
        }

        private void buttonSelectInstanceModel_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog() { Filter = "YAML File (*.yaml)|*.yaml" };
            if (dialog.ShowDialog() == true)
            {
                tbInstanceModelFilePath.Text = dialog.FileName;
                AddOperationHistory($"Selected Instance Model - {tbInstanceModelFilePath.Text}");
                buttonRead.IsEnabled = true;
            }
        }

        private void buttonRead_Click(object sender, RoutedEventArgs e)
        {
            if (hasCIMSchemaReadAndParsed)
            {
                using (var instanceSchemaStream = File.OpenRead(tbInstSchemaFilePath.Text))
                {
                    using (var instanceModelStream = File.OpenRead(tbInstanceModelFilePath.Text))
                    {
                        var reader = new YamlFieldOfSenseReader() { Logger = logger };
                        if (reader.Read(instanceSchemaStream, instanceModelStream))
                        {
                            reader.Parse(repository);
                            if (reader.Validate())
                            {
                                logger.LogInformation("Instance Model validation has been done without any problem.");
                                AddOperationHistory($"Validated - {tbInstanceModelFilePath.Text}");
                                parsedFosItems.Add(new FoSItem() { FilePath = tbInstanceModelFilePath.Text, FoSId=reader.ParsedFoSId });
                            }
                            else
                            {
                                logger.LogInformation($"Validation Failed - {tbInstanceModelFilePath.Text}");
                            }
                        }
                        else
                        {
                            logger.LogInformation($"Read Failed - {tbInstanceModelFilePath.Text}");
                        }
                    }
                }
            }
        }

        public ObservableCollection<FoSItem> ParsedFoSItems { get => parsedFosItems; }
        public ObservableCollection<FoSItem> parsedFosItems = new ObservableCollection<FoSItem>();

        public class FoSItem
        {
            public string FilePath { get; set; }
            public string FoSId { get; set; }
        }

        public ObservableCollection<ConceptualDomain> ParsedDomains { get => parsedDomains; }
        public ObservableCollection<ConceptualDomain> parsedDomains = new ObservableCollection<ConceptualDomain>();

        private void lbDomains_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedDomain = (ConceptualDomain)lbDomains.SelectedItem;

            var cimDefWindow = new CIMDefWindow() { ConceptualDomain=selectedDomain };
            cimDefWindow.Closed += (s, args) => cimDefWindow = null;
            cimDefWindow.Show();
        }
    }
}