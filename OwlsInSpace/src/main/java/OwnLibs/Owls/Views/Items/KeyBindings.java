package OwnLibs.Owls.Views.Items;

import java.awt.Color;

import com.spvessel.spacevil.ButtonCore;
import com.spvessel.spacevil.CoreWindow;
import com.spvessel.spacevil.Frame;
import com.spvessel.spacevil.GraphicsMathService;
import com.spvessel.spacevil.HorizontalStack;
import com.spvessel.spacevil.ItemsLayoutBox;
import com.spvessel.spacevil.Label;
import com.spvessel.spacevil.Prototype;
import com.spvessel.spacevil.Core.InterfaceFloating;
import com.spvessel.spacevil.Core.InterfaceItem;
import com.spvessel.spacevil.Core.MouseArgs;
import com.spvessel.spacevil.Decorations.CustomFigure;
import com.spvessel.spacevil.Decorations.ItemState;
import com.spvessel.spacevil.Flags.ItemAlignment;
import com.spvessel.spacevil.Flags.ItemStateType;
import com.spvessel.spacevil.Flags.KeyCode;
import com.spvessel.spacevil.Flags.LayoutType;
import com.spvessel.spacevil.Flags.SizePolicy;

public class KeyBindings extends Prototype implements InterfaceFloating {

    public KeyBindings(CoreWindow coreWindow) {
        window = new Frame();
        closeBtn = new ButtonCore();
        // IFloating
        ItemsLayoutBox.addItem(coreWindow, this, LayoutType.FLOATING);
        // Event parameters
        setPassEvents(false);

        setSizePolicy(SizePolicy.EXPAND, SizePolicy.EXPAND);
        setBackground(0, 0, 0, 180);
        setVisible(false);

    }

    private Frame window;
    private ButtonCore closeBtn;
    private boolean isInit = false;

    @Override
    public void initElements() {
        addItem(window);
        window.setAlignment(ItemAlignment.HCENTER, ItemAlignment.VCENTER);
        window.setSize(500, 350);
        window.setSizePolicy(SizePolicy.FIXED, SizePolicy.FIXED);
        window.setBackground(50, 50, 50);
        window.setShadow(5, 2, 2, new Color(0, 0, 0));

        closeBtn.setBackground(100, 100, 100);
        closeBtn.setSize(12, 12);
        closeBtn.setAlignment(ItemAlignment.RIGHT, ItemAlignment.TOP);
        closeBtn.setMargin(0, 5, 5, 0);
        closeBtn.setCustomFigure(new CustomFigure(false, GraphicsMathService.getCross(16, 16, 2, 45)));
        closeBtn.addItemState(ItemStateType.HOVERED, new ItemState(new Color(186, 95, 97)));

        HorizontalStack layout = new HorizontalStack();
        layout.setMargin(20, 20, 20, 20);
        layout.setSpacing(15, 10);
        layout.setContentAlignment(ItemAlignment.HCENTER);

        Label key_bindings = getShortcutLabel("CTRL + SHIFT + R\n" + "CTRL + SHIFT + E\n" + "CTRL + SHIFT + S\n"
                + "CTRL + S\n" + "CTRL + E\n" + "F11\n\n" + "CTRL + SHIFT + N\n" + "CTRL + N\n" + "CTRL + K\n"
                + "CTRL + F\n" + "CTRL + I\n\n" + "CTRL + H\n" + "ESCAPE", ItemAlignment.RIGHT, ItemAlignment.VCENTER);
        key_bindings.setWidthPolicy(SizePolicy.FIXED);
        key_bindings.setWidth(150);

        Label name_bindings = getShortcutLabel(
                "Toggle to Tree Panel\n" + "Toggle to Edit Area\n" + "Save all files\n" + "Save opened file\n"
                        + "Toggle on/off Edit Mode\n" + "Toggle Fullscreen Mode\n\n" + "Create New Folder\n"
                        + "Create New File\n" + "Add keyword to file\n" + "Search files by keywords\n"
                        + "Import file\n\n" + "Show this tooltip\n" + "Close this tooltip",
                ItemAlignment.LEFT, ItemAlignment.VCENTER);
        name_bindings.setWidthPolicy(SizePolicy.FIXED);
        name_bindings.setWidth(190);

        Label divider = getShortcutLabel("-\n-\n-\n-\n-\n-\n\n-\n-\n-\n-\n-\n\n-\n-", ItemAlignment.LEFT,
                ItemAlignment.VCENTER);

        divider.setWidthPolicy(SizePolicy.FIXED);
        divider.setWidth(divider.getTextWidth() + 4);

        window.addItems(layout, closeBtn);
        layout.addItems(key_bindings, divider, name_bindings);

        eventKeyPress.add((sender, args) -> {
            if (args.key == KeyCode.ESCAPE) {
                hide();
            }
        });

        closeBtn.eventMouseClick.add((sender, args) -> {
            hide();
        });

        isInit = true;
    }

    private Label getShortcutLabel(String text, ItemAlignment... textAlignment) {
        Label label = new Label(text);
        label.setTextAlignment(textAlignment);
        label.setFontSize(14);

        return label;
    }

    @Override
    public void hide() {
        setVisible(false);
        getHandler().setFocus();
    }

    @Override
    public boolean isOutsideClickClosable() {
        return false;
    }

    @Override
    public void setOutsideClickClosable(boolean arg0) {

    }

    @Override
    public void show() {
        if (!isInit)
            initElements();
        setVisible(true);
        setFocus();
    }

    @Override
    public void show(InterfaceItem arg0, MouseArgs arg1) {
        show();
    }
}