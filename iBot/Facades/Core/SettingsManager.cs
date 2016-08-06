using System.Linq;
using IBot.Facades.TranslationExtensions;
using CoreSettingsManager = IBot.Core.SettingsManager;
using CoreConnectionSettings = IBot.Core.Settings.ConnectionSettings;
using FacadeConnectionSettings = IBot.Facades.Core.Settings.ConnectionSettings;
using CoreGeneralSettings = IBot.Core.Settings.GeneralSettings;
using FacadeGeneralSettings = IBot.Facades.Core.Settings.GeneralSettings;

namespace IBot.Facades.Core
{
    public class SettingsManager
    {
        public static Result<FacadeConnectionSettings> GetConnectionSettings()
        {
            var result = new Result<FacadeConnectionSettings>("Unable to load Connection settings.", 400,
                new FacadeConnectionSettings());

            var settings = CoreSettingsManager.GetSettings<CoreConnectionSettings>();
            if (settings != null)
                result = new Result<FacadeConnectionSettings>("Connection settings successfully loaded.", 0,
                    settings.ToFacadeSettings());

            return result;
        }

        public static Result<FacadeConnectionSettings> SaveConnectionSettings(FacadeConnectionSettings settings)
        {
            var result = new Result<FacadeConnectionSettings>("Unable to save Connection settings.", 400,
                settings);

            var fullSettings = CoreSettingsManager.GetSettings<CoreConnectionSettings>();
            CopyValues(fullSettings, settings);

            if (CoreSettingsManager.SetSettings(fullSettings))
            {
                result = new Result<FacadeConnectionSettings>("Connection settings successfully saved.", 0,
                    fullSettings.ToFacadeSettings());
            }

            return result;
        }

        public static Result<FacadeGeneralSettings> GetGeneralSettings()
        {
            var result = new Result<FacadeGeneralSettings>("Unable to load General settings.", 400,
                new FacadeGeneralSettings());

            var settings = CoreSettingsManager.GetSettings<CoreGeneralSettings>();
            if (settings != null)
                result = new Result<FacadeGeneralSettings>("General settings successfully loaded.", 0,
                                                           settings.ToFacadeSettings());

            return result;
        }

        public static Result<FacadeGeneralSettings> SaveConnectionSettings(FacadeGeneralSettings settings)
        {
            var result = new Result<FacadeGeneralSettings>("Unable to save General settings.", 400,
                                                           settings);

            var fullSettings = CoreSettingsManager.GetSettings<CoreGeneralSettings>();
            CopyValues(fullSettings, settings);

            if (CoreSettingsManager.SetSettings(fullSettings))
            {
                result = new Result<FacadeGeneralSettings>("General settings successfully saved.", 0,
                                                           fullSettings.ToFacadeSettings());
            }

            return result;
        }

        #region Helpers
        public static string GetOwnerChannel() => CoreSettingsManager.GetOwnerChannel();
        #endregion

        private static void CopyValues<T, T2>(T target, T2 source)
        {
            var tTarget = typeof(T);
            var tSource = typeof(T2);

            var targetProperties = tTarget.GetProperties().Where(prop => prop.CanRead && prop.CanWrite).ToList();
            var sourceProperties = tSource.GetProperties().Where(prop => prop.CanRead && prop.CanWrite).ToList();

            foreach (var prop in targetProperties)
            {
                var find = sourceProperties.Find(info => info.Name == prop.Name);
                var value = find?.GetValue(source, null);
                if (value != null)
                    prop.SetValue(target, value, null);
            }
        }
    }
}
