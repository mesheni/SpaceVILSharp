package com.spvessel;

import com.spvessel.Common.DefaultsService;
import com.spvessel.Core.EventCommonMethod;
import com.spvessel.Core.InterfaceBaseItem;
import com.spvessel.Core.InterfaceCommonMethod;
import com.spvessel.Decorations.Style;
import com.spvessel.Flags.ScrollBarVisibility;

public class ComboBoxDropDown extends DialogWindow {
    public ListBox itemList; // = new ListBox();
    public ButtonCore selection;
    public EventCommonMethod selectionChanged = new EventCommonMethod();

    public ComboBoxDropDown() {
    }

    @Override
    public void initWindow() {
        itemList = new ListBox();
        WindowLayout Handler = new WindowLayout(this, "DropDown_" + getCount(), "DropDown_" + getCount());
        setHandler(Handler);
        Handler.isOutsideClickClosable = true;
        Handler.isBorderHidden = true;
        Handler.isAlwaysOnTop = true;
        Handler.isCentered = false;
        Handler.isResizable = false;
        
        itemList.setVScrollBarVisible(ScrollBarVisibility.NEVER);
        itemList.setHScrollBarVisible(ScrollBarVisibility.NEVER);
        InterfaceCommonMethod selection_changed = () -> onSelectionChanged();
        itemList.getArea().selectionChanged.add(selection_changed);
        
        Handler.addItem(itemList);

        // setStyle(DefaultsService.getDefaultStyle("SpaceVIL.ComboBoxDropDown"));
        setStyle(DefaultsService.getDefaultStyle(ComboBoxDropDown.class));
    }

    public void add(InterfaceBaseItem item) {
        itemList.addItem(item);
    }

    public void remove(InterfaceBaseItem item) {
        itemList.removeItem(item);
    }

    private void onSelectionChanged() {
        if (itemList.getListContent().get(itemList.getSelection()) instanceof MenuItem) {
            MenuItem l = (MenuItem) itemList.getListContent().get(itemList.getSelection());
            selection.setText(l.getText());
            getHandler().resetItems();
            close();
            selectionChanged.execute();
        }
    }

    public void setCurrentIndex(int index) {
        itemList.setSelection(index);
    }

    public int getCurrentIndex() {
        return itemList.getSelection();
    }

    @Override
    public void show() {
        super.show();
        WindowLayoutBox.setFocusedWindow(this);
    }

    public void Dispose() {
        getHandler().close();
    }

    public void setStyle(Style style) {
        if (style == null)
            return;

        getHandler().setBackground(style.background);
        getHandler().setPadding(style.padding);

        Style itemlist_style = style.getInnerStyle("itemlist");
        if (itemlist_style != null) {
            itemList.setBackground(itemlist_style.background);
            itemList.setAlignment(itemlist_style.alignment);
        }
    }
}