package com.spvessel.spacevil;

import java.awt.Color;
import java.util.LinkedList;
import java.util.List;

import com.spvessel.spacevil.Common.DefaultsService;
import com.spvessel.spacevil.Core.EventCommonMethod;
import com.spvessel.spacevil.Core.InterfaceBaseItem;
import com.spvessel.spacevil.Core.InterfaceFloating;
import com.spvessel.spacevil.Core.InterfaceItem;
import com.spvessel.spacevil.Core.MouseArgs;
import com.spvessel.spacevil.Decorations.Style;
import com.spvessel.spacevil.Flags.KeyCode;
import com.spvessel.spacevil.Flags.LayoutType;
import com.spvessel.spacevil.Flags.MouseButton;
import com.spvessel.spacevil.Flags.ScrollBarVisibility;

public class ComboBoxDropDown extends Prototype implements InterfaceFloating {
    public EventCommonMethod selectionChanged = new EventCommonMethod();
    public Prototype returnFocus = null;
    public ListBox itemList = new ListBox();
    private String _text_selection = "";

    public String getText() {
        return _text_selection;
    }

    public int getCurrentIndex() {
        return itemList.getSelection();
    }

    public void setCurrentIndex(int index) {
        if (!_init)
            initElements();

        itemList.setSelection(index);
        if (itemList.getSelectionItem() instanceof MenuItem) {
            MenuItem selection = (MenuItem) itemList.getSelectionItem();
            _text_selection = selection.getText();
        }
    }

    private List<InterfaceBaseItem> _queue = new LinkedList<>();

    private static int count = 0;
    public MouseButton activeButton = MouseButton.BUTTON_LEFT;

    private boolean _init = false;
    private boolean _ouside = true;

    /**
     * Close the ContextMenu it mouse click is outside (true or false)
     */
    public boolean isOutsideClickClosable() {
        return _ouside;
    }

    public void setOutsideClickClosable(boolean value) {
        _ouside = value;
    }

    /**
     * Constructs a ContextMenu
     * 
     * @param handler parent window for the ContextMenu
     */
    public ComboBoxDropDown(WindowLayout handler) {
        setPassEvents(false);
        setVisible(false);
        setHandler(handler);
        setItemName("ComboBoxDropDown_" + count);
        count++;
        ItemsLayoutBox.addItem(getHandler(), this, LayoutType.FLOATING);
        setStyle(DefaultsService.getDefaultStyle(ComboBoxDropDown.class));
        setShadow(5, 3, 3, new Color(0, 0, 0, 180));
    }

    /**
     * Initialization and adding of all elements in the ContextMenu
     */
    @Override
    public void initElements() {
        setConfines();
        itemList.setVScrollBarVisible(ScrollBarVisibility.AS_NEEDED);
        itemList.setHScrollBarVisible(ScrollBarVisibility.AS_NEEDED);
        itemList.getArea().selectionChanged.add(this::onSelectionChanged);

        super.addItem(itemList);

        for (InterfaceBaseItem item : _queue) {
            itemList.addItem(item);
        }
        _queue = null;
        _init = true;

        itemList.getArea().eventKeyPress.add((sender, args) -> {
            if (args.key == KeyCode.ESCAPE) {
                hide();
            }
        });
        updateSize();
    }

    private void onSelectionChanged() {
        if (itemList.getSelectionItem() instanceof MenuItem) {
            MenuItem selection = (MenuItem) itemList.getSelectionItem();
            _text_selection = selection.getText();
        }
        hide();
        selectionChanged.execute();
    }

    /**
     * Returns count of the ContextMenu lines
     */
    public int getListCount() {
        return itemList.getListContent().size();
    }

    /**
     * Returns ContextMenu items list
     */
    public List<InterfaceBaseItem> getListContent() {
        return itemList.getListContent();
    }

    /**
     * Add item to the ContextMenu
     */
    @Override
    public void addItem(InterfaceBaseItem item) {
        _queue.add(item);
    }

    /**
     * Remove item from the ContextMenu
     */
    @Override
    public void removeItem(InterfaceBaseItem item) {
        itemList.removeItem(item);
    }

    private void updateSize() {
        int height = 0;
        int width = getWidth();
        List<InterfaceBaseItem> list = itemList.getListContent();
        for (InterfaceBaseItem item : list) {
            InterfaceBaseItem wrapper = itemList.getWrapper(item);
            height += (wrapper.getHeight() + itemList.getArea().getSpacing().vertical);

            int tmp = getPadding().left + getPadding().right + item.getMargin().left + item.getMargin().right;

            if (item instanceof MenuItem) {
                MenuItem m = (MenuItem) item;
                tmp += m.getTextWidth() + m.getMargin().left + m.getMargin().right + m.getPadding().left
                        + m.getPadding().right;
            } else
                tmp = tmp + item.getWidth() + item.getMargin().left + item.getMargin().right;

            if (width < tmp)
                width = tmp;
        }
        // SetSize(width, height);
        setWidth(width);
        setHeight(height);
    }

    /**
     * Show the ContextMenu
     * 
     * @param sender the item from which the show request is sent
     * @param args   mouse click arguments (cursor position, mouse button, mouse
     *               button press/release, etc.)
     */
    public void show(InterfaceItem sender, MouseArgs args) {
        if (args.button.getValue() == activeButton.getValue()) {
            if (!_init)
                initElements();
            setVisible(true);
            setConfines();
            itemList.getArea().setFocus();
        }
    }

    /**
     * Hide the ContextMenu without destroying
     */
    public void hide() {
        setX(-getWidth());
        setVisible(false);
        // itemList.Unselect();
        if (returnFocus != null)
            returnFocus.setFocus();
    }

    /**
     * Set confines according to position and size of the ContextMenu
     */
    @Override
    public void setConfines() {
        super.setConfines(getX(), getX() + getWidth(), getY(), getY() + getHeight());
    }

    /**
     * Set style of the ContextMenu
     */
    @Override
    public void setStyle(Style style) {
        if (style == null)
            return;
        setPadding(style.padding);
        setSizePolicy(style.widthPolicy, style.heightPolicy);
        setBackground(style.background);

        Style inner_style = style.getInnerStyle("itemlist");
        if (inner_style != null) {
            itemList.setStyle(inner_style);
        }
        inner_style = style.getInnerStyle("listarea");
        if (inner_style != null) {
            itemList.getArea().setStyle(inner_style);
        }
    }
}