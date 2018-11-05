package com.spvessel.Items;

import com.spvessel.Common.DefaultsService;
import com.spvessel.Cores.*;
import com.spvessel.Decorations.Indents;
import com.spvessel.Decorations.Style;
import com.spvessel.Flags.ItemAlignment;
import com.spvessel.Flags.KeyCode;
import com.spvessel.Flags.KeyMods;

import java.awt.*;
import java.nio.ByteBuffer;
import java.nio.charset.Charset;
import java.util.Arrays;
import java.util.LinkedList;
import java.util.List;
import java.util.concurrent.locks.Lock;
import java.util.concurrent.locks.ReentrantLock;

public class TextEdit extends VisualItem
        implements InterfaceTextEditable, InterfaceTextShortcuts, InterfaceDraggable, InterfaceScrollable {
    static int count = 0;
    private TextLine _text_object;
    private Rectangle _cursor;
    private int _cursor_position = 0;
    private Rectangle _selectedArea;
    private boolean _isEditable = true;

    // private int _cursorXMin = 0;
    private int _cursorXMax = Integer.MAX_VALUE;
    // private int _lineXShift = 0;

    public Rectangle getSelectionArea() {
        return _selectedArea;
    }

    private int _selectFrom = -1;
    private int _selectTo = -1;
    private boolean _isSelect = false;
    private boolean _justSelected = false;
    /*
     * private final KeyCode BackspaceCode = KeyCode.BACKSPACE;// 14; private final
     * KeyCode DeleteCode = KeyCode.DELETE;// 339; private final KeyCode
     * LeftArrowCode = KeyCode.LEFT;// 331; private final KeyCode RightArrowCode =
     * KeyCode.RIGHT;// 333; private final KeyCode EndCode = KeyCode.END;// 335;
     * private final KeyCode HomeCode = KeyCode.HOME;// 327; private final KeyCode
     * ACode = KeyCode.A;// 30;
     */
    private List<KeyCode> ShiftValCodes;
    private List<KeyCode> InsteadKeyMods;

    private Lock textInputLock = new ReentrantLock();

    public TextEdit() {
        _text_object = new TextLine();
        _cursor = new Rectangle();
        _selectedArea = new Rectangle();

        setItemName("TextEdit_" + count);
        count++;

        eventMousePressed.add(this::onMousePressed);
        eventMouseDrag.add(this::onDragging);
        eventKeyPress.add(this::onKeyPress);
        eventKeyRelease.add(this::onKeyRelease);
        eventTextInput.add(this::onTextInput);
        eventScrollUp.add(this::onScrollUp);
        eventScrollDown.add(this::onScrollDown);

        ShiftValCodes = new LinkedList<>(Arrays.asList(KeyCode.LEFT, KeyCode.RIGHT, KeyCode.END, KeyCode.HOME));
        InsteadKeyMods = new LinkedList<>(Arrays.asList(KeyCode.LEFTSHIFT, KeyCode.RIGHTSHIFT, KeyCode.LEFTCONTROL,
                KeyCode.RIGHTCONTROL, KeyCode.LEFTALT, KeyCode.RIGHTALT, KeyCode.LEFTSUPER, KeyCode.RIGHTSUPER));

        // setStyle(DefaultsService.getDefaultStyle("SpaceVIL.TextEdit"));
        setStyle(DefaultsService.getDefaultStyle(com.spvessel.Items.TextEdit.class));
    }

    protected void onMousePressed(Object sender, MouseArgs args) {
        replaceCursorAccordingCoord(args.position.getX());
        if (_isSelect)
            unselectText();
    }

    protected void onDragging(Object sender, MouseArgs args) {
        replaceCursorAccordingCoord(args.position.getX());

        if (!_isSelect) {
            _isSelect = true;
            _selectFrom = _cursor_position;
        } else {
            _selectTo = _cursor_position;
            makeSelectedArea(_selectFrom, _selectTo);
        }
    }

    protected void onScrollUp(Object sender, MouseArgs args) {
        int w = getTextWidth();

        if (w < _cursorXMax)
            return;
        int sh = getLineXShift();
        if (sh >= 0)
            return;
        
        int curPos = _cursor.getX();
        int curCoord = curPos - sh;

        sh += _text_object.getFontDims()[0];
        if (sh > 0)
            sh = 0;

        _text_object.setLineXShift(sh);
        _cursor.setX(curCoord + sh);

        curPos = _cursor.getX() - curPos;
        _selectedArea.setX(_selectedArea.getX() + curPos);
    }

    protected void onScrollDown(Object sender, MouseArgs args) {
        int w = getTextWidth();

        if (w < _cursorXMax)
            return;
        int sh = getLineXShift();
        if (w + sh <= _cursorXMax)
            return;

        int curPos = _cursor.getX();
        int curCoord = curPos - sh;

        sh -= _text_object.getFontDims()[0];
        if (w + sh < _cursorXMax)
            sh = _cursorXMax - w;

        _text_object.setLineXShift(sh);
        _cursor.setX(curCoord + sh);

        curPos = _cursor.getX() - curPos;
        _selectedArea.setX(_selectedArea.getX() + curPos);
    }

    public void invokeScrollUp(MouseArgs args) {
        eventScrollUp.execute(this, args);
    }

    public void invokeScrollDown(MouseArgs args) {
        eventScrollDown.execute(this, args);
    }

    private void replaceCursorAccordingCoord(int realPos) {
        int w = getTextWidth();
        if (_text_object.getTextAlignment().contains(ItemAlignment.RIGHT) && (w < _cursorXMax))
            realPos -= getX() + (getWidth() - w) - getPadding().right - _text_object.getMargin().right;
        else
            realPos -= getX() + getPadding().left + _text_object.getMargin().left;

        _cursor_position = coordXToPos(realPos);
        replaceCursor();
    }

    private int coordXToPos(int coordX) {
        int pos = 0;

        List<Integer> lineLetPos = _text_object.getLetPosArray();
        if (lineLetPos == null)
            return pos;

        for (int i = 0; i < lineLetPos.size(); i++) {
            if (lineLetPos.get(i) + getLineXShift() <= coordX + 3)
                pos = i + 1;
            else
                break;
        }

        return pos;
    }

    protected void onKeyRelease(InterfaceItem sender, KeyArgs args) {

    }

    protected void onKeyPress(InterfaceItem sender, KeyArgs args) {
        textInputLock.lock();
        try {
            // Console.WriteLine(args.key);
            if (!_isEditable) {
                if (args.mods.equals(KeyMods.CONTROL) && (args.key == KeyCode.A || args.key == KeyCode.a)) {
                    _selectFrom = 0;
                    _cursor_position = getText().length();
                    _selectTo = _cursor_position;
                    replaceCursor();
                    _isSelect = true;
                    makeSelectedArea(_selectFrom, _selectTo);
                }
                return;
            }

            if (!_isSelect && _justSelected) {
                _selectFrom = -1;// 0;
                _selectTo = -1;// 0;
                _justSelected = false;
            }
            if (args.mods != KeyMods.NO) {
                // Выделение не сбрасывается, проверяются сочетания
                // 
                switch (args.mods) {
                case SHIFT:
                    if (ShiftValCodes.contains(args.key)) {
                        if (!_isSelect) {
                            _isSelect = true;
                            _selectFrom = _cursor_position;
                        }
                    }

                    break;

                case CONTROL:
                    if (args.key == KeyCode.A || args.key == KeyCode.a) {
                        _selectFrom = 0;
                        _cursor_position = getText().length();
                        replaceCursor();

                        _isSelect = true;
                    }
                    break;

                // alt, super ?
                }
            } else {
                if (args.key == KeyCode.BACKSPACE || args.key == KeyCode.DELETE) {
                    if (_isSelect)
                        cutText();
                    else {
                        if (args.key == KeyCode.BACKSPACE && _cursor_position > 0) // backspace
                        {
                            StringBuilder sb = new StringBuilder(getText());
                            setText(sb.deleteCharAt(_cursor_position - 1).toString());
                            _cursor_position--;
                            replaceCursor();
                        }
                        if (args.key == KeyCode.DELETE && _cursor_position < getText().length()) // delete
                        {
                            StringBuilder sb = new StringBuilder(getText());
                            setText(sb.deleteCharAt(_cursor_position).toString());
                            replaceCursor();
                        }
                    }
                } else if (_isSelect && !InsteadKeyMods.contains(args.key))
                    unselectText();
            }

            if (args.key == KeyCode.LEFT && _cursor_position > 0) // arrow left
            {
                if (!_justSelected) {
                    _cursor_position--;
                    replaceCursor();
                }
            }
            if (args.key == KeyCode.RIGHT && _cursor_position < getText().length()) // arrow right
            {
                if (!_justSelected) {
                    _cursor_position++;
                    replaceCursor();
                }
            }
            if (args.key == KeyCode.END) // end
            {
                _cursor_position = getText().length();
                replaceCursor();
            }
            if (args.key == KeyCode.HOME) // home
            {
                _cursor_position = 0;
                replaceCursor();
            }

            if (_isSelect) {
                if (_selectTo != _cursor_position) {
                    _selectTo = _cursor_position;
                    makeSelectedArea(_selectFrom, _selectTo);
                }
            }
        } finally {
            textInputLock.unlock();
        }
    }

    private int cursorPosToCoord(int cPos) {
        int coord = 0;
        if (_text_object.getLetPosArray() == null)
            return coord;
        //int letCount = _text_object.getLetPosArray().size();
        // _cursor_position = (_cursor_position < 0) ? 0 : _cursor_position;
        // _cursor_position = (_cursor_position > letCount) ? letCount :
        // _cursor_position;

        if (cPos > 0)
            coord = _text_object.getLetPosArray().get(cPos - 1);

        if (getLineXShift() + coord < 0) {
            _text_object.setLineXShift(-coord);
        }
        if (getLineXShift() + coord > _cursorXMax)
            _text_object.setLineXShift(_cursorXMax - coord);

        return getLineXShift() + coord;
    }

    private void replaceCursor() {

        int pos = cursorPosToCoord(_cursor_position);
        // Console.WriteLine(pos);

        int w = getTextWidth();
        if (_text_object.getTextAlignment().contains(ItemAlignment.RIGHT) && (w < _cursorXMax)) {
            _cursor.setX(getX() + getWidth() - w + pos - _cursor.getWidth() - getPadding().right
                    - _text_object.getMargin().right);
        } else
            _cursor.setX(getX() + getPadding().left + pos + _text_object.getMargin().left);
    }

    protected void onTextInput(Object sender, TextInputArgs args) {
        if (!_isEditable)
            return;
        textInputLock.lock();
        try {
            byte[] input = ByteBuffer.allocate(4).putInt(args.character).array();
            String str = new String(input, Charset.forName("UTF-32")); // StandardCharsets.UTF_16);

            if (_isSelect)
                unselectText();// cutText();
            if (_justSelected)
                cutText();

            StringBuilder sb = new StringBuilder(getText());
            setText(sb.insert(_cursor_position, str).toString());
            _cursor_position++;
            replaceCursor();
            // System.out.println("input in TextEdit " + _cursor_position + " " +
            // _cursor.getX());
        } finally {
            textInputLock.unlock();
        }
    }

    @Override
    public void setFocused(boolean value) {
        super.setFocused(value);
        if (isFocused() && _isEditable)
            _cursor.setVisible(true);
        else
            _cursor.setVisible(false);
    }

    public void setTextAlignment(ItemAlignment... alignment) {
        setTextAlignment(Arrays.asList(alignment));
    }

    public void setTextAlignment(List<ItemAlignment> alignment) {
        List<ItemAlignment> ial = new LinkedList<>();
        if (alignment.contains(ItemAlignment.RIGHT)) {
            ial.add(ItemAlignment.RIGHT);
            ial.add(ItemAlignment.VCENTER);
        } else {
            ial.add(ItemAlignment.LEFT);
            ial.add(ItemAlignment.VCENTER);
        }
        _text_object.setTextAlignment(ial);
    }

    public void setTextMargin(Indents margin) {
        _text_object.setMargin(margin);
    }

    public void setFont(Font font) {
        _text_object.setFont(font);
    }

    public void setFontSize(int size) {
        _text_object.setFontSize(size);
    }

    public void setFontStyle(int style) {
        _text_object.setFontStyle(style);
    }

    public void setFontFamily(String font_family) {
        _text_object.setFontFamily(font_family);
    }

    public Font getFont() {
        return _text_object.getFont();
    }

    public void setText(String text) {
        textInputLock.lock();
        try {
            // _text_object.setLineXShift(_lineXShift, getWidth());
            _text_object.setItemText(text);
            _text_object.checkXShift(_cursorXMax);
            // _text_object.UpdateData(UpdateType.Critical); //Doing in the _text_object
        } finally {
            textInputLock.unlock();
        }
    }

    public String getText() {
        return _text_object.getItemText();
    }

    public void setForeground(Color color) {
        _text_object.setForeground(color);
    }

    public void setForeground(int r, int g, int b) {
        _text_object.setForeground(r, g, b);
    }

    public void setForeground(int r, int g, int b, int a) {
        _text_object.setForeground(r, g, b, a);
    }

    public void setForeground(float r, float g, float b) {
        _text_object.setForeground(r, g, b);
    }

    public void setForeground(float r, float g, float b, float a) {
        _text_object.setForeground(r, g, b, a);
    }

    public Color getForeground() {
        return _text_object.getForeground();
    }

    public boolean isEditable() {
        return _isEditable;
    }

    public void setEditable(boolean value) {
        if (_isEditable == value)
            return;
        _isEditable = value;

        if (_isEditable)
            _cursor.setVisible(true);
        else
            _cursor.setVisible(false);
    }

    @Override
    public void setWidth(int width) {
        super.setWidth(width);
        _cursorXMax = getWidth() - _cursor.getWidth() - getPadding().left - getPadding().right
                - _text_object.getMargin().left - _text_object.getMargin().right;
        _text_object.setAllowWidth(_cursorXMax);
        _text_object.checkXShift(_cursorXMax);
        replaceCursor();
    }

    @Override
    public void initElements() {
        addItems(_selectedArea, _text_object, _cursor);

        _cursorXMax = getWidth() - _cursor.getWidth() - getPadding().left - getPadding().right
                - _text_object.getMargin().left - _text_object.getMargin().right;
        _text_object.setAllowWidth(_cursorXMax);
        _text_object.setLineXShift();
    }

    public int getTextWidth() {
        return _text_object.getWidth();
    }

    public int getTextHeight() {
        return _text_object.getHeight();
    }

    private void makeSelectedArea(int fromPt, int toPt) {
        // Console.WriteLine("from " + fromPt + " to " + toPt);
        fromPt = cursorPosToCoord(fromPt);
        toPt = cursorPosToCoord(toPt);

        if (fromPt == toPt) {
            _selectedArea.setWidth(0);
            return;
        }
        int fromReal = Math.min(fromPt, toPt);
        int toReal = Math.max(fromPt, toPt);
        int width = toReal - fromReal + 1;

        int w = getTextWidth();
        if (_text_object.getTextAlignment().contains(ItemAlignment.RIGHT) && (w < _cursorXMax))
            _selectedArea
                    .setX(getX() + getWidth() - w + fromReal - getPadding().right - _text_object.getMargin().right);
        else
            _selectedArea.setX(getX() + getPadding().left + fromReal + _text_object.getMargin().left);
        _selectedArea.setWidth(width);
    }

    public String getSelectedText() {
        if (_selectFrom == _selectTo)
            return "";
        String text = getText();
        int fromReal = Math.min(_selectFrom, _selectTo);
        int toReal = Math.max(_selectFrom, _selectTo);
        if (fromReal < 0)
            return "";
        String selectedText = text.substring(fromReal, toReal); // - fromReal
        return selectedText;
    }

    public void pasteText(String pasteStr) {
        if (!_isEditable)
            return;
        textInputLock.lock();
        try {
            if (_isSelect)
                cutText();
            String text = getText();
            String newText = text.substring(0, _cursor_position) + pasteStr + text.substring(_cursor_position);
            setText(newText);
            _cursor_position += pasteStr.length();
            replaceCursor();
        } finally {
            textInputLock.unlock();
        }
    }

    public String cutText() {
        if (!_isEditable)
            return "";
        textInputLock.lock();
        try {
            String str = getSelectedText();
            if (_selectFrom == _selectTo)
                return str;
            int fromReal = Math.min(_selectFrom, _selectTo);
            int toReal = Math.max(_selectFrom, _selectTo);
            StringBuilder sb = new StringBuilder(getText());
            setText(sb.delete(fromReal, toReal).toString()); // - fromReal
            _cursor_position = fromReal;
            replaceCursor();
            if (_isSelect)
                unselectText();
            _justSelected = false;
            return str;
        } finally {
            textInputLock.unlock();
        }
    }

    private void unselectText() {
        _isSelect = false;
        _justSelected = true;
        makeSelectedArea(_cursor_position, _cursor_position);
    }

    /*
     * internal void ShowCursor(bool isShow) { if (isShow) _cursor.setWidth(2); else
     * _cursor.setWidth(0); }
     */
    private int nearestPosToCursor(double xPos) {
        List<Integer> endPos = _text_object.getLetPosArray();
        int pos = (int) endPos.stream().map(x -> Math.abs(x - xPos)).sorted().toArray()[0];
        return pos;
    }

    void setCursorPosition(double newPos) {
        _cursor_position = nearestPosToCursor(newPos);
    }

    public void clear() {
        setText("");
    }

    // style
    @Override
    public void setStyle(Style style) {
        if (style == null)
            return;
        super.setStyle(style);
        setForeground(style.foreground);
        setFont(style.font);
        setTextAlignment(style.textAlignment);

        Style inner_style = style.getInnerStyle("selection");
        if (inner_style != null) {
            _selectedArea.setStyle(inner_style);
        }
        inner_style = style.getInnerStyle("cursor");
        if (inner_style != null) {
            _cursor.setStyle(inner_style);
        }
    }

    private int getLineXShift() {
        return _text_object.getLineXShift();
    }

}