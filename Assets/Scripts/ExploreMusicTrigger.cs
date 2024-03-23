using UnityEngine;


namespace viva
{

    public class ExploreMusicTrigger : MonoBehaviour
    {
        private static int exploreCounter = 0;

        public void OnTriggerEnter(Collider collider)
        {
            Camera player = collider.GetComponent<Camera>();
            if (!player) return;

            if (exploreCounter == 0)
            {
                GameDirector.instance.SetUserIsExploring(true);
            }
            exploreCounter++;
        }

        public void OnTriggerExit(Collider collider)
        {
            Camera camera = collider.GetComponent<Camera>();
            if (!camera) return;

            --exploreCounter;
            if (exploreCounter == 0)
            {
                GameDirector.instance.SetUserIsExploring(false);
            }
        }
    }

}