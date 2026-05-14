namespace DiceMiner.Localization
{
    public class Language
    {
        public string Name { get; }
        public string LocalizationKey { get; }

        public Language(string locKey)
        {
            LocalizationKey = locKey;
            Name = locKey;
        }
        
    }
}