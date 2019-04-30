package com.spvessel.spacevil;

import com.spvessel.spacevil.Core.EventCommonMethod;
import com.spvessel.spacevil.Core.EventCommonMethodState;
import com.spvessel.spacevil.Core.EventDropMethodState;
import com.spvessel.spacevil.Core.EventInputTextMethodState;
import com.spvessel.spacevil.Core.EventKeyMethodState;
import com.spvessel.spacevil.Core.EventMouseMethodState;
import com.spvessel.spacevil.Core.Geometry;
import com.spvessel.spacevil.Core.InterfaceBaseItem;
import com.spvessel.spacevil.Core.Position;
import com.spvessel.spacevil.Decorations.Border;
import com.spvessel.spacevil.Decorations.CornerRadius;
import com.spvessel.spacevil.Decorations.Indents;
import com.spvessel.spacevil.Flags.MSAA;
import com.spvessel.spacevil.Flags.RedrawFrequency;

import java.awt.Color;
import java.awt.image.BufferedImage;
import java.util.*;

public abstract class CoreWindow {
    private static int count = 0;
    private UUID windowUUID;

    /**
     * Constructs a CoreWindow
     */
    public CoreWindow() {
        windowUUID = UUID.randomUUID();
        setWindowName("Window_" + count);
        setWindowTitle("Window_" + count);
        setDefaults();
        count++;
        setHandler(new WindowLayout(this));
        setSize(300, 300);
        setMinSize(150, 100);
    }

    public void setParameters(String name, String title) {
        setWindowName(name);
        setWindowTitle(title);
    }

    public void setParameters(String name, String title, int width, int height) {
        setWindowName(name);
        setWindowTitle(title);
        setSize(width, height);
    }

    public void setParameters(String name, String title, int width, int height, boolean isBorder) {
        setWindowName(name);
        setWindowTitle(title);
        setSize(width, height);
        isBorderHidden = !isBorder;
    }

    private WindowLayout wnd_layout;

    /**
     * Parent item for the CoreWindow
     */
    WindowLayout getLayout() {
        return wnd_layout;
    }

    /**
     * Parent item for the CoreWindow
     */
    void setHandler(WindowLayout wl) {
        if (wl == null) {
            System.out.println("Window handler can't be null");
            return;
        }
        wnd_layout = wl;
        wl.setCoreWindow();
    }

    /**
     * Show the CoreWindow
     */
    public void show() {
        wnd_layout.show();
        // WindowLayoutBox.setFocusedWindow(this);
    }

    /**
     * Close the CoreWindow
     */
    public void close() {
        wnd_layout.close();
    }

    /**
     * Initialize the window
     */
    abstract public void initWindow();
    // abstract public void InitElements();

    /**
     * @return count of all CoreWindows
     */
    public int getCount() {
        return count;
    }

    /**
     * @return CoreWindow unique ID
     */
    public UUID getWindowGuid() {
        return windowUUID;
    }

    // ------------------------------------------------------------------------------------------
    public void setBackground(Color color) {
        wnd_layout.getContainer().setBackground(color);
    }

    public void setBackground(int r, int g, int b) {
        wnd_layout.getContainer().setBackground(GraphicsMathService.colorTransform(r, g, b));
    }

    public void setBackground(int r, int g, int b, int a) {
        wnd_layout.getContainer().setBackground(GraphicsMathService.colorTransform(r, g, b, a));
    }

    public void setBackground(float r, float g, float b) {
        wnd_layout.getContainer().setBackground(GraphicsMathService.colorTransform(r, g, b));
    }

    public void setBackground(float r, float g, float b, float a) {
        wnd_layout.getContainer().setBackground(GraphicsMathService.colorTransform(r, g, b, a));
    }

    public Color getBackground() {
        return wnd_layout.getContainer().getBackground();
    }

    public void setPadding(Indents padding) {
        wnd_layout.getContainer().setPadding(padding);
    }

    public void setPadding(int left, int top, int right, int bottom) {
        wnd_layout.getContainer().setPadding(left, top, right, bottom);
    }

    public List<InterfaceBaseItem> getItems() {
        return wnd_layout.getContainer().getItems();
    }

    public void addItem(InterfaceBaseItem item) {
        wnd_layout.getContainer().addItem(item);
    }

    public void addItems(InterfaceBaseItem... items) {
        for (InterfaceBaseItem item : items) {
            wnd_layout.getContainer().addItem(item);
        }
    }

    public void insertItem(InterfaceBaseItem item, int index) {
        wnd_layout.getContainer().insertItem(item, index);
    }

    public boolean removeItem(InterfaceBaseItem item) {
        return wnd_layout.getContainer().removeItem(item);
    }

    public void clear() {
        wnd_layout.getContainer().clear();
    }

    private String _name;

    public void setWindowName(String value) {
        _name = value;
    }

