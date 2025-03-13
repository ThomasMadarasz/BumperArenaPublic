using UnityEngine.Localization.Settings;

namespace Utils.Translation
{
    public static class TranslationManager
    {
        public static string Translate(string entry)
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString(entry);
        }
    }
}