using Unity.Entities;
using UnityEngine;

namespace ColonyConquest.Core
{
    /// <summary>
    /// Authoring в SubScene: после baking — сущность с <see cref="LocalTransform"/> и <see cref="PlayerMoveTargetTag"/>.
    /// </summary>
    public class PlayerMoveTargetAuthoring : MonoBehaviour
    {
    }

    public sealed class PlayerMoveTargetBaker : Baker<PlayerMoveTargetAuthoring>
    {
        public override void Bake(PlayerMoveTargetAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerMoveTargetTag());
        }
    }
}
