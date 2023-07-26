using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct PlayerInput : IInputComponentData
{
    public int Horizontal;
    public int Vertical;
    public int Jump;
}


[DisallowMultipleComponent]
public class PlayerInputAuthoring : MonoBehaviour
{
    

    class Baking : Unity.Entities.Baker<PlayerInputAuthoring>
    {
        public override void Bake(PlayerInputAuthoring authoring)
        {
            AddComponent<PlayerInput>();
        }
    }
}

[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial struct SamplePlayerInput : ISystem
{

    public void OnUpdate(ref SystemState state)
    {
        bool _left = Input.GetKey(KeyCode.A);
        bool _right = Input.GetKey(KeyCode.D);
        bool _down = Input.GetKey(KeyCode.S);
        bool _up = Input.GetKey(KeyCode.W);
        bool _jump = Input.GetKeyDown(KeyCode.Space);

        foreach (var _playerInput in SystemAPI.Query<RefRW<PlayerInput>>().WithAll<GhostOwnerIsLocal>())
        {
            _playerInput.ValueRW = default;
            if (_left)
                _playerInput.ValueRW.Horizontal -= 1;
            if (_right)
                _playerInput.ValueRW.Horizontal += 1;
            if (_down)
                _playerInput.ValueRW.Vertical -= 1;
            if (_up)
                _playerInput.ValueRW.Vertical += 1;
            if (_jump)
                _playerInput.ValueRW.Jump += 1;
        }
    }
}