using Scriban;

namespace Chronovault.Packager;

/// <summary>
/// Processes Scriban templates with package configuration values.
/// </summary>
public static class TemplateEngine
{
    /// <summary>
    /// Renders a template file with the provided model.
    /// </summary>
    public static string RenderTemplate(string templateContent, PackageModel model)
    {
        var template = Template.Parse(templateContent);
        return template.Render(model);
    }

    /// <summary>
    /// Processes all templates in the Templates directory and outputs to the build folder.
    /// </summary>
    public static void ProcessTemplates(string templatesDir, string outputDir, PackageModel model)
    {
        var templateFiles = Directory.EnumerateFiles(templatesDir, "*.template");

        foreach (var templatePath in templateFiles)
        {
            var templateContent = File.ReadAllText(templatePath);
            var renderedContent = RenderTemplate(templateContent, model);

            // Output file name: remove .template extension and apply naming
            var fileName = Path.GetFileNameWithoutExtension(templatePath);

            // Special handling for different file types
            if (fileName.EndsWith(".xml"))
            {
                fileName = $"{model.ServiceName}.xml";
            }
            else if (fileName.EndsWith(".bat"))
            {
                var batName = fileName.Replace(".bat", "");
                fileName = $"{model.ServiceName}.{batName}.bat";
            }
            else if (fileName.EndsWith(".md"))
            {
                // UsageGuide.md → README.md (standard documentation name)
                fileName = "README.md";
            }

            var outputPath = Path.Combine(outputDir, fileName);
            File.WriteAllText(outputPath, renderedContent);

            Console.WriteLine($"   ✅ Generated: {fileName}");
        }
    }
}

