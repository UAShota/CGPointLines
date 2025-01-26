using System.Collections;
using UnityEngine;

namespace GCrazyGames
{
    public class GTerra : GObject
    {
        private IEnumerator Blink()
        {
            for (float tmpI = 0; tmpI < 1f; tmpI += 0.1f)
            {
                transform.localScale = new Vector3(tmpI, tmpI, tmpI);
                yield return new WaitForSeconds(0.1f);
            }
            yield return null;
        }

        public override void SetOwner(GOwner aOwner)
        {
            base.SetOwner(aOwner);
            if (aOwner == GOwner.Enemy)
            {
                transform.localScale = new Vector3(0, 0, 0);
                StartCoroutine(Blink());
            }
        }
    }
}