    public String getWindowName() {
        return _name;
    }

    private String _title;

    public void setWindowTitle(String title) {
        _title = title;
    }

    public String getWindowTitle() {
        return _title;
    }

    // geometry
    private Geometry _itemGeometry = new Geometry();

    public void setMinWidth(int width) {
        _itemGeometry.setMinWidth(width);
        if (wnd_layout.getContainer() != null)
            wnd_layout.getContainer().setMinWidth(width);
    }

    public void setWidth(int width) {
        _itemGeometry.setWidth(width);
        if (wnd_layout.getContainer() != null)
            wnd_layout.getContainer().setWidth(width);
    }

    public void setMaxWidth(int width) {
        _itemGeometry.setMaxWidth(width);
        if (wnd_layout.getContainer() != null)
            wnd_layout.getContainer().setMaxWidth(width);
    }

    public void setMinHeight(int height) {
        _itemGeometry.setMinHeight(height);
        if (wnd_layout.getContainer() != null)
            wnd_layout.getContainer().setMinHeight(height);
    }

    public void setHeight(int height) {
        _itemGeometry.setHeight(height);
        if (wnd_layout.getContainer() != null)
            wnd_layout.getContainer().setHeight(height);
    }

    public void setMaxHeight(int height) {
        _itemGeometry.setMaxHeight(height);
        if (wnd_layout.getContainer() != null)
            wnd_layout.getContainer().setMaxHeight(height);
    }

    public int getMinWidth() {
        return _itemGeometry.getMinWidth();
    }

    public int getWidth() {
        return _itemGeometry.getWidth();
    }

    public int getMaxWidth() {
        return _itemGeometry.getMaxWidth();
    }

    public int getMinHeight() {
        return _itemGeometry.getMinHeight();
    }

    public int getHeight() {
        return _itemGeometry.getHeight();
    }

    public int getMaxHeight() {
        return _itemGeometry.getMaxHeight();
    }

    public void setSize(int width, int height) {
        setWidth(width);
        setHeight(height);
    }

    public void setMinSize(int width, int height) {
        setMinWidth(width);
        setMinHeight(height);
    }

    public void setMaxSize(int width, int height) {
        setMaxWidth(width);
        setMaxHeight(height);
    }

    public int[] getSize() {
        return _itemGeometry.getSize();
    }

    // position
    private Position _itemPosition = new Position();

    public void setX(int x) {
        _itemPosition.setX(x);
    }

    public int getX() {
        return _itemPosition.getX();
    }

    public void setY(int y) {
        _itemPosition.setY(y);
    }

    public int getY() {
        return _itemPosition.getY();
    }

    private void setDefaults() {
        isDialog = false;
        isClosed = true;
        isHidden = false;
        isResizable = true;
        isAlwaysOnTop = false;
        isBorderHidden = false;
        isCentered = false;
        isFocusable = true;
        isOutsideClickClosable = false;
        isMaximized = false;
        isTransparent = false;
    }

    public boolean isDialog;
    public boolean isClosed;
    public boolean isHidden;
    public boolean isResizable;
    public boolean isAlwaysOnTop;
    public boolean isBorderHidden;
    public boolean isCentered;
    public boolean isFocusable;
    public boolean isOutsideClickClosable;
    public boolean isMaximized;
    boolean isTransparent;

    public boolean isFocused;

    MSAA _msaa = MSAA.MSAA_4X;

    public void setAntiAliasingQuality(MSAA msaa) {
        _msaa = msaa;
    }

    void setFocusable(boolean value) {
        wnd_layout.setFocusable(value);
    }

    public void setFocus(Boolean value) {
        if (isFocused == value)
            return;
        isFocused = value;
        if (value)
            setWindowFocused();
    }

    public void setWindowFocused() {
        wnd_layout.setWindowFocused();
    }

    public void minimize() {
        wnd_layout.minimize();
    }

    public void maximize() {
        wnd_layout.maximize();
    }

    public Prototype getFocusedItem() {
        return wnd_layout.getFocusedItem();
    }

    public void setFocusedItem(Prototype item) {
        wnd_layout.setFocusedItem(item);
    }

    public void setFocus() {
        wnd_layout.getContainer().setFocus();
    }

    public void resetItems() {
        wnd_layout.resetItems();
    }

    public void resetFocus() {
        wnd_layout.resetFocus();
    }

    public void setIcon(BufferedImage icon_big, BufferedImage icon_small) {
        wnd_layout.setIcon(icon_big, icon_small);
    }

    public void setHidden(Boolean value) {
        wnd_layout.setHidden(value);
        isHidden = value;
    }

    public void setRenderFrequency(RedrawFrequency value) {
        wnd_layout.setRenderFrequency(value);
    }

    public RedrawFrequency getRenderFrequency() {
        return wnd_layout.getRenderFrequency();
    }

