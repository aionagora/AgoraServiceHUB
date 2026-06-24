using MudBlazor;

namespace AgoraHub360.ERP.Web.Theme;

/// <summary>
/// Tema corporativo mínimo AgoraHub360 ERP.
/// Versión estable sin Shadows, Typography ni LayoutProperties complejos.
/// </summary>
public static class AgoraMudTheme
{
    public static MudTheme Create()
    {
        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#1B3A5C",
                Secondary = "#16A085",
                Tertiary = "#E67E22",
                Info = "#3B82F6",
                Success = "#22C55E",
                Warning = "#E67E22",
                Error = "#EF4444",
                Background = "#F5F7FA",
                Surface = "#FFFFFF",
                DrawerBackground = "#FFFFFF",
                TextPrimary = "#1F2937",
                TextSecondary = "#6B7280",
                TextDisabled = "#9CA3AF",
                DrawerText = "#1F2937",
                AppbarBackground = "#FFFFFF",
                AppbarText = "#1F2937",
                DrawerIcon = "#4A5568",
                Divider = "#E5E7EB",
                OverlayDark = "rgba(0,0,0,0.35)",
                LinesDefault = "#E5E7EB",
                LinesInputs = "#D1D5DB",
                ActionDefault = "#4A5568",
                ActionDisabled = "#9CA3AF",
                TableLines = "#E5E7EB",
                TableStriped = "#F9FAFB",
            }
        };
    }
}
