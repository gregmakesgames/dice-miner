using UnityEngine;

public sealed class GameDataDemo : MonoBehaviour
{
    [SerializeField] private string configType = "EnemyShip";
    [SerializeField] private string configId = "shark";

    private void Start()
    {
        ConfigEntity entity = GameDataRegistry.Get(configType, configId);
        if (entity == null)
        {
            Debug.LogWarning($"GameDataDemo: config '{configType}:{configId}' not found.");
            return;
        }

        float speed = entity.GetFloat("speed");
        Vector3 offset = entity.GetVector3("spawnOffset");
        Color tint = entity.GetColor("tintColor");
        ConfigEntity movement = entity.GetRef("movement");
        string movementId = movement != null ? movement.Id : "<none>";

        Debug.Log($"GameDataDemo: speed={speed}, offset={offset}, tint={tint}, movementRef={movementId}");
    }
}
