using BillIssue.Web.Data;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace BillIssue.Web.Services
{
    public class StateService
    {
        private readonly IJSRuntime _jsRuntime;
        private const string LocalStorageStateKey = "themeSettings";

        private const string DefaultMenuSetting = "default-menu";
        private const string MenuStyleSetting = "menu-click";

        private AppState? _currentState;

        public async Task<AppState> GetAppState()
        {
            if (_currentState == null)
            {
                AppState? loadedState = await LoadState();

                if (loadedState == null)
                {
                    _currentState = new AppState();
                }
                else
                {
                    _currentState = loadedState;
                }
            }

            return _currentState;
        }

        public event Action OnChange;

        public StateService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            OnChange = () => { };
        }

        // Event to notify subscribers about state changes
        public event Action? OnStateChanged;

        private void NotifyStateChanged() => OnStateChanged?.Invoke();

        public async Task ChangeColorTheme(bool firstLoad = false)
        {
            AppState currentState = await GetAppState();
            string targetTheme = currentState.ColorTheme;

            if (!firstLoad)
            {
                targetTheme = currentState.ColorTheme == "light" ? "dark" : "light";
            }

            currentState.ColorTheme = targetTheme;
            await _jsRuntime.InvokeVoidAsync("interop.setclearCssVariables");

            await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-theme-mode", targetTheme);
            await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-header-styles", targetTheme);
            await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-menu-styles", targetTheme);
            if (targetTheme == "light")
            {
                await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-menu-styles", "dark");
            }
            else
            {
                await MenuColorFn(currentState, targetTheme);
            }
            await _jsRuntime.InvokeVoidAsync("interop.removeAttributeFromHtml", "--body-bg-rgb");
            await _jsRuntime.InvokeVoidAsync("interop.removeAttributeFromHtml", "--body-bg-rgb2");
            await _jsRuntime.InvokeVoidAsync("interop.removeAttributeFromHtml", "--light-rgb");
            await _jsRuntime.InvokeVoidAsync("interop.removeAttributeFromHtml", "--form-control-bg");
            await _jsRuntime.InvokeVoidAsync("interop.removeAttributeFromHtml", "--input-border");

            await HeaderColorFn(currentState, targetTheme);
            await NavigationStylesFn();
            NotifyStateChanged();

            await PersistState();
        }

        public async Task MenuColorFn(AppState state, string val)
        {
            state.MenuColor = val; // Update the color theme in the app state
            await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-menu-styles", val);
            NotifyStateChanged();
        }

        public async Task HeaderColorFn(AppState state, string val)
        {
            state.HeaderColor = val; // Update the color theme in the app state
            await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-header-styles", val);
            NotifyStateChanged();
        }

        public async Task MenuStylesFn(AppState state, string val)
        {
            state.MenuStyles = val; // Update the color theme in the app state
            await _jsRuntime.InvokeVoidAsync("interop.removeAttributeFromHtml", "data-vertical-style");
            await _jsRuntime.InvokeVoidAsync("interop.removeAttributeFromHtml", "data-hor-style");
            await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-nav-style", val);
            await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-toggled", $"{val}-closed");
            NotifyStateChanged();
        }

        public async Task NavigationStylesFn()
        {
            await _jsRuntime.InvokeVoidAsync("interop.addAttributeToHtml", "data-vertical-style", "overlay");
            await _jsRuntime.InvokeVoidAsync("interop.removeAttributeFromHtml", "data-nav-style");

            if (await _jsRuntime.InvokeAsync<int>("interop.inner", "innerWidth") > 992)
            {
                await _jsRuntime.InvokeVoidAsync("interop.removeAttributeFromHtml", "data-toggled");
            }
            NotifyStateChanged();
        }

        public async Task RetrieveFromLocalStorage()
        {
            AppState currentState = await GetAppState();

            await MenuStylesFn(currentState, MenuStyleSetting);
            await ChangeColorTheme(true);
        }

        private async Task<AppState?> LoadState()
        {
            string appStateJson = await _jsRuntime.InvokeAsync<string>("interop.getLocalStorageItem", LocalStorageStateKey);
            AppState? loadedAppState = null;

            try
            {
                loadedAppState = JsonConvert.DeserializeObject<AppState>(appStateJson);
            }
            catch (Exception ex)
            {
                return loadedAppState;
            }

            return loadedAppState;
        }

        private async Task PersistState()
        {
            AppState state = await GetAppState();
            await _jsRuntime.InvokeAsync<string>("interop.setLocalStorageItem", [LocalStorageStateKey, JsonConvert.SerializeObject(state)]);
        }
    }
}