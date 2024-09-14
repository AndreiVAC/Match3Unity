using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Container
{
    public class Shelf : MonoBehaviour
    {
        public List<List<int>> slotSpawns = new List<List<int>>();
        public List<Slot> slots = new List<Slot>();
        void Start()
        {
            foreach (Slot slot in slots)
            {
                slot.Shelf = this;
            }
            SpawnObjects();
        }

        private void AdvanceLayer()
        {
            if (slotSpawns.Count > 1)
            {
                slotSpawns.RemoveAt(0); //pop to next layer
                for(int i = 0; i < slots.Count; i++)
                {
                    slots[i].SpawnType = slotSpawns[0][i]; // set new object types
                }
                SpawnObjects(); // respawn new objects
                UpdateNextLayerDisplay();
            }
            else
            {
                foreach (Slot slot in slots) // for last layer case, all slots should be empty
                {
                    slot.SpawnType = -1;
                }
            }
        }
        private void SpawnObjects()
        {
            foreach (Slot slot in slots)
            {
                int slotType = slot.SpawnType;
                if(slotType == -1) // don't spawn object if empty slot id.
                {
                    continue;
                }
                SortableObject spawnObject = Level.Manager.Instance.sortableTypes[slotType];
                var spawnPos = slot.transform.position;
                SortableObject newObject = Instantiate(spawnObject, spawnPos, Quaternion.identity);
                slot.ObjectHeld = newObject;
                newObject.ShelfSlot = slot;
                newObject.LastPosition = spawnPos;
                newObject.Type = slotType ;
            }
        }
        public Slot FindClosestSlot(Vector3 itemPosition)
        {
            Slot closestSlot = null;
            float closestDistance = Mathf.Infinity;
            foreach (Slot slot in slots)
            {
                float distance = Vector3.Distance(slot.transform.position, itemPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestSlot = slot;
                }
            }
            return closestSlot;
        }

        private void UpdateNextLayerDisplay()
        {
            
            bool isLastLayer = slotSpawns.Count <= 1;
            for(int i = 0; i<slots.Count ;i++)
            {
                GameObject currentObjectIcon = slots[i].NextObjectIcon;
                SpriteRenderer currentSpriteRenderer = currentObjectIcon.GetComponent<SpriteRenderer>();

                if(isLastLayer)
                {
                    currentSpriteRenderer.enabled = false;
                    continue;
                }

                var nextType = slotSpawns[1][i];
                if(nextType == -1)
                {
                    currentSpriteRenderer.enabled = false;
                    continue;
                }

                SortableObject nextObject = Level.Manager.Instance.sortableTypes[nextType];
                SpriteRenderer nextSpriteRenderer = nextObject.GetComponent<SpriteRenderer>();

                currentSpriteRenderer.enabled = true;
                currentSpriteRenderer.sprite = nextSpriteRenderer.sprite;
                var nextColor = nextSpriteRenderer.color;
                nextColor.a = 0.3f;
                currentSpriteRenderer.color = nextColor;

                currentObjectIcon.transform.localScale = nextSpriteRenderer.transform.localScale*1.5f;
            }

        }

        public void UpdateShelf()
        {
            UpdateNextLayerDisplay();
            // check for matches, or if it's empty
            if (DoesFrontLayerHaveMatch())
            {
                foreach(Slot slot in slots)
                {
                    slot.DestroyObjectHeld();
                    slot.SpawnType = -1;
                }
            }
            if (IsFrontLayerEmpty())
            {
                AdvanceLayer();
            }
            Level.Manager.Instance.updateGameStateNextFrame();
        }

        private bool IsFrontLayerEmpty()
        {
            foreach (Slot slot in slots)
            {
                if (slot.SpawnType != -1)
                {
                    return false;
                }
            }
            return true;
        }

        private bool DoesFrontLayerHaveMatch()
        {
            if (slots[0].SpawnType == -1)
            {
                return false;
            }
            var totalSlots = slots.Count();
            if (totalSlots <= 1)
            {
                return false;
            }
            for (int i = 1; i < totalSlots; i++)
            {
                var currentSlot = slots[i].SpawnType;
                var previousSlot = slots[i - 1].SpawnType;
                if (currentSlot == -1 || currentSlot != previousSlot)
                {
                    return false;
                }
            }
            return true;
        }
    }
}