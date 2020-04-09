using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using SpaceVIL.Common;
using SpaceVIL.Core;
using SpaceVIL.Decorations;

namespace SpaceVIL
{
    internal class TextEditStorage : Prototype, ITextEditable, ITextShortcuts, IDraggable
    {
        static int count = 0;
        private TextLine _text_object;
        private TextLine _substrate_text;

        private Rectangle _cursor;
        private int _cursorPosition = 0;
        private Rectangle _selectedArea;
        private bool _isEditable = true;

        private int _cursorXMax = SpaceVILConstants.SizeMaxValue;

        internal Rectangle GetSelectionArea()
        {
            return _selectedArea;
        }

        private int _selectFrom = -1;
        private int _selectTo = -1;
        private bool _isSelect = false;
        private bool _justSelected = false;

        private List<KeyCode> ShiftValCodes;
        private List<KeyCode> InsteadKeyMods;

        private readonly Object textInputLock = new Object();

        private int scrollStep = 15;

        internal TextEditStorage()
        {
            _text_object = new TextLine();
            _text_object.SetRecountable(true);
            _substrate_text = new TextLine();

            _cursor = new Rectangle();
            _selectedArea = new Rectangle();

            SetItemName("TextEditStorage_" + count);
            count++;

            EventMousePress += OnMousePressed;
            EventMouseDrag += OnDragging;
            EventKeyPress += OnKeyPress;
            EventKeyRelease += OnKeyRelease;
            EventTextInput += OnTextInput;
            EventScrollUp += OnScrollUp;
            EventScrollDown += OnScrollDown;
            EventMouseDoubleClick += OnMouseDoubleClick;

            ShiftValCodes = new List<KeyCode>() { KeyCode.Left, KeyCode.Right, KeyCode.End, KeyCode.Home };
            InsteadKeyMods = new List<KeyCode>() {KeyCode.LeftShift, KeyCode.RightShift, KeyCode.LeftControl,
                KeyCode.RightControl, KeyCode.LeftAlt, KeyCode.RightAlt, KeyCode.LeftSuper, KeyCode.RightSuper};

            undoQueue = new LinkedList<TextEditState>();
            redoQueue = new LinkedList<TextEditState>();
            undoQueue.AddFirst(new TextEditState(GetText(), _cursorPosition));

            SetCursor(EmbeddedCursor.IBeam);
        }

        protected internal override void SetFocused(bool value)
        {
            base.SetFocused(value);
            if (IsFocused() && _isEditable)
                _cursor.SetVisible(true);
            else
                _cursor.SetVisible(false);
        }
        
        private void OnMouseDoubleClick(object sender, MouseArgs args)
        {
            if (args.Button == MouseButton.ButtonLeft)
                SelectAll();
        }

        private void OnMousePressed(object sender, MouseArgs args)
        {
            Monitor.Enter(textInputLock);
            try
            {
                if (args.Button == MouseButton.ButtonLeft)
                {
                    ReplaceCursorAccordingCoord(args.Position.GetX());
                    if (_isSelect)
                    {
                        UnselectText();
                        CancelJustSelected();
                    }
                }
            }
            finally
            {
                Monitor.Exit(textInputLock);
            }
        }

        private void OnDragging(object sender, MouseArgs args)
        {
            Monitor.Enter(textInputLock);
            try
            {
                if (args.Button == MouseButton.ButtonLeft)
                {
                    ReplaceCursorAccordingCoord(args.Position.GetX());

                    if (!_isSelect)
                    {
                        _isSelect = true;
                        _selectFrom = _cursorPosition;
                    }
                    else
                    {
                        _selectTo = _cursorPosition;
                        MakeSelectedArea(); //_selectFrom, _selectTo);
                    }
                }
            }
            finally
            {
                Monitor.Exit(textInputLock);
            }
        }

