namespace Chronovault.Packager.Helpers;

/// <summary>
/// Extension methods for fluent API usage.
/// </summary>
public static class PackageModelExtensions
{
    /// <summary>
    /// Applies templates from the templates directory to the output directory.
    /// </summary>
    public static void ApplyTemplates(this PackageModel model, string templatesDir, string outputDir)
    {
        TemplateEngine.ProcessTemplates(templatesDir, outputDir, model);
    }
}
