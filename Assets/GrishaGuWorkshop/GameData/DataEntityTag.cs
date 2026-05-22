using System;
using System.Collections.Generic;
using System.Linq;
using GrishaGuWorkshop.Converters;
using Newtonsoft.Json;

namespace GrishaGuWorkshop
{
    [Serializable]
    public abstract class DataEntityTag
    {
        [JsonProperty(ItemConverterType = typeof(DataEntityTagJsonConverter))]
        public List<DataEntityTag> tags = new();

        public bool HasTag<T>() where T : DataEntityTag
        {
            return tags.Any(tag => tag is T);
        }

        public T GetTag<T>() where T : DataEntityTag
        {
            return tags.FirstOrDefault(tag => tag is T) as T;
        }

        public IEnumerable<T> GetAllTags<T>() where T : DataEntityTag
        {
            return tags.OfType<T>();
        }
    }
}
