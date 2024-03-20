namespace viva
{


    public class PokerListenerBehavior : PassiveBehaviors.PassiveTask
    {

        private Character target = null;
        private PokerCard targetPokerDeck = null;

        public PokerListenerBehavior(Companion _self) : base(_self, 0.3f)
        {
            self.onGiftItemCallstack.AddCallback(OnItemBeg);
        }

        private bool OnItemBeg(Item item)
        {
            var pokerCard = item as PokerCard;
            if (!pokerCard)
            {
                return false;
            }
            if (pokerCard.mainOwner == null)
            {
                return false;
            }
            if (self.active.IsTaskActive(self.active.poker))
            {
                return false;
            }
            if (pokerCard.isFanGroupChild || pokerCard.fanGroupSize > 0)
            {
                return false;
            }
            if (self.active.poker.AttemptJoinPokerGame(pokerCard))
            {
                var playAnim = new AutonomyPlayAnimation(self.autonomy, "agree poker", self.GetAnimationFromSet(AnimationSet.AGREE));
                playAnim.onSuccess += delegate
                {
                    self.active.SetTask(self.active.poker); //begin poker game
                };
                playAnim.AddRequirement(new AutonomyFaceDirection(self.autonomy, "face poker dealer", delegate (TaskTarget target) { target.SetTargetPosition(item.transform.position); }));
                playAnim.AddRequirement(new AutonomyWait(self.autonomy, "wait random", UnityEngine.Random.value * 0.5f));
                self.autonomy.Interrupt(playAnim);
            }
            else
            {
                var playAnim = new AutonomyPlayAnimation(self.autonomy, "disagree poker", Companion.Animation.FLOOR_SIT_REFUSE);
                self.autonomy.Interrupt(playAnim);
            }
            return true;
        }
    }

}