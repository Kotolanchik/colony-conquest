using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ColonyConquest.Core
{
    /// <summary>
    /// Сдвигает сущности с <see cref="PlayerMoveTargetTag"/> по <see cref="InputCommandState.Move"/> (плоскость XZ).
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(InputGatherSystem))]
    public partial struct PlayerMoveFromInputSystem : ISystem
    {
        private const float MoveSpeed = 5f;

        public void OnUpdate(ref SystemState state)
        {
            var move = SystemAPI.GetSingleton<InputCommandState>().Move;
            var delta = new float3(move.x, 0f, move.y) * MoveSpeed * SystemAPI.Time.DeltaTime;

            foreach (var lt in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<PlayerMoveTargetTag>())
            {
                var t = lt.ValueRO;
                lt.ValueRW = LocalTransform.FromPositionRotationScale(
                    t.Position + delta,
                    t.Rotation,
                    t.Scale);
            }
        }
    }
}
