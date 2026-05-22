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

        public DataEntity()
        {
            
        }
    }
}
