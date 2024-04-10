using UnityEngine;
using Viva.Util;


namespace Viva
{

    public class HorseKeyboardControls : HorseControls
    {

        public TransformBlend playerArmatureBlend = new TransformBlend();

        public HorseKeyboardControls(Horse _horse) : base(_horse)
        {
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            playerArmatureBlend.SetTarget(false, player.armature, true, true, 0.0f, 0.6f, 1.0f);
            player.headRigidBody.isKinematic = true;

            GameDirector.input.actions.Keyboard.w.performed += ctx => OnInputHorseGear(1);
            GameDirector.input.actions.Keyboard.s.performed += ctx => OnInputHorseGear(-1);
            GameDirector.input.actions.Keyboard.a.performed += ctx => OnInputHorseDirection(-1);
            GameDirector.input.actions.Keyboard.a.canceled += ctx => OnInputHorseDirection(0);
            GameDirector.input.actions.Keyboard.d.performed += ctx => OnInputHorseDirection(1);
            GameDirector.input.actions.Keyboard.d.canceled += ctx => OnInputHorseDirection(0);
        }

        public override void OnExit(Player player)
        {
            base.OnExit(player);
            player.headRigidBody.isKinematic = false;

            player.transform.position = horse.spine1.position + Vector3.down * 0.5f + horse.transform.forward;

            GameDirector.input.actions.Keyboard.w.performed -= ctx => OnInputHorseGear(1);
            GameDirector.input.actions.Keyboard.s.performed -= ctx => OnInputHorseGear(-1);
            GameDirector.input.actions.Keyboard.a.performed -= ctx => OnInputHorseDirection(-1);
            GameDirector.input.actions.Keyboard.a.canceled -= ctx => OnInputHorseDirection(0);
            GameDirector.input.actions.Keyboard.d.performed -= ctx => OnInputHorseDirection(1);
            GameDirector.input.actions.Keyboard.d.canceled -= ctx => OnInputHorseDirection(0);
        }

        public override void OnFixedUpdateControl(Player player)
        {

            if (GameDirector.instance.controlsAllowed == GameDirector.ControlsAllowed.ALL)
            {
                player.UpdateInputKeyboardRotateHead();
            }
        }

        public override void OnLateUpdateControl(Player player)
        {

            Vector3 headForce = Vector3.right * player.mouseVelocitySum.x;
            headForce += Vector3.up * player.mouseVelocitySum.y;

            float angleSlow = 1.0f - Mathf.Abs(player.head.forward.y) * 0.5f;
            player.head.rotation *= Quaternion.Euler(player.mouseVelocitySum.x, player.mouseVelocitySum.y, 0.0f);
            player.mouseVelocitySum *= 0.6f;

            //controls
            BlendPlayerTransform(horse.keyboardPlayerMountOffset);

            // clamp headRotation to face straight
            player.head.rotation = Quaternion.RotateTowards(
                player.transform.rotation,
                player.head.rotation,
                150.0f
            );
            //remove roll from head rotation
            player.head.rotation = Quaternion.LookRotation(player.head.forward, Vector3.up);

            player.ApplyHeadTransformToArmature();
            playerArmatureBlend.Blend(horse.keyboardPlayerRigOffset.position, Quaternion.Euler(horse.keyboardPlayerRigOffset.eulerRotation));
        }

        private void OnInputHorseGear(int shift)
        {
            if (horse.IsCurrentAnimation(horse.locomotionID))
            {
                horse.ShiftSpeed(shift);
            }
        }

        private void OnInputHorseDirection(float direction)
        {
            if (horse.IsCurrentAnimation(horse.locomotionID))
            {
                horse.targetSide = direction;
            }
        }
    }

}