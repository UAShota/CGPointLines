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
    }
}