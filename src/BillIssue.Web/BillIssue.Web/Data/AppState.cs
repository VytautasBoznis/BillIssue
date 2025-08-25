using BillIssue.Data.Enums;

namespace BillIssue.Web.Data
{
    public class AppState
    {
        public string ColorTheme { get; set; } = "light";
        public string Direction { get; set; } = "ltr";
        public string NavigationStyles { get; set; } = "vertical";
        public string MenuStyles { get; set; } = "vertical";
        public string LayoutStyles { get; set; } = "default-menu";
        public string PageStyles { get; set; } = "regular";
        public string WidthStyles { get; set; } = "fullwidth";
        public string MenuPosition { get; set; } = "fixed";
        public string HeaderPosition { get; set; } = "fixed";
        public string MenuColor { get; set; } = "dark";
        public string HeaderColor { get; set; } = "light";
        public string ThemePrimary { get; set; } = "";
        public string ThemeBackground { get; set; } = "";
        public string BackgroundImage { get; set; } = "";
        public LanguageTypeEnum Language { get; set; } = LanguageTypeEnum.English;
    }
}
