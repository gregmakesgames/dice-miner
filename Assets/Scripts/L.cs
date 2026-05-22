using System.Collections.Generic;
using System.Text.RegularExpressions;
using DiceMiner.Localization;
using UnityEngine;

namespace DiceMiner
{
    public static class L
    {
        private const string DEFAULT_LANGUAGE = "en";
        private const string SELECTED_LANGUAGE_PP_KEY = "selected_language";
        
        private static Language _currentLanguage;
        private static Dictionary<string, string> _translation;
        
        private static LocalizationConfigReader _transactionConfigReader;

        private static bool _isInitialized = false;
        
        private static void EnsureInited()
        {
            if (_isInitialized) return;
            
            _transactionConfigReader = new LocalizationConfigReader();
            _transactionConfigReader.ReadLocsfromConfig();
            _translation = new Dictionary<string, string>();
            _isInitialized = true;
            var savedInPrefsLanguage = PlayerPrefs.GetString(SELECTED_LANGUAGE_PP_KEY, DEFAULT_LANGUAGE);
            SetLanguage(savedInPrefsLanguage);
        }
        
        public static string Get(string key, params string[] replaces)
        {
            EnsureInited();
            
            if (_translation.ContainsKey(key))
            {
                var result = _translation[key];

                for (int i = 1; i <= replaces.Length; i++)
                {
                    result = result.Replace("{" + i + "}", replaces[i]);
                }
                
                return result;
            }

            
            UnityEngine.Debug.LogWarning($"Localization key missing (locale: {_currentLanguage}: {key}");

            return key;
        }
        
        public static void SetLanguage(string languageCode)
        {
            SetLanguage(LanguageProvider.GetLanguage(languageCode));
        }
        
        public static void SetLanguage(Language language)
        {
            EnsureInited();
            _currentLanguage = language;
            _translation = _transactionConfigReader.GetTranslation(_currentLanguage);
        }
    }
}