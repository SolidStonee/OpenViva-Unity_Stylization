using UnityEngine;


namespace Viva
{

    public class ExploreMusicTrigger : MonoBehaviour
    {
        private static int exploreCounter = 0;

        public void OnTriggerEnter(Collider collider)
        {
            Player player = collider.GetComponent<Player>();
            if (!player) return;

            if (exploreCounter == 0)
            {
                GameDirector.instance.SetUserIsExploring(true);
            }
            exploreCounter++;
        }

        public void OnTriggerExit(Collider collider)
        {
            Player player = collider.GetComponent<Player>();
            if (!player) return;

            --exploreCounter;
            if (exploreCounter == 0)
            {
                GameDirector.instance.SetUserIsExploring(false);
            }
        }
    }

}