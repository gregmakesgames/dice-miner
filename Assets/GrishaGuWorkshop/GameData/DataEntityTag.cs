using System;
using System.Collections.Generic;
using System.Linq;

namespace GrishaGuWorkshop
{
    [Serializable]
    public abstract class DataEntityTag
    {
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
