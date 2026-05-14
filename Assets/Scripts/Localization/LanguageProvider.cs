using System.Collections.Generic;
using System.Linq;

namespace DiceMiner.Localization
{
    public static class LanguageProvider
    {
        private static Dictionary<string, Language> _languages = new Dictionary<string, Language>()
            {
                {"ru", new Language("ru")}, 
                {"en", new Language("en")},
            };

        public static Language GetLanguage(string code)
        {
            return _languages[code];
        }

        public static IReadOnlyList<Language> GetAvailableLanguages()
        {
            return _languages.Values.ToList();
        }
    }
}