using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace GrishaGuWorkshop
{
    public abstract class DataEntity
    {
        [JsonProperty("id")]
        public string Id { get; private set; } = string.Empty;

        public List<DataEntityTag> tags;

        public T GetTag<T>() where T : DataEntityTag
        {
            return tags.FirstOrDefault(tag => tag is T) as T;
        }
    }
}
