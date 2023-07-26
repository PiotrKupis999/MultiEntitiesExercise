using Unity.Entities;
using UnityEngine;

public struct PlayerSpawner : IComponentData
{
    public Entity Player;
}

[DisallowMultipleComponent]
public class PlayerSpawnerAuthoring : MonoBehaviour
{
    public GameObject Player;

    class Baker : Baker<PlayerSpawnerAuthoring>
    {
        public override void Bake(PlayerSpawnerAuthoring authoring)
        {
            PlayerSpawner _component = default(PlayerSpawner);
            _component.Player = GetEntity(authoring.Player);
            AddComponent(_component);
        }
    }
}