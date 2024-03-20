using UnityEngine;


namespace viva
{


    public class ClothingBehavior : PassiveBehaviors.PassiveTask
    {

        public ClothingBehavior(Companion _self) : base(_self, Mathf.Infinity)
        {
        }

        public void AttemptReactToOutfitChange()
        {
            self.SetTargetAnimation(Companion.Animation.STAND_OUTFIT_LIKE);
        }
    }

}