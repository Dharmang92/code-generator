using System.IO;
using System.Windows;
using Mono.TextTemplating;
using Pluralize.NET;

namespace CodeGenerator;

public partial class MainWindow : Window
{
    public readonly IPluralize Pluralizer;
    public Dictionary<string, string> placeholders;
    public readonly string DesktopPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

    public MainWindow()
    {
        Pluralizer = new Pluralizer();
        InitializeComponent();
    }

    public async void Generate_Click(object sender, RoutedEventArgs e)
    {

        MessageBoxResult result = MessageBox.Show("Are you sure?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            var pluralEntityName = Pluralizer.Pluralize(EntityName.Text);
            var templates = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SourceFiles");
            //TODO: provide checkbox for below
            //string[] endpoints = ["List", "GetById", "Create", "UpdateById", "Delete"];
            string[] endpoints = ["List", "GetById", "Create", "UpdateById"];

            placeholders = new()
            {
                { "EntityName", EntityName.Text },
                { "PluralEntityName", pluralEntityName },
                { "CamelEntityName", EntityName.Text.ToLowerFirstChar() },
                { "ParamEntityName", pluralEntityName.ToParamCase() },
                { "SentenceEntityName", pluralEntityName.ToSentenceCase() },
                { "ServiceName", string.IsNullOrEmpty(ServiceName.Text) ? EntityName.Text.ToLowerFirstChar() : ServiceName.Text },
                { "LowerServiceName", string.IsNullOrEmpty(ServiceName.Text) ? EntityName.Text.ToLowerFirstChar() : ServiceName.Text }
            };

            #region Endpoints
            CreateFolder(DesktopPath, EntityName.Text, out var endpointFolder);
            foreach (var endpoint in endpoints)
            {
                try
                {
                    await GenerateFromTemplate(Path.Combine(templates, $"{endpoint}.tt"), endpointFolder.FullName, endpoint);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error generating '{endpoint}': " + ex.Message);
                    return;
                }
            }
            #endregion

            MessageBox.Show("Successfully generated source files!");
        }
    }

    public async Task GenerateFromTemplate(string templatePath, string folderPath, string outputFilename)
    {
        var generator = new TemplateGenerator();
        var session = generator.GetOrCreateSession();
        foreach (var placeholder in placeholders)
        {
            session.Add(placeholder.Key, placeholder.Value);
        }

        //TODO: first parse template and check for errors, after that only generate files
        await generator.ProcessTemplateAsync(templatePath, Path.Combine(folderPath, outputFilename));
        if (generator.Errors.Count != 0)
        {
            throw new Exception($"Found errors in {outputFilename}.tt");
        }
    }

    public void CreateFolder(string path, string name, out DirectoryInfo folder)
    {
        var folderPath = Path.Combine(path, name);
        folder = !Directory.Exists(folderPath) ? Directory.CreateDirectory(folderPath) : new DirectoryInfo(folderPath);
    }
}