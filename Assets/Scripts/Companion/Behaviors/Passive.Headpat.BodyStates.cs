using UnityEngine;


namespace viva
{


    public partial class HeadpatBehavior : PassiveBehaviors.PassiveTask
    {

        private Companion.Animation GetRegularHeadpatEndAnim()
        {

            switch (self.bodyState)
            {
                case BodyState.STAND:
                    if (self.IsHappy())
                    {

                        wantsMoreHeadpats = !wantsMoreHeadpats;
                        if (!wantsMoreHeadpats)
                        {
                            if (Random.value > 0.5f && headpatProperTotal > 2.0f)
                            {
                                return Companion.Animation.STAND_HEADPAT_HAPPY_SATISFACTION;
                            }
                            else
                            {
                                return Companion.Animation.STAND_HAPPY_IDLE1;
                            }
                        }
                        else
                        {
                            return Companion.Animation.STAND_HEADPAT_HAPPY_WANTED_MORE;
                        }
                    }
                    else
                    {
                        switch ((int)(Random.value * 3.0f))
                        {
                            case 0:
                                return Companion.Animation.STAND_ANGRY_IDLE1;
                            case 1:
                                return Companion.Animation.STAND_HEADPAT_INTERRUPT;
                            default:
                                return Companion.Animation.STAND_HEADPAT_ANGRY_END;
                        }
                    }
                case BodyState.FLOOR_SIT:
                    if (self.IsHappy())
                    {
                        wantsMoreHeadpats = !wantsMoreHeadpats;
                        if (wantsMoreHeadpats)
                        {
                            return Companion.Animation.FLOOR_SIT_HEADPAT_HAPPY_WANTED_MORE;
                        }
                    }
                    return Companion.Animation.NONE;
                case BodyState.SQUAT:
                    if (self.IsHappy())
                    {

                        wantsMoreHeadpats = !wantsMoreHeadpats;
                        if (wantsMoreHeadpats)
                        {
                            return Companion.Animation.SQUAT_HEADPAT_HAPPY_WANTED_MORE;
                        }
                    }
                    return Companion.Animation.NONE;
                default:
                    return Companion.Animation.NONE;
            }
        }

        private Companion.Animation GetHeadpatSucceededAnimation()
        {
            return self.bodyStateAnimationSets[(int)self.bodyState].GetRandomAnimationSet(AnimationSet.HEADPAT_SUCCESS);
            // switch( self.bodyState ){
            // case BodyState.STAND:
            // 	return Companion.Animation.STAND_HEADPAT_CLIMAX_TO_HAPPY;
            // case BodyState.FLOOR_SIT:
            // 	return Companion.Animation.FLOOR_SIT_HEADPAT_ANGRY_CLIMAX_TO_HAPPY;
            // case BodyState.BATHING_IDLE:
            // 	return Companion.Animation.BATHTUB_HEADAPAT_ANGRY_PROPER_TO_HAPPY;
            // case BodyState.SQUAT:
            // 	return Companion.Animation.SQUAT_HEADPAT_CLIMAX_TO_HAPPY;
            // default:
            // 	return Companion.Animation.NONE;
            // }
        }

        private Companion.Animation GetHeadpatCancelSuccessAnimation()
        {
            return self.bodyStateAnimationSets[(int)self.bodyState].GetRandomAnimationSet(AnimationSet.HEADPAT_CANCEL_SUCCESS);
            // switch( self.bodyState ){
            // case BodyState.STAND:
            // 	return Companion.Animation.STAND_HEADPAT_CLIMAX_TO_CANCEL_RIGHT;
            // case BodyState.FLOOR_SIT:
            // 	return Companion.Animation.FLOOR_SIT_HEADPAT_ANGRY_BRUSH_AWAY;
            // case BodyState.BATHING_IDLE:
            // 	return Companion.Animation.BATHTUB_HEADPAT_BRUSH_AWAY;
            // case BodyState.SQUAT:
            // 	return Companion.Animation.SQUAT_HEADPAT_CLIMAX_TO_CANCEL_RIGHT;
            // default:
            // 	return Companion.Animation.NONE;
            // }
        }

        private Companion.Animation GetHeadpatEndRoughAnimation()
        {
            return self.bodyStateAnimationSets[(int)self.bodyState].GetRandomAnimationSet(AnimationSet.HEADPAT_BRUSH_AWAY);

            // switch( self.bodyState ){
            // case BodyState.STAND:
            // 	return Companion.Animation.STAND_HEADPAT_ANGRY_BRUSH_AWAY;
            // case BodyState.FLOOR_SIT:
            // 	return Companion.Animation.FLOOR_SIT_HEADPAT_ANGRY_BRUSH_AWAY;
            // case BodyState.BATHING_IDLE:
            // case BodyState.BATHING_RELAX:
            // 	return Companion.Animation.BATHTUB_HEADPAT_BRUSH_AWAY;
            // case BodyState.BATHING_ON_KNEES:
            // 	return Companion.Animation.BATHTUB_ON_KNEES_HEADPAT_BRUSH_AWAY_TO_ANGRY_IDLE;
            // case BodyState.SQUAT:
            // 	return Companion.Animation.SQUAT_HEADPAT_ANGRY_BRUSH_AWAY;
            // default:
            // 	return Companion.Animation.NONE;
            // }
        }

