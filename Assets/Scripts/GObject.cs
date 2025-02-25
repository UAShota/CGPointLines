using UnityEngine;

namespace GCrazyGames
{
    /// <summary>
    /// Base game object
    /// </summary>
    internal class GObject : GTouchObject
    {
        #region Mono properties

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

        #endregion

        #region Methods

        /// <summary>
        /// Object owner
        /// </summary>
        internal GOwner Owner { get; private set; }

        /// <summary>
        /// Owner setter
        /// </summary>
        /// <param name="aOwner">Owner</param>
        internal virtual void SetOwner(GOwner aOwner)
        {
            MatRenderer.material = aOwner switch
            {
                GOwner.Enemy => MatEnemy,
                GOwner.Main => MatMain,
                _ => MatFree,
            };
            Owner = aOwner;
        }

        #endregion
    }
}