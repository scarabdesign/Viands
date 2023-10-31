using System.Diagnostics;
using Viands.Data;

namespace Viands.Support
{

    public static class LoginUtils
    {
        public static List<Action> OnLoginChangedCallbacks = new List<Action>();

        public async static Task<string> GetCurrentUserApiKey()
        {
            var user = await GetCurrentUser();
            return user?.apikey;
        }

        public async static Task<v_users> GetCurrentUser()
        {
            return await Users.GetCurrentUser();
        }

        public async static Task UnsetCurrentUser()
        {
            await Users.UnsetCurrentUser();
            GlobalCallbacks.Trigger(GlobalCallbacks.CBKeys.UpdateUser, null);
        }

        public static async void CheckLogin()
        {
            GlobalCallbacks.Trigger(GlobalCallbacks.CBKeys.ShowHideLogin, false);
            var cur = await GetCurrentUser();
            if (cur == null)
            {
                GlobalCallbacks.Trigger(GlobalCallbacks.CBKeys.ShowHideLogin, true);
            }
        }

        public static string GetUserAPIKey(string email)
        {
            //this should change immensely at some point
            return Utils.Base64Encode(email);
        }

        public async static void LogOut()
        {
            await UnsetCurrentUser();
            GlobalCallbacks.Trigger(GlobalCallbacks.CBKeys.UpdateUser, null);
            CheckLogin();
        }

        public async static Task UpdateUser(v_users user, bool isCreate)
        {
            await Users.UpsertUser(user);
            if (isCreate)
            {
                await SettingsUtils.SetUpDefaults(user.apikey);
            }
            var registered = await ViandsService.ViandsRegisterUser(user);
            Debug.WriteLine("registered: " + registered);
        }
    }
}