        private void OnScrollUp(object sender, MouseArgs args)
        {
            int w = GetTextWidth();

            if (w < _cursorXMax) return;
            int sh = GetLineXShift();
            if (sh >= 0) return;

            int curPos = _cursor.GetX();
            int curCoord = curPos - sh;

            sh += scrollStep;
            if (sh > 0) sh = 0;

            _text_object.SetLineXShift(sh);
            _cursor.SetX(curCoord + sh);

            // curPos = _cursor.GetX() - curPos;
            // _selectedArea.SetX(_selectedArea.GetX() + curPos);
            if (_justSelected)
                CancelJustSelected();
            MakeSelectedArea(); //_selectFrom, _selectTo);
        }

        private void OnScrollDown(object sender, MouseArgs args)
        {
            int w = GetTextWidth();

            if (w < _cursorXMax) return;
            int sh = GetLineXShift();
            if (w + sh <= _cursorXMax) return;

            int curPos = _cursor.GetX();
            int curCoord = curPos - sh;

            sh -= scrollStep;
            if (w + sh < _cursorXMax)
                sh = _cursorXMax - w;

            _text_object.SetLineXShift(sh);
            _cursor.SetX(curCoord + sh);

            // curPos = _cursor.GetX() - curPos;
            // _selectedArea.SetX(_selectedArea.GetX() + curPos);
            if (_justSelected)
                CancelJustSelected();
            MakeSelectedArea(); //_selectFrom, _selectTo);
        }

        private void ReplaceCursorAccordingCoord(int realPos)
        {
            int w = GetTextWidth();
            if (_text_object.GetTextAlignment().HasFlag(ItemAlignment.Right) && (w < _cursorXMax))
                realPos -= GetX() + (GetWidth() - w) - GetPadding().Right - _text_object.GetMargin().Right - _cursor.GetWidth();
            else
                realPos -= GetX() + GetPadding().Left + _text_object.GetMargin().Left;

            _cursorPosition = CoordXToPos(realPos);
            ReplaceCursor();
        }

        private int CoordXToPos(int coordX)
        {
            int pos = 0;

            List<int> lineLetPos = _text_object.GetLetPosArray();
            if (lineLetPos == null) return pos;

            for (int i = 0; i < lineLetPos.Count; i++)
            {
                if (lineLetPos[i] + GetLineXShift() <= coordX + 3)
                    pos = i + 1;
                else break;
            }

            return pos;
        }

        private void OnKeyRelease(object sender, KeyArgs args)
        {

        }
        private void OnKeyPress(object sender, KeyArgs args)
        {
            Monitor.Enter(textInputLock);
            try
            {
                TextShortcutProcessor.ProcessShortcut(this, args);

                if (!_isEditable)
                {
                    return;
                }

                if (!_isSelect && _justSelected)
                {
                    CancelJustSelected();
                }

                if (args.Mods != 0)
                {
                    //Выделение не сбрасывается, проверяются сочетания
                    switch (args.Mods)
                    {
                        case KeyMods.Shift:
                            if (ShiftValCodes.Contains(args.Key))
                            {
                                if (!_isSelect)
                                {
                                    _isSelect = true;
                                    _selectFrom = _cursorPosition;
                                }
                            }

                            break;

                            //alt, super ?
                    }
                }
                else
                {
                    if (args.Key == KeyCode.Backspace || args.Key == KeyCode.Delete)
                    {
                        if (_isSelect)
                            PrivCutText();
                        else
                        {
                            if (args.Key == KeyCode.Backspace && _cursorPosition > 0)//backspace
                            {
                                _cursorPosition--;
                                PrivSetText(PrivGetText().Remove(_cursorPosition, 1));
                                //ReplaceCursor();
                            }
                            if (args.Key == KeyCode.Delete && _cursorPosition < PrivGetText().Length)//delete
                            {
                                PrivSetText(PrivGetText().Remove(_cursorPosition, 1));
                                //ReplaceCursor();
                            }
                        }
                    }
                    else
                        if (_isSelect && !InsteadKeyMods.Contains(args.Key))
                            UnselectText();
                }

                if (args.Key == KeyCode.Left && _cursorPosition > 0)//arrow left
                {
                    if (!_justSelected)
                    {
                        _cursorPosition--;
                        ReplaceCursor();
                    }
                }
                if (args.Key == KeyCode.Right && _cursorPosition < PrivGetText().Length)//arrow right
                {
                    if (!_justSelected)
                    {
                        _cursorPosition++;
                        ReplaceCursor();
                    }
                }
                if (args.Key == KeyCode.End)//end
                {
                    _cursorPosition = PrivGetText().Length;
                    ReplaceCursor();
                }
                if (args.Key == KeyCode.Home)//home
                {
                    _cursorPosition = 0;
                    ReplaceCursor();
                }

                if (_isSelect)
                {
                    if (_selectTo != _cursorPosition)
                    {
                        _selectTo = _cursorPosition;
                        MakeSelectedArea();
                    }
                }
            }
            finally
            {
                Monitor.Exit(textInputLock);
            }
        }

