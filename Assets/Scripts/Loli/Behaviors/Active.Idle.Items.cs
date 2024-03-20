﻿using Steamworks.Data;
using UnityEngine;


namespace viva
{


    public partial class IdleBehavior : ActiveBehaviors.ActiveTask
    {

        private float eatTimer = 0.0f;
        private float eatWaitTime = 0.5f;
        private float itemIdleAnimWaitTimer = 0.0f;
        private float polaroidFrameInspectTimer = 0.0f;
        private float lastPolaroidFramePantyTime = 0.0f;
        private float waterReedInteractiontimer = 0.0f;
        public Companion.Animation polaroidFrameReactAnim = Companion.Animation.NONE;
        private PolaroidFrameRippedFX activeRippedFX = null;
        private GameObject targetEdible = null;

        private void InitItemSubBehaviors()
        {
        }

        public class TransitionToPolaroidFrameReact : Companion.TransitionHandle
        {

            public TransitionToPolaroidFrameReact() : base(Companion.TransitionHandle.TransitionType.NO_MIRROR)
            {
            }
            public override void Transition(Companion self)
            {
                self.UpdateAnimationTransition(self.active.idle.polaroidFrameReactAnim);
            }
        }

        public void EatTargetEdible()
        {

            if (targetEdible == null)
            {
                return;
            }
            if (self.animator.IsInTransition(1))
            {
                return;
            }

            Object.Destroy(targetEdible);
            if (self.currentAnim == Companion.Animation.STAND_HAPPY_DONUT_LAST_BITE_RIGHT ||
                self.currentAnim == Companion.Animation.STAND_HAPPY_DONUT_LAST_BITE_LEFT)
            {

                Item item = targetEdible.transform.parent.GetComponent(typeof(Item)) as Item;
                OccupyState occupyState = item.mainOwner.FindOccupyStateByHeldItem(item);
                if (occupyState != null)
                {
                    occupyState.AttemptDrop();
                }
                Object.Destroy(targetEdible.transform.parent.gameObject);
                self.ShiftHappiness(2); //+2 happiness
            }
        }

        public void UpdateIdleHoldItemInteraction(CompanionHandState handState)
        {
            if (handState.heldItem == null)
            {
                return;
            }
            bool throwInstead = false;
            if (!self.IsHappy())
            {
                switch (handState.heldItem.settings.itemType)
                {
                    case Item.Type.HAT:
                    case Item.Type.DUCKY:
                    case Item.Type.EGG:
                        throwInstead = true;
                        break;
                }
            }

            if (throwInstead)
            {
                AttemptToThrowHandObject(handState);
            }
            else
            {
                switch (handState.heldItem.settings.itemType)
                {
                    case Item.Type.DONUT:
                        UpdateEdibleItemInteraction(handState);
                        break;
                    case Item.Type.PASTRY:
                        Pastry pastry = handState.heldItem as Pastry;
                        if (pastry.isBaked)
                        {
                            UpdateEdibleItemInteraction(handState);
                        }
                        break;
                    case Item.Type.HAT:
                        UpdateIdleHatInteraction(handState);
                        break;
                    case Item.Type.WATER_REED:
                        UpdateIdleWaterReedInteraction(handState);
                        break;
                    case Item.Type.POLAROID_FRAME:
                        UpdateIdlePolaroidFrameInteraction(handState);
                        break;
                }
            }
        }

        public void AttachBagOnShoulder()
        {

            if (self.rightHandState.heldItem && self.rightHandState.heldItem.settings.itemType == Item.Type.BAG)
            {
                (self.rightHandState.heldItem as Bag).FlagWearOnLoliShoulder(false);
            }
            else if (self.leftHandState.heldItem && self.leftHandState.heldItem.settings.itemType == Item.Type.BAG)
            {
                (self.leftHandState.heldItem as Bag).FlagWearOnLoliShoulder(true);
            }
        }

        public void RemoveBagOnShoulder()
        {

            if (self.currentAnim == Companion.Animation.STAND_REMOVE_BAG_RIGHT)
            {
                if (self.rightShoulderState.heldItem)
                {
                    self.leftCompanionHandState.GrabItemRigidBody(self.rightShoulderState.heldItem);
                }
            }
            else
            {
                if (self.leftShoulderState.heldItem)
                {
                    self.rightCompanionHandState.GrabItemRigidBody(self.leftShoulderState.heldItem);
                }
            }
        }

