using UnityEngine;

namespace GCrazyGames
{
    public class GObject : MonoBehaviour
    {
        public Material MatFree;
        public Material MatMain;
        public Material MatEnemy;
        public Renderer MatRenderer;

        public GOwner Owner { get; private set; }

        public virtual void SetOwner(GOwner aOwner)
        {
            MatRenderer.material = aOwner switch
            {
                GOwner.Enemy => MatEnemy,
                GOwner.Main => MatMain,
                _ => MatFree,
            };
            Owner = aOwner;
        }
    }
}