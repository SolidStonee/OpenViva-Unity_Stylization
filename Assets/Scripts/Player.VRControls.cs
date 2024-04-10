using UnityEngine;

namespace Viva
{

    public class VRController : InputController
    {

        public VRController() : base(null)
        {
        }

        public override void OnEnter(Player player)
        {

            player.head.localPosition = Vector3.zero;
            player.head.localRotation = Quaternion.identity;
        }

        public override void OnFixedUpdateControl(Player player)
        {
            if (GameDirector.instance.controlsAllowed == GameDirector.ControlsAllowed.HAND_INPUT_ONLY || GameSettings.main.vrControls != Player.VRControlType.TRACKPAD)
            {
                return;
            }
            player.UpdateTrackpadBodyRotation();
            player.UpdateVRTrackpadMovement();
            player.LateUpdateVRInputTeleportationMovement();

            player.FixedUpdatePlayerCapsule(player.head.localPosition.y);

            player.ApplyVRHandsToAnimation();
        }
    }

}