        private void UpdateEdibleItemInteraction(CompanionHandState handState)
        {
            if (handState.UpdateHoldItemInteractTimer(ref eatTimer, eatWaitTime))
            {
                eatWaitTime = 2.0f + Random.value * 3.0f;

                targetEdible = null;
                int edibleCount = 0;
                for (int i = 0; i < handState.heldItem.transform.childCount; i++)
                {
                    GameObject candidate = handState.heldItem.transform.GetChild(i).gameObject;
                    if (!candidate.activeSelf)
                    {
                        continue;
                    }

                    if (candidate.name.Contains("EDIBLE"))
                    {
                        targetEdible = candidate;
                        edibleCount++;
                    }
                }
                if (targetEdible != null)
                {
                    if (edibleCount > 1)
                    {
                        if (handState == self.rightHandState)
                        {
                            self.SetTargetAnimation(Companion.Animation.STAND_HAPPY_EAT_ITEM_RIGHT);
                        }
                        else
                        {
                            self.SetTargetAnimation(Companion.Animation.STAND_HAPPY_DONUT_EAT_LEFT);
                        }
                    }
                    else
                    {
                        if (handState == self.rightHandState)
                        {
                            self.SetTargetAnimation(Companion.Animation.STAND_HAPPY_DONUT_LAST_BITE_RIGHT);
                        }
                        else
                        {
                            self.SetTargetAnimation(Companion.Animation.STAND_HAPPY_DONUT_LAST_BITE_LEFT);
                        }
                    }
                }
            }
        }

        private void UpdateIdleHatInteraction(CompanionHandState handState)
        {
            if (handState == self.rightHandState)
            {
                self.SetTargetAnimation(Companion.Animation.STAND_WEAR_SUNHAT_RIGHT);
            }
            else if (handState == self.leftHandState)
            {
                self.SetTargetAnimation(Companion.Animation.STAND_WEAR_SUNHAT_LEFT);
            }
        }

        public void ResetPolaroidFrameInspectTimer()
        {
            polaroidFrameInspectTimer = -15.0f;
        }

        private void UpdateIdlePolaroidFrameInteraction(CompanionHandState handState)
        {
            if (handState.UpdateHoldItemInteractTimer(ref polaroidFrameInspectTimer, 15.0f))
            {

                PolaroidFrame frame = handState.heldItem as PolaroidFrame;
                if (handState == self.rightHandState)
                {
                    self.SetTargetAnimation(Companion.Animation.STAND_POLAROID_FRAME_REACT_IN_RIGHT);
                    switch (frame.photoSummary)
                    {
                        case PolaroidFrame.PhotoSummary.GENERIC:
                            polaroidFrameReactAnim = Companion.Animation.STAND_POLAROID_FRAME_REACT_NORMAL_RIGHT;
                            break;
                        case PolaroidFrame.PhotoSummary.PANTY:
                            polaroidFrameReactAnim = Companion.Animation.STAND_POLAROID_FRAME_REACT_PANTY_RIGHT;
                            self.ShiftHappiness(-2);
                            break;
                    }
                }
                else if (handState == self.leftHandState)
                {
                    self.SetTargetAnimation(Companion.Animation.STAND_POLAROID_FRAME_REACT_IN_LEFT);
                    switch (frame.photoSummary)
                    {
                        case PolaroidFrame.PhotoSummary.GENERIC:
                            polaroidFrameReactAnim = Companion.Animation.STAND_POLAROID_FRAME_REACT_NORMAL_LEFT;
                            break;
                        case PolaroidFrame.PhotoSummary.PANTY:
                            polaroidFrameReactAnim = Companion.Animation.STAND_POLAROID_FRAME_REACT_PANTY_LEFT;
                            self.ShiftHappiness(-2);
                            break;
                    }
                    self.SetLookAtTarget(GameDirector.player.head, 1.0f);
                }
            }
            else if (Time.time - lastPolaroidFramePantyTime < 3.0f)
            {

                PolaroidFrame frame = handState.heldItem as PolaroidFrame;
                if (frame.photoSummary == PolaroidFrame.PhotoSummary.PANTY)
                {
                    if (handState == self.rightHandState)
                    {
                        self.SetTargetAnimation(Companion.Animation.STAND_POLAROID_FRAME_REACT_RIP);
                    }
                }
            }
        }