        private int CursorPosToCoord(int cPos, bool isx)
        {
            int coord = 0;
            if (_text_object.GetLetPosArray() == null) return coord;
            int letCount = _text_object.GetLetPosArray().Count;
            
            if (cPos > 0)
            {
                coord = _text_object.GetLetPosArray()[cPos - 1];
                if ((GetTextWidth() >= _cursorXMax) || !_text_object.GetTextAlignment().HasFlag(ItemAlignment.Right))
                {
                    coord += _cursor.GetWidth();
                }
            }

            if (isx)
            {
                if (GetLineXShift() + coord < 0)
                {
                    _text_object.SetLineXShift(-coord);
                }
                if (GetLineXShift() + coord > _cursorXMax)
                    _text_object.SetLineXShift(_cursorXMax - coord);
            }

            return GetLineXShift() + coord;
        }

        private void ReplaceCursor()
        {
            int len = PrivGetText().Length;

            if (_cursorPosition > len)
            {
                _cursorPosition = len;
                //ReplaceCursor();
            }
            int pos = CursorPosToCoord(_cursorPosition, true);

            int w = GetTextWidth();

            if (_text_object.GetTextAlignment().HasFlag(ItemAlignment.Right) && (w < _cursorXMax))
            {
                int xcp = GetX() + GetWidth() - w + pos - GetPadding().Right // - _cursor.GetWidth()
                    - _text_object.GetMargin().Right - _cursor.GetWidth();
                if (_cursorPosition == 0)
                    xcp -= _cursor.GetWidth();
                _cursor.SetX(xcp);
            }
            else
            {
                int cnt = GetX() + GetPadding().Left + pos + _text_object.GetMargin().Left;
                _cursor.SetX(cnt);
            }
        }

        private void OnTextInput(object sender, TextInputArgs args)
        {
            if (!_isEditable) return;
            Monitor.Enter(textInputLock);
            try
            {
                byte[] input = BitConverter.GetBytes(args.Character);
                string str = Encoding.UTF32.GetString(input);
                if (_isSelect || _justSelected)
                {
                    UnselectText();
                    PrivCutText();
                }
                if (_justSelected) CancelJustSelected(); //_justSelected = false;

                _cursorPosition++;
                PrivSetText(PrivGetText().Insert(_cursorPosition - 1, str));
                //ReplaceCursor();
                //Console.WriteLine("input in TextEdit " + _cursorPosition);
            }
            finally
            {
                Monitor.Exit(textInputLock);
            }
        }

        private void PrivSetText(String text)
        {
            Monitor.Enter(textInputLock);
            try
            {
                if (_substrate_text.IsVisible())
                    _substrate_text.SetVisible(false);
                if (text == null || text.Equals(String.Empty))
                    _substrate_text.SetVisible(true);

                _text_object.SetItemText(text);
                _text_object.CheckXShift(_cursorXMax);

                // _cursorPosition = PrivGetText().Length;
                ReplaceCursor();

                if (!nothingFlag)
                {
                    redoQueue = new LinkedList<TextEditState>();
                }
                else
                {
                    nothingFlag = false;
                }
                if (undoQueue.Count > queueCapacity)
                    undoQueue.RemoveLast();
                undoQueue.AddFirst(new TextEditState(GetText(), _cursorPosition));
            }
            finally
            {
                Monitor.Exit(textInputLock);
            }
        }

