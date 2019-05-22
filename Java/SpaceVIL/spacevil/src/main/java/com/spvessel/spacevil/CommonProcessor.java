package com.spvessel.spacevil;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.util.ArrayDeque;
import java.util.Deque;
import java.util.LinkedList;
import java.util.List;
import java.util.UUID;

import static org.lwjgl.glfw.GLFW.*;

import com.spvessel.spacevil.Core.InterfaceBaseItem;
import com.spvessel.spacevil.Core.InterfaceFloating;
import com.spvessel.spacevil.Core.MouseArgs;
import com.spvessel.spacevil.Core.Pointer;
import com.spvessel.spacevil.Flags.InputEventType;

final class CommonProcessor {
    WindowProcessor wndProcessor;
    ToolTip toolTip;
    ActionManagerAssigner manager;
    GLWHandler handler;
    CoreWindow window;
    WindowLayout layout;
    WContainer rootContainer;
    UUID guid;
    MouseArgs margs;
    List<Prototype> underHoveredItems;
    List<Prototype> underFocusedItems;
    Prototype draggableItem = null;
    Prototype hoveredItem = null;
    Prototype focusedItem = null;
    int wGlobal = 0;
    int hGlobal = 0;
    int xGlobal = 0;
    int yGlobal = 0;
    boolean inputLocker = false;
    Pointer ptrPress = new Pointer();
    Pointer ptrRelease = new Pointer();
    Pointer ptrClick = new Pointer();
    InputDeviceEvent events;

    CommonProcessor() {
        this.events = new InputDeviceEvent();
        margs = new MouseArgs();
        margs.clear();
        underFocusedItems = new LinkedList<>();
        underHoveredItems = new LinkedList<>();
        wndProcessor = new WindowProcessor(this);
    }

    void initProcessor(GLWHandler handler, ToolTip toolTip) {
        this.handler = handler;
        this.toolTip = toolTip;
        window = handler.getCoreWindow();
        layout = window.getLayout();
        rootContainer = layout.getContainer();
        guid = window.getWindowGuid();
        this.manager = new ActionManagerAssigner(layout);
    }

    boolean getHoverPrototype(float xpos, float ypos, InputEventType action) {
        inputLocker = true;
        List<Prototype> queue = new LinkedList<>();
        underHoveredItems.clear();

        List<InterfaceBaseItem> layout_box_of_items = new LinkedList<InterfaceBaseItem>();
        layout_box_of_items.add(rootContainer);
        layout_box_of_items.addAll(getInnerItems(rootContainer));

        for (InterfaceBaseItem item : ItemsLayoutBox.getLayoutFloatItems(guid)) {
            if (!item.isVisible() || !item.isDrawable())
                continue;
            layout_box_of_items.add(item);

            if (item instanceof Prototype)
                layout_box_of_items.addAll(getInnerItems((Prototype) item));
        }
        inputLocker = false;

        for (InterfaceBaseItem item : layout_box_of_items) {
            if (item instanceof Prototype) {
                Prototype tmp = (Prototype) item;
                if (!tmp.isVisible() || !tmp.isDrawable())
                    continue;
                if (tmp.getHoverVerification(xpos, ypos)) {
                    queue.add(tmp);
                } else {
                    tmp.setMouseHover(false);
                    if (item instanceof InterfaceFloating && action == InputEventType.MOUSE_PRESS) {
                        InterfaceFloating float_item = (InterfaceFloating) item;
                        if (float_item.isOutsideClickClosable()) {
                            if (item instanceof ContextMenu) {
                                ContextMenu to_close = (ContextMenu) item;
                                if (to_close.closeDependencies(margs)) {
                                    float_item.hide();
                                }
                            } else {
                                float_item.hide();
                            }
                        }
                    }
                }
            }
        }

        if (queue.size() > 0) {
            if (hoveredItem != null && hoveredItem != queue.get(queue.size() - 1))
                manager.assignActionsForSender(InputEventType.MOUSE_LEAVE, margs, hoveredItem, underFocusedItems,
                        false);

            hoveredItem = queue.get(queue.size() - 1);
            hoveredItem.setMouseHover(true);
            glfwSetCursor(handler.getWindowId(), hoveredItem.getCursor().getCursor());

            if (window.isBorderHidden && window.isResizable && !window.isMaximized) {
                int handlerContainerWidth = rootContainer.getWidth();
                int handlerContainerHeight = rootContainer.getHeight();

                boolean cursorNearLeftTop = (xpos <= SpaceVILConstants.borderCursorTolerance
                        && ypos <= SpaceVILConstants.borderCursorTolerance);
                boolean cursorNearLeftBottom = (ypos >= handlerContainerHeight - SpaceVILConstants.borderCursorTolerance
                        && xpos <= SpaceVILConstants.borderCursorTolerance);
                boolean cursorNearRightTop = (xpos >= handlerContainerWidth - SpaceVILConstants.borderCursorTolerance
                        && ypos <= SpaceVILConstants.borderCursorTolerance);
                boolean cursorNearRightBottom = (xpos >= handlerContainerWidth - SpaceVILConstants.borderCursorTolerance
                        && ypos >= handlerContainerHeight - SpaceVILConstants.borderCursorTolerance);

                if (cursorNearRightTop || cursorNearRightBottom || cursorNearLeftBottom || cursorNearLeftTop) {
                    handler.setCursorType(GLFW_CROSSHAIR_CURSOR);
                } else {
                    if (xpos > handlerContainerWidth - SpaceVILConstants.borderCursorTolerance
                            || xpos <= SpaceVILConstants.borderCursorTolerance)
                        handler.setCursorType(GLFW_HRESIZE_CURSOR);

                    if (ypos > handlerContainerHeight - SpaceVILConstants.borderCursorTolerance
                            || ypos <= SpaceVILConstants.borderCursorTolerance)
                        handler.setCursorType(GLFW_VRESIZE_CURSOR);
                }
            }

            underHoveredItems = queue;
            Deque<Prototype> tmp = new ArrayDeque<>(underHoveredItems);
            while (!tmp.isEmpty()) {
                Prototype item = tmp.pollLast();
                if (item.equals(hoveredItem) && hoveredItem.isDisabled())
                    continue;
                item.setMouseHover(true);
                if (!item.isPassEvents(InputEventType.MOUSE_HOVER))
                    break;
            }
            manager.assignActionsForHoveredItem(InputEventType.MOUSE_HOVER, margs, hoveredItem, underHoveredItems,
                    false);
            return true;
        } else
            return false;
    }

