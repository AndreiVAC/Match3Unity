using UnityEngine;
namespace Mouse
{
    public class MouseDrag : MonoBehaviour
    {
        private SortableObject lastObjectClicked = null;
        private Transform dragging = null;
        private Vector3 offset;
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit2D raycast = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, float.PositiveInfinity, LayerMask.GetMask("SortableObject"));
                if (raycast)
                {
                    lastObjectClicked = raycast.collider.GetComponent<SortableObject>();
                    lastObjectClicked.LastPosition = lastObjectClicked.transform.position;
                    lastObjectClicked.GetComponent<SpriteRenderer>().sortingOrder = 2;
                    dragging = raycast.transform;
                    offset = dragging.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);

                    Timer.StageTimer.Instance.ObjectMoved();
                }
            }

            if (dragging != null)
            {
                dragging.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + offset;
                if (Input.GetMouseButtonUp(0))
                {
                    dragging = null;
                    if (lastObjectClicked)
                    {
                        lastObjectClicked.ClickReleased();
                        lastObjectClicked.GetComponent<SpriteRenderer>().sortingOrder = 1;
                        lastObjectClicked = null;
                    }
                }
            }
        }
    }
}