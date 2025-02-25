using System.Collections.Generic;
using UnityEngine;

namespace GCrazyGames
{
    /// <summary>
    /// Base draggable object
    /// </summary>
    internal class GPoint : GObject
    {
        #region Variables

        /// <summary>
        /// Drag line object
        /// </summary>
        public LineRenderer Line;

        /// <summary>
        /// Point quadro links
        /// </summary>
        public List<GPoint> Links { get; private set; }
        /// <summary>
        /// Coord by X
        /// </summary>
        public int X { get; private set; }
        /// <summary>
        /// Coord by Y
        /// </summary>
        public int Y { get; private set; }

        /// <summary>
        /// Selected property
        /// </summary>
        private bool FIsTargeted;

        #endregion

        #region Methods

        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="aX">Coord by X</param>
        /// <param name="aY">Coord by Y</param>
        public void Init(int aX, int aY)
        {
            X = aX;
            Y = aY;
            Links = new();
            SetOwner(GOwner.Free);
        }

        /// <summary>
        /// Show as selected
        /// </summary>
        /// <param name="aSelected">On/Off selecting</param>
        public void MarkSelected(bool aSelected)
        {
            if (!aSelected && FIsTargeted)
                MatRenderer.material.color = Color.yellow;
            else if (aSelected)
                MatRenderer.material.color = GUtils.IntToColor(0xAFB6FF00);
        }

        /// <summary>
        /// Show as target
        /// </summary>
        /// <param name="aIsTargeted">On/Off targeting</param>
        public void MarkTarget(bool aIsTargeted)
        {
            if (aIsTargeted)
                MatRenderer.material.color = Color.yellow;
            else
                MatRenderer.material.color = Color.white;
            FIsTargeted = aIsTargeted;
        }

        /// <summary>
        /// Change enable state
        /// </summary>
        /// <param name="aEnabled">On/Off state</param>
        public void SetEnable(bool aEnabled)
        {
            GetComponent<BoxCollider>().enabled = aEnabled;
        }

        /// <summary>
        /// Mono frame update
        /// </summary>
        private void Update()
        {
            if (Line.enabled)
            {
                Line.SetPosition(0, transform.position);
                Line.SetPosition(1, GUtils.GetMousePos(TouchPosition));
            }
        }

        #endregion
    }
}