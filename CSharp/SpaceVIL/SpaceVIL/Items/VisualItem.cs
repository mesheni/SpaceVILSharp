using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace SpaceVIL
{
    public delegate void EventCommonMethod();
    public delegate void EventCommonMethodState(IItem sender);
    public delegate void EventMouseMethodState(IItem sender, MouseArgs args);
    public delegate void EventKeyMethodState(IItem sender, KeyArgs args);
    public delegate void EventInputTextMethodState(IItem sender, TextInputArgs args);

    public class VisualItem : BaseItem
    {
        internal Prototype _main;
        // //common events
        // internal EventCommonMethodState EventFocusGet;
        // internal EventCommonMethodState EventFocusLost;
        // public EventCommonMethodState EventResized;
        // public EventCommonMethodState EventDestroyed;
        // //mouse input
        // public EventMouseMethodState EventMouseHover;
        // public EventMouseMethodState EventMouseClick;
        // public EventMouseMethodState EventMousePressed;
        // public EventMouseMethodState EventMouseRelease;
        // public EventMouseMethodState EventMouseDrag;
        // public EventMouseMethodState EventMouseDrop;
        // public EventMouseMethodState EventScrollUp;
        // public EventMouseMethodState EventScrollDown;
        // //keyboard input
        // public EventKeyMethodState EventKeyPress;
        // public EventKeyMethodState EventKeyRelease;
        // public EventInputTextMethodState EventTextInput;

        private String _tooltip = String.Empty;
        public String GetToolTip()
        {
            return _tooltip;
        }
        public void SetToolTip(String text)
        {
            _tooltip = text;
        }
        //container
        private Spacing _spacing = new Spacing();
        public Spacing GetSpacing()
        {
            return _spacing;
        }
        public void SetSpacing(Spacing spacing)
        {
            _spacing = spacing;
        }
        public void SetSpacing(int horizontal = 0, int vertical = 0)
        {
            _spacing.Horizontal = horizontal;
            _spacing.Vertical = vertical;
        }
        private Indents _padding = new Indents();
        public Indents GetPadding()
        {
            return _padding;
        }
        public void SetPadding(Indents padding)
        {
            _padding = padding;
        }
        public void SetPadding(int left = 0, int top = 0, int right = 0, int bottom = 0)
        {
            _padding.Left = left;
            _padding.Top = top;
            _padding.Right = right;
            _padding.Bottom = bottom;
        }
        public EventManager eventManager = null;
        private List<IBaseItem> _content = new List<IBaseItem>();
        public virtual List<IBaseItem> GetItems()
        {
            return _content;
        }
        public void SetContent(List<IBaseItem> content)
        {
            _content = content;
        }

        private void CastAndUpdate(IBaseItem item)
        {
            Prototype prototype = item as Prototype;
            if (prototype != null)
                prototype.GetCore().UpdateGeometry();
            else
                (item as BaseItem).UpdateGeometry();
        }
        public virtual void AddItem(IBaseItem item)
        {
            Monitor.Enter(GetHandler().EngineLocker);
            try
            {
                if (item.Equals(this))
                {
                    Console.WriteLine("Trying to add current item in himself.");
                    return;
                }
                item.SetHandler(GetHandler());

                AddChildren(item);

                _content.Add(item);

                ItemsLayoutBox.AddItem(GetHandler(), item, LayoutType.Static);

                //needs to force update all attributes
                CastAndUpdate(item);
                item.InitElements();

                // VisualItem vi = item as VisualItem;
                // if (vi != null)
                //     vi.UpdateState();
            }
            catch (Exception ex)
            {
                Console.WriteLine(item.GetItemName() + "\n" + ex.ToString());
            }
            finally
            {
                Monitor.Exit(GetHandler().EngineLocker);
            }
        }
        internal virtual void InsertItem(IBaseItem item, Int32 index)
        {
            Monitor.Enter(GetHandler().EngineLocker);
            try
            {
                if (item.Equals(this))
                {
                    Console.WriteLine("Trying to add current item in himself.");
                    return;
                }
                item.SetHandler(GetHandler());
                AddChildren(item);
                if (index > _content.Count)
                    _content.Add(item);
                else
                    _content.Insert(index, item);
                ItemsLayoutBox.AddItem(GetHandler(), item, LayoutType.Static);
                //needs to force update all attributes
                CastAndUpdate(item);
                item.InitElements();
                // VisualItem vi = item as VisualItem;
                // if (vi != null)
                //     vi.UpdateState();
            }
            catch (Exception ex)
            {
                Console.WriteLine(item.GetItemName() + "\n" + ex.ToString());
            }
            finally
            {
                Monitor.Exit(GetHandler().EngineLocker);
            }
        }

        internal void CascadeRemoving(IBaseItem item, LayoutType type)
        {
            VisualItem container = item as VisualItem;// предполагаю что элемент контейнер
            if (container != null)//и если это действительно контейнер
            {
                //то каждому вложенному элементу вызвать команду удалить своих вложенных элементов
                List<IBaseItem> tmp = container.GetItems();
                while (tmp.Count > 0)
                {
                    IBaseItem child = tmp.ElementAt(0);
                    container.RemoveItem(child);
                    tmp.Remove(child);
                }
            }
        }

        public virtual void RemoveItem(IBaseItem item)
        {
            Monitor.Enter(GetHandler().EngineLocker);
            try
            {
                LayoutType type;
                if (item is IFloating)
                {
                    CascadeRemoving(item, LayoutType.Floating);
                    type = LayoutType.Floating;
                }
                else
                {
                    CascadeRemoving(item, LayoutType.Static);
                    type = LayoutType.Static;
                }

                //removing
                _content.Remove(item);

                Prototype prototype = item as Prototype;
                if (prototype != null)
                    prototype.GetCore().RemoveItemFromListeners();
                else
                    (item as BaseItem).RemoveItemFromListeners();

                ItemsLayoutBox.RemoveItem(GetHandler(), item, type);
            }
            catch (Exception ex)
            {
                Console.WriteLine(item.GetItemName() + "\n" + ex.ToString());
            }
            finally
            {
                Monitor.Exit(GetHandler().EngineLocker);
            }
        }
        internal override void AddEventListener(GeometryEventType type, IBaseItem listener)
        {
            eventManager.Subscribe(type, listener);
        }
        internal override void RemoveEventListener(GeometryEventType type, IBaseItem listener)
        {
            eventManager.Unsubscribe(type, listener);
        }

        //item
        private Border _border = new Border();
        public Border GetBorder()
        {
            return _border;
        }

        public void SetBorder(Border border)
        {
            _border = border;
            GetState(ItemStateType.Base).Border = _border;
            UpdateState();
        }

        protected Dictionary<ItemStateType, ItemState> states = new Dictionary<ItemStateType, ItemState>();
        protected ItemStateType _state = ItemStateType.Base;
        internal ItemStateType GetCurrentState()
        {
            return _state;
        }
        internal void SetState(ItemStateType state)
        {
            _state = state;
            UpdateState();
        }
        public VisualItem(
        int xpos = 0,
        int ypos = 0,
        int width = 0,
        int height = 0,
        string name = "VisualItem")
        {
            /*if (GetParent().GetHandler() != null)
                SetHandler(GetParent().GetHandler());*/
            //create state
            states.Add(ItemStateType.Base, new ItemState()
            {
                Value = true,
                Background = GetBackground(),
                // Text = null,
                // ImageUri = null
            });

            //common default prop
            eventManager = new EventManager();
            SetItemName(name);
        }

        //overrides
        public override void SetWidth(int width)
        {
            int value = width - GetWidth();
            if (value != 0)
            {
                base.SetWidth(width);

                if (GetParent() != null && GetWidthPolicy() == SizePolicy.Fixed)
                {
                    var layout = GetParent() as IHLayout;
                    var grid = GetParent() as IGrid;

                    if (layout == null && grid == null)
                        UpdateBehavior();

                    if (layout != null)
                        layout.UpdateLayout();
                    if (grid != null)
                        grid.UpdateLayout();
                }
                eventManager.NotifyListeners(GeometryEventType.ResizeWidth, value);
            }
        }
        public override void SetHeight(int height)
        {
            int value = height - GetHeight();
            if (value != 0)
            {
                base.SetHeight(height);

                if (GetParent() != null && GetHeightPolicy() == SizePolicy.Fixed)
                {
                    // Console.WriteLine(GetParent());
                    var layout = GetParent() as IVLayout;
                    var grid = GetParent() as IGrid;

                    if (layout == null && grid == null)
                        UpdateBehavior();

                    if (layout != null)
                        layout.UpdateLayout();
                    if (grid != null)
                        grid.UpdateLayout();
                }
                eventManager.NotifyListeners(GeometryEventType.ResizeHeight, value);
            }
        }
        public void SetPosition(int _x, int _y)
        {
            this.SetX(_x);
            this.SetY(_y);
        }
        public override void SetX(int _x)
        {
            int value = _x - GetX();
            if (value != 0)
            {
                base.SetX(_x);
                eventManager.NotifyListeners(GeometryEventType.Moved_X, value);
            }
        }
        public override void SetY(int _y)
        {
            int value = _y - GetY();
            if (value != 0)
            {
                base.SetY(_y);
                eventManager.NotifyListeners(GeometryEventType.Moved_Y, value);
            }
        }

        // common properties
        private bool _pass_events = true;

        public bool GetPassEvents()
        {
            return _pass_events;
        }

        public void SetPassEvents(bool value)
        {
            if (_pass_events == value)
                return;
            _pass_events = value;
        }

        private bool _disabled;

        public bool IsDisabled()
        {
            return _disabled;
        }

        public void SetDisabled(bool value)
        {
            if (_disabled == value)
                return;
            _disabled = value;
            UpdateState();
        }

        private bool _hover;

        public bool IsMouseHover()
        {
            return _hover;
        }

        public void SetMouseHover(bool value)
        {
            if (_hover == value)
                return;
            _hover = value;
            UpdateState();
        }

        private bool _pressed;

        public bool IsMousePressed()
        {
            return _pressed;
        }

        public void SetMousePressed(bool value)
        {
            if (_pressed == value)
                return;
            _pressed = value;
            UpdateState();
        }

        private bool _focused;

        public bool IsFocused()
        {
            return _focused;
        }

        public void SetFocused(bool value)
        {
            if (_focused == value)
                return;
            _focused = value;
            UpdateState();
        }

        protected override void UpdateInnersDrawable(bool value)
        {
            foreach (var item in _content)
            {
                item.SetVisible(value);
            }
        }
        //common methods
        public void AddItemState(ItemStateType type, ItemState state)
        {
            if (states.ContainsKey(type))
            {
                state.Value = true;
                states[type] = state;
            }
            else
            {
                states.Add(type, state);
            }
        }
        public void RemoveItemState(ItemStateType type)
        {
            if (type == ItemStateType.Base)
                return;
            if (states.ContainsKey(type))
                states.Remove(type);
        }
        public void RemoveAllItemStates()
        {
            var itemsToRemove = states.Where(f => f.Key != ItemStateType.Base).ToArray();
            foreach (var item in itemsToRemove)
                states.Remove(item.Key);
        }
        public ItemState GetState(ItemStateType type)
        {
            if (states.ContainsKey(type))
                return states[type];
            return null;
        }
        public override void SetBackground(Color color)
        {
            GetState(ItemStateType.Base).Background = color;
            UpdateState();
        }
        public override void SetBackground(int r, int g, int b)
        {
            base.SetBackground(r, g, b);
            GetState(ItemStateType.Base).Background = GetBackground();
            UpdateState();
        }
        public override void SetBackground(int r, int g, int b, int a)
        {
            base.SetBackground(r, g, b, a);
            GetState(ItemStateType.Base).Background = GetBackground();
            UpdateState();
        }
        public override void SetBackground(float r, float g, float b)
        {
            base.SetBackground(r, g, b);
            GetState(ItemStateType.Base).Background = GetBackground();
            UpdateState();
        }
        public override void SetBackground(float r, float g, float b, float a)
        {
            base.SetBackground(r, g, b, a);
            GetState(ItemStateType.Base).Background = GetBackground();
            UpdateState();
        }

        internal virtual void UpdateState()
        {
            base.SetBackground(GetState(_state).Background);
            // GetBorder().Fill = GetState(_state).Border.Fill;
            Border br = GetState(_state).Border;
            _border = br;
            // if (br.Thickness > 0)
            // {
            //     if (!br.IsVisible)
            //         GetBorder().Radius = GetState(ItemStateType.Base).Border.Radius;
            //     else
            //         GetBorder().Radius = GetState(_state).Border.Radius;
            //     GetBorder().Thickness = GetState(_state).Border.Thickness;
            // }

            if (GetState(_state).Shape != null)
                IsCustom = GetState(_state).Shape;

            //mixing    
            if (IsDisabled() && states.ContainsKey(ItemStateType.Disabled))
            {
                base.SetBackground(GraphicsMathService.MixColors(GetState(_state).Background, GetState(ItemStateType.Disabled).Background));

                GetBorder().Fill = GraphicsMathService.MixColors(GetState(_state).Background, GetState(ItemStateType.Disabled).Border.Fill);
                br = GetState(ItemStateType.Disabled).Border;
                if (br.Thickness > 0)
                {
                    if (!br.IsVisible)
                        GetBorder().Radius = GetState(ItemStateType.Base).Border.Radius;
                    else
                        GetBorder().Radius = GetState(ItemStateType.Disabled).Border.Radius;
                    GetBorder().Thickness = GetState(ItemStateType.Disabled).Border.Thickness;
                }
                return;
            }

            if (IsFocused() && states.ContainsKey(ItemStateType.Focused))
            {
                base.SetBackground(GraphicsMathService.MixColors(GetState(_state).Background, GetState(ItemStateType.Focused).Background));

                GetBorder().Fill = GraphicsMathService.MixColors(GetState(_state).Background, GetState(ItemStateType.Focused).Border.Fill);
                br = GetState(ItemStateType.Focused).Border;
                if (br.Thickness > 0)
                {
                    if (!br.IsVisible)
                        GetBorder().Radius = GetState(ItemStateType.Base).Border.Radius;
                    else
                        GetBorder().Radius = GetState(ItemStateType.Focused).Border.Radius;
                    GetBorder().Thickness = GetState(ItemStateType.Focused).Border.Thickness;
                }
            }

            if (IsMouseHover() && states.ContainsKey(ItemStateType.Hovered))
            {
                base.SetBackground(GraphicsMathService.MixColors(GetState(_state).Background, GetState(ItemStateType.Hovered).Background));

                Border h_br = new Border();
                h_br = GetState(ItemStateType.Hovered).Border;
                h_br.Fill = GraphicsMathService.MixColors(GetState(_state).Background, GetState(ItemStateType.Hovered).Border.Fill);

                if (h_br.Thickness > 0)
                {
                    // if (!br.IsVisible)
                    //     GetBorder().Radius = GetState(ItemStateType.Base).Border.Radius;
                    // else
                    //     GetBorder().Radius = GetState(ItemStateType.Hovered).Border.Radius;
                    // GetBorder().Thickness = GetState(ItemStateType.Hovered).Border.Thickness;
                    _border = h_br;
                }
            }

            if (IsMousePressed() && states.ContainsKey(ItemStateType.Pressed))
            {
                base.SetBackground(GraphicsMathService.MixColors(GetState(_state).Background, GetState(ItemStateType.Pressed).Background));

                GetBorder().Fill = GraphicsMathService.MixColors(GetState(_state).Background, GetState(ItemStateType.Pressed).Border.Fill);
                br = GetState(ItemStateType.Pressed).Border;
                if (br.Thickness > 0)
                {
                    if (!br.IsVisible)
                        GetBorder().Radius = GetState(ItemStateType.Base).Border.Radius;
                    else
                        GetBorder().Radius = GetState(ItemStateType.Pressed).Border.Radius;
                    GetBorder().Thickness = GetState(ItemStateType.Pressed).Border.Thickness;
                }
            }
        }

        internal virtual bool GetHoverVerification(float xpos, float ypos)
        {
            switch (HoverRule)
            {
                case ItemRule.Lazy:
                    return LazyHoverVerification(xpos, ypos);
                case ItemRule.Strict:
                    return StrictHoverVerification(xpos, ypos);
                default:
                    return false;
            }
        }
        private bool StrictHoverVerification(float xpos, float ypos)
        {
            List<float[]> tmp = UpdateShape();
            if (tmp == null)
                return false;

            float Ax, Ay, Bx, By, Cx, Cy, Px, Py, m, l;
            bool result = false;

            for (int point = 0; point < tmp.Count; point += 3)
            {
                Px = xpos;
                Py = ypos;
                Ax = tmp[point][0];
                Ay = tmp[point][1];
                Bx = tmp[point + 1][0];
                By = tmp[point + 1][1];
                Cx = tmp[point + 2][0];
                Cy = tmp[point + 2][1];


                Bx = Bx - Ax; By = By - Ay;
                Cx = Cx - Ax; Cy = Cy - Ay;
                Px = Px - Ax; Py = Py - Ay;
                Ax = 0; Ay = 0;

                m = (Px * By - Bx * Py) / (Cx * By - Bx * Cy);
                if (m >= 0)
                {
                    l = (Px - m * Cx) / Bx;
                    if (l >= 0 && (m + l) <= 1)
                    {
                        result = true;
                        return result;
                    }
                }
            }

            return result;
        }
        private bool LazyHoverVerification(float xpos, float ypos)
        {
            // if (this is ContextMenu)
            // {
            //     Console.WriteLine("context menu");
            //     Console.WriteLine(
            //         _confines_x_0 + " " +
            //         _confines_x_1 + " " +
            //         _confines_y_0 + " " +
            //         _confines_y_1 + " " +
            //         GetParent().GetItemName()
            //     );
            // }

            bool result = false;
            float minx = GetX();
            float maxx = GetX() + GetWidth();
            float miny = GetY();
            float maxy = GetY() + GetHeight();

            if (_confines_x_0 > minx)
                minx = _confines_x_0;

            if (_confines_x_1 < maxx)
                maxx = _confines_x_1;

            if (_confines_y_0 > miny)
                miny = _confines_y_0;

            if (_confines_y_1 < maxy)
                maxy = _confines_y_1;

            if (xpos >= minx
                && xpos <= maxx
                && ypos >= miny
                && ypos <= maxy)
            {
                result = true;
            }
            else
            {
            }
            return result;
        }

        public CustomFigure IsCustom = null;

        public override List<float[]> MakeShape()
        {
            if (IsCustom != null)
            {
                SetTriangles(IsCustom.GetFigure());

                if (IsCustom.IsFixed())
                    return GraphicsMathService.ToGL(IsCustom.UpdatePosition(GetX(), GetY()), GetHandler());
                else
                    return GraphicsMathService.ToGL(UpdateShape(), GetHandler());
            }

            // SetTriangles(GraphicsMathService.GetRoundSquare(GetWidth(), GetHeight(), Border.Radius, GetX(), GetY()));
            SetTriangles(GraphicsMathService.GetRoundSquare(GetBorder().Radius, GetWidth(), GetHeight(), GetX(), GetY()));
            //GetState(ItemStateType.Base).Shape = new CustomFigure(true, GraphicsMathService.GetRoundSquare(GetWidth(), GetHeight(), Border.Radius, GetX(), GetY()));
            return GraphicsMathService.ToGL(this as IBaseItem, GetHandler());
        }

        //style
        internal bool _is_style_set = false;
        public override void SetStyle(Style style)
        {
            if (style == null)
                return;

            _is_style_set = true;

            SetBackground(style.Background);
            SetSizePolicy(style.WidthPolicy, style.HeightPolicy);
            SetSize(style.Width, style.Height);
            SetMinSize(style.MinWidth, style.MinHeight);
            SetMaxSize(style.MaxWidth, style.MaxHeight);
            SetAlignment(style.Alignment);
            SetPosition(style.X, style.Y);
            SetPadding(style.Padding);
            SetSpacing(style.Spacing);
            SetMargin(style.Margin);
            GetBorder().Radius = style.BorderRadius;
            GetBorder().Thickness = style.BorderThickness;
            GetBorder().Fill = style.BorderFill;
            SetVisible(style.IsVisible);
            RemoveAllItemStates();

            ItemState core_state = new ItemState(style.Background);
            core_state.Border.Radius = style.BorderRadius;
            core_state.Border.Thickness = style.BorderThickness;
            core_state.Border.Fill = style.BorderFill;

            foreach (var state in style.GetAllStates())
            {
                AddItemState(state.Key, state.Value);
            }
            if (style.Shape != null)
            {
                IsCustom = new CustomFigure(style.IsFixedShape, style.Shape);
                core_state.Shape = IsCustom;
                // GetState(ItemStateType.Base).Shape = IsCustom;
            }
            AddItemState(ItemStateType.Base, core_state);
        }
        public override Style GetCoreStyle()
        {
            Style style = new Style();
            style.SetSize(GetWidth(), GetHeight());
            style.SetSizePolicy(GetWidthPolicy(), GetHeightPolicy());
            style.Background = GetBackground();
            style.MinWidth = GetMinWidth();
            style.MinHeight = GetMinHeight();
            style.MaxWidth = GetMaxWidth();
            style.MaxHeight = GetMaxHeight();
            style.X = GetX();
            style.Y = GetY();
            style.Padding = new Indents(GetPadding().Left, GetPadding().Top, GetPadding().Right, GetPadding().Bottom);
            style.Margin = new Indents(GetMargin().Left, GetMargin().Top, GetMargin().Right, GetMargin().Bottom);
            style.Spacing = new Spacing(GetSpacing().Horizontal, GetSpacing().Vertical);
            style.Alignment = GetAlignment();
            style.BorderFill = GetBorder().Fill;
            style.BorderRadius = GetBorder().Radius;
            style.BorderThickness = GetBorder().Thickness;
            style.IsVisible = IsVisible();
            if (IsCustom != null)
            {
                style.Shape = IsCustom.GetFigure();
                style.IsFixedShape = IsCustom.IsFixed();
            }
            foreach (var state in states)
            {
                style.AddItemState(state.Key, state.Value);
            }

            return style;
        }

        public override void InitElements() { }
    }
}
