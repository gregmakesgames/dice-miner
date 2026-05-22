using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace GrishaGuWorkshop
{
    public class DataEntity
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        public List<DataEntityTag> tags = new();

        public T GetTag<T>() where T : DataEntityTag
        {
            return tags.FirstOrDefault(tag => tag is T) as T;
        }

        public DataEntity()
        {
            
        }
    }
}
