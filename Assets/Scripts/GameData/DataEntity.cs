using Newtonsoft.Json;

namespace GameData
{
    public abstract class DataEntity
    {
        [JsonProperty("id")]
        public string Id { get; private set; } = string.Empty;

        public T GetTag<T>() where T : DataEntityTag, new()
        {
            return null;
        }
    }
}