        internal void SetText(String text)
        {
            if (_isSelect || _justSelected)
            {
                UnselectText();
                CancelJustSelected();
            }
            PrivSetText(text);
            _cursorPosition = PrivGetText().Length;
            ReplaceCursor();
        }

        private String PrivGetText()
        {
            return _text_object.GetItemText();
        }

        internal String GetText()
        {
            return PrivGetText();
        }

        internal virtual bool IsEditable
        {
            get { return _isEditable; }
            set
            {
                if (_isEditable == value)
                {
                    return;
                }
                _isEditable = value;

                if (_isEditable)
                {
                    _cursor.SetVisible(true);
                }
                else
                {
                    _cursor.SetVisible(false);
                }
            }
        }

        public override void SetWidth(int width)
        {
            base.SetWidth(width);
            _cursorXMax = GetWidth() - _cursor.GetWidth() - GetPadding().Left - GetPadding().Right
                    - _text_object.GetMargin().Left - _text_object.GetMargin().Right; // _cursorXMin;// ;
            _text_object.SetAllowWidth(_cursorXMax);
            _text_object.CheckXShift(_cursorXMax); //_text_object.SetLineXShift();

            _substrate_text.SetAllowWidth(_cursorXMax);
            _substrate_text.CheckXShift(_cursorXMax);

            ReplaceCursor();
            if (_text_object.GetTextAlignment().HasFlag(ItemAlignment.Right))
            {
                MakeSelectedArea();
            }
        }

        public override void InitElements()
        {
            AddItems(_substrate_text, _selectedArea, _text_object, _cursor);

            // _cursorXMax = GetWidth() - _cursor.GetWidth() - GetPadding().Left - GetPadding().Right
            //         - _text_object.GetMargin().Left - _text_object.GetMargin().Right; // _cursorXMin;// ;
            // _text_object.SetAllowWidth(_cursorXMax);
            // _text_object.SetLineXShift();

            int scctp = _text_object.GetFontDims().lineSpacer; //[0];
            if (scctp > scrollStep) scrollStep = scctp;

            _text_object.SetCursorWidth(_cursor.GetWidth());
            _substrate_text.SetCursorWidth(_cursor.GetWidth());
        }

        internal int GetTextWidth()
        {
            return _text_object.GetWidth();
        }

        internal int GetTextHeight()
        {
            return _text_object.GetHeight();
        }

        private void MakeSelectedArea()
        {
            MakeSelectedArea(_selectFrom, _selectTo);
        }

        private void MakeSelectedArea(int fromPt, int toPt)
        {
            if (fromPt == -1)
                fromPt = 0;
            if (toPt == -1)
                toPt = 0;
            fromPt = CursorPosToCoord(fromPt, false);
            toPt = CursorPosToCoord(toPt, false);

            if (fromPt == toPt)
            {
                _selectedArea.SetWidth(0);
                return;
            }
            int fromReal = Math.Min(fromPt, toPt);
            int toReal = Math.Max(fromPt, toPt);

            if (fromReal < 0)
                fromReal = 0;
            if (toReal > _cursorXMax)
                toReal = _cursorXMax;

            int width = toReal - fromReal + 1;

            int w = GetTextWidth();
            if (_text_object.GetTextAlignment().HasFlag(ItemAlignment.Right) && (w < _cursorXMax))
                _selectedArea.SetX(GetX() + GetWidth() - w + fromReal - GetPadding().Right -
                    _text_object.GetMargin().Right - _cursor.GetWidth());
            else
                _selectedArea.SetX(GetX() + GetPadding().Left + fromReal + _text_object.GetMargin().Left);
            _selectedArea.SetWidth(width);
        }

        private string PrivGetSelectedText()
        {
            Monitor.Enter(textInputLock);
            try
            {
                if (_selectFrom == -1)
                    _selectFrom = 0;
                if (_selectTo == -1)
                    _selectTo = 0;
                if (_selectFrom == _selectTo) return "";
                string text = PrivGetText();
                int fromReal = Math.Min(_selectFrom, _selectTo);
                int toReal = Math.Max(_selectFrom, _selectTo);
                if (fromReal < 0)
                    return "";
                string selectedText = text.Substring(fromReal, toReal - fromReal);
                return selectedText;
            }
            finally
            {
                Monitor.Exit(textInputLock);
            }
        }