    public EventCommonMethod eventClose = new EventCommonMethod();
    public EventCommonMethod eventMinimize = new EventCommonMethod();
    public EventCommonMethod eventHide = new EventCommonMethod();

    protected void release() {
        eventClose.clear();
        eventMinimize.clear();
        eventHide.clear();
        freeEvents();
    }

    void setWindow(WContainer window) {
        wnd_layout.setWindow(window);
    }

    float[] getDpiScale() {
        return wnd_layout.getDpiScale();
    }

    void setDpiScale(float w, float h) {
        wnd_layout.setDpiScale(w, h);
    }

    int ratioW = -1;
    int ratioH = -1;
    boolean isKeepAspectRatio = false;

    public void setAspectRatio(int w, int h) {
        isKeepAspectRatio = true;
        ratioW = w;
        ratioH = h;
    }

    EventCommonMethodState eventFocusGet = new EventCommonMethodState();
    EventCommonMethodState eventFocusLost = new EventCommonMethodState();
    public EventCommonMethodState eventResize = new EventCommonMethodState();
    public EventCommonMethodState eventDestroy = new EventCommonMethodState();
    public EventMouseMethodState eventMouseHover = new EventMouseMethodState();
    public EventMouseMethodState eventMouseLeave = new EventMouseMethodState();
    public EventMouseMethodState eventMouseClick = new EventMouseMethodState();
    public EventMouseMethodState eventMouseDoubleClick = new EventMouseMethodState();
    public EventMouseMethodState eventMousePress = new EventMouseMethodState();
    public EventMouseMethodState eventMouseDrag = new EventMouseMethodState();
    public EventMouseMethodState eventMouseDrop = new EventMouseMethodState();
    public EventMouseMethodState eventScrollUp = new EventMouseMethodState();
    public EventMouseMethodState eventScrollDown = new EventMouseMethodState();
    public EventKeyMethodState eventKeyPress = new EventKeyMethodState();
    public EventKeyMethodState eventKeyRelease = new EventKeyMethodState();
    public EventInputTextMethodState eventTextInput = new EventInputTextMethodState();
    public EventDropMethodState eventDrop = new EventDropMethodState();

    void freeEvents() {
        eventFocusGet.clear();
        eventFocusLost.clear();
        eventResize.clear();
        eventDestroy.clear();

        eventMouseHover.clear();
        eventMouseClick.clear();
        eventMouseDoubleClick.clear();
        eventMousePress.clear();
        eventMouseDrag.clear();
        eventMouseDrop.clear();
        eventScrollUp.clear();
        eventScrollDown.clear();

        eventKeyPress.clear();
        eventKeyRelease.clear();

        eventTextInput.clear();
    }

    public void setBorder(Border border) {
        if (wnd_layout.getContainer() != null)
            wnd_layout.getContainer().setBorder(border);
    }

    public void setBorderFill(Color fill) {
        if (wnd_layout.getContainer() != null)
            wnd_layout.getContainer().setBorderFill(fill);
    }

    public void setBorderFill(int r, int g, int b) {
        if (wnd_layout.getContainer() != null)
            wnd_layout.getContainer().setBorderFill(r, g, b);
    }

    public void setBorderFill(int r, int g, int b, int a) {
        if (wnd_layout.getContainer() != null)
            wnd_layout.getContainer().setBorderFill(r, g, b, a);
    }

    public void setBorderFill(float r, float g, float b) {
        if (wnd_layout.getContainer() != null)
            wnd_layout.getContainer().setBorderFill(r, g, b);
    }

    public void setBorderFill(float r, float g, float b, float a) {
        if (wnd_layout.getContainer() != null)
            wnd_layout.getContainer().setBorderFill(r, g, b, a);
    }

    public void setBorderRadius(CornerRadius radius) {
        if (wnd_layout.getContainer() != null)
            wnd_layout.getContainer().setBorderRadius(radius);
    }

    public void setBorderRadius(int radius) {
        if (wnd_layout.getContainer() != null)
            wnd_layout.getContainer().setBorderRadius(new CornerRadius(radius));
    }

    public void setBorderThickness(int thickness) {
        if (wnd_layout.getContainer() != null)
            wnd_layout.getContainer().setBorderThickness(thickness);
    }

    public CornerRadius GetBorderRadius() {
        if (wnd_layout.getContainer() != null)
            return wnd_layout.getContainer().getBorderRadius();
        return null;
    }

    public int GetBorderThickness() {
        if (wnd_layout.getContainer() != null)
            return wnd_layout.getContainer().getBorderThickness();
        return 0;
    }

    public Color GetBorderFill() {
        if (wnd_layout.getContainer() != null)
            return wnd_layout.getContainer().getBorderFill();
        return new Color(0, 0, 0, 0);
    }
}