using System.Collections;
using UnityEngine;

namespace GCrazyGames
{
    /// <summary>
    /// Quadro corner object
    /// </summary>
    public class GTerra : GObject
    {
        /// <summary>
        /// Sound renderer
        /// </summary>
        public AudioSource SoundRenderer;
        /// <summary>
        /// Main awake sound
        /// </summary>
        public AudioClip SoundMain;
        /// <summary>
        /// Enemy awake sound
        /// </summary>
        public AudioClip SoundEnemy;

        /// <summary>
        /// Enemy animated show
        /// </summary>
        /// <returns>Couroutine result</returns>
        private IEnumerator Blink()
        {
            for (float tmpI = 0; tmpI < 1f; tmpI += 0.1f)
            {
                transform.localScale = new Vector3(tmpI, tmpI, tmpI);
                yield return new WaitForSeconds(0.1f);
            }
            yield return null;
        }

        /// <summary>
        /// Setting owner type of object
        /// </summary>
        /// <param name="aOwner">Owner type</param>
        public override void SetOwner(GOwner aOwner)
        {
            base.SetOwner(aOwner);
            if (aOwner == GOwner.Enemy)
            {
                transform.localScale = new Vector3(0, 0, 0);
                StartCoroutine(Blink());
                SoundRenderer.PlayOneShot(SoundEnemy);
            }
            else
            {
                SoundRenderer.PlayOneShot(SoundMain);
            }
        }
    }
}