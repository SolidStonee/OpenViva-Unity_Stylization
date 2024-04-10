using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Viva.Util;

namespace Viva
{


    public partial class PauseMenu : UIMenu
    {

        private MenuTutorial menuTutorialPhase = MenuTutorial.NONE;
        private Coroutine tutorialCoroutine = null;
        private bool m_finishedDisplayHint = true;
        public bool finishedDisplayedHint { get { return m_finishedDisplayHint; } }

        [SerializeField]
        private RectTransform HUD_canvas;

        [SerializeField]
        private GameObject tutorialCircle = null;

        [SerializeField]
        private GameObject tutorialDuckPrefab = null;

        [SerializeField]
        private GameObject tutorialFramePrefab = null;

        [SerializeField]
        private GameObject achievementPrefab;

        GameObject duck;

        Item frame;

        public enum HintType
        {
            ACHIEVEMENT,
            HINT,
            HINT_NO_IMAGE
        }

        public enum MenuTutorial
        {
            NONE,
            WAIT_TO_OPEN_PAUSE_MENU,
            WAIT_TO_ENTER_CHECKLIST,
            WAIT_TO_EXIT_CHECKLIST,

            WAIT_CROUCH,
            WAIT_TO_WAVE,
            WAIT_TO_COME_HERE,
            WAIT_TO_START_PICKUP,
            WAIT_TO_PRESENT,

            WAIT_TO_PICKUP_FRAME,
            WAIT_TO_RIP_FRAME,

            FINISHED_ALL
        }

        [SerializeField]
        private Sprite[] hintTypeFrames = new Sprite[System.Enum.GetValues(typeof(HintType)).Length];
        [SerializeField]
        private Sprite[] hintTypeImages = new Sprite[System.Enum.GetValues(typeof(HintType)).Length];

        [SerializeField]
        private AudioClip menuSound;

        [SerializeField]
        private AudioClip achievementSound;

        private int achievementPanelsActive = 0;

        private void ExitTutorial()
        {
            if (tutorialCoroutine == null)
            {
                return;
            }
            Debug.Log("[TUTORIAL] Exited");
            tutorialCircle.SetActive(false);
            menuTutorialPhase = MenuTutorial.FINISHED_ALL;
            GameDirector.instance.StopCoroutine(tutorialCoroutine);
            tutorialCoroutine = null;
            //destroy tutorial objects
            Destroy(frame);
            Destroy(duck);
            //resume game
            GameDirector.player.transform.position = lastPositionBeforeTutorial;
            //reset music
            GameDirector.instance.SetMusic(GameDirector.instance.GetDefaultMusic(), 3.0f);
            GameDirector.instance.SetUserIsExploring(false);
        }
        private void CheckIfExitedTutorialCircle()
        {
            if (Vector3.SqrMagnitude(transform.position - tutorialCircle.transform.position) > 90.0f)
            {
                ExitTutorial();
            }
        }

        public void ContinueTutorial(MenuTutorial continuePhase)
        {
            if (tutorialCoroutine != null)
            {
                if ((MenuTutorial)((int)menuTutorialPhase + 1) == continuePhase)
                {
                    menuTutorialPhase = continuePhase;
                    Debug.Log("Continued tutorial! Now at " + menuTutorialPhase);
                }
            }
        }

        public MenuTutorial GetMenuTutorialPhase()
        {
            return menuTutorialPhase;
        }

