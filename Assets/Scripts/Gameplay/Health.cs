using GrishaGuWorkshop;

namespace DiceMiner.Gameplay
{
    public class Health : GameObjectBehaviour
    {
        public int MaxHp { get; private set; }
        public int Hp => MaxHp - _markedDamage;
        private int _markedDamage;
        public bool TryMakeDamage(int damage)
        {
            _markedDamage = damage;
            return true;

        }
    }
}