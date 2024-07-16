using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Viva
{


    public partial class Player : Character
    {

        private List<BoxCollider> waterBoxColliders = new List<BoxCollider>();
        private Coroutine waterBoxCoroutine = null;

        public Collider surfaceCollider;

        [SerializeField]
        private GameObject handRigidBodyPrefab;
        [SerializeField]
        private AudioLowPassFilter lowPassFilter;
        [SerializeField]
        private AudioClip underwaterSoundLoop;
        [SerializeField]
        private AudioSource headSoundSource;

        public override void OnCharacterCollisionEnter(CharacterCollisionCallback ccc, Collision collision)
        {
            Item colliderItem = collision.collider.gameObject.GetComponent(typeof(Item)) as Item;
            if (colliderItem == null)
            {
                return;
            }
            switch (ccc.collisionPart)
            {
                case CharacterCollisionCallback.Type.RIGHT_PALM:
                    AttemptPoke(colliderItem.mainOwner, collision.collider, rightPlayerHandState);
                    break;
                case CharacterCollisionCallback.Type.LEFT_PALM:
                    AttemptPoke(colliderItem.mainOwner, collision.collider, leftPlayerHandState);
                    break;
            }
        }

        public override void OnCharacterCollisionExit(CharacterCollisionCallback ccc, Collision collision)
        {
            Item colliderItem = collision.collider.gameObject.GetComponent(typeof(Item)) as Item;
            if (colliderItem == null)
            {
                switch (ccc.collisionPart)
                {
                    case CharacterCollisionCallback.Type.HEAD:
                        if (collision.collider.gameObject.layer == WorldUtil.waterLayer)
                        {
                            RemoveWaterBoxCollider(collision.collider as BoxCollider);
                        }
                        break;
                    case CharacterCollisionCallback.Type.ROOT:
                        CheckExitRegion(collision.collider);
                        break;
                }
            }
        }

        public override void OnCharacterTriggerEnter(CharacterTriggerCallback ccc, Collider collider)
        {
            if (collider.gameObject.layer == WorldUtil.waterLayer)
            {
                OnEnterWaterRegion(ccc, collider);
            }
            Item colliderItem = collider.gameObject.GetComponent(typeof(Item)) as Item;
            if (colliderItem == null)
            {
                if (ccc.collisionPart == CharacterTriggerCallback.Type.ROOT)
                {
                    CheckRootEnterRegion(collider);
                }
                return;
            }
            switch (ccc.collisionPart)
            {
                case CharacterTriggerCallback.Type.RIGHT_PALM:
                    rightPlayerHandState.AddToNearestItems(colliderItem, ccc.collisionPart);
                    break;
                case CharacterTriggerCallback.Type.LEFT_PALM:
                    leftPlayerHandState.AddToNearestItems(colliderItem, ccc.collisionPart);
                    break;
                case CharacterTriggerCallback.Type.VIEW:
                    //don't use view for picking up hair items since they get in the way
                    if (colliderItem.settings.itemType != Item.Type.CHARACTER_HAIR)
                    {
                        rightPlayerHandState.AddToNearestItems(colliderItem, ccc.collisionPart);
                        leftPlayerHandState.AddToNearestItems(colliderItem, ccc.collisionPart);
                    }
                    break;
            }
        }
        public override void OnCharacterTriggerStay(CharacterTriggerCallback ccc, Collider collider)
        {
            if (ccc.collisionPart == CharacterTriggerCallback.Type.ROOT)
            {
                if (collider.gameObject.layer == WorldUtil.wallsStatic)
                {
                    if(collider.gameObject.GetComponent<Terrain>())
                        groundTerrain = collider.gameObject.GetComponent<Terrain>();
                    else
                        groundTerrain = null;

                    surfaceCollider = collider;
                    
                }
                    
                UpdateFootstepCheck();
            }
        }
        public override void OnCharacterTriggerExit(CharacterTriggerCallback ccc, Collider collider)
        {
            Item colliderItem = collider.gameObject.GetComponent(typeof(Item)) as Item;
            if (colliderItem == null)
            {
                return;
            }
            switch (ccc.collisionPart)
            {
                case CharacterTriggerCallback.Type.RIGHT_PALM:
                    rightPlayerHandState.RemoveFromNearestItems(colliderItem, ccc.collisionPart);
                    break;
                case CharacterTriggerCallback.Type.LEFT_PALM:
                    leftPlayerHandState.RemoveFromNearestItems(colliderItem, ccc.collisionPart);
                    break;
                case CharacterTriggerCallback.Type.VIEW:
                    if (colliderItem.settings.itemType != Item.Type.CHARACTER_HAIR)
                    {
                        rightPlayerHandState.RemoveFromNearestItems(colliderItem, ccc.collisionPart);
                        leftPlayerHandState.RemoveFromNearestItems(colliderItem, ccc.collisionPart);
                    }
                    break;
            }
        }

        public bool AttemptApplyHeadpatAsSource(PlayerHandState playerHandState)
        {

            if (playerHandState != rightPlayerHandState && playerHandState != leftPlayerHandState)
            {
                return false;
            }
            //can only headpat if hand is not busy
            if (playerHandState.holdType != HoldType.NULL)
            {
                return false;
            }
            if (playerHandState.animSys.currentAnim != Player.Animation.IDLE)
            {
                return false;
            }
            if (playerHandState.HasAttribute(HandState.Attribute.SOAPY))
            {
                playerHandState.animSys.SetTargetAndIdleAnimation(Animation.HEADPAT_SCRUB);
            }
            else
            {
                playerHandState.animSys.SetTargetAndIdleAnimation(Animation.HEADPAT);
            }
            return true;
        }

        public PlayerHandState GetHandStateFromSelfItem(Item item)
        {
            PlayerHandState playerHandState = null;
            if (item == rightPlayerHandState.selfItem)
            {
                playerHandState = rightPlayerHandState;
            }
            else if (item == leftPlayerHandState.selfItem)
            {
                playerHandState = leftPlayerHandState;
            }
            return playerHandState;
        }

        private void CheckRootEnterRegion(Collider collider)
        {
            if (collider.name == "KEYBOARD_HANDICAP")
            {
                SphereCollider sc = collider as SphereCollider;
                if (sc == null)
                {
                    Debug.LogError("ERROR HalfKeyboardCrouch only supported for sphere colliders");
                }
                else
                {
                    BeginKeyboardHalfCrouch(sc);
                }
            }
            //else if (collider.name == "EXPLORING")
            //{
            //    GameDirector.instance.SetUserIsExploring(true);
            //}
        }

        private void CheckExitRegion(Collider collider)
        {
            if (collider.name == "KEYBOARD_HANDICAP")
            {
                StopKeyboardHalfCrouch();
            }
            //else if (collider.name == "EXPLORING")
            //{
            //    GameDirector.instance.SetUserIsExploring(false);
            //}
        }

        private void AttemptPoke(Character owner, Collider collider, PlayerHandState handState)
        {

            Companion companion = owner as Companion;
            if (companion == null)
            {
                return;
            }
            //can only poke if in a cleared state
            if (handState.holdType != HoldType.NULL)
            {
                return;
            }
            //must be in pointing animation
            if (handState.animSys.currentAnim != Player.Animation.POINT)
            {
                return;
            }
            //Viva.DevTools.LogExtended("Companion Collider: " + companion.IdentifyCollider(collider), true, true);
            switch (companion.IdentifyCollider(collider))
            {
                case Companion.BodyPart.TUMMY_CC:
                    if (companion.passive.poke.AttemptTummyPoke(handState.selfItem))
                    {
                        if (handState.animSys.currentAnim == Animation.IDLE)
                        {
                            handState.animSys.SetTargetAnimation(Animation.POKE);
                        }
                    }
                    break;
            }
        }

        private void OnEnterWaterRegion(CharacterTriggerCallback ccc, Collider collider)
        {
            switch (ccc.collisionPart)
            {
                case CharacterTriggerCallback.Type.RIGHT_PALM:
                    rightPlayerHandState.CleanHand();
                    break;
                case CharacterTriggerCallback.Type.LEFT_PALM:
                    leftPlayerHandState.CleanHand();
                    break;
                case CharacterTriggerCallback.Type.HEAD:
                    AddWaterBoxCollider(collider as BoxCollider);
                    break;
            }
        }

        private void AddWaterBoxCollider(BoxCollider bc)
        {
            if (bc == null)
            {
                return;
            }
            waterBoxColliders.Add(bc);
            if (waterBoxCoroutine == null)
            {
                waterBoxCoroutine = GameDirector.instance.StartCoroutine(UnderwaterCheck());
            }
        }

        private IEnumerator UnderwaterCheck()
        {

            while (true)
            {
                bool underwater = false;
                foreach (BoxCollider bc in waterBoxColliders)
                {
                    var umc = bc.gameObject.GetComponent<UnderwaterMaterialChange>();
                    if (bc.bounds.Contains(base.head.position))
                    {
                        underwater = true;
                        if (umc != null)
                        {
                            umc.OnEnterUnderwater();
                        }
                    }
                    else if (umc != null)
                    {
                        umc.OnExitUnderwater();
                    }
                }
                //if (underwater != GameDirector.instance.postProcessing.usingUnderwater)
                //{
                    SetEnableUnderwaterEffects(underwater);
                //}
                yield return new WaitForFixedUpdate();
            }
        }

        private void RemoveWaterBoxCollider(BoxCollider bc)
        {
            if (bc == null)
            {
                return;
            }

            waterBoxColliders.Remove(bc);
            var umc = bc.gameObject.GetComponent<UnderwaterMaterialChange>();
            if (umc != null)
            {
                umc.OnExitUnderwater();
            }

            if (waterBoxColliders.Count == 0)
            {

                if (waterBoxCoroutine != null)
                {
                    GameDirector.instance.StopCoroutine(waterBoxCoroutine);
                    waterBoxCoroutine = null;
                    //if (GameDirector.instance.postProcessing.usingUnderwater)
                    //{
                        SetEnableUnderwaterEffects(false);
                    //}
                }
            }
        }

        private void SetEnableUnderwaterEffects(bool enable)
        {

            headSoundSource.Stop();
            lowPassFilter.enabled = enable;
            if (enable)
            {
                //GameDirector.instance.postProcessing.EnableUnderwaterEffect();
                headSoundSource.clip = underwaterSoundLoop;
                headSoundSource.loop = true;
                headSoundSource.Play();
            }
            else
            {
                //GameDirector.instance.postProcessing.DisableUnderwaterEffect();
            }

            //fix a bug with Unity causing low pass filter from not working
            headItem.transform.GetComponent<AudioListener>().enabled = false;
            headItem.transform.GetComponent<AudioListener>().enabled = true;
        }
    }

}