using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.Burst;
using UnityEngine;
using Unity.Entities.UniversalDelegates;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[BurstCompile]
public partial struct PlayerMovementSystem : ISystem
{

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var _speed = Time.deltaTime * 4;
        foreach (var (_input, _trans) in SystemAPI.Query<RefRO<PlayerInput>, RefRW<LocalTransform>>().WithAll<Simulate>())
        {
            var _moveInput = new float2(_input.ValueRO.Horizontal, _input.ValueRO.Vertical);
            _moveInput = math.normalizesafe(_moveInput) * _speed;
            _trans.ValueRW.Position += new float3(_moveInput.x, _input.ValueRO.Jump, _moveInput.y);
        }

    }
}
