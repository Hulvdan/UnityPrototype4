namespace BeastConsole.GUI {
using System.Collections.Generic;
#pragma warning disable 0649
using System.Linq;
using System.Text;
using Backend;
using Backend.Internal;
using UnityEngine;

internal class ConsoleGui {
    public static event System.Action<bool> OnStateChanged = delegate { };

    internal Options _options;

    [System.Serializable]
    internal class Options {
        public KeyCode ConsoleKey = KeyCode.BackQuote;

        // public int MaxConsoleLines = 120;
        // public float LinePadding = 10;
        public bool LogHandler = true;
        public GUISkin skin;

        public Colors colors = new();

        [System.Serializable]
        public class Colors {
            public Color error = new(1, 1, 1, 1);
            public Color warning = new(1, 1, 1, 1);
            public Color log = new(1, 1, 1, 1);
            public Color command = new(1, 1, 1, 1);
            public Color suggestionGreyed = new(1, 1, 1, 1);
        }
    }

    GUISkin skin;

    ConsoleBackend m_backend;
    bool m_consoleShown;
    bool drawConsole;
    bool consoleWasOpened;
    bool InputToggleConsole = false;
    bool GUIToggleConsole = false;
    bool moveToEnd = false;

    int consoleSize;

    float ConsoleHeight {
        get {
            switch (consoleSize) {
                case 1:
                    return Screen.height / 3F;
                case 2:
                    return Screen.height / 2F;
                case 3:
                    return Screen.height / 2F + Screen.height / 4F;
            }

            return Screen.height / 3F;
        }
    }

    Rect _rect_Console;
    float _console_TargetPosition;
    float _console_CurrentPosition;
    Vector2 consoleScrollPosition;
    float ClosedPosition => -(ConsoleHeight + 20F);

    Rect inputFieldRect => new(5, _rect_Console.y + _rect_Console.height + 5, 400,
        skin.textField.CalcHeight(new("Input"), 50));

    int currentSuggestionIndex = -1;
    string currentSuggestion;
    int currentCommandHistoryIndex = -1;

    string console_input;

    const string INPUT_FIELD_NAME = "ifield";
    GUIStyle suggestionStyle;
    GUIStyle suggestionActiveStyle;
    Texture2D img_box;
    StringBuilder sb = new();

    string greycolorstr;

    struct MsgData {
        public string msg;
        public int count;

        public MsgData(string str) {
            msg = str;
            count = 0;
        }
    }

    List<MsgData> msgHistory = new();

    internal ConsoleGui(ConsoleBackend backend, Options options) {
        skin = options.skin;

        greycolorstr = ConsoleUtility.ToHex(options.colors.suggestionGreyed);
        suggestionStyle = skin.customStyles.Where(x => x.name == "suggestion").FirstOrDefault();
        suggestionActiveStyle =
            skin.customStyles.Where(x => x.name == "suggestionActive").FirstOrDefault();
        img_box = skin.customStyles.Where(x => x.name == "img_box").FirstOrDefault().normal
            .background;
        m_backend = backend;
        _options = options;

        m_backend.OnWriteLine += OnWriteLine;
        SetSize(PlayerPrefs.GetInt("beastconsole.size"));
        _console_CurrentPosition = ClosedPosition;
        _console_TargetPosition = ClosedPosition;

        m_backend.RegisterVariable<int>(SetSize, this, "console.size",
            "Set the size of the console, 1/2/3");
        m_backend.RegisterCommand("clr", "clear the console log", this, Clear);
    }

    void SetSize(int size) {
        consoleSize = Mathf.Clamp(size, 1, 3);
        PlayerPrefs.SetInt("beastconsole.size", size);
    }

    internal void Update() {
        _rect_Console = new(0, 0, Screen.width, ConsoleHeight);
        consoleWasOpened = false;

        InputToggleConsole = Input.GetKeyDown(_options.ConsoleKey);

        if (InputToggleConsole || GUIToggleConsole) {
            GUIToggleConsole = false;
            InputToggleConsole = false;

            // Do Open
            if (!m_consoleShown) {
                _console_TargetPosition = 0F;

                if (OnStateChanged != null) {
                    OnStateChanged(true);
                }

                m_consoleShown = true;
                console_input = "";
                consoleWasOpened = true;

                ScrollToBottom();
            }
            else {
                _console_TargetPosition = ClosedPosition;

                m_consoleShown = false;
                if (OnStateChanged != null) {
                    OnStateChanged(false);
                }
            }
        }

        drawConsole = m_consoleShown;
        _console_CurrentPosition = _console_TargetPosition;
        _rect_Console = new(
            0,
            _console_CurrentPosition,
            _rect_Console.width,
            _rect_Console.height
        );
    }

    internal void OnGUI() {
        if (!drawConsole) {
            return;
        }

        GUI.skin = skin;
        DrawConsole();
        if (m_consoleShown) {
            ControlInputField();
        }
    }

    void DrawConsole() {
        GUI.Box(_rect_Console, "Beast console");
        if (m_consoleShown) {
            DrawHistory();
        }
    }

    void ControlInputField() {
        var e = Event.current;

        if (!consoleWasOpened && e.type == EventType.KeyDown && e.keyCode == _options.ConsoleKey) {
            GUI.FocusControl(null);
            GUIToggleConsole = true;
            e.Use();
            return;
        }

        if (e.type == EventType.KeyDown) {
            if (GUI.GetNameOfFocusedControl() == INPUT_FIELD_NAME) {
                if (e.keyCode == KeyCode.Return) {
                    e.Use();
                    if (currentSuggestionIndex == -1) {
                        if (!string.IsNullOrEmpty(console_input)) {
                            try {
                                HandleInput(console_input);
                            }
                            finally {
                                console_input = "";
                                ScrollToBottom();
                            }
                        }
                    }
                    else {
                        if (!string.IsNullOrEmpty(currentSuggestion)) {
                            try {
                                HandleInput(currentSuggestion);
                            }
                            finally {
                                currentSuggestion = null;
                                currentSuggestionIndex = -1;
                                console_input = "";
                                ScrollToBottom();
                            }
                        }
                    }
                }
                else if (e.keyCode == KeyCode.Tab) {
                    e.Use();
                    if (currentSuggestionIndex == -1) {
                        AutoComplete(console_input);
                        moveToEnd = true;
                    }
                    else {
                        console_input = currentSuggestion;
                        moveToEnd = true;
                    }
                }
                else if (e.keyCode == KeyCode.DownArrow) {
                    e.Use();
                    if (currentCommandHistoryIndex == -1) {
                        currentSuggestionIndex++;
                        currentCommandHistoryIndex = -1;
                    }
                    else if (currentCommandHistoryIndex > -1) {
                        currentCommandHistoryIndex--;
                        SetCmdHistoryItem();
                        moveToEnd = true;
                    }
                }
                else if (e.keyCode == KeyCode.UpArrow) {
                    e.Use();
                    if (currentSuggestionIndex != -1) {
                        currentSuggestionIndex--;
                        currentCommandHistoryIndex = -1;
                    }
                    else {
                        currentCommandHistoryIndex++;
                        SetCmdHistoryItem();
                        moveToEnd = true;
                    }
                }
                else if (e.isKey) {
                    currentSuggestionIndex = -1;
                    currentCommandHistoryIndex = -1;
                }
            }
        }

        DrawInputField();
        DrawAutoCompleteSuggestions(console_input);

        if (consoleWasOpened) {
            GUI.FocusControl(INPUT_FIELD_NAME);
        }
    }

    void SetCmdHistoryItem() {
        var cmdhis = m_backend.m_commandHistory;
        var count = cmdhis.Count;
        currentCommandHistoryIndex = Mathf.Clamp(currentCommandHistoryIndex, -1, count - 1);
        if (count == 0 || currentCommandHistoryIndex < 0) {
            return;
        }

        console_input = cmdhis[cmdhis.Count - currentCommandHistoryIndex - 1];
    }

    void DrawInputField() {
        GUI.SetNextControlName(INPUT_FIELD_NAME);
        var inputField = inputFieldRect;
        var size = skin.textField.CalcSize(new(console_input));

        if (inputField.width < size.x) {
            inputField.width = size.x + 10F;
        }

        console_input = GUI.TextField(inputField, console_input);
        if (moveToEnd) {
            var txt = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor),
                GUIUtility.keyboardControl);
            txt.text = console_input;
            txt.MoveLineEnd();
            moveToEnd = false;
        }
    }

    void OnWriteLine(string str) {
        var count = msgHistory.Count;
        if (count != 0) {
            var lastMsgData = msgHistory[msgHistory.Count - 1];
            if (lastMsgData.msg == str) {
                lastMsgData.count++;
                msgHistory[msgHistory.Count - 1] = lastMsgData;
            }
            else {
                msgHistory.Add(new(str));
            }
        }
        else {
            msgHistory.Add(new(str));
        }

        ScrollToBottom();
    }

    void DrawAutoCompleteSuggestions(string str) {
        if (string.IsNullOrEmpty(str)) {
            return;
        }

        var results = m_backend.m_commandsTrie.GetByPrefix(str);
        var count = results.Count();

        if (currentSuggestionIndex > count - 1) {
            currentSuggestionIndex = 0;
        }

        var inputrect = InputFieldBottom();
        var num = 0;
        foreach (var item in results) {
            var length = ConsoleUtility.WrapInColor(greycolorstr, console_input, out var result);
            sb.Clear();
            sb.Append(result);
            sb.Append(item.Value);
            sb.Remove(length, console_input.Length);
            var value = sb.ToString();

            var size = suggestionStyle.CalcSize(new(value));
            var pos = new Rect(inputrect.x, inputrect.y + size.y * num, size.x, size.y);

            string description = null;
            if (m_backend.m_masterDictionary.TryGetValue(item.Value, out var cmd)) {
                description = cmd.m_description;
            }

            if (currentSuggestionIndex == num) {
                currentSuggestion = item.Value;
                GUI.Label(pos, item.Value, suggestionActiveStyle);
            }
            else {
                GUI.Label(pos, value, suggestionStyle);
            }

            if (!string.IsNullOrEmpty(description)) {
                var descriptionSize = suggestionStyle.CalcSize(new(description));

                if (currentSuggestionIndex == num) {
                    GUI.Label(
                        new(pos.x + pos.width + 5F, pos.y, descriptionSize.x, descriptionSize.y),
                        description, suggestionStyle);
                }
                else {
                    GUI.color = new(1, 1, 1, 0.5F);
                    GUI.Label(
                        new(pos.x + pos.width + 5F, pos.y, descriptionSize.x, descriptionSize.y),
                        description, suggestionStyle);
                    GUI.color = new(1, 1, 1, 1);
                }
            }

            num++;
        }
    }

    void DrawHistory() {
        var list = msgHistory;
        var count = list.Count;

        var totalHeight = GetHistoryContentHeight();

        var historyRect = _rect_Console;

        var viewRect = new Rect(historyRect.x, historyRect.y, historyRect.width - 10, totalHeight);

        consoleScrollPosition = GUI.BeginScrollView(historyRect, consoleScrollPosition, viewRect);
        {
            var currentYPos = historyRect.height > viewRect.height
                ? historyRect.height
                : viewRect.height;

            for (var i = count - 1; i >= 0; i--) {
                var msgData = list[i];
                var msg = msgData.count > 0 ? msgData.msg + "   x" + msgData.count : msgData.msg;
                var height = CalcHeightForLine(msg);
                var rect = new Rect(0, currentYPos -= height, viewRect.width, height);
                // if (i % 2 == 0)
                // {
                //     GUI.DrawTexture(new Rect(0F, rect.y, Screen.width, rect.height), img_box);
                // }

                GUI.Label(rect, msg);
            }
        }
        GUI.EndScrollView();
    }

    void ScrollToBottom() {
        consoleScrollPosition = new(0, GetHistoryContentHeight());
    }

    float CalcHeightForLine(string line) {
        return skin.label.CalcHeight(new(line), Screen.width);
    }

    Rect InputFieldBottom() {
        var rect = inputFieldRect;
        var size = skin.textField.CalcSize(new(console_input));

        return new(rect.x, rect.y + size.y + 5F, 0, 0);
    }

    Rect CaretPosition() {
        var rect = inputFieldRect;
        var size = skin.textField.CalcSize(new(console_input));

        return new(rect.x + size.x, rect.y + size.y + 5F, 0, 0);
    }

    float GetHistoryContentHeight() {
        var totalHeight = 0F;
        var list = m_backend.m_outputHistory;
        var count = list.Count;
        for (var i = 0; i < count; i++) {
            var cmd = list[i];
            totalHeight += CalcHeightForLine(cmd);
        }

        return totalHeight;
    }

    void HandleInput(string text) {
        m_backend.ExecuteLine(text);
    }

    public static Rect RectWithPadding(Rect rect, int padding) {
        return new(rect.x + padding, rect.y + padding, rect.width - padding - padding,
            rect.height - padding - padding);
    }

    float Remap(float value, float from1, float to1, float from2, float to2) {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    void AutoComplete(string input) {
        var lookup = m_backend.CComParameterSplit(input);
        if (lookup.Length == 0) {
            // don't auto complete if we have typed any parameters so far or nothing at all...
            return;
        }

        var nearestMatch = m_backend.m_masterDictionary.AutoCompleteLookup(lookup[0]);
        // only complete to the next dot if there is one present in the completion string which
        // we don't already have in the lookup string
        var dotIndex = 0;
        do {
            dotIndex = nearestMatch.m_name.IndexOf(".", dotIndex + 1);
        } while (dotIndex > 0 && dotIndex < lookup[0].Length);

        var insertion = nearestMatch.m_name;
        if (dotIndex >= 0) {
            insertion = nearestMatch.m_name.Substring(0, dotIndex + 1);
        }

        if (insertion.Length < input.Length) {
            //do
            //{
            //    if (AutoCompleteTailString("true", input))
            //        break;
            //    if (AutoCompleteTailString("false", input))
            //        break;
            //    if (AutoCompleteTailString("True", input))
            //        break;
            //    if (AutoCompleteTailString("False", input))
            //        break;
            //    if (AutoCompleteTailString("TRUE", input))
            //        break;
            //    if (AutoCompleteTailString("FALSE", input))
            //        break;
            //}
            //while (false);
        }
        else if (insertion.Length >= input.Length) // SE - is this really correct?
        {
            console_input = insertion;
        }

        if (insertion[insertion.Length - 1] != '.') {
            console_input = insertion;
        }
    }

    /// <summary>
    /// Clears out the console log
    /// </summary>
    /// <example>
    /// <code>
    /// SmartConsole.Clear();
    /// </code>
    /// </example>
    internal void Clear(string[] parameters) {
        //we dont want to clear our history, instead we clear the screen
        msgHistory.Clear();
    }
}
}
