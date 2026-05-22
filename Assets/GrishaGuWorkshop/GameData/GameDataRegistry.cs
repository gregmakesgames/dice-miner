using System.Collections.Generic;
using System.Linq;

namespace GrishaGuWorkshop
{
    public class GameDataRegistry
    {
        private List<DataEntity> _entities = new();

        private bool isLoaded;

        public void Load(bool forceReload = false)
        {
            if (isLoaded && !forceReload)
            {
                return;
            }

            _entities = GameDataIO.LoadEntities();
            isLoaded = true;
        }

        public T Get<T>(string id) where T : DataEntity
        {
            Load();

            return _entities.OfType<T>().FirstOrDefault(x => x.Id == id);
        }

        public List<T> GetAll<T>() where T : DataEntity
        {
            Load();
            return _entities.OfType<T>().ToList();
        }

        public List<DataEntity> GetAllWithTag<TG>() where TG : DataEntityTag
        {
            Load();
            return _entities.Where(x => x.GetTag<TG>() != null).ToList();
        }

        public List<T> GetAllOfTypeWithTag<T, TG>() where TG : DataEntityTag where T : DataEntity
        {
            Load();
            return _entities.OfType<T>().Where(x => x.GetTag<TG>() != null).ToList();
        }
    }
}
