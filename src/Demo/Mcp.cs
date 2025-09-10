#:property Title=LaTeX to Image MCP
#:property Version=0.5.0
#:package Smith@0.2.5
#:package DotNetConfig.Configuration@1.2.*
#:package ModelContextProtocol@0.3.0-preview.*
#:package Microsoft.Extensions.Http@9.*

using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

var builder = App.CreateBuilder(args);
builder.Configuration.AddDotNetConfig();

var initialized = false;
bool? darkMode = bool.TryParse(builder.Configuration["latex:darkMode"], out var dm) ? dm : null;
string? fontSize = builder.Configuration["latex:fontSize"];
// See https://editor.codecogs.com/docs/4-LaTeX_rendering.php#overview_anchor
var fonts = new Dictionary<string, string>
{
    { "Tiny", "tiny" },
    { "Small", "small" },
    { "Large", "large" },
    { "LARGE", "LARGE" },
    { "Huge", "huge"}
};

builder.Services
    .AddHttpClient()
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTool(
        name: "latex",
        title: "LaTeX to Image",
        description:
            """
            Converts LaTeX equations into markdown-formatted images for inline display.
            Users can use #latex_setprefs tool to set preferences for dark mode and font size.
            Available font sizes: tiny, small, large, LARGE, huge.
            """,
        tool: async (IHttpClientFactory httpFactory, IMcpServer server,
            [Description("The LaTeX equation to render.")] string latex)
            =>
        {
            // On first tool run, we ask for preferences for dark mode and font size.
            if (!initialized)
            {
                initialized = true;
                (darkMode, fontSize) = await SetPreferences(server, darkMode, fontSize);
            }

            var colors = darkMode switch
            {
                true => @"\fg{white}",
                false => @"\fg{black}",
                null => @"\bg{white}\fg{black}"
            };

            var query = WebUtility.UrlEncode(@"\dpi{300}\" + (fontSize ?? "small") + colors + new string([.. latex.Where(c => !char.IsWhiteSpace(c))]));
            var url = $"https://latex.codecogs.com/png.image?{query}";

            using var client = httpFactory.CreateClient();
            using var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var base64 = Convert.ToBase64String(await response.Content.ReadAsByteArrayAsync());
            return $"> ![LaTeX Equation](data:image/png;base64,{base64})";
        })
    .WithTool(
        name: "latex_getprefs",
        title: "Get LaTeX Preferences",
        description: "Gets the saved LaTeX rendering preferences for dark mode and font size.",
        tool: () => new { darkMode, fontSize },
        options: ToolJsonOptions.Default)
    .WithTool(
        name: "latex_setprefs",
        title: "Set LaTeX Preferences",
        description: "Sets the LaTeX rendering preferences for dark mode and font size.",
        tool: async (IMcpServer server,
            [Description("Use dark mode by inverting the colors in the output.")] bool? darkMode = null,
            [Description("Font size to use in the output: tiny=5pt, small=9pt, large=12pt, LARGE=18pt, huge=20pt")] string? fontSize = null)
            => (darkMode, fontSize) = await SetPreferences(server, darkMode, fontSize),
        options: ToolJsonOptions.Default);

await builder.Build().RunAsync();

/// <summary>Saves the LaTeX rendering preferences to configuration.</summary>
async ValueTask<(bool? darkMode, string? fontSize)> SetPreferences(IMcpServer server, bool? darkMode, string? fontSize)
{
    if ((darkMode is null || fontSize is null || !fonts.ContainsValue(fontSize)) && server.ClientCapabilities?.Elicitation != null)
    {
        var result = await server.ElicitAsync(new()
        {
            Message = "Specify LaTeX rendering preferences",
            RequestedSchema = new()
            {
                Required = ["darkMode", "fontSize"],
                Properties =
                {
                    { "darkMode",  new ElicitRequestParams.BooleanSchema()
                        {
                            Title = "Dark Mode",
                            Description = "Use dark mode?",
                            Default = darkMode
                        }
                    },
                    { "fontSize",  new ElicitRequestParams.EnumSchema()
                        {
                            Title = "Font Size",
                            Description = "Font size to use for the LaTeX rendering.",
                            Enum = [.. fonts.Values],
                            EnumNames = [.. fonts.Keys],
                        }
                    },
                },
            }
        });

        if (result.Action == "accept" && result.Content is { } content)
        {
            darkMode = content["darkMode"].GetBoolean();
            fontSize = content["fontSize"].GetString() ?? "tiny";

            DotNetConfig.Config.Build(DotNetConfig.ConfigLevel.Global)
                .GetSection("latex")
                .SetBoolean("darkMode", darkMode.Value)
                .SetString("fontSize", fontSize);
        }
        // action == cancel is not supported in vscode
        // actoin == decline would be equal to "ignore" so we just don't set anything.
        return (darkMode, fontSize);
    }
    else
    {
        // We persist to ~/.netconfig
        var config = DotNetConfig.Config.Build(DotNetConfig.ConfigLevel.Global).GetSection("latex");
        if (darkMode != null)
            config = config.SetBoolean("darkMode", darkMode.Value);
        if (fontSize != null && fonts.ContainsValue(fontSize))
            config = config.SetString("fontSize", fontSize);
        else
            fontSize = null;

        return (darkMode, fontSize);
    }
}