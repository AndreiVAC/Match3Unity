using Container;
using UnityEngine;


[System.Serializable]
public class SortableObject : MonoBehaviour
{
     [SerializeField]
    private Slot _shelfSlot = null;

    public Slot ShelfSlot
    {
        get => _shelfSlot;
        set => _shelfSlot = value;
    }

    private Vector3 _lastPosition;

    public Vector3 LastPosition
    {
        get => _lastPosition;
        set => _lastPosition = value;
    }

    private int _type = -1;

    public int Type
    {
        get => _type;
        set => _type = value;
    }
    public void ClickReleased()
    {
        Vector3 clickPosition = Level.Manager.Instance.mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Shelf shelfClicked = GetShelfAtPosition(clickPosition);

        if (shelfClicked == null)
        {
            ResetPosition();
            return;
        }

        Slot availableShelfSlot = shelfClicked.FindClosestSlot(clickPosition);

        if (ShelfSlot == availableShelfSlot)
        {
            ResetPosition();
            return;
        }

        if (!availableShelfSlot.IsAvailable())
        {
            SwapSlotObjects(availableShelfSlot, ShelfSlot);
            return;
        }
        PlaceInSlot(availableShelfSlot);
    }

    public static void SwapSlotObjects(Slot firstSlot, Slot secondSlot)
    {
        var firstObject = firstSlot.ObjectHeld;
        var secondObject = secondSlot.ObjectHeld;
        var firstSlotPosition = firstSlot.transform.position;
        firstObject.LastPosition = secondSlot.transform.position;
        secondObject.LastPosition = firstSlotPosition;


        firstObject.ShelfSlot = secondSlot;
        secondObject.ShelfSlot = firstSlot;

        firstObject.ShelfSlot.SpawnType = firstObject.Type;
        secondObject.ShelfSlot.SpawnType = secondObject.Type;

        firstObject.ShelfSlot.ObjectHeld = firstObject;
        secondObject.ShelfSlot.ObjectHeld = secondObject;

        firstSlot.Shelf.UpdateShelf();
        secondSlot.Shelf.UpdateShelf();

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
        transform.position = _lastPosition;
    }

    public void Move(Vector3 newPos)
    {
        transform.position = newPos;
    }

    private void PlaceInSlot(Slot newSlot)
    {
        transform.position = newSlot.transform.position;
        LastPosition = transform.position;
        ShelfSlot.SpawnType = -1;
        newSlot.SpawnType = Type;
        newSlot.ObjectHeld = this;
        ShelfSlot.Shelf.UpdateShelf();
        ShelfSlot = newSlot;
        newSlot.Shelf.UpdateShelf();
    }
}