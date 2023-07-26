using Unity.Entities;
using UnityEngine;

public struct Player : IComponentData
{
}

[DisallowMultipleComponent]
public class PlayerAuthoring : MonoBehaviour
{
    class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            Player _component = default;
            AddComponent(_component);
        }
    }
}