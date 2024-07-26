using UnityEngine;

namespace Panty
{
    public class So_StGrid : ScriptableObject
    {
        [SerializeField] private StGrid mGrid;
        public StGrid Grid => mGrid;
    }
}