        private void AttemptRipPolaroidFrame(OccupyState handState)
        {
            if (handState.heldItem == null)
            {
                return;
            }
            PolaroidFrame polaroidFrame = handState.heldItem as PolaroidFrame;
            if (polaroidFrame != null)
            {
                polaroidFrame.SpawnRippedInstance(
                    self.rightHandState.fingerAnimator.targetBone,
                    self.leftHandState.fingerAnimator.targetBone
                );
                activeRippedFX = polaroidFrame.SpawnRipFXParticleEmitter();
                handState.AttemptDrop();
                polaroidFrame.PlayRipSound();
            }
        }

        public void SpawnPolaroidRippedFX()
        {
            //destroy all panty polaroids held
            AttemptRipPolaroidFrame(self.rightHandState);
            AttemptRipPolaroidFrame(self.leftHandState);
            if (activeRippedFX != null)
            {
                activeRippedFX.EmitFX(self.rightHandState.fingerAnimator.hand.position);
            }
        }

        private void UpdateIdleWaterReedInteraction(CompanionHandState handState)
        {
            if (handState.UpdateHoldItemInteractTimer(ref waterReedInteractiontimer, 60.0f))
            {
                //idle anim only fires with 1 object in hand
                if (self.rightHandState.holdType == HoldType.NULL ||
                    self.leftHandState.holdType == HoldType.NULL)
                {

                    if (handState == self.rightHandState)
                    {
                        self.SetTargetAnimation(Companion.Animation.STAND_CATTAIL_IDLE1_RIGHT);
                    }
                    else if (handState == self.leftHandState)
                    {
                        self.SetTargetAnimation(Companion.Animation.STAND_CATTAIL_IDLE1_LEFT);
                    }
                }
            }
            else if (self.happiness == Companion.Happiness.VERY_ANGRY)
            {
                self.active.cattail.StartCattailBehavior();
            }
        }

        public void AttemptToThrowHandObject(CompanionHandState handState)
        {

            if (handState.heldItem == null)
            {
                return;
            }
            if (self.CanSeePoint(GameDirector.player.head.position))
            {
                var facedirection = CompanionUtility.SetRootFacingTarget(self, "face player", 2.0f, GameDirector.player.floorPos);
                self.autonomy.Interrupt(facedirection);
                float bearing = Tools.Bearing(self.transform, GameDirector.player.floorPos);
                if (Mathf.Abs(bearing) < 20.0f)
                {
                    if (handState.rightSide)
                    {
                        self.SetTargetAnimation(Companion.Animation.STAND_ANGRY_THROW_RIGHT);
                    }
                    else
                    {
                        self.SetTargetAnimation(Companion.Animation.STAND_ANGRY_THROW_LEFT);
                    }
                }
            }
        }

        public void AttachHandHat(OccupyState handState)
        {
            Item item = handState.heldItem;
            if (item == null)
            {
                return;
            }
            self.headState.WearOnHead(item, self.headModel.hatLocalPosAndPitch, HoldType.OBJECT, 1.0f);

            GameDirector.player.CompleteAchievement(Player.ObjectiveType.FIND_HAT, new Achievement("FIND_HAT"));
        }

        public void ThrowHandObject(CompanionHandState handState)
        {

            if (handState.heldItem == null)
            {
                return;
            }
            Item item = handState.heldItem;
            if (item.settings.itemType == Item.Type.DUCKY)
            {
                GameDirector.player.CompleteAchievement(Player.ObjectiveType.THROW_DUCK, new Achievement("THROW_DUCK"));
            }

            handState.AttemptDrop();
            item.rigidBody.velocity = (GameDirector.player.head.position - handState.fingerAnimator.hand.position).normalized * 4.5f;
        }
    }

}