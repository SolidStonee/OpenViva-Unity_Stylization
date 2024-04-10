﻿using UnityEngine;

namespace Viva
{


    public partial class Player : Character
    {

        [Header("Controls")]
        [SerializeField]
        private ControlType m_controls = ControlType.VR;
        public ControlType controls { get { return m_controls; } set { m_controls = value; } }
        public float walkSpeed = 0.2f;
        public Vector3 moveVel = Vector3.zero;
        public Vector2 movement { get; private set; } = Vector2.zero;
        public Vector2 mouseVelocity { get; private set; }
        public Vector2 mouseVelocitySum = Vector2.zero;
        public Vector3 mousePosition { get; private set; } = Vector2.zero;
        public Vector2 handScrolling { get; private set; }
        public bool keyboardAlt { get; private set; } = false;

        [SerializeField]
        private ObjectFingerPointer m_objectFingerPointer;
        public ObjectFingerPointer objectFingerPointer { get { return m_objectFingerPointer; } }
        [SerializeField]
        private CapsuleCollider m_characterCC;
        public CapsuleCollider characterCC { get { return m_characterCC; } }
        [SerializeField]
        private Rigidbody m_rigidBody;
        public Rigidbody rigidBody { get { return m_rigidBody; } }
        [SerializeField]
        private Transform m_armature;
        public Transform armature { get { return m_armature; } }
        [SerializeField]
        private Animator animator;
        [SerializeField]
        public SkinnedMeshRenderer handsSMR;
        [SerializeField]
        public Rigidbody headRigidBody;
        [SerializeField]
        private GameObject keyboardHelperItemDetector;
        [SerializeField]
        private PlayerHeadState m_playerHeadState;
        public PlayerHeadState playerHeadState { get { return m_playerHeadState; } }
        [SerializeField]
        private PlayerHandState m_rightPlayerHandState;
        public PlayerHandState rightPlayerHandState { get { return m_rightPlayerHandState; } }
        [SerializeField]
        private PlayerHandState m_leftPlayerHandState;
        public PlayerHandState leftPlayerHandState { get { return m_leftPlayerHandState; } }
        [SerializeField]
        private PauseMenu m_pauseMenu;
        public PauseMenu pauseMenu { get { return m_pauseMenu; } }
        public GameObject crosshair;
        private InputController m_controller;
        public InputController controller { get { return m_controller; } }
        private bool grabbingEnabled = true;
        private bool initialized = false;

        protected override Vector3 CalculateFloorPosition()
        {
            return new Vector3(base.head.position.x, transform.position.y, base.head.position.z);
        }

        public override bool IsSittingOnFloor()
        {
            return base.head.position.y - transform.position.y < 0.8f;
        }

        public void SetInputController(InputController newController)
        {

            if (newController == null)
            {
                if (m_controls == ControlType.KEYBOARD)
                {
                    newController = new KeyboardController();
                }
                else
                {
                    newController = new VRController();
                }
            }
            if (controller != null)
            {
                controller.OnExit(this);
            }
            m_controller = newController;
            controller.OnEnter(this);
        }

        protected override void OnCharacterAwake()
        {

            InitAnimations();

            rightPlayerHandState.Initialize(animationInfos);
            leftPlayerHandState.Initialize(animationInfos);
            GameSettings.main.SetDefaultCalibration();

            if (initialized)
            {
                return;
            }
            //settings are built in to start in vr then switch to keyboard if it fails
            initialized = true;
            BindAllControls();
            SetControls(ControlType.VR);

            ReloadCurrentControlType();
            SetInputController(null);    //ensure controller exists on initialize
            GameSettings.main.Apply();
            BindButtonStateCallbacks();
        }

        private void BindButtonStateCallbacks()
        {
            rightPlayerHandState.gripState.onDown += delegate
            {
                objectFingerPointer.PointDown(rightPlayerHandState);
            };
            rightPlayerHandState.gripState.onUp += delegate
            {
                objectFingerPointer.PointUp(rightPlayerHandState);
            };
            leftPlayerHandState.gripState.onDown += delegate
            {
                objectFingerPointer.PointDown(leftPlayerHandState);
            };
            leftPlayerHandState.gripState.onUp += delegate
            {
                objectFingerPointer.PointUp(leftPlayerHandState);
            };
        }

        private void ReloadCurrentControlType()
        {
            Debug.Log("[Controls] Reloaded " + m_controls);
            bool usingKeyboardControls = m_controls == ControlType.KEYBOARD;

            SetEnableKeyboardControls(usingKeyboardControls);
            SetEnableVRControls(!usingKeyboardControls);

            SetInputController(null);  //refresh controller
            GameSettings.main.Apply();
            if (GameDirector.instance.IsAnyUIMenuActive())
            {
                //open correct updated control scheme
                GameDirector.instance.UpdateSourcePlayerUIControlType();
                pauseMenu.clickControls();
                pauseMenu.OrientPauseBookToPlayer();
            }
        }

