using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ColonyConquest.Core
{
    /// <summary>
    /// Читает <see cref="InputActionsJson"/> (Input System), пишет в синглтон <see cref="InputCommandState"/>.
    /// SystemBase — чтобы хранить managed <see cref="InputAction"/> между кадрами.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(GameBootstrapSystem))]
    public partial class InputGatherSystem : SystemBase
    {
        private InputAction _moveAction;
        private InputAction _toggleBuildAction;
        private InputAction _interactAction;

        protected override void OnCreate()
        {
            using (var q = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<InputCommandState>()))
            {
                if (q.CalculateEntityCount() == 0)
                    EntityManager.CreateSingleton(new InputCommandState());
            }

            var asset = InputActionAsset.FromJson(InputActionsJson.ColonyJson);
            var gameplay = asset.FindActionMap("Gameplay");
            _moveAction = gameplay.FindAction("Move");
            _toggleBuildAction = gameplay.FindAction("ToggleBuild");
            _interactAction = gameplay.FindAction("Interact");
            gameplay.Enable();
        }

        protected override void OnUpdate()
        {
            float2 move = (float2)_moveAction.ReadValue<Vector2>();
            ref var cmd = ref SystemAPI.GetSingletonRW<InputCommandState>().ValueRW;
            cmd.Move = move;
            cmd.ToggleBuildPressed = (byte)(_toggleBuildAction.WasPressedThisFrame() ? 1 : 0);
            cmd.InteractHeld = (byte)(_interactAction.IsPressed() ? 1 : 0);
        }
    }
}