        public string GetSelectedText()
        {
            return PrivGetSelectedText();
        }

        private void PrivPasteText(string pasteStr)
        {
            if (!_isEditable)
            {
                return;
            }
            Monitor.Enter(textInputLock);
            try
            {
                if (_isSelect)
                {
                    PrivCutText();
                }

                if ((pasteStr == null) || pasteStr.Equals(""))
                {
                    return;
                }

                string text = PrivGetText();
                string newText = text.Substring(0, _cursorPosition) + pasteStr + text.Substring(_cursorPosition);
                _cursorPosition += pasteStr.Length;
                PrivSetText(newText);
                //ReplaceCursor();
            }
            finally
            {
                Monitor.Exit(textInputLock);
            }
        }

        public virtual void PasteText(string pasteStr)
        {   
            PrivPasteText(pasteStr);
        }

        private string PrivCutText()
        {
            if (!_isEditable)
            {
                return "";
            }
            Monitor.Enter(textInputLock);
            try
            {
                if (_selectFrom == -1)
                {
                    _selectFrom = 0;
                }
                if (_selectTo == -1)
                {
                    _selectTo = 0;
                }

                string str = PrivGetSelectedText();
                if (_selectFrom == _selectTo)
                {
                    return str;
                }
                int fromReal = Math.Min(_selectFrom, _selectTo);
                int toReal = Math.Max(_selectFrom, _selectTo);
                _cursorPosition = fromReal;
                PrivSetText(PrivGetText().Remove(fromReal, toReal - fromReal));
                ReplaceCursor();
                if (_isSelect)
                {
                    UnselectText();
                }
                CancelJustSelected();
                return str;
            }
            finally
            {
                Monitor.Exit(textInputLock);
            }
        }

        public string CutText()
        {
            return PrivCutText();
        }

        private void UnselectText()
        {
            _isSelect = false;
            _justSelected = true;
            MakeSelectedArea(_cursorPosition, _cursorPosition);
        }

        private void CancelJustSelected()
        {
            _selectFrom = -1;
            _selectTo = -1;
            _justSelected = false;
        }

        public override void Clear()
        {
            SetText("");
        }

        private int GetLineXShift()
        {
            return _text_object.GetLineXShift();
        }

        internal bool IsBeginning()
        {
            return (_cursorPosition == 0);
        }

        public void SelectAll()
        {
            Monitor.Enter(textInputLock);
            try
            {
                _selectFrom = 0;
                _cursorPosition = PrivGetText().Length;
                _selectTo = _cursorPosition;
                ReplaceCursor();
                _isSelect = true;
                MakeSelectedArea();
            }
            finally
            {
                Monitor.Exit(textInputLock);
            }
        }

        private int queueCapacity = SpaceVILConstants.TextUndoCapacity;
        private bool nothingFlag = false;
        public void Undo()
        {
            // _text_object.Undo();
            UndoAction();
            // ReplaceCursor();
        }
        private LinkedList<TextEditState> undoQueue;
        private void UndoAction()
        {
            if (undoQueue.Count == 1)
                return;

            TextEditState tmpText = undoQueue.First.Value;
            if (tmpText != null)
            {
                undoQueue.RemoveFirst();
                if (redoQueue.Count > queueCapacity)
                    redoQueue.RemoveLast();
                redoQueue.AddFirst(new TextEditState(tmpText.textState, tmpText.cursorState));

                tmpText = undoQueue.First.Value;
                if (tmpText != null)
                {
                    undoQueue.RemoveFirst();
                    nothingFlag = true;

                    PrivSetText(tmpText.textState);
                    _cursorPosition = tmpText.cursorState;
                    undoQueue.First.Value.cursorState = _cursorPosition;
                    ReplaceCursor();
                }
            }
        }

