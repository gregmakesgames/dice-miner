using System.Collections.Generic;

namespace DiceMiner.Gameplay
{
    public class FieldController
    {
        private List<FieldEntity> _entities;
        public IReadOnlyList<FieldEntity> Entities => _entities;
    }
}