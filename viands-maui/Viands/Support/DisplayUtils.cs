namespace Viands.Support
{
    public static class DisplayUtils
    {
        public static bool InListEditMode = false;
        public static bool InItemEditMode = false;

        private static bool showNavButtonTitles = false;
        public static bool ShowNavButtonTitles
        {
            get
            {
                return showNavButtonTitles;
            }
            set
            {
                showNavButtonTitles = value;
                if (showNavButtonTitles) {
                    GlobalCallbacks.Trigger(GlobalCallbacks.CBKeys.ShowNavButtonTitles, null);
                }
                else
                {
                    GlobalCallbacks.Trigger(GlobalCallbacks.CBKeys.HideNavButtonTitles, null);
                }
            }
        }

        private static bool showEditorButtonTitles = false;
        public static bool ShowEditorButtonTitles
        {
            get
            {
                return showEditorButtonTitles;
            }
            set
            {
                showEditorButtonTitles = value;
                if (showEditorButtonTitles)
                {
                    GlobalCallbacks.Trigger(GlobalCallbacks.CBKeys.ShowEditorButtonTitles, null);
                }
                else
                {
                    GlobalCallbacks.Trigger(GlobalCallbacks.CBKeys.HideEditorButtonTitles, null);
                }
            }
        }

        public const int MinPopupWidth = 200;
        public const int MinPopupHeight = 300;
        public const int MaxPopupWidth = 500;
        public const int MaxPopupHeight = 700;

        public const int ScrollWhenHeightUnder = 400;

        public static Size GetWindowSize()
        {
            var app = Application.Current as App;
            return app?.GetWindowSize() ?? Size.Zero;
        }

        public static Size GetWindowOrPopupSize()
        {
            var app = Application.Current as App;
            return app?.GetWindowSize() ?? new Size(MinPopupWidth, MinPopupHeight);
        }


        public static Size GetLoginSize()
        {
            return new Size(400, 400);
        }

        public static List<Action<Size>> OnDisplayChangedCallbacks = new List<Action<Size>>();

        public static void ExitEditModes()
        {
            InListEditMode = false;
            InItemEditMode = false;
            GlobalCallbacks.Trigger(GlobalCallbacks.CBKeys.EditListModeExited, null);
            GlobalCallbacks.Trigger(GlobalCallbacks.CBKeys.EditItemsModeExited, null);
        }

        public static void ToggleEditListMode() 
        {
            InListEditMode = !InListEditMode;
            GlobalCallbacks.Trigger(GlobalCallbacks.CBKeys.EditListModeToggled, InListEditMode);
        }

        public static void ToggleEditItemsMode()
        {
            InItemEditMode = !InItemEditMode;
            GlobalCallbacks.Trigger(GlobalCallbacks.CBKeys.EditItemsModeToggled, InItemEditMode);
        }
    }
}
