using UnityEngine;

namespace Moleio.Core
{
    public interface IMoleInput
    {
        Vector2 Move { get; }
        bool DashHeld { get; }
    }
}
