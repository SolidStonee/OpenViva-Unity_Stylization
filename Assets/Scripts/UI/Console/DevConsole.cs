using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Viva.console
{
    public abstract class ConsoleCommand
    {
        public abstract string Name { get; protected set; }
        public abstract string Command { get; protected set; }
        public abstract string Description { get; protected set; }
        public abstract string Help { get; protected set; }
        public abstract string Example { get; protected set; }

        public void AddCommandToConsole()
        {
            if (!DevConsole.Commands.ContainsKey(Command))
            {
                DevConsole.AddCommandsToConsole(Command, this);
                string addMessage = " command has been added to the console.";
                DevConsole.AddStaticMessageToConsole(Name + addMessage);
            }
        }

        public abstract void RunCommand(string[] data);
    }

    public class DevConsole : MonoBehaviour
    {
        public static DevConsole Instance { get; set; }
        public static Dictionary<string, ConsoleCommand> Commands { get; set; }

        [SerializeField]
        private Canvas _consoleCanvas;

        [SerializeField]
        private ScrollRect _scrollRect;
        
        public Text _consoleText;
        
        public Text _inputText;
        
        [SerializeField]
        private Text _autocompleteText;
        
        public InputField _consoleInput;

        [Tooltip("Define how many commands can be hold in the clipboard. If set to 0, clipboard will be off.")]
        public int _clipboardSize;

        private string[] _clipboard;

        private int _clipboardIndexer = 0;

        private int _clipboardCursor = 0;

        [SerializeField]
        [Tooltip("Specify minimum amount of characters for autocomplete key(TAB) to work.")]
        private int _tabMinCharLength = 3;

        private List<string> _suggestions;
        private int _suggestionIndex;

        #region Colors

        public static string RequiredColor = "#FA8072";
        public static string OptionalColor = "#00FF7F";
        public static string WarningColor = "#ffcc00";
        public static string ExecutedColor = "#e600e6";

        #endregion

        #region Typical Console Messages

        public static string CommandNotRecognized = $"Command not <color={WarningColor}>recognized</color>";
        public static string ArgumentNotRecognized = $"Argument not <color={WarningColor}>recognized</color>";
        public static string ExecutedSuccessfully = $"Command executed <color={ExecutedColor}>successfully</color>";
        public static string ParametersAmount = $"Wrong <color={WarningColor}>amount of parameters</color>";
        public static string TypeNotSupported = $"Type of command <color={WarningColor}>not supported</color>";
        public static string SceneNotFound = $"Scene <color={WarningColor}>not found</color>. Make sure that you have placed it inside <color={WarningColor}>build settings</color>";
        public static string ClipboardCleared = $"\nConsole clipboard <color={OptionalColor}>cleared</color>";

        #endregion

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Commands = new Dictionary<string, ConsoleCommand>();
            _suggestions = new List<string>();
        }

        private void Start()
        {
            _clipboard = new string[_clipboardSize];
            _consoleCanvas.gameObject.SetActive(false);

            var primary = "#F9F0E6";

            _consoleText.text = "\n\n---------------------------------------------------------------------------------\n" +
                               $"<size=30><color={primary}>OpenViva Developer Console</color></size> \n" +
                               "---------------------------------------------------------------------------------\n\n" +
                               "Type <color=orange>help</color> for list of available commands. \n" +
                               "Type <color=orange>help <command></color> for command details. \n \n \n";

            RegisterAllCommands();
        }

        private void RegisterAllCommands()
        {
            var commandTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsSubclassOf(typeof(ConsoleCommand)) && !t.IsAbstract);

            foreach (var type in commandTypes)
            {
                var command = (ConsoleCommand)Activator.CreateInstance(type);
                command.AddCommandToConsole();
            }
        }

        public static void AddCommandsToConsole(string name, ConsoleCommand command)
        {
            if (!Commands.ContainsKey(name))
            {
                Commands.Add(name, command);
            }
        }

        private void Update()
        {
            // Disable if in VR
            if (GameDirector.player.controls == Player.ControlType.VR)
            {
                _consoleCanvas.gameObject.SetActive(false);
                _consoleInput.DeactivateInputField();
                return;
            }
            if (Keyboard.current[Key.F1].wasPressedThisFrame)
            {
                if (!_consoleCanvas.gameObject.activeInHierarchy)
                {
                    GameDirector.instance.SetEnableControls(GameDirector.ControlsAllowed.NONE);
                    _consoleCanvas.gameObject.SetActive(true);
                    _consoleInput.ActivateInputField();
                    _consoleInput.Select();
                }
                else
                {
                    if (!GameDirector.player.pauseMenu.IsPauseMenuOpen)
                    {
                        GameDirector.instance.SetEnableControls(GameDirector.ControlsAllowed.ALL);
                    } 
                    _consoleCanvas.gameObject.SetActive(false);
                }
            }

            if (_consoleCanvas.gameObject.activeInHierarchy)
            {
                if (Keyboard.current[Key.Enter].wasPressedThisFrame)
                {
                    if (string.IsNullOrEmpty(_inputText.text) == false)
                    {
                        AddMessageToConsole(_inputText.text);
                        ParseInput(_inputText.text);

                        if (_clipboardSize != 0)
                        {
                            StoreCommandInTheClipboard(_inputText.text);
                        }
                    }
                    _consoleInput.text = "";
                    _consoleInput.ActivateInputField();
                    _consoleInput.Select();
                }

                HandleClipboardNavigation();
                HandleAutocomplete();
            }

            if (_consoleCanvas.gameObject.activeInHierarchy == false)
            {
                _consoleInput.text = "";
            }
        }

        private void HandleClipboardNavigation()
        {
            if (Keyboard.current[Key.UpArrow].wasPressedThisFrame)
            {
                if (_clipboardSize != 0 && _clipboardIndexer != 0)
                {
                    if (_clipboardCursor == _clipboardIndexer)
                    {
                        _clipboardCursor--;
                        _consoleInput.text = _clipboard[_clipboardCursor];
                    }
                    else if (_clipboardCursor > 0)
                    {
                        _clipboardCursor--;
                        _consoleInput.text = _clipboard[_clipboardCursor];
                    }
                    else
                    {
                        _consoleInput.text = _clipboard[0];
                    }
                    _consoleInput.caretPosition = _consoleInput.text.Length;
                }
            }

            if (Keyboard.current[Key.DownArrow].wasPressedThisFrame)
            {
                if (_clipboardSize != 0 && _clipboardIndexer != 0)
                {
                    if (_clipboardCursor < _clipboardIndexer)
                    {
                        _clipboardCursor++;
                        _consoleInput.text = _clipboard[_clipboardCursor];
                        _consoleInput.caretPosition = _consoleInput.text.Length;
                    }
                }
            }
        }

        private void HandleAutocomplete()
        {
            string currentText = _consoleInput.text;
            _autocompleteText.text = "";

            if (string.IsNullOrEmpty(currentText))
                return;

            if (Keyboard.current[Key.Tab].wasPressedThisFrame)
            {
                if (_suggestions.Count == 0 || !_suggestions.Contains(currentText))
                {
                    _suggestions = Commands.Keys
                        .Where(cmd => cmd.StartsWith(currentText, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    _suggestionIndex = 0;
                }

                if (_suggestions.Count > 0)
                {
                    _consoleInput.text = _suggestions[_suggestionIndex];
                    _consoleInput.caretPosition = _consoleInput.text.Length;
                    _suggestionIndex = (_suggestionIndex + 1) % _suggestions.Count;
                }
            }

            var matchingCommand = Commands.Keys
                .FirstOrDefault(cmd => cmd.StartsWith(currentText, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(matchingCommand) && !matchingCommand.Equals(currentText, StringComparison.OrdinalIgnoreCase))
            {
                _autocompleteText.text = currentText + matchingCommand.Substring(currentText.Length);
            }
        }

        private IEnumerator ScrollDown()
        {
            yield return new WaitForSeconds(0.1f);
            _scrollRect.verticalNormalizedPosition = 0f;
        }

        private void StoreCommandInTheClipboard(string command)
        {
            _clipboard[_clipboardIndexer] = command;

            if (_clipboardIndexer < _clipboardSize - 1)
            {
                _clipboardIndexer++;
                _clipboardCursor = _clipboardIndexer;
            }
            else if (_clipboardIndexer == _clipboardSize - 1)
            {
                // Clear clipboard & reset 
                _clipboardIndexer = 0;
                _clipboardCursor = 0;
                Array.Clear(_clipboard, 0, _clipboardSize);

                AddStaticMessageToConsole(ClipboardCleared);
            }
        }

        private void AddMessageToConsole(string msg)
        {
            _consoleText.text += msg + "\n";
        }

        //You can add Debug information to the console with this
        public static void AddStaticMessageToConsole(string msg)
        {
            Instance._consoleText.text += msg + "\n";
        }

        private void ParseInput(string input)
        {
            string[] commandSplitInput = input.Split(null);

            if (string.IsNullOrWhiteSpace(input))
            {
                AddMessageToConsole(CommandNotRecognized);
                return;
            }

            if (Commands.ContainsKey(commandSplitInput[0]) == false)
            {
                AddMessageToConsole(CommandNotRecognized);
            }
            else
            {
                Commands[commandSplitInput[0]].RunCommand(commandSplitInput);
            }
        }
    }
}