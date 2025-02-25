using UnityEngine;

namespace GCrazyGames
{
    /// <summary>
    /// Game player roles
    /// </summary>
    public enum GOwner
    {
        #region Types

        /// <summary>
        /// Unassigned
        /// </summary>
        Free,
        /// <summary>
        /// Primary
        /// </summary>
        Main,
        /// <summary>
        /// Counter
        /// </summary>
        Enemy

        #endregion
    }


    /// <summary>
    /// Game utils
    /// </summary>
    public static class GUtils
    {
        #region Methods

        /// <summary>
        /// Prefab manager
        /// </summary>
        /// <typeparam name="T">Generic mono type</typeparam>
        /// <param name="aName">Resource name</param>
        /// <param name="aX">Position at X</param>
        /// <param name="aY">Position at Y</param>
        /// <param name="aParent">Parent transform</param>
        /// <returns></returns>
        public static T CreatePrefab<T>(string aName, int aX, int aY, Transform aParent)
        {
            return Object.Instantiate(Resources.Load<GameObject>("Prefabs/" + aName),
                new Vector3(aX, aY, 0), Quaternion.identity, aParent).GetComponent<T>();
        }

        /// <summary>
        /// Getting the coords of the mouse in the world
        /// </summary>
        /// <param name="aPosition">Current world coordinates</param>
        /// <returns>Translated mouse position</returns>
        public static Vector3 GetMousePos(Vector3 aPosition)
        {
            var tmpCursorPos = new Vector3(aPosition.x, aPosition.y, -Camera.main.transform.position.z);
            return Camera.main.ScreenToWorldPoint(tmpCursorPos);
        }

        /// <summary>
        /// Hex color to RGB
        /// </summary>
        /// <param name="aColor">Hex color</param>
        /// <returns>RGB color</returns>
        public static Color IntToColor(uint aColor)
        {
            Color32 LResult = Color.clear;
            LResult.a = (byte)((aColor) & 0xFF);
            LResult.b = (byte)((aColor >> 8) & 0xFF);
            LResult.g = (byte)((aColor >> 16) & 0xFF);
            LResult.r = (byte)((aColor >> 24) & 0xFF);
            return LResult;
        }

        #endregion
    }
}