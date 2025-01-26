using System.Collections.Generic;
using UnityEngine;

namespace GCrazyGames
{
    public class GPoint : GObject
    {
        public LineRenderer Line;

        public List<GPoint> Links { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        private GMain FMain;
        private bool FIsTargeted;

        public void Init(GMain aMain, int aX, int aY)
        {
            X = aX;
            Y = aY;
            FMain = aMain;
            Links = new();
            SetOwner(GOwner.Free);
        }

        public void MarkSelected(bool aSelected)
        {
            if (!aSelected && FIsTargeted)
                MatRenderer.material.color = Color.yellow;
            else if (aSelected)
                MatRenderer.material.color = Color.blue;
        }

        public void MarkTarget(bool aIsTargeted)
        {
            if (aIsTargeted)
                MatRenderer.material.color = Color.yellow;
            else
                MatRenderer.material.color = Color.white;
            FIsTargeted = aIsTargeted;
        }

        public void SetEnable(bool aEnabled)
        {
            GetComponent<BoxCollider>().enabled = aEnabled;
        }

        private void OnMouseDrag()
        {
            FMain.BeginDrag(this);
        }

        private void OnMouseEnter()
        {
            FMain.CheckEnter(this);
        }

        private void OnMouseExit()
        {
            FMain.CheckExit(this);
        }

        private void OnMouseUp()
        {
            FMain.EndDrag(this);
        }

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