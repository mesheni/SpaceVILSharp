using System;
using System.Drawing;
using System.Collections.Generic;
using SpaceVIL.Core;
using SpaceVIL.Common;
using SpaceVIL.Decorations;

namespace SpaceVIL
{
    public class ComboBoxDropDown : Prototype, IFloating
    {
        public EventCommonMethod SelectionChanged;
        public Prototype ReturnFocus = null;
        public ListBox ItemList = new ListBox();
        private String _text_selection = String.Empty;
        public String GetText()
        {
            return _text_selection;
        }
        public int GetCurrentIndex()
        {
            return ItemList.GetSelection();
        }
        public void SetCurrentIndex(int index)
        {
            if (!_init)
                InitElements();

            ItemList.SetSelection(index);
            MenuItem selection = ItemList.GetSelectionItem() as MenuItem;
            if (selection != null)
                _text_selection = selection.GetText();
        }
        private Queue<IBaseItem> _queue = new Queue<IBaseItem>();

        private static int count = 0;
        public MouseButton ActiveButton = MouseButton.ButtonLeft;

        private bool _init = false;
        private bool _ouside = true;

        /// <summary>
        /// Close the ComboBoxDropDown it mouse click is outside (true or false)
        /// </summary>
        public bool IsOutsideClickClosable()
        {
            return _ouside;
        }
        public void SetOutsideClickClosable(bool value)
        {
            _ouside = value;
        }
        // private bool _lock_ouside = true;
        // public bool IsLockOutside()
        // {
        //     return _lock_ouside;
        // }
        // public void SetLockOutside(bool value)
        // {
        //     _lock_ouside = value;
        // }

        /// <summary>
        /// Constructs a ComboBoxDropDown
        /// </summary>
        /// <param name="handler"> parent window for the ComboBoxDropDown </param>
        public ComboBoxDropDown(WindowLayout handler)
        {
            SetPassEvents(false);
            SetVisible(false);
            SetHandler(handler);
            SetItemName("ComboBoxDropDown_" + count);
            count++;
            ItemsLayoutBox.AddItem(GetHandler(), this, LayoutType.Floating);
            SetStyle(DefaultsService.GetDefaultStyle(typeof(SpaceVIL.ComboBoxDropDown)));
            SetShadow(5, 3, 3, Color.FromArgb(180, 0, 0, 0));
        }

        /// <summary>
        /// Initialization and adding of all elements in the ComboBoxDropDown
        /// </summary>
        public override void InitElements()
        {
            SetConfines();
            ItemList.SetVScrollBarVisible(ScrollBarVisibility.AsNeeded);
            ItemList.SetHScrollBarVisible(ScrollBarVisibility.AsNeeded);
            ItemList.GetArea().SelectionChanged += OnSelectionChanged;

            base.AddItem(ItemList);

            foreach (var item in _queue)
                ItemList.AddItem(item);
            _queue = null;
            _init = true;

            ItemList.GetArea().EventKeyPress += (sender, args) =>
            {
                if (args.Key == KeyCode.Escape)
                    Hide();
            };
        }

        void OnSelectionChanged()
        {
            MenuItem selection = ItemList.GetSelectionItem() as MenuItem;
            if (selection != null)
                _text_selection = selection.GetText();
            Hide();
            SelectionChanged?.Invoke();
        }

        /// <summary>
        /// Returns count of the ComboBoxDropDown lines
        /// </summary>
        public int GetListCount()
        {
            return ItemList.GetListContent().Count;
        }

        /// <summary>
        /// Returns ComboBoxDropDown items list
        /// </summary>
        public List<IBaseItem> GetListContent()
        {
            return ItemList.GetListContent();
        }

        /// <summary>
        /// Add item to the ComboBoxDropDown
        /// </summary>
        public override void AddItem(IBaseItem item)
        {
            _queue.Enqueue(item);
        }

        /// <summary>
        /// Remove item from the ComboBoxDropDown
        /// </summary>
        public override void RemoveItem(IBaseItem item)
        {
            ItemList.RemoveItem(item);
        }

        void UpdateSize()
        {
            int height = 0;
            int width = GetWidth();
            List<IBaseItem> list = ItemList.GetListContent();
            foreach (var item in list)
            {
                height += (item.GetHeight() + ItemList.GetArea().GetSpacing().Vertical);

                int tmp = GetPadding().Left + GetPadding().Right + item.GetMargin().Left + item.GetMargin().Right;

                MenuItem m = item as MenuItem;
                if (item != null)
                {
                    tmp += m.GetTextWidth() + m.GetMargin().Left + m.GetMargin().Right + m.GetPadding().Left + m.GetPadding().Right;
                }
                else
                    tmp = tmp + item.GetWidth() + item.GetMargin().Left + item.GetMargin().Right;

                if (width < tmp)
                    width = tmp;
            }
            // SetSize(width, height);
            SetWidth(width);
            SetHeight(height);
        }

        /// <summary>
        /// Show the ComboBoxDropDown
        /// </summary>
        /// <param name="sender"> the item from which the show request is sent </param>
        /// <param name="args"> mouse click arguments (cursor position, mouse button,
        /// mouse button press/release, etc.) </param>
        public void Show(IItem sender, MouseArgs args)
        {
            if (args.Button == ActiveButton)
            {
                if (!_init)
                    InitElements();

                SetVisible(true);
                SetConfines();
                ItemList.GetArea().SetFocus();
            }
        }

        /// <summary>
        /// Hide the ComboBoxDropDown without destroying
        /// </summary>
        public void Hide()
        {
            SetX(-GetWidth());
            SetVisible(false);
            // ItemList.Unselect();
            ReturnFocus?.SetFocus();
        }

        /// <summary>
        /// Set confines according to position and size of the ComboBoxDropDown
        /// </summary>
        public override void SetConfines()
        {
            base.SetConfines(
                GetX(),
                GetX() + GetWidth(),
                GetY(),
                GetY() + GetHeight()
            );
        }

        //style
        /// <summary>
        /// Set style of the ComboBoxDropDown
        /// </summary>
        public override void SetStyle(Style style)
        {
            if (style == null)
                return;
            SetPadding(style.Padding);
            SetSizePolicy(style.WidthPolicy, style.HeightPolicy);
            SetBackground(style.Background);

            Style inner_style = style.GetInnerStyle("itemlist");
            if (inner_style != null)
            {
                ItemList.SetStyle(inner_style);
            }
            inner_style = style.GetInnerStyle("listarea");
            if (inner_style != null)
            {
                ItemList.GetArea().SetStyle(inner_style);
            }
        }
    }
}