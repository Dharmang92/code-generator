using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Fluid;

namespace CodeGenerator;

public partial class MainWindow : Window
{
    public Dictionary<string, string> Placeholders;
    public HashSet<string> Endpoints = ["List", "GetById", "Create", "UpdateById"];
    public readonly string DesktopPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
    public readonly string TemplatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SourceFiles");

    public MainWindow()
    {
        InitializeComponent();
    }

    public async void Generate_Click(object sender, RoutedEventArgs e)
    {
        if(string.IsNullOrEmpty(EntityName.Text))
        {
            MessageBox.Show("Please enter Entity Name!");
            return;
        };

        MessageBoxResult result = MessageBox.Show("Are you sure?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        var serviceName = string.IsNullOrEmpty(ServiceName.Text) ? EntityName.Text : ServiceName.Text;
        serviceName = serviceName.Contains("service", StringComparison.OrdinalIgnoreCase)
            ? serviceName
            : $"{serviceName}Service";
        var modelNamespace = ModelPath.Text.Contains("namespace", StringComparison.OrdinalIgnoreCase)
            ? ModelPath.Text
            : $"namespace {ModelPath.Text}";

        Placeholders = new Dictionary<string, string>
        {
            { "EntityName", EntityName.Text },
            { "ServiceName", serviceName },
            { "ModelPath", ModelPath.Text },
            { "ModelNamespace", modelNamespace }
        };
        var parsedEndpointImportsTemplate = string.IsNullOrEmpty(EndpointImports.Text) ? await GetParsedTemplate(Path.Combine(TemplatePath, "EndpointImports.txt"), Placeholders) : EndpointImports.Text;
        Placeholders.Add("EndpointImports", parsedEndpointImportsTemplate);

        CreateFolder(Path.Combine(DesktopPath), EntityName.Text, out var rootFolder);

        #region Endpoints
        CreateFolder(rootFolder.FullName, "Endpoints", out var endpointsFolder);
        foreach (var endpoint in Endpoints)
        {
            try
            {
                await GenerateFromTemplate(Path.Combine(TemplatePath, $"{endpoint}.txt"), endpointsFolder.FullName, endpoint);
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
                await GenerateFromTemplate(Path.Combine(TemplatePath, $"{model.Value}.txt"), modelsFolder.FullName, model.Key);
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
            await GenerateFromTemplate(Path.Combine(TemplatePath, "AutoMapper.txt"), modelsFolder.FullName, "MapperProfile");
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
            await GenerateFromTemplate(Path.Combine(TemplatePath, "Service.txt"), servicesFolder.FullName, serviceName);
            await GenerateFromTemplate(Path.Combine(TemplatePath, "IService.txt"), servicesFolder.FullName, $"I{serviceName}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generating 'Services':\n\n" + ex.Message);
            return;
        }
        #endregion

        MessageBox.Show("Successfully generated source files! 🚀🔥");
    }

    public async Task GenerateFromTemplate(string templatePath, string folderPath, string outputFilename)
    {
        var parsedTemplate = await GetParsedTemplate(templatePath, Placeholders);
        var filePath = Path.Combine(folderPath, $"{outputFilename}.cs");
        if (!File.Exists(filePath) || (Override.IsChecked ?? false))
        { 
            await File.WriteAllTextAsync(filePath, parsedTemplate);
        }
    }

    public async Task<string?> GetParsedTemplate(string templatePath, object model)
    {
        var parser = new FluidParser();
        if (parser.TryParse(await File.ReadAllTextAsync(templatePath), out var template, out var error))
        {
            var options = new TemplateOptions();
            options.Filters.AddFilter("LowerFirstChar", Utils.LowerFirstChar);
            options.Filters.AddFilter("ParamCase", Utils.ParamCase);
            options.Filters.AddFilter("SentenceCase", Utils.SentenceCase);
            options.Filters.AddFilter("Pluralize", Utils.Pluralize);

            var context = new TemplateContext(model, options);
            var generatedFile = await template.RenderAsync(context);
            return generatedFile;
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

    private void CheckBox_Checked(object sender, RoutedEventArgs e)
    {
        var chk = (CheckBox)sender;
            switch (chk.Name)
            {
                case "List":
                    Endpoints.Add("List");
                    break; 
                case "GetById":
                    Endpoints.Add("GetById");
                    break;
                case "Create":
                    Endpoints.Add("Create");
                    break;
                case "UpdateById":
                    Endpoints.Add("UpdateById");
                    break;
        }
    }

    private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        var chk = (CheckBox)sender;
        switch (chk.Name)
        {
            case "List":
                Endpoints.Remove("List");
                break;
            case "GetById":
                Endpoints.Remove("GetById");
                break;
            case "Create":
                Endpoints.Remove("Create");
                break;
            case "UpdateById":
                Endpoints.Remove("UpdateById");
                break;
        }
    }
}