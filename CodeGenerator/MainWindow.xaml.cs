using System.IO;
using System.Windows;
using Fluid;
using Mono.TextTemplating;

namespace CodeGenerator;

public partial class MainWindow : Window
{
    public Dictionary<string, string> placeholders;
    public readonly string DesktopPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

    public MainWindow()
    {
        InitializeComponent();
    }

    public async void Generate_Click(object sender, RoutedEventArgs e)
    {

        MessageBoxResult result = MessageBox.Show("Are you sure?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            var templates = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SourceFiles");
            var serviceName = string.IsNullOrEmpty(ServiceName.Text) ? EntityName.Text : ServiceName.Text;
            serviceName = serviceName.Contains("service", StringComparison.OrdinalIgnoreCase)
                ? serviceName
                : $"{serviceName}Service";

            placeholders = new()
            {
                { "EntityName", EntityName.Text },
                { "ServiceName", serviceName }
            };

            CreateFolder(Path.Combine(DesktopPath), EntityName.Text, out var rootFolder);

            #region Endpoints
            //TODO: provide checkbox for below
            string[] endpoints = ["List", "GetById", "Create", "UpdateById"];
            CreateFolder(rootFolder.FullName, "Endpoints", out var endpointsFolder);
            foreach (var endpoint in endpoints)
            {
                try
                {
                    await GenerateFromTemplateFluid(Path.Combine(templates, $"{endpoint}.txt"), endpointsFolder.FullName, endpoint);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error generating '{endpoint}':\n\n" + ex.Message);
                    return;
                }
            }
            #endregion

            #region Models
            var models = new Dictionary<string, string>()
            {
                { $"Create.{EntityName.Text}Request", "Create.Request" },
                { $"GetById.{EntityName.Text}GetByIdRequest", "GetById.Request" },
                { $"GetById.{EntityName.Text}GetByIdResponse", "GetById.Response" },
                { $"List.{EntityName.Text}ListResponse", "List.Response" },
                { $"UpdateById.{EntityName.Text}UpdateByIdRequest", "UpdateById.Request" },
            };
            CreateFolder(rootFolder.FullName, "Models", out var modelsFolder);
            foreach (var model in models)
            {
                try
                {
                    await GenerateFromTemplateFluid(Path.Combine(templates, $"{model.Value}.txt"), modelsFolder.FullName, model.Key);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error generating '{model}':\n\n" + ex.Message);
                    return;
                }
            }
            // AutoMapper
            try
            {
                await GenerateFromTemplateFluid(Path.Combine(templates, "AutoMapper.txt"), modelsFolder.FullName, "MapperProfile");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating 'AutoMapper':\n\n" + ex.Message);
                return;
            }
            #endregion

            #region Services
            CreateFolder(rootFolder.FullName, "Services", out var servicesFolder);
            try
            {
                await GenerateFromTemplateFluid(Path.Combine(templates, "Service.txt"), servicesFolder.FullName, serviceName);
                await GenerateFromTemplateFluid(Path.Combine(templates, "IService.txt"), servicesFolder.FullName, $"I{serviceName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating 'Services':\n\n" + ex.Message);
                return;
            }
            #endregion

            MessageBox.Show("Successfully generated source files!");
        }
    }

    public async Task GenerateFromTemplateFluid(string templatePath, string folderPath, string outputFilename)
    {
        var parser = new FluidParser();
        if (parser.TryParse(await File.ReadAllTextAsync(templatePath), out var template, out var error))
        {
            var options = new TemplateOptions();
            options.Filters.AddFilter("LowerFirstChar", Utils.LowerFirstChar);
            options.Filters.AddFilter("ParamCase", Utils.ParamCase);
            options.Filters.AddFilter("SentenceCase", Utils.SentenceCase);
            options.Filters.AddFilter("Pluralize", Utils.Pluralize);

            var context = new TemplateContext(placeholders, options);
            var generatedFile = await template.RenderAsync(context);
            if (generatedFile != null)
            {
                await File.WriteAllTextAsync(Path.Combine(folderPath, $"{outputFilename}.cs"), generatedFile);
            }
        }
        else
        {
            throw new Exception(error);
        }
    }

    public void CreateFolder(string path, string name, out DirectoryInfo folder)
    {
        var folderPath = Path.Combine(path, name);
        folder = !Directory.Exists(folderPath) ? Directory.CreateDirectory(folderPath) : new DirectoryInfo(folderPath);
    }
}