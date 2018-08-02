﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceVIL
{
    class TextBlock : VisualItem, ITextEditable
    {
        private static int count = 0;
        private string _wholeText = "";   
        private Rectangle _cursor;
        private Point _cursor_position = new Point(0, 0);
        private CustomSelector _selectedArea;
        private List<TextLine> _linesList;

        private Point _selectFrom = new Point(0, 0);
        private Point _selectTo = new Point(0, 0);
        private bool _isSelect = false;

        private int _minLineSpacer;
        private int _minFontY;
        private int _maxFontY;
        private int _lineHeight;

        private Font _elementFont;

        private const int BackspaceCode = 14;
        private const int DeleteCode = 339;
        private const int LeftArrowCode = 331;
        private const int RightArrowCode = 333;
        private const int UpArrowCode = 328;
        private const int DownArrowCode = 336;
        private const int EndCode = 335;
        private const int HomeCode = 327;
        private const int ACode = 30;
        private const int EnterCode = 28;

        private List<int> ShiftValCodes;

        public TextBlock()
        {
            SetItemName("TextBlock_" + count);
            SetBackground(180, 180, 180);
            SetForeground(Color.Black);
            SetPadding(5, 0, 5, 0);
            count++;

            _linesList = new List<TextLine>();
            TextLine te = new TextLine();
            _linesList.Add(te);

            _cursor = new Rectangle();
            _selectedArea = new CustomSelector();
            _selectedArea.SetBackground(Color.FromArgb(50, 0, 0, 0));

            EventMouseClick += EmptyEvent;
            EventKeyPress += OnKeyPress;
            EventKeyRelease += OnKeyRelease;
            EventTextInput += OnTextInput;

            ShiftValCodes = new List<int>() {LeftArrowCode, RightArrowCode, EndCode,
                HomeCode, UpArrowCode, DownArrowCode};

            int[] output = FontEngine.GetSpacerDims(te.GetFont());
            _minLineSpacer = 3;// output[0];
            _minFontY = output[1];
            _maxFontY = output[2];
            _lineHeight = Math.Abs(_maxFontY - _minFontY);
            if (_lineSpacer < _minLineSpacer)
                _lineSpacer = _minLineSpacer;

            _cursor.SetHeight(_lineHeight + _lineSpacer);
        }

        protected virtual void OnKeyRelease(object sender, int scancode, KeyMods mods)
        {
        }
        protected virtual void OnKeyPress(object sender, int scancode, KeyMods mods)
        {
            //Console.WriteLine(scancode);
            if (mods != 0)
            {
                //Выделение не сбрасывается, проверяются сочетания
                switch (mods)
                {
                    case KeyMods.Shift:
                        if (ShiftValCodes.Contains(scancode))
                        {
                            if (!_isSelect)
                            {
                                _isSelect = true;
                                _selectFrom = _cursor_position;
                            }
                        }

                        break;

                    case KeyMods.Control:
                        if (scancode == ACode)
                        {
                            _selectFrom.X = 0;
                            _selectFrom.Y = 0;
                            _cursor_position.Y = _linesList.Count();
                            _cursor_position.X = GetLineLetCount(_cursor_position.Y);
                            ReplaceCursor();

                            _isSelect = true;
                        }
                        break;

                        //alt, super ?
                }
            }
            else
            {
                if (scancode == BackspaceCode || scancode == DeleteCode)
                {
                    /*
                    if (_isSelect)
                        CutText();
                    else
                    {
                        if (scancode == BackspaceCode && _cursor_position > 0)//backspace
                        {
                            SetText(GetText().Remove(_cursor_position - 1, 1));
                            _cursor_position--;
                            ReplaceCursor();
                        }
                        if (scancode == DeleteCode && _cursor_position < GetText().Length)//delete
                        {
                            SetText(GetText().Remove(_cursor_position, 1));
                        }
                    }
                    */
                }
                else
                    if (_isSelect)
                        UnselectText();
            }

            if (scancode == LeftArrowCode)//arrow left
            {
                if (_cursor_position.X > 0)
                    _cursor_position.X--;
                else if (_cursor_position.Y > 0)
                {
                    _cursor_position.Y--;
                    _cursor_position.X = GetLineLetCount(_cursor_position.Y);
                }
                ReplaceCursor();
            }
            if (scancode == RightArrowCode)//arrow right
            {
                if (_cursor_position.X < GetLineLetCount(_cursor_position.Y))
                    _cursor_position.X++;
                else if (_cursor_position.Y < _linesList.Count - 1)
                {
                    _cursor_position.Y++;
                    _cursor_position.X = 0;
                }

                ReplaceCursor();
            }
            if (scancode == UpArrowCode)//arrow up
            {
                if (_cursor_position.Y > 0)
                    _cursor_position.Y--;
                //?????

                ReplaceCursor();
            }
            if (scancode == DownArrowCode)//arrow down
            {
                if (_cursor_position.Y < _linesList.Count - 1)
                    _cursor_position.Y++;
                //?????

                ReplaceCursor();
            }

            if (scancode == EndCode)//end
            {
                _cursor_position.X = GetLineLetCount(_cursor_position.Y);
                ReplaceCursor();
            }
            if (scancode == HomeCode)//home
            {
                _cursor_position.X = 0;
                ReplaceCursor();
            }

            if (scancode == EnterCode)//enter
            {
                _cursor_position.Y++;
                _cursor_position.X = 0;
                AddNewLine("", _cursor_position.Y);
                
                ReplaceCursor();
            }
            
            if (_isSelect)
            {
                if (_selectTo != _cursor_position)
                { 
                    _selectTo = _cursor_position;
                    MakeSelectedArea(_selectFrom, _selectTo);
                }
            }
        }

        public virtual void OnTextInput(object sender, uint codepoint, KeyMods mods)
        {
            byte[] input = BitConverter.GetBytes(codepoint);
            string str = Encoding.UTF32.GetString(input);
            if (_isSelect) CutText();
            SetTextInLine(_linesList[_cursor_position.Y].GetItemText().Insert(_cursor_position.X, str));
            _cursor_position.X++;
            ReplaceCursor();
        }

        private Point CursorPosToCoord(Point cPos)
        {
            //Console.WriteLine(cPos.X + " " + cPos.Y);
            //Console.WriteLine(_linesList[cPos.Y].GetLetPosArray().Count);

            Point coord = new Point(0, 0);
            int letCount = GetLineLetCount(cPos.Y);
            coord.Y = (int)_linesList[cPos.Y].GetLineYShift();
            if (letCount == 0)
            {
                coord.X = 0;
                return coord;
            }
            if (cPos.X == 0)
            {
                coord.X = 0;
                return coord;
            }
            else
            {
                if (!(cPos.X == 0 && cPos.Y == 0))
                {
                    if (cPos.X > letCount)
                    {
                        coord.X = _linesList[cPos.Y].GetLetPosArray()[letCount - 1];
                    }
                    else
                    {
                        coord.X = _linesList[cPos.Y].GetLetPosArray()[cPos.X - 1];
                    }
                }
            }
            return coord;
        }

        private void ReplaceCursor()
        {
            //Point pos = CursorPosToCoord(_cursor_position);
            //pos = AddXYShifts(pos.X, pos.Y);
            Point pos = AddXYShifts(0, 0, _cursor_position);
            _cursor.SetX(pos.X);
            _cursor.SetY(pos.Y);
        }

        private int _lineSpacer;
        void SetLineSpacer(int lineSpacer)
        {
            if (lineSpacer < _minLineSpacer)
                lineSpacer = _minLineSpacer;

            if (lineSpacer != _lineSpacer)
            {
                _lineSpacer = lineSpacer;

                if (_linesList == null) return;
                foreach (TextLine te in _linesList)
                {
                    te.SetLineYShift(_lineHeight + _lineSpacer);
                }
            }

            _cursor.SetHeight(_lineHeight + _lineSpacer);
        }

        public int GetLineSpacer()
        {
            return _lineSpacer;
        }

        internal string GetWholeText() {
            StringBuilder sb = new StringBuilder();
            if (_linesList == null) return "";
            if (_linesList.Count == 1) {
                sb.Append(_linesList[0].GetText());
            }
            else
            { 
                foreach (TextLine te in _linesList)
                {
                    sb.Append(te.GetText());
                    sb.Append("\n");
                }
                sb.Remove(sb.Length - 3, 2);
            }
            _wholeText = sb.ToString();
            return _wholeText;
        }

        public void SetTextAlignment(ItemAlignment alignment)
        {
            if (_linesList == null) return;
            foreach (TextLine te in _linesList)
                te.SetTextAlignment(alignment);
        }
        public void SetFont(Font font)
        {
            if (!font.Equals(_elementFont))
            { 
                _elementFont = font;

                int[] output = FontEngine.GetSpacerDims(font);
                _minLineSpacer = 3; // output[0];
                _minFontY = output[1];
                _maxFontY = output[2];
                _lineHeight = Math.Abs(_maxFontY - _minFontY);
                if (_lineSpacer < _minLineSpacer)
                    _lineSpacer = _minLineSpacer;

                if (_linesList == null) return;
                foreach (TextLine te in _linesList)
                    te.SetFont(font);

                _cursor.SetHeight(_lineHeight + _lineSpacer);
            }
        }
        public Font GetFont()
        {
            return _elementFont;
        }
        public void SetText(String text)
        {
            if (!text.Equals(GetWholeText()))
            {
                SplitAndMakeLines(text);
            }
        }

        private void SetTextInLine(String text)
        {
            _linesList[_cursor_position.Y].SetItemText(text);
            _linesList[_cursor_position.Y].UpdateData(UpdateType.Critical);
        }

        public int GetTextWidth()
        {
            int w = 0, w0;
            if (_linesList == null) return w;
            foreach (TextLine te in _linesList)
            {
                w0 = te.GetWidth();
                w = (w < w0) ? w0 : w;
            }
            return w;
        }

        public int GetTextHeight()
        {
            return _lineHeight*_linesList.Count + _lineSpacer*(_linesList.Count - 1);
        }

        private void SplitAndMakeLines(String text) {
            _linesList = new List<TextLine>();
            
            _wholeText = text;
            //Console.WriteLine(text + " " + _elementFont.Name);
            string[] line = text.Split('\n');
            int inc = 0;
            
            foreach (String textPart in line)
            {
                AddNewLine(textPart, inc);
                inc++;
            }

            _cursor_position.Y = line.Length;
            _cursor_position.X = GetLineLetCount(_cursor_position.Y);
            ReplaceCursor();
        }
        /*
        internal void UpdateData(UpdateType updateType) {
            //foreach (TextEdit te in _linesList)
                //te.Up
        }
        */
        public void SetForeground(Color color)
        {
            if (_linesList != null && !color.Equals(GetForeground())) { 
                foreach (TextLine te in _linesList)
                    te.SetForeground(color);
            }
        }
        public Color GetForeground()
        {
            if (_linesList == null) return Color.White; //?????
            return _linesList[0].GetForeground();
        }

        public override void InitElements()
        {
            //text
            SetLineContainerAlignment(ItemAlignment.Left | ItemAlignment.VCenter);
            //cursor
            _cursor.IsVisible = false;
            _cursor.SetBackground(0, 0, 0);
            //_cursor.SetMargin(0, 5, 0, 5);
            _cursor.SetWidth(2);
            
            _cursor.SetHeight(_lineHeight + _lineSpacer);
            _cursor.SetSizePolicy(SizePolicy.Fixed, SizePolicy.Fixed);
            //selectedArea
            //_selectedArea.SetMargin(0, 5, 0, 5);
            _selectedArea.SetSizePolicy(SizePolicy.Fixed, SizePolicy.Fixed);
            //adding
            AddItems(_selectedArea, _cursor);
            AddAllLines();

            //update text data
            UpdateLinesData(UpdateType.Critical);
        }

        private void SetLineContainerAlignment(ItemAlignment alignment) {
            foreach (TextLine tl in _linesList)
                tl.SetAlignment(alignment);
        }

        private void UpdateLinesData(UpdateType updateType) {
            foreach (TextLine tl in _linesList)
                tl.UpdateData(updateType);
        }

        private void AddAllLines() {
            RemoveItem(_cursor);
            foreach (TextLine tl in _linesList)
                AddItem(tl);
            AddItem(_cursor);
        }

        private void RemoveAllLines() {
            foreach (TextLine tl in _linesList)
                RemoveItem(tl);
        }

        public override bool IsFocused
        {
            get
            {
                return base.IsFocused;
            }
            set
            {
                base.IsFocused = value;
                if (IsFocused)
                    _cursor.IsVisible = true;
                else
                    _cursor.IsVisible = false;
            }
        }

        private int GetLineLetCount(int lineNum) {
            if (lineNum >= _linesList.Count)
                return 0;
            else
            {
                return _linesList[lineNum].GetItemText().Length;
            }
        }

        private void MakeSelectedArea(Point from, Point to)
        {
            //Console.WriteLine("from " + from + " to " + to);
            if (from.X == to.X && from.Y == to.Y)
            {
                _selectedArea.SetRectangles(null);
                return;
            }

            List<Point> selectionRectangles = new List<Point>();
            ///!!!Добавить высоту строк
            Point fromReal, toReal;
            Point tmp = new Point();
            if (from.Y == to.Y)
            {
                if (from.X < to.X)
                {
                    fromReal = from;
                    toReal = to;
                }
                else
                {
                    fromReal = to;
                    toReal = from;
                }
                selectionRectangles.Add(AddXYShifts(0, -_cursor.GetHeight(), fromReal));
                selectionRectangles.Add(AddXYShifts(0, 0, toReal));
                _selectedArea.SetRectangles(selectionRectangles);
                return;
            }
            else
            {
                if (from.Y < to.Y)
                {
                    fromReal = from;
                    toReal = to;
                }
                else
                {
                    fromReal = to;
                    toReal = from;
                }
            }

            selectionRectangles.Add(AddXYShifts(0, -_cursor.GetHeight(), fromReal));
            tmp.X = GetLineLetCount(fromReal.Y);
            tmp.Y = fromReal.Y;
            selectionRectangles.Add(AddXYShifts(0, 0, tmp));
            tmp.X = 0;
            tmp.Y = toReal.Y;
            selectionRectangles.Add(AddXYShifts(0, -_cursor.GetHeight(), tmp));
            selectionRectangles.Add(AddXYShifts(0, 0, toReal));
            
            for (int i = fromReal.Y + 1; i < toReal.Y; i++)
            {
                tmp.X = 0;
                tmp.Y = i;
                selectionRectangles.Add(AddXYShifts(0, -_cursor.GetHeight(), tmp));
                tmp.X = GetLineLetCount(i);
                tmp.Y = i;
                selectionRectangles.Add(AddXYShifts(0, 0, tmp));
            }
            //Console.WriteLine(selectionRectangles.Count);
            _selectedArea.SetRectangles(selectionRectangles);
        }

        private Point AddXYShifts(int xShift, int yShift, Point point)
        {
            Point outPoint = CursorPosToCoord(point);
            outPoint.X += GetX() + GetPadding().Left + xShift;
            outPoint.Y += GetY() + GetPadding().Top + yShift;

            return outPoint;
        }

        public string CutText()
        {
            /*
            string str = GetSelectedText();
            if (_selectFrom == _selectTo) return str;
            int fromReal = Math.Min(_selectFrom, _selectTo);
            int toReal = Math.Max(_selectFrom, _selectTo);
            SetText(GetText().Remove(fromReal, toReal - fromReal));
            _cursor_position = fromReal;
            ReplaceCursor();
            UnselectText();
            return str;
            */
            return "";
        }

        private void UnselectText()
        {
            _isSelect = false;
            _selectFrom.X = 0;
            _selectFrom.Y = 0;
            _selectTo.X = 0;
            _selectTo.Y = 0;
            MakeSelectedArea(_selectFrom, _selectTo);
        }

        private void AddNewLine(String text, int lineNum)
        {
            RemoveItem(_cursor);
            TextLine te = new TextLine();
            if (_elementFont != null)
                te.SetFont(_elementFont);
            AddItem(te);
            te.SetItemText(text);
            te.SetLineYShift((_lineHeight + _lineSpacer) * lineNum);
            _linesList.Add(te);
            AddItem(_cursor);
        }
    }

}