        private Companion.Animation GetHeadpatStartAnimation()
        {
            if (self.IsTired())
            {
                return self.bodyStateAnimationSets[(int)self.bodyState].GetRandomAnimationSet(AnimationSet.HEADPAT_START_TIRED);
            }
            if (self.IsHappy())
            {
                return self.bodyStateAnimationSets[(int)self.bodyState].GetRandomAnimationSet(AnimationSet.HEADPAT_START_HAPPY);
            }
            else
            {
                return self.bodyStateAnimationSets[(int)self.bodyState].GetRandomAnimationSet(AnimationSet.HEADPAT_START_ANGRY);
            }

            // switch( self.bodyState ){
            // case BodyState.STAND:
            // 	if( self.IsTired() ){
            // 		return GetTiredHeadpatStartAnimation();
            // 	}else{
            // 		return GetStandHeadpatStartAnimation();
            // 	}
            // case BodyState.FLOOR_SIT:
            // 	return GetFloorSitHeadpatStartAnimation();
            // case BodyState.BATHING_RELAX:
            // 	return self.GetAnimationFromMood( Companion.Animation.BATHTUB_RELAX_TO_HAPPY_IDLE );
            // case BodyState.BATHING_IDLE:
            // 	return self.GetAnimationFromMood( Companion.Animation.BATHTUB_HEADPAT_HAPPY_IDLE );
            // case BodyState.BATHING_ON_KNEES:
            // 	return Companion.Animation.BATHTUB_ON_KNEES_HEADPAT_IDLE;
            // case BodyState.TIRED_LAY_PILLOW_SIDE:
            // 	return self.active.sleeping.GetLaySidePillowHeadpatStartAnimation();
            // case BodyState.SLEEP_PILLOW_SIDE:
            // 	return self.active.sleeping.GetSleepSidePillowHeadpatStartAnimation();
            // case BodyState.SLEEP_PILLOW_UP:
            // 	return self.active.sleeping.GetSleepPillowUpHeadpatStartAnimation();
            // case BodyState.AWAKE_PILLOW_UP:
            // 	return self.active.sleeping.GetAwakePillowUpHeadpatStartAnimation();
            // case BodyState.SQUAT:
            // 	return self.GetAnimationFromMood( Companion.Animation.SQUAT_HEADPAT_HAPPY_START );
            // default:
            // 	return Companion.Animation.NONE;
            // }
        }

        private Companion.Animation GetHeadpatIdleAnimation()
        {

            switch (self.bodyState)
            {
                case BodyState.STAND:
                    if (self.IsTired())
                    {
                        return GetTiredHeadpatIdleAnimation();
                    }
                    else
                    {
                        return self.GetAnimationFromMood(Companion.Animation.STAND_HEADPAT_HAPPY_LOOP);
                    }
                case BodyState.FLOOR_SIT:
                    return self.GetAnimationFromMood(Companion.Animation.FLOOR_SIT_LOCOMOTION_HAPPY);
                case BodyState.BATHING_RELAX:
                    return self.GetAnimationFromMood(Companion.Animation.BATHTUB_RELAX_TO_HAPPY_IDLE);
                case BodyState.BATHING_IDLE:
                    return self.GetAnimationFromMood(Companion.Animation.BATHTUB_HEADPAT_HAPPY_IDLE);
                case BodyState.BATHING_ON_KNEES:
                    return Companion.Animation.BATHTUB_ON_KNEES_HEADPAT_IDLE;
                // case BodyState.TIRED_LAY_PILLOW_SIDE:
                // 	return self.active.sleeping.GetLaySidePillowHeadpatIdleAnimation();
                // case BodyState.SLEEP_PILLOW_SIDE:
                // 	return self.active.sleeping.GetSleepSidePillowHeadpatIdleAnimation();
                // case BodyState.SLEEP_PILLOW_UP:
                // 	return self.active.sleeping.GetSleepPillowUpHeadpatIdleAnimation();
                // case BodyState.AWAKE_PILLOW_UP:
                // 	return self.active.sleeping.GetAwakePillowUpHeadpatIdleAnimation();
                case BodyState.SQUAT:
                    return self.GetAnimationFromMood(Companion.Animation.SQUAT_HEADPAT_HAPPY_LOOP);
                default:
                    return Companion.Animation.NONE;
            }
        }

        private Companion.Animation GetStandHeadpatStartAnimation()
        {
            if (self.IsHappy())
            {
                if (wantsMoreHeadpats)
                {
                    return Companion.Animation.STAND_HEADPAT_HAPPY_AFTER_MORE;
                }
                else
                {
                    return Companion.Animation.STAND_HEADPAT_HAPPY_START;
                }
            }
            else
            {
                return Companion.Animation.STAND_HEADPAT_ANGRY_LOOP;
            }
        }

        private Companion.Animation GetFloorSitHeadpatStartAnimation()
        {
            if (self.IsHappy())
            {
                return Companion.Animation.FLOOR_SIT_HEADPAT_HAPPY_START;
            }
            else
            {
                return Companion.Animation.FLOOR_SIT_HEADPAT_ANGRY_LOOP;
            }
        }
    }

}