using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Burst;
using UnityEngine;
using UnityEditor;
using System;

public struct GoInGameRequest : IRpcCommand
{
}

[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial struct GoInGameClientSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerSpawner>();
        var _builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NetworkId>()
            .WithNone<NetworkStreamInGame>();
        state.RequireForUpdate(state.GetEntityQuery(_builder));
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var _commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (_id, _entity) in SystemAPI.Query<RefRO<NetworkId>>().WithEntityAccess().WithNone<NetworkStreamInGame>())
        {
            _commandBuffer.AddComponent<NetworkStreamInGame>(_entity);
            var req = _commandBuffer.CreateEntity();
            _commandBuffer.AddComponent<GoInGameRequest>(req);
            _commandBuffer.AddComponent(req, new SendRpcCommandRequest { TargetConnection = _entity });
        }
        _commandBuffer.Playback(state.EntityManager);
    }
}

[BurstCompile]
// When server receives go in game request, go in game and delete request
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct GoInGameServerSystem : ISystem
{
    private ComponentLookup<NetworkId> _networkIdFromEntity;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerSpawner>();
        var _builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<GoInGameRequest>()
            .WithAll<ReceiveRpcCommandRequest>();
        state.RequireForUpdate(state.GetEntityQuery(_builder));
        _networkIdFromEntity = state.GetComponentLookup<NetworkId>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {


        var _prefab = SystemAPI.GetSingleton<PlayerSpawner>().Player;
        state.EntityManager.GetName(_prefab, out var _prefabName);
        var _worldName = new FixedString32Bytes(state.WorldUnmanaged.Name);

        

        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        _networkIdFromEntity.Update(ref state);

        foreach (var (_reqSrc, _reqEntity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>().WithAll<GoInGameRequest>().WithEntityAccess())
        {
            commandBuffer.AddComponent<NetworkStreamInGame>(_reqSrc.ValueRO.SourceConnection);
            var _networkId = _networkIdFromEntity[_reqSrc.ValueRO.SourceConnection];
            

            UnityEngine.Debug.Log($"'{_worldName}' setting connection '{_networkId.Value}' to in game, spawning a Ghost '{_prefabName}' for them!");

            

            var _player = commandBuffer.Instantiate(_prefab);
            commandBuffer.SetComponent(_player, new GhostOwner { NetworkId = _networkId.Value });



            // Add the player to the linked entity group so it is destroyed automatically on disconnect
            commandBuffer.AppendToBuffer(_reqSrc.ValueRO.SourceConnection, new LinkedEntityGroup { Value = _player });
            commandBuffer.DestroyEntity(_reqEntity);
        }
        commandBuffer.Playback(state.EntityManager);
    }


}