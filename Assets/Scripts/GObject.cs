using UnityEngine;

namespace GCrazyGames
{
    /// <summary>
    /// Base game object
    /// </summary>
    public class GObject : MonoBehaviour
    {
        /// <summary>
        /// Default material
        /// </summary>
        public Material MatFree;
        /// <summary>
        /// Material for player
        /// </summary>
        public Material MatMain;
        /// <summary>
        /// Material for AI
        /// </summary>
        public Material MatEnemy;
        /// <summary>
        /// Material renderer
        /// </summary>
        public Renderer MatRenderer;

        /// <summary>
        /// Object owner
        /// </summary>
        public GOwner Owner { get; private set; }

        /// <summary>
        /// Owner setter
        /// </summary>
        /// <param name="aOwner">Owner</param>
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