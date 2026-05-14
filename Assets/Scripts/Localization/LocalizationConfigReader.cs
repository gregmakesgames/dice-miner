using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace DiceMiner.Localization
{
    public class LocalizationConfigReader
    {
        private Dictionary<string, Dictionary<string, string>> _translations = new Dictionary<string, Dictionary<string, string>>()
        {
            {"en", new Dictionary<string, string>()},
            {"ru", new Dictionary<string, string>()},
        };
        
        public void ReadLocsfromConfig()
        {
            _translations.Clear();
            foreach (var elem in LanguageProvider.GetAvailableLanguages())
            {
                _translations.Add(elem.LocalizationKey,  new Dictionary<string, string>());
            }
            
            // here read all translations;
        }
        
        public Dictionary<string, string> GetTranslation(Language language)
        {
            return _translations[language.LocalizationKey];
        }
    }
    
    public class TranslationRaw
    {
        public string key;
        public string en;
        public string ru;
    }
}