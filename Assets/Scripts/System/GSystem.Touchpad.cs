using UnityEngine;
using UnityEngine.EventSystems;

namespace GCrazyGames
{
    /// <summary>
    /// Mouse and touch controller
    /// </summary>
    internal class GTouchObject : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        #region Variables

        /// <summary>
        /// Touch event delegate
        /// </summary>
        /// <param name="aObject">Event object</param>
        public delegate void GTouchEvent(GTouchObject aObject);
        /// <summary>
        /// Touch drag beginned
        /// </summary>
        public event GTouchEvent OnTouchBeginDrag;
        /// <summary>
        /// Touch drag process
        /// </summary>
        public event GTouchEvent OnTouchDrag;
        /// <summary>
        /// Touch drag ended
        /// </summary>
        public event GTouchEvent OnTouchEndDrag;
        /// <summary>
        /// Touch enter over touch
        /// </summary>
        public event GTouchEvent OnTouchOverEnter;
        /// <summary>
        /// Touch leave over touch
        /// </summary>
        public event GTouchEvent OnTouchOverLeave;
        /// <summary>
        /// Current cursor position in screen ccordinates
        /// </summary>
        public Vector3 TouchPosition { get; private set; }

        /// <summary>
        /// Last point under the drag
        /// </summary>
        private GTouchObject FLastPoint;

        #endregion

        #region Methods

        /// <summary>
        /// Touch drag begin
        /// </summary>
        /// <param name="aEventData">Touch event data</param>
        public void OnBeginDrag(PointerEventData aEventData)
        {
            OnTouchBeginDrag.Invoke(this);
        }

        /// <summary>
        /// Touch drag end
        /// </summary>
        /// <param name="aEventData">Touch event data</param>
        public void OnEndDrag(PointerEventData aEventData)
        {
            OnTouchEndDrag.Invoke(this);
        }

        /// <summary>
        /// Touch drag process
        /// </summary>
        /// <param name="aEventData">Touch event data</param>
        public void OnDrag(PointerEventData aEventData)
        {
            TouchPosition = aEventData.pointerCurrentRaycast.screenPosition;
            OnTouchDrag.Invoke(this);

            // Physical raycast for colliders
            Ray tmpRay = Camera.main.ScreenPointToRay(aEventData.pointerCurrentRaycast.screenPosition);
            GTouchObject tmpPoint = null;

            // Check current touch object
            if (Physics.Raycast(tmpRay, out RaycastHit tmpHitPoint) && (tmpHitPoint.collider != null))
                tmpPoint = tmpHitPoint.collider.gameObject.GetComponent<GTouchObject>();

            // Checking the last point
            if (FLastPoint && FLastPoint != tmpPoint)
                OnTouchOverLeave.Invoke(FLastPoint);

            // Checking the new point
            if (tmpPoint && FLastPoint != tmpPoint)
                OnTouchOverEnter.Invoke(tmpPoint);

            // Set the new pount
            FLastPoint = tmpPoint;
        }

        #endregion
    }
}