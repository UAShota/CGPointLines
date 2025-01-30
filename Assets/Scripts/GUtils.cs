using UnityEngine;

namespace GCrazyGames
{
    public enum GOwner
    {
        Free,
        Main,
        Enemy
    }

    public static class GUtils
    {
        public static T CreatePrefab<T>(string aName, int aX, int aY, Transform aParent)
        {
            return Object.Instantiate(Resources.Load<GameObject>("Prefabs/" + aName),
                new Vector3(aX, aY, 0), Quaternion.identity, aParent).GetComponent<T>();
        }

        public static Vector3 GetMousePos()
        {
            var tmpCursorPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z);
            return Camera.main.ScreenToWorldPoint(tmpCursorPos);
        }

        /// <summary>
        /// Hex цвет в RGB цвет
        /// </summary>
        /// <param name="AColor">Hex цвет</param>
        /// <returns>RGB цвет</returns>
        public static Color IntToColor(uint AColor)
        {
            Color32 LResult = Color.clear;
            LResult.a = (byte)((AColor) & 0xFF);
            LResult.b = (byte)((AColor >> 8) & 0xFF);
            LResult.g = (byte)((AColor >> 16) & 0xFF);
            LResult.r = (byte)((AColor >> 24) & 0xFF);
            return LResult;
        }
    }
}