        public void Redo()
        {
            // _text_object.Redo();
            RedoAction();
            // ReplaceCursor();
        }
        private LinkedList<TextEditState> redoQueue;
        private void RedoAction()
        {
            if (redoQueue.Count == 0)
                return;

            TextEditState tmpText = redoQueue.First.Value;
            if (tmpText != null)
            {
                redoQueue.RemoveFirst();
                nothingFlag = true;

                PrivSetText(tmpText.textState);
                _cursorPosition = tmpText.cursorState;
                undoQueue.First.Value.cursorState = _cursorPosition;
                ReplaceCursor();
            }
        }

        internal void SetSubstrateText(String substrateText)
        {
            _substrate_text.SetItemText(substrateText);
        }

        internal String GetSubstrateText()
        {
            return _substrate_text.GetItemText();
        }

        internal void AppendText(String text)
        {
            UnselectText();
            CancelJustSelected();
            _cursorPosition = PrivGetText().Length;
            PasteText(text);
        }

        internal class TextEditState
        {
            internal String textState;
            internal int cursorState;
            internal TextEditState()
            {
            }
            internal TextEditState(String textState, int cursorState)
            {
                this.textState = textState;
                this.cursorState = cursorState;
            }
        }

        //decorations-------------------------------------------------------------------------------------------------------
        internal void SetTextAlignment(ItemAlignment alignment)
        {
            ItemAlignment ial;
            if (alignment.HasFlag(ItemAlignment.Right))
            {
                ial = ItemAlignment.Right | ItemAlignment.VCenter;
            }
            else
            {
                ial = ItemAlignment.Left | ItemAlignment.VCenter;
            }
            _text_object.SetTextAlignment(ial);
            _substrate_text.SetTextAlignment(ial);
        }

        internal ItemAlignment GetTextAlignment()
        {
            return _text_object.GetTextAlignment();
        }

        internal void SetTextMargin(Indents margin)
        {
            _text_object.SetMargin(margin);
            _substrate_text.SetMargin(margin);
        }
        internal Indents GetTextMargin() {
            return _text_object.GetMargin();
        }

        internal void SetFont(Font font)
        {
            _text_object.SetFont(font);
            _substrate_text.SetFont(GraphicsMathService.ChangeFontFamily(font.FontFamily, _substrate_text.GetFont())); //new Font(font.FontFamily, _substrate_text.GetFont().Size, _substrate_text.GetFont().Style));
        }
        internal void SetFontSize(int size)
        {
            _text_object.SetFontSize(size);
        }
        internal void SetFontStyle(FontStyle style)
        {
            _text_object.SetFontStyle(style);
        }
        internal void SetFontFamily(FontFamily font_family)
        {
            _text_object.SetFontFamily(font_family);
            _substrate_text.SetFontFamily(font_family);
        }
        internal Font GetFont()
        {
            return _text_object.GetFont();
        }

        internal void SetForeground(Color color)
        {
            _text_object.SetForeground(color);
        }
        internal Color GetForeground()
        {
            return _text_object.GetForeground();
        }

        internal void SetSubstrateFontSize(int size)
        {
            _substrate_text.SetFontSize(size);
        }

        internal void SetSubstrateFontStyle(FontStyle style)
        {
            _substrate_text.SetFontStyle(style);
        }

        internal void SetSubstrateForeground(Color foreground)
        {
            _substrate_text.SetForeground(foreground);
        }

        internal Color GetSubstrateForeground()
        {
            return _substrate_text.GetForeground();
        }
        
        //style
        public override void SetStyle(Style style)
        {
            if (style == null)
                return;
            base.SetStyle(style);
            SetForeground(style.Foreground);
            SetFont(style.Font);
            SetTextAlignment(style.TextAlignment);

            Style inner_style = style.GetInnerStyle("selection");
            if (inner_style != null)
            {
                _selectedArea.SetStyle(inner_style);
            }
            inner_style = style.GetInnerStyle("cursor");
            if (inner_style != null)
            {
                _cursor.SetStyle(inner_style);
            }
            inner_style = style.GetInnerStyle("substrate");
            if (inner_style != null)
            {
                _substrate_text.SetFont(inner_style.Font);
                _substrate_text.SetForeground(inner_style.Foreground);
            }
        }
    }
}