    private List<InterfaceBaseItem> getInnerItems(Prototype root) {
        List<InterfaceBaseItem> list = new LinkedList<InterfaceBaseItem>();
        List<InterfaceBaseItem> root_items = root.getItems();

        for (InterfaceBaseItem item : root_items) {
            if (!item.isVisible() || !item.isDrawable())
                continue;
            if (item instanceof Prototype) {
                Prototype leaf = (Prototype) item;
                if (leaf.isDisabled())
                    continue;
                list.add(item);
                list.addAll(getInnerItems(leaf));
            }
        }
        return list;
    }

    <T> Prototype isInListHoveredItems(Class<T> type) {
        Prototype wanted = null;
        List<Prototype> list = new LinkedList<Prototype>(underHoveredItems);
        for (Prototype item : list) {
            try {
                boolean found = type.isInstance(item);
                if (found) {
                    wanted = item;
                    if (wanted instanceof InterfaceFloating)
                        return wanted;
                }
            } catch (ClassCastException cce) {
                continue;
            }
        }
        return wanted;
    }

    void findUnderFocusedItems(Prototype item) {
        Deque<Prototype> queue = new ArrayDeque<>();
        if (item == rootContainer) {
            underFocusedItems = null;
            return;
        }
        Prototype parent = item.getParent();
        while (parent != null) {
            queue.addFirst(parent);
            parent = parent.getParent();
        }
        underFocusedItems = new LinkedList<Prototype>(queue);
        underFocusedItems.remove(focusedItem);
    }

    void setFocusedItem(Prototype item) {
        if (item == null) {
            focusedItem = null;
            return;
        }
        if (focusedItem != null) {
            if (focusedItem.equals(item))
                return;
            focusedItem.setFocused(false);
        }
        focusedItem = item;
        focusedItem.setFocused(true);
        findUnderFocusedItems(item);
    }

    void resetFocus() {
        if (focusedItem != null)
            focusedItem.setFocused(false);
        focusedItem = rootContainer;
        focusedItem.setFocused(true);
        if (underFocusedItems != null)
            underFocusedItems.clear();
    }

    void resetItems() {
        resetFocus();
        if (hoveredItem != null)
            hoveredItem.setMouseHover(false);
        hoveredItem = null;
        underHoveredItems.clear();
    }

    static String getResourceString(String resource) {
        StringBuilder result = new StringBuilder();
        InputStream inputStream = CommonProcessor.class.getResourceAsStream(resource);
        try (BufferedReader scanner = new BufferedReader(new InputStreamReader(inputStream))) {
            String line;
            while ((line = scanner.readLine()) != null) {
                result.append(line).append("\n");
            }

        } catch (IOException e) {
            e.printStackTrace();
        }
        return result.toString();
    }
}