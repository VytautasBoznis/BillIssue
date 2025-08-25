using BillIssue.Interfaces.Multilanguage;
using BillIssue.Web.Data;
using Nito.AsyncEx;

namespace BillIssue.Web.Services
{
    public class MultilanguageService
    {
        private readonly StateService _stateService;
        private readonly IMultilanguageFacade _multilanguageFacade;

        public MultilanguageService(StateService stateService, IMultilanguageFacade multilanguageFacade)
        {
            _stateService = stateService;
            _multilanguageFacade = multilanguageFacade;
        }

        public async Task<string> GetTranslation(string multilanguageKey)
        {
            AppState appState = await _stateService.GetAppState();
            return await _multilanguageFacade.Get(multilanguageKey, languageType: appState.Language);
        }

        public async Task<Dictionary<string, string>> GetTranslations(string[] multilanguageKeys)
        {
            AppState appState = await _stateService.GetAppState();
            Dictionary<string, Task<string>> translationTasks = new Dictionary<string, Task<string>>();

            foreach(string key in multilanguageKeys)
            {
                translationTasks.Add(key, _multilanguageFacade.Get(key, languageType: appState.Language));
            }

            await translationTasks.Select(t => t.Value).WhenAll();
            return translationTasks.ToDictionary(tt => tt.Key, tt => tt.Value.Result);
        }

    }
}
