using System.Text.RegularExpressions;
using NJsonSchema.CodeGeneration.TypeScript;
using NSwag.CodeGeneration.TypeScript;
using NSwag.Generation;

namespace Startup.Documentation;

public static class GenerateTypescriptClient
{
    public static async Task GenerateTypeScriptClient(this WebApplication app, string path)
    {
        var document = await app.Services.GetRequiredService<IOpenApiDocumentGenerator>()
            .GenerateAsync("v1");

        var settings = new TypeScriptClientGeneratorSettings
        {
            Template = TypeScriptTemplate.Fetch,
            TypeScriptGeneratorSettings =
            {
                TypeStyle = TypeScriptTypeStyle.Interface,
                DateTimeType = TypeScriptDateTimeType.Date,
                NullValue = TypeScriptNullValue.Undefined,
                TypeScriptVersion = 5.2m,
                GenerateCloneMethod = false,
                MarkOptionalProperties = true
            }
        };

        var generator = new TypeScriptClientGenerator(document, settings);
        var code = generator.GenerateFile();

        // Use regex to remove the BaseDto interface
        var regex = new Regex(@"export interface BaseDto\s*{[^}]*}", RegexOptions.Multiline);
        var cleanedCode = regex.Replace(code, string.Empty);

        // Split cleaned code into lines for further processing
        var lines = cleanedCode.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();

        // Add the import at the top
        lines.Insert(0, "import { BaseDto } from 'ws-request-hook';");

        // Log the lines after modification (optional)
        app.Services.GetRequiredService<ILogger<Program>>()
            .LogInformation("Lines after modification:\n" + string.Join(Environment.NewLine, lines));

        var modifiedCode = string.Join(Environment.NewLine, lines);

        var outputPath = Path.Combine(Directory.GetCurrentDirectory() + path);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        await File.WriteAllTextAsync(outputPath, modifiedCode);

        app.Services.GetRequiredService<ILogger<Program>>()
            .LogInformation("TypeScript client generated at: " + outputPath);
    }
}
