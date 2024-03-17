using PWCommon5;
using UnityEditor;
using UnityEngine;
namespace ProceduralWorlds.SceneOptimizer
{
    public class PWStyles : EditorUtils.CommonStyles
    {
        #region Const
        private const string kStandardSpritePath = "UI/Skin/UISprite.psd";
        private const string kBackgroundSpriteResourcePath = "UI/Skin/Background.psd";
        private const string kInputFieldBackgroundPath = "UI/Skin/InputFieldBackground.psd";
        private const string kKnobPath = "UI/Skin/Knob.psd";
        private const string kCheckmarkPath = "UI/Skin/Checkmark.psd";
        #endregion
        #region Static
        private static Color SPLINE_CURVE_COLOR = new Color(0.8f, 0.8f, 0.8f);
        private static Color CURVE_BUTTON_COLOR = new Color(0.8f, 0.8f, 0.8f);
        private static Color EXTRUSION_CURVE_COLOR = new Color(0.8f, 0.8f, 0.8f);
        private static Color DIRECTION_BUTTON_COLOR = Color.red;
        private static Color UP_BUTTON_COLOR = Color.green;
        #endregion
        #region Variables
        public readonly Texture2D knobTexture2D;
        public readonly GUIStyle gpanel;
        public readonly GUIStyle dropPanel;
        public readonly GUIStyle actionButton;
        public readonly GUIStyle wrappedText;
        public readonly GUIStyle resFlagsPanel;
        public readonly GUIStyle resTreeFoldout;
        public readonly GUIStyle staticResHeader;
        public readonly GUIStyle dynamicResHeader;
        public readonly GUIStyle boldLabel;
        public readonly GUIStyle advancedToggle;
        public readonly GUIStyle advancedToggleDown;
        public readonly GUIStyle helpNoWrap;
        public readonly GUIStyle boxLabelLeft;
        public readonly GUIStyle boxWithLeftLabel;
        public readonly GUIStyle addBtn;
        public readonly GUIStyle inlineToggleBtn;
        public readonly GUIStyle inlineToggleBtnDown;
        public readonly GUIStyle areaDebug;
        public readonly Texture2D undoIco;
        public readonly GUIStyle nodeBtn;
        public readonly GUIStyle cancelBtn;
        public readonly GUIStyle dirBtn;
        public readonly GUIStyle upBtn;
        public readonly GUIStyle redBackground;
        #endregion
        #region Constructors
        public PWStyles()
        {
            #region Area Debug
            areaDebug = new GUIStyle("label");
            areaDebug.normal.background = GetBGTexture(GetColorFromHTML("#ff000055"));
            #endregion
            #region Box
            gpanel = new GUIStyle(GUI.skin.box);
            gpanel.normal.textColor = GUI.skin.label.normal.textColor;
            gpanel.fontStyle = FontStyle.Bold;
            gpanel.alignment = TextAnchor.UpperLeft;
            boxLabelLeft = new GUIStyle(gpanel);
            boxLabelLeft.richText = true;
            boxLabelLeft.wordWrap = false;
            boxLabelLeft.margin.right = 0;
            boxLabelLeft.overflow.right = 1;
            boxWithLeftLabel = new GUIStyle(gpanel);
            boxWithLeftLabel.richText = true;
            boxWithLeftLabel.wordWrap = false;
            boxWithLeftLabel.margin.left = 0;
            #endregion
            #region Drop Panel
            dropPanel = new GUIStyle(GUI.skin.label);
            dropPanel.alignment = TextAnchor.UpperLeft;
            #endregion
            #region Add Button
            addBtn = new GUIStyle("button");
            addBtn.margin = new RectOffset(4, 4, 0, 0);
            #endregion
            #region ActionButton
            actionButton = new GUIStyle(GUI.skin.button);
            actionButton.normal.background = GetBGTexture(new Color(0.21f, 0.32f, 0.09f));
            #endregion
            #region Inline Toggle Button
            inlineToggleBtn = new GUIStyle(toggleButton);
            inlineToggleBtn.margin = deleteButton.margin;
            inlineToggleBtnDown = new GUIStyle(toggleButtonDown);
            inlineToggleBtnDown.margin = inlineToggleBtn.margin;
            #endregion
            #region Resource Tree
            resTreeFoldout = new GUIStyle(EditorStyles.foldout);
            resTreeFoldout.fontStyle = FontStyle.Bold;
            #endregion
            #region Red Flags Panel
            resFlagsPanel = new GUIStyle(GUI.skin.window);
            resFlagsPanel.normal.textColor = GUI.skin.label.normal.textColor;
            //resFlagsPanel.fontStyle = FontStyle.Bold;
            resFlagsPanel.alignment = TextAnchor.UpperCenter;
            resFlagsPanel.margin = new RectOffset(0, 0, 5, 7);
            resFlagsPanel.padding = new RectOffset(10, 10, 3, 3);
            resFlagsPanel.stretchWidth = true;
            resFlagsPanel.stretchHeight = false;
            #endregion
            #region Wrap Style
            wrappedText = new GUIStyle(GUI.skin.label);
            wrappedText.fontStyle = FontStyle.Normal;
            wrappedText.wordWrap = true;
            #endregion
            #region Static / Dynamic Resource Header
            staticResHeader = new GUIStyle();
            staticResHeader.overflow = new RectOffset(2, 2, 2, 2);
            dynamicResHeader = new GUIStyle(staticResHeader);
            #endregion
            #region Bold Label
            boldLabel = new GUIStyle("Label");
            boldLabel.fontStyle = FontStyle.Bold;
            #endregion
            #region Advanced Toggle
            advancedToggle = toggleButton;
            advancedToggle.padding = new RectOffset(5, 5, 0, 0);
            advancedToggle.margin = deleteButton.margin;
            advancedToggle.fixedHeight = deleteButton.fixedHeight;
            advancedToggleDown = toggleButtonDown;
            advancedToggleDown.padding = advancedToggle.padding;
            advancedToggleDown.margin = advancedToggle.margin;
            advancedToggleDown.fixedHeight = advancedToggle.fixedHeight;
            #endregion
            #region Help
            helpNoWrap = new GUIStyle(help);
            helpNoWrap.wordWrap = false;
            #endregion
            #region Undo Icon
            undoIco = Resources.Load("pwundo" + PWConst.VERSION_IN_FILENAMES) as Texture2D;
            #endregion
            #region Background
            redBackground = new GUIStyle();
            redBackground.active.background = GetBGTexture(Color.red);
            #endregion
            #region Spline Buttons
            knobTexture2D = AssetDatabase.GetBuiltinExtraResource<Texture2D>(kKnobPath);
            nodeBtn = new GUIStyle("button");
            nodeBtn.active.background = GetBGTexture(Color.white);
            dirBtn = new GUIStyle("button");
            dirBtn.active.background = knobTexture2D; // GetBGTexture(Color.white); 
            dirBtn.normal.background = knobTexture2D;
            upBtn = new GUIStyle("button");
            upBtn.active.background = GetBGTexture(Color.white);
            #endregion
            #region Cancel Button
            cancelBtn = new GUIStyle("button");
            //cancelBtn.normal.textColor = Color.red;
            #endregion
            #region Unity Personal / Pro Colors
            // Setup colors for Unity Pro
            if (EditorGUIUtility.isProSkin)
            {
                resFlagsPanel.normal.background = Resources.Load("pwdarkBoxp" + PWConst.VERSION_IN_FILENAMES) as Texture2D;
                staticResHeader.normal.background = GetBGTexture(GetColorFromHTML("2d2d2dff"));
                dynamicResHeader.normal.background = GetBGTexture(GetColorFromHTML("2d2d4cff"));
            }
            // or Unity Personal
            else
            {
                resFlagsPanel.normal.background = Resources.Load("pwdarkBox" + PWConst.VERSION_IN_FILENAMES) as Texture2D;
                staticResHeader.normal.background = GetBGTexture(GetColorFromHTML("a2a2a2ff"));
                dynamicResHeader.normal.background = GetBGTexture(GetColorFromHTML("a2a2c1ff"));
            }
            #endregion
        }
        #endregion
    }
}