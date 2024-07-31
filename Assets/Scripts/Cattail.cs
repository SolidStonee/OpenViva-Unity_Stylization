using UnityEngine;

namespace Viva
{


    public class Cattail : Item
    {

        [SerializeField]
        private SoundSet smackFX;

        public void PlaySmackSound()
        {
            SoundManager.main.RequestHandle(transform.localPosition, transform).PlayOneShot(smackFX.GetRandomAudioClip());
        }
    }

}