        private IEnumerator Tutorial()
        {
            tutorialCircle.SetActive(true);
            Player player = GameDirector.player;
            yield return new WaitForSeconds(1.0f);

            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "Welcome"), true, HintType.HINT_NO_IMAGE);
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "LeaveGreenCircle"), true, HintType.HINT);

            while (!m_finishedDisplayHint)
            {
                CheckIfExitedTutorialCircle();
                yield return null;
            }
            if (player.controls == Player.ControlType.KEYBOARD)
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "OpenPauseBook"), true, HintType.HINT, MenuTutorial.WAIT_TO_OPEN_PAUSE_MENU);
                while (menuTutorialPhase <= MenuTutorial.NONE)
                {
                    CheckIfExitedTutorialCircle();
                    yield return null;
                }
            }
            else
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "OpenPauseBookVR"), true, HintType.HINT, MenuTutorial.WAIT_TO_OPEN_PAUSE_MENU);
                while (menuTutorialPhase <= MenuTutorial.NONE)
                {
                    CheckIfExitedTutorialCircle();
                    yield return null;
                }
            }
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "Welcome"), true, HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_ENTER_CHECKLIST);
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "RecommendCalibrate"), true, HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_ENTER_CHECKLIST);
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "WhenReady"), true, HintType.HINT, MenuTutorial.WAIT_TO_ENTER_CHECKLIST);
            while (menuTutorialPhase <= MenuTutorial.WAIT_TO_OPEN_PAUSE_MENU)
            {
                CheckIfExitedTutorialCircle();
                yield return null;
            }
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "CompleteTasks"), true, HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_EXIT_CHECKLIST);
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "WhenReady2"), true, HintType.HINT, MenuTutorial.WAIT_TO_EXIT_CHECKLIST);
            while (menuTutorialPhase <= MenuTutorial.WAIT_TO_ENTER_CHECKLIST)
            {
                CheckIfExitedTutorialCircle();
                yield return null;
            }

            if (player.controls == Player.ControlType.KEYBOARD)
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "WalkRun"), true, HintType.HINT);
            }
            else
            {
                if (GameSettings.main.vrControls == Player.VRControlType.TRACKPAD)
                {
                    DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "WalkRun2"), true, HintType.HINT_NO_IMAGE);
                    DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "RunFaster"), true, HintType.HINT);
                }
                else
                {
                    DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "Teleport"), true, HintType.HINT);
                }
            }
            while (!m_finishedDisplayHint)
            {
                CheckIfExitedTutorialCircle();
                yield return null;
            }
            if (player.controls == Player.ControlType.KEYBOARD)
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "SitDown"), true, HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_CROUCH);
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "HandGames"), true, HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_CROUCH);
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "Crouch"), true, HintType.HINT, MenuTutorial.WAIT_CROUCH);
                while (menuTutorialPhase <= MenuTutorial.WAIT_TO_EXIT_CHECKLIST)
                {
                    CheckIfExitedTutorialCircle();
                    if (player.GetKeyboardCurrentHeight() < 1.0f)
                    {
                        ContinueTutorial(MenuTutorial.WAIT_CROUCH);
                    }
                    yield return null;
                }
            }
            else
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "SitDown"), true, HintType.HINT_NO_IMAGE);
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "HandGames"), true, HintType.HINT);
                float time = Time.time;
                while (Time.time - time < 5.0f)
                {
                    CheckIfExitedTutorialCircle();
                    yield return null;
                }
                menuTutorialPhase = MenuTutorial.WAIT_CROUCH;
            }

            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "MakeGestures"), true, HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_WAVE);
            if (player.controls == Player.ControlType.KEYBOARD)
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "SayHello"), true, HintType.HINT, MenuTutorial.WAIT_TO_WAVE);
            }
            else
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "SayHelloVR"), true, HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_WAVE);
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "UntilIcon"), true, HintType.HINT, MenuTutorial.WAIT_TO_WAVE);
            }
            while (menuTutorialPhase <= MenuTutorial.WAIT_CROUCH)
            {
                CheckIfExitedTutorialCircle();
                yield return null;
            }
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "Good"), true, HintType.HINT);
            while (!m_finishedDisplayHint)
            {
                CheckIfExitedTutorialCircle();
                yield return null;
            }
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "NowGesture"), true, HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_COME_HERE);
            if (player.controls == Player.ControlType.KEYBOARD)
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "Follow"), true, HintType.HINT, MenuTutorial.WAIT_TO_COME_HERE);
            }
            else
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "FollowVR"), true, HintType.HINT, MenuTutorial.WAIT_TO_COME_HERE);
            }
            while (menuTutorialPhase <= MenuTutorial.WAIT_TO_WAVE)
            {
                CheckIfExitedTutorialCircle();
                yield return null;
            }
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "FollowYou"), true, HintType.HINT);
            while (!m_finishedDisplayHint)
            {
                CheckIfExitedTutorialCircle();
                yield return null;
            }

            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "PickupDuck"), true, HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_START_PICKUP);
            duck = GameObject.Instantiate(tutorialDuckPrefab, tutorialCircle.transform.position + Vector3.up * 0.1f, Quaternion.identity);
            if (player.controls == Player.ControlType.KEYBOARD)
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "Pickup"), true, HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_START_PICKUP);
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "Drop"), true, HintType.HINT, MenuTutorial.WAIT_TO_START_PICKUP);
            }
            else
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "PickupDropVR"), true, HintType.HINT, MenuTutorial.WAIT_TO_START_PICKUP);
            }
            while (menuTutorialPhase <= MenuTutorial.WAIT_TO_COME_HERE)
            {
                CheckIfExitedTutorialCircle();
                if (player.rightHandState.heldItem != null)
                {
                    if (player.rightHandState.heldItem.gameObject == duck)
                    {
                        ContinueTutorial(MenuTutorial.WAIT_TO_START_PICKUP);
                    }
                }
                else if (player.leftHandState.heldItem != null)
                {
                    if (player.leftHandState.heldItem.gameObject == duck)
                    {
                        ContinueTutorial(MenuTutorial.WAIT_TO_START_PICKUP);
                    }
                }
                yield return null;
            }
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "ExtendDucky"), true, HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_PRESENT);
            if (player.controls == Player.ControlType.KEYBOARD)
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "Drop"), true, HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_PRESENT);
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "ExtendOffering"), true, HintType.HINT, MenuTutorial.WAIT_TO_PRESENT);
            }
            while (menuTutorialPhase <= MenuTutorial.WAIT_TO_START_PICKUP)
            {
                CheckIfExitedTutorialCircle();
                yield return null;
            }
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "LikeThat"), true, HintType.HINT);
            while (!m_finishedDisplayHint)
            {
                CheckIfExitedTutorialCircle();
                yield return null;
            }

            //polaroid frame tutorial section
            frame = GameObject.Instantiate(tutorialFramePrefab, tutorialCircle.transform.position + Vector3.up * 0.1f, Quaternion.identity).GetComponent(typeof(PolaroidFrame)) as PolaroidFrame;
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "PickupPolaroid"), true, HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_PICKUP_FRAME);
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "DropRest"), true, HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_PICKUP_FRAME);
            if (player.controls == Player.ControlType.KEYBOARD)
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "Drop"), true, HintType.HINT, MenuTutorial.WAIT_TO_PICKUP_FRAME);
            }
            else
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "PickupDropVR"), true, HintType.HINT, MenuTutorial.WAIT_TO_START_PICKUP);
            }

            while (menuTutorialPhase <= MenuTutorial.WAIT_TO_PRESENT)
            {
                CheckIfExitedTutorialCircle();
                int held = System.Convert.ToInt32(player.rightHandState.heldItem != null);
                held += System.Convert.ToInt32(player.leftHandState.heldItem != null);
                if (held == 1)
                {
                    if (player.rightHandState.heldItem == frame ||
                        player.leftHandState.heldItem == frame)
                    {
                        ContinueTutorial(MenuTutorial.WAIT_TO_PICKUP_FRAME);
                    }
                }
                yield return null;
            }

            if (player.controls == Player.ControlType.KEYBOARD)
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "RipFrame"), true, HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_RIP_FRAME);
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "OnceItsHeld"), true, HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_RIP_FRAME);
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "ClickWithBoth"), true, HintType.HINT, MenuTutorial.WAIT_TO_RIP_FRAME);
            }
            else
            {
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "RipFrameVR"), true, HintType.HINT_NO_IMAGE, MenuTutorial.WAIT_TO_RIP_FRAME);
                DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "ClickWithBothVR"), true, HintType.HINT, MenuTutorial.WAIT_TO_RIP_FRAME);
            }
            while (menuTutorialPhase <= MenuTutorial.WAIT_TO_PICKUP_FRAME)
            {
                CheckIfExitedTutorialCircle();
                if (frame == null)
                {
                    ContinueTutorial(MenuTutorial.WAIT_TO_RIP_FRAME);
                }
                yield return null;
            }

            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "TutorialFinished"), true, HintType.HINT);
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "UnderDevelopment"), true, HintType.HINT_NO_IMAGE);          
            DisplayHUDMessage(LocalizationManager.GetLocalizedStringFromTable("Tutorial", "PreviousLocation"), true, HintType.HINT_NO_IMAGE);

            ExitTutorial();
        }

        //TODO: Move everything to do with Hints and Achievement Messages to its own class (GameDirector.Hud)
        public void DisplayHUDMessage(string message, bool playsound, HintType hintType, MenuTutorial waitForPhase = MenuTutorial.NONE)
        {
            GameDirector.instance.StartCoroutine(HandleHUDMessage(message, playsound, hintType, waitForPhase));
        }

        private void OrientHUDToPlayer()
        {

            if (GameDirector.player.controls == Player.ControlType.KEYBOARD)
            {
                HUD_canvas.transform.localScale = Vector3.one * 0.001f;
                HUD_canvas.transform.position = GameDirector.instance.mainCamera.transform.position + GameDirector.instance.mainCamera.transform.forward * 0.8f;
                HUD_canvas.transform.rotation = Quaternion.LookRotation(GameDirector.instance.mainCamera.transform.forward, GameDirector.instance.mainCamera.transform.up);
            }
            else
            {
                HUD_canvas.transform.localScale = Vector3.one * 0.002f;
                Vector3 floorForward = GameDirector.instance.mainCamera.transform.forward;
                floorForward.y = 0.001f;
                floorForward = floorForward.normalized;

                HUD_canvas.transform.position = GameDirector.player.head.position + floorForward * 1.5f - Vector3.up * 0.5f;
                HUD_canvas.transform.rotation = Quaternion.LookRotation(floorForward, Vector3.up);
            }
        }

        private void EnsureVisibleHUD()
        {

            //stick HUD close to player if he moves far
            Player player = GameDirector.player;
            if (player.controls == Player.ControlType.KEYBOARD)
            {
                OrientHUDToPlayer();
            }
            else
            {  //if VR
                if ((HUD_canvas.position - player.head.position).sqrMagnitude > 6.0f)
                {
                    OrientHUDToPlayer();
                }
                else if (Mathf.Abs(Tools.Bearing(player.head, HUD_canvas.position)) > 40.0f)
                {
                    OrientHUDToPlayer();
                }
            }
        }

        public IEnumerator HandleHUDMessage(string message, bool playsound, HintType hintType, MenuTutorial waitForPhase)
        {
            m_finishedDisplayHint = false;

            GameObject achievementPanel = Instantiate(achievementPrefab, Vector3.zero, HUD_canvas.rotation, HUD_canvas);

            Vector3 targetPos = new Vector3(0.0f, achievementPanelsActive++ * -40.0f + 360.0f, 0.0f);
            if (playsound)
            {
                if (hintType == HintType.ACHIEVEMENT)
                    GameDirector.instance.PlayGlobalSound(achievementSound);
                else if (hintType == HintType.HINT)
                    GameDirector.instance.PlayGlobalSound(menuSound);
            }

            Image image = achievementPanel.GetComponent(typeof(Image)) as Image;
            image.sprite = hintTypeFrames[(int)hintType];
            Text text = achievementPanel.transform.GetChild(0).GetComponent(typeof(Text)) as Text;
            text.text = message;

            Image hintImage = achievementPanel.transform.GetChild(1).GetComponent(typeof(Image)) as Image;
            hintImage.sprite = hintTypeImages[(int)hintType];

            Color fade = Color.white;
            Color textFade = Color.black;

            float timer = 0.0f;
            while (timer < 0.4f)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Min(1.0f, timer / 0.4f);
                achievementPanel.transform.localPosition = Vector3.LerpUnclamped(Vector3.zero, targetPos, Tools.EaseOutQuad(alpha));
                achievementPanel.transform.localScale = Vector3.one * Mathf.Lerp(1.0f, 2.0f, 1.0f - Mathf.Abs(0.5f - alpha) * 2.0f);

                fade.a = alpha;
                image.color = fade;
                hintImage.color = fade;
                textFade.a = alpha;
                text.color = textFade;
                EnsureVisibleHUD();
                yield return null;
            }

            if (waitForPhase != MenuTutorial.NONE)
            {
                while (true)
                {

                    if (menuTutorialPhase >= waitForPhase)
                    {
                        break;
                    }
                    else
                    {
                        EnsureVisibleHUD();
                        yield return null;
                    }
                }
            }
            else
            {
                float length = 2.0f + message.Length * 0.06f;
                while (timer < length)
                {
                    timer += Time.deltaTime;
                    EnsureVisibleHUD();
                    yield return null;
                }
            }
            timer = 0.0f;
            achievementPanelsActive--;
            m_finishedDisplayHint = true;
            while (timer < 0.5f)
            {
                timer += Time.deltaTime;
                float alpha = 1.0f - timer / 0.5f;
                fade.a = alpha;
                image.color = fade;
                hintImage.color = fade;
                textFade.a = alpha;
                text.color = textFade;
                yield return null;
            }
            Destroy(achievementPanel);
        }
    }

}