using MudBlazor;

namespace TaskFlow.Presentation;

/// <summary>
/// Central MudBlazor theme configuration aligned with visual guide tokens.
/// </summary>
public static class TaskFlowTheme
{
    /// <summary>
    /// Gets TaskFlow brand theme.
    /// </summary>
    public static MudTheme Theme { get; } = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#1F86FF",
            Secondary = "#8A2BE2",
            Info = "#2D9CFF",
            Success = "#30C76A",
            Warning = "#F2B01E",
            Error = "#EF4B6C",
            Background = "#F7F8FA",
            Surface = "#FFFFFF",
            AppbarBackground = "#FFFFFF",
            DrawerBackground = "#F7F8FA",
            TextPrimary = "#151821",
            TextSecondary = "#3A3F49",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#2D9CFF",
            Secondary = "#D34FAF",
            Info = "#2D9CFF",
            Success = "#30C76A",
            Warning = "#F2B01E",
            Error = "#EF4B6C",
            Background = "#111317",
            Surface = "#1B1E24",
            AppbarBackground = "#1B1E24",
            DrawerBackground = "#1B1E24",
            DrawerText = "#E9ECF1",
            TextPrimary = "#E9ECF1",
            TextSecondary = "#9FA6B2",
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "12px",
        },
    };
}
