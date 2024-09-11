using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Container
{
    public class Shelf : MonoBehaviour
    {
        public List<SlotLayer> slotLayers;
        void Start()
        {
            foreach (SlotLayer slotLayer in slotLayers)
            {
                foreach (Slot slot in slotLayer.slots)
                {
                    slot.setShelf(this);
                }
            }
            spawnFrontLayerSlots();
        }
        private void spawnFrontLayerSlots()
        {
            Debug.Log(slotLayers);
            foreach (Slot slot in slotLayers[0].slots)
            {
                if (slot.spawnObject == null)
                {
                    continue;
                }
                var spawnPos = slot.getOffsetPosition + slot.getShelf.transform.position;
                SortableObject newObject = Instantiate(slot.spawnObject, spawnPos, Quaternion.identity);
                slot.setType(newObject.getType);
                slot.setObjectHeld(newObject);
                newObject.setShelfSlot(slot);
                newObject.setLastPosition(spawnPos);
            }
        }
        public Slot GetClosestFrontLayerSlot(Vector3 itemPosition)
        {
            Slot closestSlot = null;
            float closestDistance = Mathf.Infinity;
            foreach (Slot slot in slotLayers[0].slots)
            {
                float distance = Vector3.Distance(slot.getOffsetPosition + slot.getShelf.transform.position, itemPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestSlot = slot;
                }
            }
            return closestSlot;
        }

        public void UpdateShelf()
        {
            if (FrontLayerHasMatch())
            {
                for (int i = 0; i < slotLayers[0].slots.Length; i++)
                {
                    slotLayers[0].slots[i].destroyObjectHeld();
                }
                foreach (Slot slot in slotLayers[0].slots)
                {
                    slot.setType(-1);
                } 
            }
            if (IsFrontLayerEmpty())
            {
                if (slotLayers.Count > 1)
                {
                    slotLayers.RemoveAt(0); //pop to next layer
                    spawnFrontLayerSlots();
                } 
            }
            Game.Manager.Instance.updateGameStateNextFrame();
        }

        private bool IsFrontLayerEmpty()
        {
            foreach (Slot slot in slotLayers[0].slots)
            {
                if (slot.getType != -1)
                {
                    return false;
                }
            }
            return true;
        }

        private bool FrontLayerHasMatch()
        {
            if (slotLayers[0].slots[0].getType == -1)
            {
                return false;
            }
            if(slotLayers[0].slots.Count() <= 1)
            {
                return false;
            }
            for (int i = 1; i < slotLayers[0].slots.Length; i++)
            {
                var currentSlot = slotLayers[0].slots[i];
                var previousSlot = slotLayers[0].slots[i - 1];
                if (currentSlot.getType == -1 || currentSlot.getType != previousSlot.getType)
                {
                    return false;
                }
            }
            return true;
        }

        void Update()
        {

        }

    }

    [System.Serializable]
    public class SlotLayer
    {
        public Slot[] slots;
        public SlotLayer(int length)
    {
        slots = new Slot[length];
        for (int i = 0; i < length; i++)
        {
            slots[i] = new Slot();
        }
    }
    }
    

    [System.Serializable]
    public class Slot
    {
        public SortableObject spawnObject{get;set;}
        [SerializeField]
        private Shelf shelf;
        public void setShelf(Shelf newShelf)
        {
            shelf = newShelf;
        }
        public Shelf getShelf => shelf;
        private SortableObject objectHeld;
        public SortableObject getObjectHeld => objectHeld;
        public void setObjectHeld(SortableObject newObject)
        {
            objectHeld = newObject;
        }
        public void destroyObjectHeld()
        {
            objectHeld.destroySelf();
        }
        public Vector3 offsetPosition;
        [SerializeField]
        private int type = -1;
        public void setType(int newType)
        {
            type = newType;
        }
        public bool isAvailable()
        {
            if (getType == -1)
            {
                return true;
            }
            return false;
        }
        public int getType => type;
        public Vector3 getOffsetPosition => offsetPosition;
    }
}