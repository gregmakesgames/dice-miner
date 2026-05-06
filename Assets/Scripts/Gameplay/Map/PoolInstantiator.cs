using UnityEngine;

namespace DiceMiner.Gameplay.Map
{
    public class PoolInstantiator : MonoBehaviour
    {
        [SerializeField] private Tile tilePrefab;
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            
            var tilePoolObject = new GameObject("[TilePool]");
            tilePoolObject.transform.SetParent(transform);
            var tilePool = new TilePool(tilePrefab, tilePoolObject.transform);
        }
    }
}