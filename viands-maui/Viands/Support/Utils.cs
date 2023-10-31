using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Viands.Support
{

    public static class ErrorHandler
    {
        public static void LogError(Exception e)
        {
            Debug.WriteLine(e.ToString());
        }
    }

    public static class Utils
    {

        public static string ToTitleCase(string input)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            var titleCase = textInfo.ToTitleCase(input.ToLowerInvariant());
            return titleCase;
        }

        public static string CapitalizeSentence(string input)
        {
            var tolower = input.ToLower();
            var parts = tolower.Split(".");
            parts.ToList().ForEach(x => Regex.Replace(x, "^[a-z]", c => c.Value.ToUpperInvariant()));
            return string.Join(".", parts);
        }

        public static string ShortDate(DateTime dt)
        {
            return dt.ToString("MM/dd/yy");
        }

        public static string Base64Encode(string text)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(text));
        }
        
        public static string Base64Decode(string base64EncodedData)
        {
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedData));
        }

        public class CancelableWait
        {
            public Task CurrentWait;
            public CancellationTokenSource CancelWait;

            public void Cancel()
            {
                CancelWait?.Cancel(false);
                CancelWait = null;
            }

            public void KillWait()
            {
                CurrentWait?.Dispose();
                CurrentWait = null;
            }

            public async void Wait(int milliseconds, Action callback = null)
            {
                Action _callback = () =>
                {
                    callback.Invoke();
                    Cancel();
                    KillWait();
                };

                if (CurrentWait != null && CancelWait != null)
                {
                    Cancel();
                    KillWait();
                }
                if (milliseconds == -1)
                {
                    return;
                }
                CancelWait = new CancellationTokenSource();
                CurrentWait = Task.Delay(milliseconds, CancelWait.Token);
                try
                {
                    await CurrentWait;
                }
                catch (TaskCanceledException)
                {
                    KillWait();
                    return;
                }
                catch (Exception err)
                {
                    Console.WriteLine(err);
                }

                if (callback != null)
                {
                    _callback();
                }
            }
        }
    }
}