        private void BindAllControls()
        {
            var vivaControls = GameDirector.input.actions;
            vivaControls.Enable();
            rightPlayerHandState.InitializeInputControls( vivaControls );
            leftPlayerHandState.InitializeInputControls( vivaControls );
            vivaControls.Keyboard.movement.performed += ctx => movement = ctx.ReadValue<Vector2>();
            vivaControls.Keyboard.keyboardAlt.performed += ctx => keyboardAlt = ctx.ReadValueAsButton();
            vivaControls.Keyboard.keyboardAlt.canceled += ctx => keyboardAlt = ctx.ReadValueAsButton();
            vivaControls.Keyboard.mouseVelocity.performed += ctx => mouseVelocity = ctx.ReadValue<Vector2>();
            vivaControls.Keyboard.mousePosition.performed += ctx => mousePosition = ctx.ReadValue<Vector2>();
            vivaControls.Keyboard.handScrolling.performed += ctx => OnInputScroll(ctx.ReadValue<Vector2>());
                
            vivaControls.Keyboard.wave.performed += ctx => OnInputWaveRightHand();
            vivaControls.Keyboard.follow.performed += ctx => OnInputFollowRightHand();
            vivaControls.Keyboard.stop.performed += ctx => OnInputStopHand();
            vivaControls.Keyboard.crouch.performed += ctx => FlipKeyboardHeight();
            vivaControls.Keyboard.pauseButton.performed += ctx => TogglePauseMenu();
        }

        public void SetControls(ControlType newControls)
        {
            controls = newControls;
            ReloadCurrentControlType();
        }

        public void SetCrosshair(bool enabled)
        {
            if (enabled && controls != ControlType.VR)
            {
                crosshair.SetActive(true);
            }
            else
            {
                crosshair.SetActive(false);
            }
        }

        public void TogglePauseMenu()
        {
            if (!GameDirector.instance.isUIMenuActive(pauseMenu))
            {
                if (keyboardAlt)
                {
                    GameDirector.instance.SetEnableCursor(true);
                    GameDirector.instance.SetEnableControls(GameDirector.ControlsAllowed.NONE);
                }
                else
                {
                    OpenPauseMenu();
                }
            }
            else
            {
                GameDirector.instance.StopUIInput();
            }
        }
        public void OpenPauseMenu()
        {
            GameDirector.instance.BeginUIInput(pauseMenu, this);
        }

        public override void OnCharacterFixedUpdate()
        {
            
            animator.Update(Time.fixedDeltaTime);
            FixedUpdateKeyboardHandAnimationEvents();
            FixedUpdateHandAnimationSystems();
            if (controls == ControlType.VR)
            {
                UpdateVRAnimationSystems();
            }

            controller.OnFixedUpdateControl(this);

            onModifyAnimations?.Invoke();

            rightPlayerHandState.ApplyRigidBodyTransform();
            leftPlayerHandState.ApplyRigidBodyTransform();
        }

        public void FixedUpdatePlayerCapsule(float headHeight)
        {

            //clamp head position to capsuleCenterWorldPos by radius
            Vector3 capsuleHeadPlane = transform.TransformPoint(new Vector3(characterCC.center.x, 0.0f, characterCC.center.z));
            capsuleHeadPlane.y = base.head.position.y;
            Vector3 toCapsule = capsuleHeadPlane - base.head.position;
            float toCapsuleDistance = toCapsule.magnitude;

            Vector3 adjustedHeadPos = capsuleHeadPlane;
            float distance = 0.0f;//System.Convert.ToInt32( controls == ControlType.OPEN_VR )*0.4f;
            if (toCapsuleDistance > distance)
            {
                adjustedHeadPos = base.head.position + (toCapsule / toCapsuleDistance) * Mathf.Min(distance, toCapsuleDistance);
            }

            Vector3 localHead = transform.InverseTransformPoint(adjustedHeadPos);
            characterCC.height = Mathf.Max(0.0f, headHeight);    //prevent going through the floor
            localHead.y /= 2.0f;
            if (localHead.y < characterCC.radius)
            {
                localHead.y += characterCC.radius - localHead.y;
            }
            characterCC.center = localHead;
        }

        public override void OnCharacterUpdate()
        {

            rightPlayerHandState.ApplyPhysicsTransform();
            leftPlayerHandState.ApplyPhysicsTransform();

            if (transform.position.y < 0.0f)
            {
                transform.position = new Vector3(62.34f, 144.57f, 325.11f);
            }
        }

        public override void OnCharacterLateUpdatePostIK()
        {

            if (!GameDirector.instance.IsAimingAtUI())
            {
                LateUpdatePostIKItemInteraction(rightPlayerHandState);
                LateUpdatePostIKItemInteraction(leftPlayerHandState);
                LateUpdatePostIKGestures();
            }
            controller.OnLateUpdateControl(this);
        }

        public void SetShowModel(bool show)
        {
            handsSMR.enabled = show;
        }
    }

}