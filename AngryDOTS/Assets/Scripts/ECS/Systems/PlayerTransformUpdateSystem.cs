﻿using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Mathematics;
using Unity.Transforms;

public struct GameState : IComponentData
{
	public int playersAlive;
	public int playersCount;
	public NativeArray<int> playerIds;
	public NativeArray<float3> playerPositions;
}

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
[UpdateBefore(typeof(CollisionSystem))]
public class PlayerTransformUpdateSystem : ComponentSystem
{
	// Query to obtain all the players
	EntityQuery playerGroup;

	protected override void OnCreate()
	{
		playerGroup = GetEntityQuery(ComponentType.ReadOnly<Health>(), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<PlayerComponent>());

		//var state = EntityManager.Instantiate();
		//EntityManager.AddComponentData(state, new GameState { });
		//SetSingleton<GameState>(new GameState { });
		EntityManager.CreateEntity(ComponentType.ReadOnly<GameState>());
		SetSingleton(new GameState { playersAlive = 0, playersCount  = 0,
			playerIds = new NativeArray<int>(10, Allocator.Persistent),
			playerPositions = new NativeArray<float3>(10, Allocator.Persistent)
		});
	}

	protected override void OnUpdate()
	{
		//if (!Settings.AnyPlayerAlive())
		//{
		//	return;
		//}
		GameState state = GetSingleton<GameState>();

		var healths = playerGroup.ToComponentDataArray<Health>(Allocator.TempJob);
		state.playersAlive = healths.Length;
		healths.Dispose();

		var playersComp = playerGroup.ToComponentDataArray<PlayerComponent>(Allocator.TempJob);
		state.playersCount = playersComp.Length;
		playersComp.Dispose();

		//if (state.playerIds != null)
		//{
		//	state.playerIds.Dispose();
		//}
		//if (state.playerPositions != null)
		//{
		//	state.playerPositions.Dispose();
		//}
		//state.playerIds = new NativeArray<int>(state.playersCount, Allocator.Persistent);
		//state.playerPositions = new NativeArray<float3>(state.playersCount, Allocator.Persistent);
		int i = 0;
		Entities.WithAll<PlayerTag>().ForEach((ref Translation pos, ref PlayerComponent player) =>
		{
			state.playerIds[i] = player.playerId;
			state.playerPositions[i] = pos.Value;
			++i;
		});

		//Entities.WithAll<PlayerTag>().ForEach((ref Translation pos, ref PlayerTag tag) =>
		//{
		//	pos = new Translation { Value = Settings.GetPlayerPosition(tag.playerIdx) };
		//});
	}
}