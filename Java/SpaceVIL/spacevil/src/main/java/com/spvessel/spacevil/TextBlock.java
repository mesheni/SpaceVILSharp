package com.spvessel.spacevil;

import com.spvessel.spacevil.Common.CommonService;
import com.spvessel.spacevil.Core.*;
import com.spvessel.spacevil.Decorations.Indents;
import com.spvessel.spacevil.Decorations.Style;
import com.spvessel.spacevil.Flags.*;

import java.awt.*;
import java.nio.ByteBuffer;
import java.nio.charset.Charset;
import java.util.*;
import java.util.List;

class TextBlock extends Prototype implements InterfaceTextEditable, InterfaceDraggable, InterfaceTextShortcuts,
        InterfaceFreeLayout, InterfaceTextWrap {

    EventCommonMethod cursorChanged = new EventCommonMethod();
    EventCommonMethod textChanged = new EventCommonMethod();

    @Override
    public void release() {
        textChanged.clear();
    }

    private static int count = 0;
    private Rectangle _cursor;
    private Point _cursorPosition = new Point(0, 0);
    private CustomSelector _selectedArea;

    private TextureStorage _textureStorage;

    private boolean _isEditable = true;

    private Point _selectFrom = new Point(-1, 0);
    private Point _selectTo = new Point(-1, 0);
    private boolean _isSelect = false;
    private boolean _justSelected = false;

    private Set<KeyCode> _cursorControlKeys;
    //    private Set<KeyCode> _insteadKeyMods;
    private Set<KeyCode> _serviceEditKeys;

    private int scrollXStep = 30;

    TextBlock() {
        setItemName("TextBlock_" + count);
        count++;

        _textureStorage = new TextureStorage();

        _cursor = new Rectangle();
        _selectedArea = new CustomSelector();

        eventMousePress.add(this::onMousePressed);
        eventMouseClick.add(this::onMouseClick);
        //        eventMouseDoubleClick.add(this::onDoubleClick);
        eventMouseDrag.add(this::onDragging);
        eventKeyPress.add(this::onKeyPress);
        eventKeyRelease.add(this::onKeyRelease);
        eventTextInput.add(this::onTextInput);
        eventScrollUp.add(this::onScrollUp);
        eventScrollDown.add(this::onScrollDown);

        _cursorControlKeys = new HashSet<>(
                Arrays.asList(KeyCode.LEFT, KeyCode.RIGHT, KeyCode.END, KeyCode.HOME, KeyCode.UP, KeyCode.DOWN));
        //        _insteadKeyMods = new HashSet<>(Arrays.asList(KeyCode.LEFTSHIFT, KeyCode.RIGHTSHIFT, KeyCode.LEFTCONTROL,
        //                KeyCode.RIGHTCONTROL, KeyCode.LEFTALT, KeyCode.RIGHTALT, KeyCode.LEFTSUPER, KeyCode.RIGHTSUPER));
        _serviceEditKeys = new HashSet<>(
                Arrays.asList(KeyCode.BACKSPACE, KeyCode.DELETE, KeyCode.ENTER, KeyCode.NUMPADENTER, KeyCode.TAB));

        _cursor.setHeight(_textureStorage.getCursorHeight());

        undoQueue = new ArrayDeque<>();
        redoQueue = new ArrayDeque<>();
        undoQueue.addFirst(new TextBlockState(getText(), new Point(_cursorPosition), getScrollYOffset()));

        setCursor(EmbeddedCursor.IBEAM);

        _isWrapText = false;
    }
    
    private void onMousePressed(Object sender, MouseArgs args) {
        _textureStorage.textInputLock.lock();
        try {
            if (args.button == MouseButton.BUTTON_LEFT) {
                replaceCursorAccordingCoord(new Point(args.position.getX(), args.position.getY()));
                if (_isSelect) {
                    unselectText();
                    cancelJustSelected();
                }
            }
        } finally {
            _textureStorage.textInputLock.unlock();
        }
    }
    
    private long _startTime = 0;
    private boolean _isDoubleClick = false;
    private Point _previousClickPos = new Point();
    
    private void onMouseClick(Object sender, MouseArgs args) {
        _textureStorage.textInputLock.lock();
        try {
            if (args.button == MouseButton.BUTTON_LEFT) {
                Point savePos = new Point(_cursorPosition);
                if (isPosSame()) {
                    if ((System.nanoTime() - _startTime) / 1000000 < 500) {
                        if (_isDoubleClick) { //} && (System.nanoTime() - _startTime) / 1000000 < 500) {
                            _isSelect = true;
                            _selectFrom = new Point(0, _cursorPosition.y);
                            _selectTo = new Point(getLettersCountInLine(_cursorPosition.y), _cursorPosition.y);
                            _cursorPosition = new Point(_selectTo);
                            replaceCursor();
                            makeSelectedArea();

                            _isDoubleClick = false;
                        } else { //if double click
                            int[] wordBounds = _textureStorage.findWordBounds(_cursorPosition);

                            if (wordBounds[0] != wordBounds[1]) {
                                _isSelect = true;
                                _selectFrom = new Point(wordBounds[0], _cursorPosition.y);
                                _selectTo = new Point(wordBounds[1], _cursorPosition.y);
                                _cursorPosition = new Point(_selectTo);
                                replaceCursor();
                                makeSelectedArea();
                            }

                            _isDoubleClick = true;
                        }

                    } else {
                        _isDoubleClick = false;
                    }
                } else {
                    _isDoubleClick = false;
                }

                _previousClickPos = savePos;
                _startTime = System.nanoTime();
            } else {
                _isDoubleClick = false;
            }
        } finally {
            _textureStorage.textInputLock.unlock();
        }
    }

    private boolean isPosSame() {
        Point pos1 = new Point(_cursorPosition);
        Point pos2 = new Point(_previousClickPos);
        int tol = 5;
        if (pos1.y != pos2.y) {
            return false;
        }
        return (pos1.x - tol <= pos2.x && pos2.x <= pos1.x + tol);
    }

    private void onDragging(Object sender, MouseArgs args) {
        _textureStorage.textInputLock.lock();
        try {
            if (args.button == MouseButton.BUTTON_LEFT) {
                replaceCursorAccordingCoord(new Point(args.position.getX(), args.position.getY()));
                if (!_isSelect) {
                    _isSelect = true;
                    _selectFrom = new Point(_cursorPosition);
                } else {
                    _selectTo = new Point(_cursorPosition);
                    makeSelectedArea();
                }
            }
            _isDoubleClick = false;
        } finally {
            _textureStorage.textInputLock.unlock();
        }
    }

    int getScrollYStep() {
        return _textureStorage.getScrollStep();
    }

    int getScrollXStep() {
        return scrollXStep;
    }

    int getScrollYOffset() {
        return _textureStorage.getScrollYOffset();
    }

    void setScrollYOffset(int offset) {
        int oldOff = _textureStorage.getScrollYOffset();
        _textureStorage.setScrollYOffset(offset);
        int diff = offset - oldOff;
        if (_justSelected) {
            cancelJustSelected();
        }
        makeSelectedArea();
        _cursor.setY(_cursor.getY() + diff);
    }

    int getScrollXOffset() {
        return _textureStorage.getScrollXOffset();
    }

    void setScrollXOffset(int offset) {
        int oldOff = _textureStorage.getScrollXOffset();
        _textureStorage.setScrollXOffset(offset);
        int diff = offset - oldOff;
        if (_justSelected) {
            cancelJustSelected();
        }
        makeSelectedArea();
        _cursor.setX(_cursor.getX() + diff);
    }

    private void onScrollUp(Object sender, MouseArgs args) {
        _cursor.setY(_textureStorage.scrollBlockUp(_cursor.getY()));

        if (_justSelected) {
            cancelJustSelected();
        }
        makeSelectedArea();
    }

    private void onScrollDown(Object sender, MouseArgs args) {
        _cursor.setY(_textureStorage.scrollBlockDown(_cursor.getY()));

        if (_justSelected) {
            cancelJustSelected();
        }
        makeSelectedArea();
    }

    private void replaceCursorAccordingCoord(Point realPos) {
        _cursorPosition = _textureStorage.replaceCursorAccordingCoord(realPos);
        replaceCursor();
    }

    private void onKeyRelease(Object sender, KeyArgs args) {
    }

    private void onKeyPress(Object sender, KeyArgs args) {
        _textureStorage.textInputLock.lock();
        try {
            TextShortcutProcessor.processShortcut(this, args);

            if (!_isEditable) {
                return;
            }

            if (!_isSelect && _justSelected) {
                cancelJustSelected();
            }

            boolean isCursorControlKey = _cursorControlKeys.contains(args.key);
            boolean hasShift = args.mods.contains(KeyMods.SHIFT);
            boolean hasControl = args.mods.contains(CommonService.getOsControlMod());

            if (!args.mods.contains(KeyMods.NO)) {
                // Выделение не сбрасывается, проверяются сочетания
                if (isCursorControlKey) {
                    if (!_isSelect) {
                        if (hasShift) {
                            if ((args.mods.size() == 1) || ((args.mods.size() == 2) && hasControl)) {
                                _isSelect = true;
                                _selectFrom = new Point(_cursorPosition);
                            }

                        }
                    } else { //_isSelect
                        if ((args.mods.size() == 1) && hasControl) {
                            unselectText();
                            cancelJustSelected();
                        }
                    }
                }

                // control + delete/backspace
                if (hasControl && (args.mods.size() == 1)) {
                    if (!_isSelect) {
                        if (args.key == KeyCode.BACKSPACE) { //remove to left
                            int[] wordBounds = _textureStorage.findWordBounds(_cursorPosition);

                            if (wordBounds[0] != wordBounds[1] && _cursorPosition.x != wordBounds[0]) {
                                _selectFrom = new Point(_cursorPosition);
                                _cursorPosition = new Point(wordBounds[0], _cursorPosition.y);
//                                replaceCursor();
                                _selectTo = new Point(_cursorPosition);
                                cutText();
                            } else {
                                onBackSpaceInput(args);
                            }
                        } else if (args.key == KeyCode.DELETE) { //remove to right
                            int[] wordBounds = _textureStorage.findWordBounds(_cursorPosition);

                            if (wordBounds[0] != wordBounds[1] && _cursorPosition.x != wordBounds[1]) {
                                _selectFrom = new Point(_cursorPosition);
                                _cursorPosition = new Point(wordBounds[1], _cursorPosition.y);
//                                replaceCursor();
                                _selectTo = new Point(_cursorPosition);
                                cutText();
                            } else {
                                onDeleteInput(args);
                            }
                        }
                    } else if (_isSelect && ((args.key == KeyCode.BACKSPACE) || (args.key == KeyCode.DELETE))) {
                        cutText();
                    }
                }

                // alt, super ?
            } else {
                if (_serviceEditKeys.contains(args.key)) {
                    if (_isSelect) {
                        cutText();
                    } else {
                        _cursorPosition = _textureStorage.checkLineFits(_cursorPosition);
                        if (args.key == KeyCode.BACKSPACE) { // backspace
                            onBackSpaceInput(args);
                        }
                        if (args.key == KeyCode.DELETE) { // delete
                            onDeleteInput(args);
                        }
                    }

                    if (args.key == KeyCode.ENTER || args.key == KeyCode.NUMPADENTER) // enter
                    {
                        _textureStorage.breakLine(_cursorPosition);
                        _cursorPosition.y++;
                        _cursorPosition.x = 0;

                        addToUndoAndReplaceCursor();
                    }

                    if (args.key == KeyCode.TAB) {
                        pasteText("    "); //privPasteText
                    }

                } else if (_isSelect) { // && !_insteadKeyMods.contains(args.key)) { //кажется, вторая проверка лишняя теперь
                    unselectText();
                }
            }

            if (isCursorControlKey) {
                if (!args.mods.contains(KeyMods.ALT) && !args.mods.contains(KeyMods.SUPER)) {

                    if (args.key == KeyCode.LEFT) { // arrow left
                        _cursorPosition = _textureStorage.checkLineFits(_cursorPosition); //NECESSARY!

                        boolean doUsual = true;

                        if (hasControl) {
                            int[] wordBounds = _textureStorage.findWordBounds(_cursorPosition);

                            if (wordBounds[0] != wordBounds[1] && _cursorPosition.x != wordBounds[0]) {
                                _cursorPosition = new Point(wordBounds[0], _cursorPosition.y);
                                replaceCursor();
                                doUsual = false;
                            }
                        }

                        if (!_justSelected && doUsual) {
                            if (_cursorPosition.x > 0) {
                                _cursorPosition.x--;
                            } else if (_cursorPosition.y > 0) {
                                _cursorPosition.y--;
                                _cursorPosition.x = getLettersCountInLine(_cursorPosition.y);
                            }
                            replaceCursor();
                        }
                    }

                    if (args.key == KeyCode.RIGHT) { // arrow right
                        boolean doUsual = true;

                        if (hasControl) {
                            int[] wordBounds = _textureStorage.findWordBounds(_cursorPosition);

                            if (wordBounds[0] != wordBounds[1] && _cursorPosition.x != wordBounds[1]) {
                                _cursorPosition = new Point(wordBounds[1], _cursorPosition.y);
                                replaceCursor();
                                doUsual = false;
                            }
                        }

                        if (!_justSelected && doUsual) {
                            if (_cursorPosition.x < getLettersCountInLine(_cursorPosition.y)) {
                                _cursorPosition.x++;
                            } else if (_cursorPosition.y < _textureStorage.getLinesCount() - 1) {
                                _cursorPosition.y++;
                                _cursorPosition.x = 0;
                            }
                            replaceCursor();
                        }
                    }

                    if (args.key == KeyCode.UP) { // arrow up
                        if (!_justSelected) {
                            if (_cursorPosition.y > 0) {
                                _cursorPosition.y--;
                            }
                            // ?????
                            replaceCursor();
                        }
                    }

                    if (args.key == KeyCode.DOWN) { // arrow down
                        if (!_justSelected) {
                            if (_cursorPosition.y < _textureStorage.getLinesCount() - 1) {
                                _cursorPosition.y++;
                            }
                            // ?????
                            replaceCursor();
                        }
                    }

                    if (args.key == KeyCode.END) { // end
                        boolean doUsual = true;

                        if (hasControl) {
                            int lineNum = _textureStorage.getLinesCount() - 1;
                            _cursorPosition = new Point(getLettersCountInLine(lineNum), lineNum);
                            replaceCursor();
                            doUsual = false;
                        }

                        if (doUsual) {
                            _cursorPosition.x = getLettersCountInLine(_cursorPosition.y);
                            replaceCursor();
                        }
                    }

                    if (args.key == KeyCode.HOME) { // home
                        boolean doUsual = true;

                        if (hasControl) {
                            _cursorPosition = new Point(0, 0);
                            replaceCursor();
                            doUsual = false;
                        }

                        if (doUsual) {
                            _cursorPosition.x = 0;
                            replaceCursor();
                        }
                    }
                }
            }
            // replaceCursor();
            if (_isSelect) {
                if (!_selectTo.equals(_cursorPosition)) {
                    _selectTo = new Point(_cursorPosition);
                    makeSelectedArea();
                }
            }
        } finally {
            _textureStorage.textInputLock.unlock();
        }
    }

    private void onBackSpaceInput(KeyArgs args) {
        if (_cursorPosition.x > 0) {
            StringBuilder sb = new StringBuilder(_textureStorage.getTextInLine(_cursorPosition.y));
            _cursorPosition.x--;
            setTextInLine(sb.deleteCharAt(_cursorPosition.x).toString());
        } else if (_cursorPosition.y > 0) {
            _cursorPosition.y--;
            _cursorPosition.x = getLettersCountInLine(_cursorPosition.y);
            _textureStorage.combineLinesOrRemoveLetter(_cursorPosition, args.key); //_textureStorage.combineLines(_cursorPosition); //.y);
            addToUndoAndReplaceCursor();
        }
//        replaceCursor();
    }

    private void onDeleteInput(KeyArgs args) {
        if (_cursorPosition.x < getLettersCountInLine(_cursorPosition.y)) {
            StringBuilder sb = new StringBuilder(_textureStorage.getTextInLine(_cursorPosition.y));
            setTextInLine(sb.deleteCharAt(_cursorPosition.x).toString());
        } else if (_cursorPosition.y < _textureStorage.getLinesCount() - 1) {
            _textureStorage.combineLinesOrRemoveLetter(_cursorPosition, args.key); //_textureStorage.combineLines(_cursorPosition); //.y);
            addToUndoAndReplaceCursor();
        }
    }

    private void onTextInput(Object sender, TextInputArgs args) {
        if (!_isEditable) {
            return;
        }
        _textureStorage.textInputLock.lock();
        try {
            ignoreSetInLine = true;
            byte[] input = ByteBuffer.allocate(4).putInt(args.character).array();
            String str = new String(input, Charset.forName("UTF-32"));

            if (_isSelect || _justSelected) {
                unselectText();
                privCutText();
            }
            if (_justSelected) {
                cancelJustSelected();
            }

            _cursorPosition = _textureStorage.checkLineFits(_cursorPosition);

            StringBuilder sb = new StringBuilder(_textureStorage.getTextInLine(_cursorPosition.y));
            _cursorPosition.x++;
            setTextInLine(sb.insert(_cursorPosition.x - 1, str).toString());

            addToUndoAndReplaceCursor();

        } finally {
            _textureStorage.textInputLock.unlock();
        }
    }

    private void replaceCursor() {
        _textureStorage.textInputLock.lock();
        try {
            // _cursorPosition = _textureStorage.checkLineFits(_cursorPosition);
            Point pos = addXYShifts(_cursorPosition);
            _cursor.setX(pos.x);
            _cursor.setY(pos.y - getLineSpacer() / 2 + 1);

            //invoke cancelJustSelected
            cursorChanged.execute();
        } finally {
            _textureStorage.textInputLock.unlock();
        }
    }

    String getText() {
        return _textureStorage.getWholeText();
    }

    void setText(String text) {
        _textureStorage.textInputLock.lock();
        try {
            if (_isSelect) {
                unselectText();
            }
            if (_justSelected) {
                cancelJustSelected();
            }

            _cursorPosition = _textureStorage.setText(text); //, _cursorPosition);
//            replacecursor();
            addToUndoAndReplaceCursor();
        } finally {
            _textureStorage.textInputLock.unlock();
        }
    }

    private void setTextInLine(String text) {
        _textureStorage.setTextInLine(text, _cursorPosition); //.y);

        if (!ignoreSetInLine) {
            addToUndoAndReplaceCursor();
        } else {
            ignoreSetInLine = false;
        }
    }

    int getTextWidth() {
        return _textureStorage.getWidth();
    }

    int getTextHeight() {
        return _textureStorage.getTextHeight();
    }

    boolean isEditable() {
        return _isEditable;
    }

    void setEditable(boolean value) {
        if (_isEditable == value) {
            return;
        }
        _isEditable = value;

        if (_isEditable) {
            _cursor.setVisible(true);
        } else {
            _cursor.setVisible(false);
        }
    }

    @Override
    public void initElements() {
        _cursor.setHeight(_textureStorage.getCursorHeight());
        addItems(_selectedArea, _textureStorage, _cursor);
        _textureStorage.initLines(_cursor.getWidth());
        if (isWrapText()) {
            reorganizeText();
        }
    }

    @Override
    public void setFocused(boolean value) {
        super.setFocused(value);
        if (isFocused() && _isEditable) {
            _cursor.setVisible(true);
        } else {
            _cursor.setVisible(false);
        }
    }

    private int getLettersCountInLine(int lineNum) {
        return _textureStorage.getLettersCountInLine(lineNum);
    }

    private void makeSelectedArea() {
        makeSelectedArea(_selectFrom, _selectTo);
    }

    private void makeSelectedArea(Point from, Point to) {
        if (from.x == to.x && from.y == to.y) {
            _selectedArea.setRectangles(null);
            return;
        }

        List<Point> selectionRectangles;

        Point fromReal, toReal;
        List<Point> listPt = realFromTo(from, to);
        fromReal = listPt.get(0);
        toReal = listPt.get(1);

        selectionRectangles = _textureStorage.selectedArrays(fromReal, toReal);

        _selectedArea.setRectangles(selectionRectangles);
    }

    private List<Point> realFromTo(Point from, Point to) {
        List<Point> ans = new LinkedList<>();
        Point fromReal, toReal;
        if (from.y == to.y) {
            if (from.x < to.x) {
                fromReal = from;
                toReal = to;
            } else {
                fromReal = to;
                toReal = from;
            }
        } else {
            if (from.y < to.y) {
                fromReal = from;
                toReal = to;
            } else {
                fromReal = to;
                toReal = from;
            }
        }

        ans.add(fromReal);
        ans.add(toReal);
        return ans;
    }

    private Point addXYShifts(Point point) {
        return _textureStorage.addXYShifts(point);
    }

    private String privGetSelectedText() {
        _textureStorage.textInputLock.lock();
        try {
            if (_selectFrom.x == -1 || _selectTo.x == -1) {
                return "";
            }
            _selectFrom = _textureStorage.checkLineFits(_selectFrom);
            _selectTo = _textureStorage.checkLineFits(_selectTo);
            if (_selectFrom.x == _selectTo.x && _selectFrom.y == _selectTo.y) {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            List<Point> listPt = realFromTo(_selectFrom, _selectTo);
            Point fromReal = listPt.get(0);
            Point toReal = listPt.get(1);

            StringBuilder stmp;
            if (fromReal.y == toReal.y) {
                stmp = new StringBuilder(_textureStorage.getTextInLine(fromReal.y));
                sb.append(stmp.substring(fromReal.x, toReal.x)); // - fromReal.x
                return sb.toString();
            }

            _textureStorage.getSelectedText(sb, fromReal, toReal);

            //            if (fromReal.x >= getLettersCountInLine(fromReal.y))
            //                sb.append("\n");
            //            else {
            //                stmp = new StringBuilder(_textureStorage.getTextInLine(fromReal.y));
            //                sb.append(stmp.substring(fromReal.x));
            //                sb.append("\n");
            //            }
            //            for (int i = fromReal.y + 1; i < toReal.y; i++) {
            //                stmp = new StringBuilder(_textureStorage.getTextInLine(i));
            //                sb.append(stmp);
            //                sb.append("\n");
            //            }
            //
            //            stmp = new StringBuilder(_textureStorage.getTextInLine(toReal.y));
            //            sb.append(stmp.substring(0, toReal.x));

            return sb.toString();
        } finally {
            _textureStorage.textInputLock.unlock();
        }
    }

    public String getSelectedText() {
        return privGetSelectedText();
    }

    private void privPasteText(String pasteStr) {
        _textureStorage.textInputLock.lock();
        try {
            if (_isSelect) {
                privCutText();
            }
            if (pasteStr == null || pasteStr.equals("")) {
                return;
            }

            _cursorPosition = _textureStorage.checkLineFits(_cursorPosition);
            _cursorPosition = _textureStorage.pasteText(pasteStr, _cursorPosition);

            //            replaceCursor();
            addToUndoAndReplaceCursor();
        } finally {
            _textureStorage.textInputLock.unlock();
        }
    }

    public void pasteText(String pasteStr) {
        if (!_isEditable) {
            return;
        }
        if (pasteStr != null) {
            privPasteText(pasteStr);
        }
    }

    private String privCutText() {
        _textureStorage.textInputLock.lock();
        try {
            if (_selectFrom.x == -1 || _selectTo.x == -1) {
                return "";
            }
            String str = privGetSelectedText();
            _selectFrom = _textureStorage.checkLineFits(_selectFrom);
            _selectTo = _textureStorage.checkLineFits(_selectTo);
            if (_selectFrom.x == _selectTo.x && _selectFrom.y == _selectTo.y) {
                return "";
            }
            List<Point> listPt = realFromTo(_selectFrom, _selectTo);
            Point fromReal = listPt.get(0);
            Point toReal = listPt.get(1);

            _textureStorage.cutText(fromReal, toReal);

            _cursorPosition = new Point(fromReal);
            if (_isSelect) {
                unselectText();
            }
            cancelJustSelected();
            replaceCursor();
            return str;
        } finally {
            _textureStorage.textInputLock.unlock();
        }
    }

    public String cutText() {
        if (!_isEditable) {
            return "";
        }
        String ans = privCutText();
        addToUndoAndReplaceCursor();
        return ans;
    }

    private void unselectText() {
        _isSelect = false;
        _justSelected = true;
        makeSelectedArea(new Point(_cursorPosition), new Point(_cursorPosition));
    }

    private void cancelJustSelected() {
        _selectFrom.x = -1;
        _selectFrom.y = 0;
        _selectTo.x = -1;
        _selectTo.y = 0;
        _justSelected = false;
    }

    @Override
    public void clear() {
        clearText();
    }

    void clearText() {
        _textureStorage.clear();
        _cursorPosition.x = 0;
        _cursorPosition.y = 0;
        if (_isSelect) {
            unselectText();
        }
        if (_justSelected) {
            cancelJustSelected();
        }

        //        replaceCursor();
        addToUndoAndReplaceCursor();
    }

    public final void selectAll() {
        _textureStorage.textInputLock.lock();
        try {
            _selectFrom.x = 0;
            _selectFrom.y = 0;
            _cursorPosition.y = _textureStorage.getLinesCount() - 1;
            _cursorPosition.x = getLettersCountInLine(_cursorPosition.y);
            _selectTo = new Point(_cursorPosition);
            replaceCursor();
            _isSelect = true;
            makeSelectedArea();
        } finally {
            _textureStorage.textInputLock.unlock();
        }
    }


    private ArrayDeque<TextBlockState> undoQueue;
    private ArrayDeque<TextBlockState> redoQueue;
    private boolean nothingFlag = false;
    private int queueCapacity = SpaceVILConstants.textUndoCapacity;
    private boolean ignoreSetInLine = false;

    public void redo() {
        if (redoQueue.size() == 0) {
            return;
        }

        TextBlockState tmpText = redoQueue.pollFirst();
        if (tmpText != null) {
            nothingFlag = true;
            setText(tmpText.textState);
            _cursorPosition = new Point(tmpText.cursorStateX, tmpText.cursorStateY);
            // undoQueue.peekFirst().cursorStateX = _cursorPosition.x;
            // undoQueue.peekFirst().cursorStateY = _cursorPosition.y;

            // undoQueue.peekFirst().scrollYOffset = tmpText.scrollYOffset;
            _textureStorage.setScrollYOffset(tmpText.scrollYOffset);
            //TODO here reverse
            if (isWrapText()) {
                _cursorPosition = _textureStorage.realCursorPosToWrap(_cursorPosition);
            }
            replaceCursor();
        }
    }

    public void undo() {
        if (undoQueue.size() == 1) {
            return;
        }

        TextBlockState tmpText = undoQueue.pollFirst();
        if (tmpText != null) {
            if (redoQueue.size() > queueCapacity) {
                redoQueue.pollLast();
            }
            redoQueue.addFirst(
                    new TextBlockState(tmpText)); //.textState, new Point(tmpText.cursorStateX, tmpText.cursorStateY)));

            tmpText = undoQueue.pollFirst();
            if (tmpText != null) {
                nothingFlag = true;
                setText(tmpText.textState);
                _cursorPosition = new Point(tmpText.cursorStateX, tmpText.cursorStateY);
                // undoQueue.peekFirst().cursorStateX = _cursorPosition.x;
                // undoQueue.peekFirst().cursorStateY = _cursorPosition.y;

                // undoQueue.peekFirst().scrollYOffset = tmpText.scrollYOffset;
                _textureStorage.setScrollYOffset(tmpText.scrollYOffset);
                //TODO here reverse
                if (isWrapText()) {
                    _cursorPosition = _textureStorage.realCursorPosToWrap(_cursorPosition);
                }
                replaceCursor();
            }
        }
    }

    private void addToUndoAndReplaceCursor() {
        replaceCursor();
        if (!nothingFlag) {
            redoQueue = new ArrayDeque<>();
        } else {
            nothingFlag = false;
        }

        if (undoQueue.size() > queueCapacity) {
            undoQueue.pollLast();
        }
        //TODO here forward
        Point realPos = new Point(_cursorPosition);
        if (isWrapText()) {
            realPos = _textureStorage.wrapCursorPosToReal(_cursorPosition);
        }
        TextBlockState tbs = new TextBlockState(getText(), realPos, getScrollYOffset());
        // if (_isSelect) {
        // tbs.fromSelectState = new Point(_selectFrom);
        // tbs.toSelectState = new Point(_selectTo);
        // }
        undoQueue.addFirst(tbs);
        textChanged.execute();
    }

    @Override
    public void setWidth(int width) {
        if (getWidth() == width) {
            return;
        }
        updateBlockWidth(width);
    }

    private void updateBlockWidth(int width) {
        Point tmpCursor = new Point(_cursorPosition);
        Point fromTmp = new Point(_selectFrom);
        Point toTmp = new Point(_selectTo);
        if (isWrapText()) {
            tmpCursor = _textureStorage.wrapCursorPosToReal(_cursorPosition);
            if (_isSelect) {
                fromTmp = _textureStorage.wrapCursorPosToReal(_selectFrom);
                toTmp = _textureStorage.wrapCursorPosToReal(_selectTo);
            }
        }
        super.setWidth(width);
        _textureStorage.updateBlockWidth(_cursor.getWidth());
        reorganizeText();
        if (isWrapText()) {
            _cursorPosition = _textureStorage.realCursorPosToWrap(tmpCursor);
            replaceCursor();
            if (_isSelect) {
                fromTmp = _textureStorage.realCursorPosToWrap(fromTmp);
                toTmp = _textureStorage.realCursorPosToWrap(toTmp);
                _selectFrom = fromTmp;
                _selectTo = toTmp;
                makeSelectedArea();
            }
        }
    }

    @Override
    public void setHeight(int height) {
        if (getHeight() == height) {
            return;
        }
        updateBlockHeight(height);
    }

    private void updateBlockHeight(int height) {
        super.setHeight(height);
        _textureStorage.updateBlockHeight();
    }

    @Override
    public void setX(int x) {
        if (getX() == x) {
            return;
        }
        super.setX(x);
        updateLayout();
    }

    @Override
    public void setY(int y) {
        if (getY() == y) {
            return;
        }
        super.setY(y);
        updateLayout();
    }

    public void updateLayout() {
        if (_textureStorage.getParent() == null) {
            return;
        }
        // ReplaceCursor();
        Point pos = addXYShifts(_cursorPosition);
        _cursor.setX(pos.x);
        _cursor.setY(pos.y - getLineSpacer() / 2 + 1);
        makeSelectedArea();
    }

    // private class TextCursor : Rectangle {
    // Point _cursorPosition = new Point(0, 0);
    // TextCursor(int height) {
    // SetItemName("TextCursor_" + count);
    // SetHeight(height);
    // }

    // }

    int getCursorWidth() {
        return _cursor.getWidth();
    }

    void appendText(String text) {
        unselectText();
        cancelJustSelected();
        int lineNum = _textureStorage.getLinesCount() - 1;
        _cursorPosition = new Point(getLettersCountInLine(lineNum), lineNum);
        privPasteText(text); //pasteText
    }

    private class TextBlockState {
        String textState;
        int cursorStateX;
        int cursorStateY;
        int scrollYOffset;

        // Point fromSelectState;
        // Point toSelectState;

        TextBlockState(String textState, Point cursorState, int scrollYOffset) {
            this.textState = textState;
            this.cursorStateX = cursorState.x;
            this.cursorStateY = cursorState.y;
            this.scrollYOffset = scrollYOffset;
            // fromSelectState = new Point(0, 0);
            // toSelectState = new Point(0, 0);
        }

        public TextBlockState(TextBlockState tbs) {
            this.textState = tbs.textState;
            this.cursorStateX = tbs.cursorStateX;
            this.cursorStateY = tbs.cursorStateY;
            this.scrollYOffset = tbs.scrollYOffset;
        }
    }

    void rewindText() {
        _cursorPosition = new Point(0, 0);
        replaceCursor();
    }

    //Wrap Text Stuff---------------------------------------------------------------------------------------------------

    private boolean _isWrapText;

    public boolean isWrapText() {
        return _isWrapText;
    }

    void setWrapText(boolean value) {
        if (value == _isWrapText) {
            return;
        }

        _textureStorage.textInputLock.lock();
        try {
            String text = getText();
            //            if (text == null || text.equals("")) {
            //                _isWrapText = value;
            //                return;
            //            }

            Point cursorTmp = _cursorPosition;
            Point fromTmp = new Point();
            Point toTmp = new Point();
            if (_isWrapText) {
                cursorTmp = _textureStorage.wrapCursorPosToReal(cursorTmp);
                if (_isSelect) {
                    fromTmp = _textureStorage.wrapCursorPosToReal(_selectFrom);
                    toTmp = _textureStorage.wrapCursorPosToReal(_selectTo);
                }
            }

            _isWrapText = value;

            _textureStorage.setText(text); //not added into redo/undo

            if (_isWrapText) { //was unwrap become wrap
                cursorTmp = _textureStorage.realCursorPosToWrap(cursorTmp);
                if (_isSelect) {
                    fromTmp = _textureStorage.realCursorPosToWrap(_selectFrom);
                    toTmp = _textureStorage.realCursorPosToWrap(_selectTo);
                }
            }

            _cursorPosition = cursorTmp;
            replaceCursor();
            if (_isSelect) {
                _selectFrom = fromTmp;
                _selectTo = toTmp;
                makeSelectedArea();
            }

        } finally {
            _textureStorage.textInputLock.unlock();
        }
    }

    // if wrapText is on && something changed
    private void reorganizeText() {
        if (!_isWrapText) {
            return;
        }
        _textureStorage.textInputLock.lock();
        try {
            _textureStorage.rewrapText();
        } finally {
            _textureStorage.textInputLock.unlock();
        }
    }

    void setScrollStepFactor(float value) {
        _textureStorage.setScrollStepFactor(value);
    }

    //decorations-------------------------------------------------------------------------------------------------------
    void setLineSpacer(int lineSpacer) {
        _textureStorage.setLineSpacer(lineSpacer);
        _cursor.setHeight(_textureStorage.getCursorHeight());
    }

    int getLineSpacer() {
        return _textureStorage.getLineSpacer();
    }

    void setTextAlignment(ItemAlignment... alignment) {
        setTextAlignment(Arrays.asList(alignment));
    }

    void setTextAlignment(List<ItemAlignment> alignment) {
        // Ignore all changes for yet
    }

    void setTextMargin(Indents margin) {
        _textureStorage.setTextMargin(margin);
        _cursorPosition = _textureStorage.checkLineFits(_cursorPosition); //???
        replaceCursor(); //???
    }

    Indents getTextMargin() {
        return _textureStorage.getTextMargin();
    }

    void setFont(Font font) {
        _textureStorage.setFont(font);
        _cursor.setHeight(_textureStorage.getCursorHeight());
        _cursorPosition = _textureStorage.checkLineFits(_cursorPosition); //???
        replaceCursor();
    }

    Font getFont() {
        return _textureStorage.getFont();
    }

    void setForeground(Color color) {
        _textureStorage.setForeground(color);
    }

    Color getForeground() {
        return _textureStorage.getForeground();
    }

    // style
    @Override
    public void setStyle(Style style) {
        if (style == null) {
            return;
        }
        super.setStyle(style);
        setForeground(style.foreground);
        setFont(style.font);
        _textureStorage.setLineContainerAlignment(style.textAlignment);

        Style inner_style = style.getInnerStyle("selection");
        if (inner_style != null) {
            _selectedArea.setStyle(inner_style);
        }
        inner_style = style.getInnerStyle("cursor");
        if (inner_style != null) {
            _cursor.setStyle(inner_style);
            if (_cursor.getHeight() == 0) {
                _cursor.setHeight(_textureStorage.getCursorHeight());
            }
        }
    }
}