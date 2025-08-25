using BillIssue.Web.Data;
using BillIssue.Web.Domain.Constants;

namespace BillIssue.Web.Services
{
    public class MenuDataService
    {
        public readonly MultilanguageService _multilanguageService;
        public readonly string[] UsedTranslationKeys;

        public MenuDataService(MultilanguageService multilanguageService) 
        {
            _multilanguageService = multilanguageService;

            UsedTranslationKeys = [ MultilanguageKeyConstants.SharedMainMenuTitle,
                MultilanguageKeyConstants.MeniuTimeLoggingText,
                MultilanguageKeyConstants.MeniuCompanyWorkspaceText,
                MultilanguageKeyConstants.MeniuCompanyUsersText,
                MultilanguageKeyConstants.MeniuProjectsText,
                MultilanguageKeyConstants.MeniuProjectOverviewText,
                MultilanguageKeyConstants.MeniuProjectUsersText
            ];
        }

        public async Task<List<MainMenuItems>> GetMenuData()
        {
            return await BuildMainMenu();
        }

        private async Task<List<MainMenuItems>> BuildMainMenu()
        {
            string mainMenuTitle = await _multilanguageService.GetTranslation(MultilanguageKeyConstants.SharedMainMenuTitle);

            Dictionary<string, string> multilanguageTranslations = await _multilanguageService.GetTranslations(UsedTranslationKeys);

            return new List<MainMenuItems>()
            {
                new MainMenuItems(
                    menuTitle: multilanguageTranslations.FirstOrDefault(mt => mt.Key == MultilanguageKeyConstants.SharedMainMenuTitle).Value
                ),
                new MainMenuItems(
                    type: "sub",
                    title: multilanguageTranslations.FirstOrDefault(mt => mt.Key == MultilanguageKeyConstants.MeniuTimeLoggingText).Value,
                    icon: "bx bx-timer",
                    selected: false,
                    active: false,
                    children: new MainMenuItems[]
                    {
                      new MainMenuItems (
                            path: "/time-logging",
                            type: "link",
                            title: multilanguageTranslations.FirstOrDefault(mt => mt.Key == MultilanguageKeyConstants.MeniuTimeLoggingText).Value,
                            selected: false,
                            active: false,
                            dirChange: false
                      ),
                    }
                ),
                new MainMenuItems(
                    type: "sub",
                    title: multilanguageTranslations.FirstOrDefault(mt => mt.Key == MultilanguageKeyConstants.MeniuCompanyWorkspaceText).Value,
                    icon: "bx bx-briefcase-alt-2",
                    selected: false,
                    active: false,
                    dirChange: false,
                    children: new MainMenuItems[]
                    {
                      new MainMenuItems (
                            path: "/company-workspace-list",
                            type: "link",
                            icon: "bx cog",
                            title: multilanguageTranslations.FirstOrDefault(mt => mt.Key == MultilanguageKeyConstants.MeniuCompanyWorkspaceText).Value,
                            selected: false,
                            active: false,
                            dirChange: false
                      ),
                      new MainMenuItems (
                            path: "/company-workspace-users",
                            type: "link",
                            icon: "bx group",
                            title: multilanguageTranslations.FirstOrDefault(mt => mt.Key == MultilanguageKeyConstants.MeniuCompanyUsersText).Value,
                            selected: false,
                            active: false,
                            dirChange: false
                      ),
                    }
                ),
                new MainMenuItems(
                    type: "sub",
                    title: multilanguageTranslations.FirstOrDefault(mt => mt.Key == MultilanguageKeyConstants.MeniuProjectsText).Value,
                    icon: "bx bx-spreadsheet",
                    selected: false,
                    active: false,
                    dirChange: false,
                    children: new MainMenuItems[]
                    {
                      new MainMenuItems (
                            path: "/project-overview",
                            type: "link",
                            icon: "bx cog",
                            title: multilanguageTranslations.FirstOrDefault(mt => mt.Key == MultilanguageKeyConstants.MeniuProjectOverviewText).Value,
                            selected: false,
                            active: false,
                            dirChange: false
                      ),
                    }
                )
            };
        }
    }
}