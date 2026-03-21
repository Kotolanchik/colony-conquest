using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Core
{
    /// <summary>
    /// Снимок ввода в кадре симуляции (WASD и т.д.) — заполняется <see cref="InputGatherSystem"/>.
    /// </summary>
    public struct InputCommandState : IComponentData
    {
        public float2 Move;
        /// <summary>1 в кадре нажатия ToggleBuild (клавиша B).</summary>
        public byte ToggleBuildPressed;
        /// <summary>1 пока удерживается Interact (ручная добыча и т.д., клавиша E).</summary>
        public byte InteractHeld;
    }
}
