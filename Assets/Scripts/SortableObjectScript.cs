using System;
using System.Collections;
using Container;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.UI;
using UnityEngine;


[System.Serializable]
public class SortableObject : MonoBehaviour
{
    public Slot shelfSlot = null;
    public void setShelfSlot(Slot newSlot)
    {
        shelfSlot = newSlot;
    }
    public void destroySelf()
    {
        Destroy(gameObject);
    }
    private Vector3 lastPosition;
    public void setLastPosition(Vector3 newLastPosition)
    {
        lastPosition = newLastPosition;
    }
    public int type = 0;
    public int getType => type;
    public void ClickReleased()
    {
        Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Shelf shelfClicked = GetShelfAtPosition(clickPosition);

        if (shelfClicked == null)
        {
            ResetPosition();
            return;
        }

        Slot availableShelfSlot = shelfClicked.GetClosestFrontLayerSlot(clickPosition);

        if (shelfSlot == availableShelfSlot)
        {
            ResetPosition();
            return;
        }

        if (!availableShelfSlot.isAvailable())
        {
            swapSlotObjects(availableShelfSlot, shelfSlot);
            return;
        }
        PlaceInSlot(availableShelfSlot);
    }

    public void swapSlotObjects(Slot firstSlot, Slot secondSlot)
    {
        var firstObject = firstSlot.getObjectHeld;
        var secondObject = secondSlot.getObjectHeld;
        var firstSlotPosition = firstSlot.getOffsetPosition + firstSlot.getShelf.transform.position;
        firstObject.setLastPosition(secondSlot.getOffsetPosition + secondSlot.getShelf.transform.position);
        secondObject.setLastPosition(firstSlotPosition);


        firstObject.setShelfSlot(secondSlot);
        secondObject.setShelfSlot(firstSlot);

        firstObject.shelfSlot.setType(firstObject.getType);
        secondObject.shelfSlot.setType(secondObject.getType);

        firstObject.shelfSlot.setObjectHeld(firstObject);
        secondObject.shelfSlot.setObjectHeld(secondObject);

        firstSlot.getShelf.UpdateShelf();
        secondSlot.getShelf.UpdateShelf();

        firstObject.ResetPosition();
        secondObject.ResetPosition();
    }

    private Shelf GetShelfAtPosition(Vector3 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero, float.PositiveInfinity, LayerMask.GetMask("Shelf"));

        if (hit)
        {
            return hit.collider.GetComponent<Shelf>();
        }

        return null;
    }

    private void ResetPosition()
    {
        transform.position = lastPosition;
    }

    public void Move(Vector3 newPos)
    {
        transform.position = newPos;
    }

    private void PlaceInSlot(Slot newSlot)
    {
        transform.position = newSlot.getOffsetPosition + newSlot.getShelf.transform.position;
        setLastPosition(transform.position);
        shelfSlot.setType(-1);
        shelfSlot.getShelf.UpdateShelf();
        newSlot.setType(type);
        newSlot.setObjectHeld(this);
        setShelfSlot(newSlot);
        newSlot.getShelf.UpdateShelf();
    }
    void Start()
    {
    }
}