using System;
using System.Collections.Generic;

namespace GrishaGuWorkshop
{
    public class SavedGame
    {
        public Dictionary<string, int> ints = new();
        public Dictionary<string, float> floats = new();
        public Dictionary<string, string> strings = new();
        public Dictionary<string, bool> bools = new();

        public static SavedGame New()
        {
            return new SavedGame();
        }

        public void SaveInt(string key, int value)
        {
            ints[key] = value;
        }

        public void SaveFloat(string key, float value)
        {
            floats[key] = value;
        }

        public void SaveString(string key, string value)
        {
            strings[key] = value;
        }

        public void SaveBool(string key, bool value)
        {
            bools[key] = value;
        }

        public void SaveEnum<E>(string key, E value) where E : struct, Enum
        {
            strings[key] = value.ToString();
        }

        public int GetInt(string key)
        {
            return ints.TryGetValue(key, out var value) ? value : 0;
        }

        public float GetFloat(string key)
        {
            return floats.TryGetValue(key, out var value) ? value : 0f;
        }

        public string GetString(string key)
        {
            return strings.TryGetValue(key, out var value) ? value : null;
        }

        public bool GetBool(string key)
        {
            return bools.TryGetValue(key, out var value) && value;
        }

        public E GetEnum<E>(string key) where E : struct, Enum
        {
            if (!strings.TryGetValue(key, out var value) || string.IsNullOrEmpty(value))
            {
                return default;
            }

            return Enum.TryParse<E>(value, out var result) ? result : default;
        }
    }
}
