using UnityEngine;

namespace Container
{
    [System.Serializable]
    public class Slot : MonoBehaviour
    {
        public GameObject NextObjectIcon;

        [SerializeField]
        private Shelf shelf;
        public Shelf Shelf
        {
            get => shelf;
            set => shelf = value;
        }

        [SerializeField]
        private SortableObject objectHeld;
        public SortableObject ObjectHeld
        {
            get => objectHeld;
            set => objectHeld = value;
        }

        [SerializeField]
        private int spawnType = -1;
        public int SpawnType
        {
            get => spawnType;
            set
            {
                if (value >= -1)
                {
                    spawnType = value;
                }
                else
                {
                    Debug.LogWarning("Invalid spawn type value.");
                }
            }
        }

        public void DestroyObjectHeld()
        {
            if (ObjectHeld != null)
            {
                Destroy(ObjectHeld.gameObject);
                ObjectHeld = null;
            }
        }

        public bool IsAvailable()
        {
            return SpawnType == -1;
        }
    }
}