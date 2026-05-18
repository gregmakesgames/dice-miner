using UnityEngine;

namespace GrishaGuWorkshop
{
    public class DestroyAfterTime : MonoBehaviour
    {
        [SerializeField] private float time;

        private float _timeLeft;

        private void Start()
        {
            _timeLeft = time;
        }

        private void Update()
        {
            _timeLeft -= Time.deltaTime;
            if (_timeLeft < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}