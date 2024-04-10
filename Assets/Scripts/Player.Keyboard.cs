using System.Collections;
using UnityEngine;


namespace Viva
{


    public partial class Player : Character
    {


        public float keyboardCurrentHeight = 1.4f;
        public float keyboardTargetHeight = 1.4f;
        public float keyboardStandingHeight = 1.4f;
        public float keyboardFloorHeight = 0.5f;
        public float keyboardArmatureZoom = 0.5f;
        private float keyboardMaxHeightOverride = 1.4f;
        private float enableMouseRotationMult = 1.0f;
        private Coroutine halfCrouchCoroutine = null;


        private void SetEnableKeyboardControls(bool enable)
        {
            if (enable)
            {
                GameDirector.input.actions.Keyboard.Enable();
                crosshair.SetActive(true);
                head.localPosition = Vector3.up * keyboardCurrentHeight;
                GameDirector.instance.mainCamera.stereoTargetEye = StereoTargetEyeMask.None;
                GameDirector.instance.mainCamera.fieldOfView = 65.0f;
            }
            else
            {
                GameDirector.input.actions.Keyboard.Disable();
            }
            keyboardHelperItemDetector.SetActive(enable);
        }

        public float GetKeyboardCurrentHeight()
        {
            return keyboardCurrentHeight;
        }

        private void BeginKeyboardHalfCrouch(SphereCollider sc)
        {
            if (halfCrouchCoroutine != null)
            {
                return;
            }
            halfCrouchCoroutine = GameDirector.instance.StartCoroutine(PersistHalfCrouch(sc));
        }

        private void StopKeyboardHalfCrouch()
        {
            if (halfCrouchCoroutine == null)
            {
                return;
            }
            GameDirector.instance.StopCoroutine(halfCrouchCoroutine);
            keyboardTargetHeight = keyboardStandingHeight;
            halfCrouchCoroutine = null;
        }

        private IEnumerator PersistHalfCrouch(SphereCollider sc)
        {

            while (true)
            {
                float dist = Vector3.Distance(sc.transform.TransformPoint(sc.center), transform.position);
                float blend = 1.0f - dist / sc.radius;
                keyboardMaxHeightOverride = Mathf.Lerp(keyboardStandingHeight, 0.5f, blend);
                yield return new WaitForFixedUpdate();
            }
        }

        public void ApplyHeadTransformToArmature()
        {
            armature.position = head.position + head.forward * Mathf.Lerp(0.2f, 0.8f, keyboardArmatureZoom);
            armature.rotation = head.rotation * Quaternion.Euler(-90.0f, 0.0f, 0.0f);
        }
        public void UpdateGUIKeyboardShortcuts()
        {
            // if( InputOLD.GetKeyDown(KeyCode.Tab) ){
            //     if( InputOLD.GetKey(KeyCode.LeftShift) ){
            //         pauseMenu.cycleButton(-1);
            //     }else{
            //         pauseMenu.cycleButton(1);
            //     }
            // }else if( InputOLD.GetKeyDown(KeyCode.Space) ){
            //     pauseMenu.clickCurrentCycledButton();
            // }
        }

        public Vector2 CalculateMouseMovement()
        {
            return new Vector2(-mouseVelocity.y, mouseVelocity.x) * (GameSettings.main.mouseSensitivity * Time.deltaTime * 0.01f);
        }

        public void SetKeyboardMouseRotationMult(float mult)
        {
            enableMouseRotationMult = mult;
        }

        public void OnInputScroll(Vector2 scroll)
        {
            Debug.Log("Scroll.X: " + scroll.x + " Scroll.Y: " + scroll.y);
            if (scroll.y > 0)
            {
                keyboardArmatureZoom += 0.15f;
            }
            if(scroll.y < 0)
            {
                keyboardArmatureZoom -= 0.15f;
            }

            keyboardArmatureZoom = Mathf.Clamp(keyboardArmatureZoom, 0, 1);

        }

        public void OnInputTogglePresentHand(PlayerHandState handState)
        {
            if (handState.animSys.currentAnim == handState.animSys.idleAnimation)
            {
                if (handState.rightSide)
                {
                    handState.animSys.SetTargetAnimation(Animation.GESTURE_PRESENT_RIGHT);
                }
                else
                {
                    handState.animSys.SetTargetAnimation(Animation.GESTURE_PRESENT_LEFT);
                }
            }
            else
            {
                handState.animSys.SetTargetAnimation(handState.animSys.idleAnimation);
            }
        }

        private void OnInputFollowRightHand()
        {
            if (rightPlayerHandState.animSys.currentAnim == rightPlayerHandState.animSys.idleAnimation)
            {
                rightPlayerHandState.animSys.SetTargetAnimation(Animation.GESTURE_COME);
            }
        }

        private void OnInputStopHand()
        {
            if (rightPlayerHandState.animSys.currentAnim == rightPlayerHandState.animSys.idleAnimation)
            {
                rightPlayerHandState.animSys.SetTargetAnimation(Animation.GESTURE_STOP);
            }
        }

        private void OnInputWaveRightHand()
        {
            if (rightPlayerHandState.animSys.currentAnim == rightPlayerHandState.animSys.idleAnimation)
            {
                rightPlayerHandState.animSys.SetTargetAnimation(Animation.GESTURE_WAVE);
            }
        }

        private void FlipKeyboardHeight()
        {
            if (keyboardTargetHeight == keyboardFloorHeight)
            {
                keyboardTargetHeight = keyboardStandingHeight;
            }
            else
            {
                keyboardTargetHeight = keyboardFloorHeight;
            }
        }

        public void UpdateInputKeyboardCrouching()
        {
            keyboardCurrentHeight += (keyboardTargetHeight - keyboardCurrentHeight) * Time.deltaTime * 5.0f;
            keyboardCurrentHeight = Mathf.Min(keyboardCurrentHeight, keyboardMaxHeightOverride);
            head.localPosition = Vector3.up * keyboardCurrentHeight;
        }

        public void UpdateInputKeyboardRotateHead()
        {
            mouseVelocitySum += CalculateMouseMovement() * enableMouseRotationMult;

            //Fix head camera roll
            head.transform.rotation = Quaternion.LookRotation(head.forward, Vector3.up);
            head.transform.rotation = Quaternion.RotateTowards(Quaternion.LookRotation(new Vector3(head.forward.x, 0.0f, head.forward.z)), head.transform.rotation, 75.0f);
        }
    }

}