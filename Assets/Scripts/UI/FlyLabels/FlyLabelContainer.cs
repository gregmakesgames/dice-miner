using UnityEngine;

namespace DiceMiner.UI.FlyLabels
{
    public class FlyLabelContainer : MonoBehaviour
    {
        private static FlyLabelContainer _instance;
        public static FlyLabelContainer Instance => _instance ??= CreateInstance();
        
        [SerializeField] 
        private RectTransform defaultAnchor;
        
        public RectTransform DefaultAnchor => defaultAnchor;

        private static FlyLabelContainer CreateInstance()
        {
            var prefab = Resources.Load<GameObject>("UI/FlyLabelContainer");
            
            var instanceObject = Instantiate(prefab);
            var instance = instanceObject.GetComponent<FlyLabelContainer>();
            
            DontDestroyOnLoad(instance.gameObject);
            
            return instance;
        }
    }
}