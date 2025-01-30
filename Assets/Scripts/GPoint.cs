using System.Collections.Generic;
using UnityEngine;

namespace GCrazyGames
{
    /// <summary>
    /// Base draggable object
    /// </summary>
    public class GPoint : GObject
    {
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
        /// Main worker vs delegates
        /// </summary>
        private GMain FMain;
        /// <summary>
        /// Selected property
        /// </summary>
        private bool FIsTargeted;

        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="aMain">Main worker</param>
        /// <param name="aX">Coord by X</param>
        /// <param name="aY">Coord by Y</param>
        public void Init(GMain aMain, int aX, int aY)
        {
            X = aX;
            Y = aY;
            FMain = aMain;
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
                MatRenderer.material.color = GUtils.IntToColor(0x72D7F300);
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
        /// Mono drag event
        /// </summary>
        private void OnMouseDrag()
        {
            FMain.BeginDrag(this);
        }

        /// <summary>
        /// Mono enter event
        /// </summary>
        private void OnMouseEnter()
        {
            FMain.CheckEnter(this);
        }

        /// <summary>
        /// Mono exit event
        /// </summary>
        private void OnMouseExit()
        {
            FMain.CheckExit(this);
        }

        /// <summary>
        /// Mono click event
        /// </summary>
        private void OnMouseUp()
        {
            FMain.EndDrag(this);
        }

        /// <summary>
        /// Mono frame update
        /// </summary>
        private void Update()
        {
            if (Line.enabled)
            {
                Line.SetPosition(0, transform.position);
                Line.SetPosition(1, GUtils.GetMousePos());
            }
        }
    }
}