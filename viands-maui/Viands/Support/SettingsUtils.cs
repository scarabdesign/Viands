using Viands.Data;

namespace Viands.Support
{

    public static class SettingsUtils
    {
        public static List<Action> OnSettingsChangedCallbacks = new List<Action>();

        public enum SettingTypes
        {
            FontSize,
            ToolsPanelClosed,
            BackupToServer,
            ShowToolsHelper
        }

        public static object[] SettingDefaults = new object[]
        {
            "1",
            false,
            true,
            false
        };

        public const int FontSizeMax = 400;
        public const int FontSizeMin = 75;
        public const int FontSizeConvMax = 13;
        public const int FontSizeConvMin = 0;

        public async static Task SetUpDefaults(string owner_id)
        {
            await Settings.UpsertSettings(new List<v_settings> { 
                new v_settings
                {
                    owner_id = owner_id,
                    key = Enum.GetName(typeof(SettingTypes), (int)SettingTypes.FontSize),
                    value = SettingDefaults[(int)SettingTypes.FontSize].ToString()
                },
                new v_settings
                {
                    owner_id = owner_id,
                    key = Enum.GetName(typeof(SettingTypes), (int)SettingTypes.ToolsPanelClosed),
                    value = (bool)SettingDefaults[(int)SettingTypes.ToolsPanelClosed] == true ? "true" : "false"
                },
                new v_settings
                {
                    owner_id = owner_id,
                    key = Enum.GetName(typeof(SettingTypes), (int)SettingTypes.BackupToServer),
                    value = (bool)SettingDefaults[(int)SettingTypes.BackupToServer] == true ? "true" : "false"
                },
                new v_settings
                {
                    owner_id = owner_id,
                    key = Enum.GetName(typeof(SettingTypes), (int)SettingTypes.ShowToolsHelper),
                    value = (bool)SettingDefaults[(int)SettingTypes.ShowToolsHelper] == true ? "true" : "false"
                }
            });
        }

        public async static Task<List<v_settings>> GetUserSettings(string owner_id)
        {
            return await Settings.GetSettings(owner_id);
        }

        public async static Task<int> GetConvFontInt()
        {
            var globalFontSize = await GetStringSetting(SettingTypes.FontSize);
            int.TryParse(globalFontSize, out int globalFontSizeInteger);
            return globalFontSizeInteger;
        }

        public async static Task<string> GetStringSetting(SettingTypes settingType)
        {
            var setting = await GetSetting(settingType);
            return setting?.value ?? SettingDefaults[(int)settingType].ToString();
        }

        public async static Task<bool> GetBoolSetting(SettingTypes settingType)
        {
            var setting = await GetSetting(settingType);
            var value = SettingDefaults[(int)settingType].ToString();
            if (setting != null && setting.value != null)
            {
                value = setting.value;
            }
            return Convert.ToBoolean(value);
        }

        public async static void SetBoolSetting(SettingTypes settingType, bool val)
        {
            SetSetting(settingType, val.ToString(), await LoginUtils.GetCurrentUserApiKey());
        }

        public static string GetConvFontString(int val)
        {
            return ((val * 25) + 75) + "%";
        }

        public async static Task<v_settings> GetSetting(SettingTypes type)
        {
            var usersettings = await GetUserSettings(await LoginUtils.GetCurrentUserApiKey());
            if (usersettings.Count() == 0) return null;
            return usersettings.Where(s => s.key == Enum.GetName(type)).FirstOrDefault();
        }

        public async static void SetSetting(SettingTypes type, string settingValue, string owner_id)
        {
            var UserSettings = await Settings.GetSettingByKey(owner_id, Enum.GetName(type));
            var setting = UserSettings ?? new v_settings
            {
                key = Enum.GetName(type),
                owner_id = owner_id
            };

            if (setting.value == settingValue)
            {
                return;
            }

            setting.value = settingValue;
            await Settings.UpsertSetting(setting);
        }
    }
}
