using System;
namespace SpaceVIL
{
    /// <summary>
    /// Input mode options.
    /// </summary>
    /// <seealso cref="SetInputMode"/>
    public enum InputMode
    {
        Cursor = 0x00033001,
        StickyKeys = 0x00033002,
        StickyMouseButton = 0x00033003
    }

    /// <summary>
    /// Key and button actions.
    /// </summary>
    public enum InputState
    {
        /// <summary>
        /// The key or mouse button was released.
        /// </summary>
        Release = 0,

        /// <summary>
        /// The key or mouse button was pressed.
        /// </summary>
        Press = 1,

        /// <summary>
        /// The key was held down until it repeated.
        /// </summary>
        Repeat = 2
    }

    /// <summary>
    /// Joysticks.
    /// </summary>
    public enum Joystick
    {
        Joystick1 = 0,
        Joystick2 = 1,
        Joystick3 = 2,
        Joystick4 = 3,
        Joystick5 = 4,
        Joystick6 = 5,
        Joystick7 = 6,
        Joystick8 = 7,
        Joystick9 = 8,
        Joystick10 = 9,
        Joystick11 = 10,
        Joystick12 = 11,
        Joystick13 = 12,
        Joystick14 = 13,
        Joystick15 = 14,
        Joystick16 = 15,
        JoystickLast = Joystick16
    }

    /// <summary>
    /// <para>Keyboard keys.</para>
    /// 
    /// <para>These key codes are inspired by the USB HID Usage Tables v1.12 (p. 53-60), but
    /// re-arranged to map to 7-bit ASCII for printable keys(function keys are put in the 256+
    /// range).</para>
    /// </summary>
    public enum KeyCode
    {
        Unknown = -1,

        // Printable keys
        Space = 32,
        Apostrophe = 39,  // '
        Comma = 44,  // ,
        Minus = 45,  // -
        Period = 46,  // .
        Slash = 47,  // /
        Alpha0 = 48,
        Alpha1 = 49,
        Alpha2 = 50,
        Alpha3 = 51,
        Alpha4 = 52,
        Alpha5 = 53,
        Alpha6 = 54,
        Alpha7 = 55,
        Alpha8 = 56,
        Alpha9 = 57,
        SemiColon = 59,  // ;
        Equal = 61,  // =
        A = 65,
        B = 66,
        C = 67,
        D = 68,
        E = 69,
        F = 70,
        G = 71,
        H = 72,
        I = 73,
        J = 74,
        K = 75,
        L = 76,
        M = 77,
        N = 78,
        O = 79,
        P = 80,
        Q = 81,
        R = 82,
        S = 83,
        T = 84,
        U = 85,
        V = 86,
        W = 87,
        X = 88,
        Y = 89,
        Z = 90,

        a = 97,
        b = 98,
        c = 99,
        d = 100,
        e = 101,
        f = 102,
        g = 103,
        h = 104,
        i = 105,
        j = 106,
        k = 107,
        l = 108,
        m = 109,
        n = 110,
        o = 111,
        p = 112,
        q = 113,
        r = 114,
        s = 115,
        t = 116,
        u = 117,
        v = 118,
        w = 119,
        x = 120,
        y = 121,
        z = 122,
        
        LeftBracket = 91,  // [
        Backslash = 92,  // \
        RightBracket = 93,  // ]
        GraveAccent = 96,  // `
        World1 = 161, // Non-US #1
        World2 = 162, // Non-US #2

        // Function keys
        Escape = 256,
        Enter = 257,
        Tab = 258,
        Backspace = 259,
        Insert = 260,
        Delete = 261,
        Right = 262,
        Left = 263,
        Down = 264,
        Up = 265,
        PageUp = 266,
        PageDown = 267,
        Home = 268,
        End = 269,
        CapsLock = 280,
        ScrollLock = 281,
        NumLock = 282,
        PrintScreen = 283,
        Pause = 284,
        F1 = 290,
        F2 = 291,
        F3 = 292,
        F4 = 293,
        F5 = 294,
        F6 = 295,
        F7 = 296,
        F8 = 297,
        F9 = 298,
        F10 = 299,
        F11 = 300,
        F12 = 301,
        F13 = 302,
        F14 = 303,
        F15 = 304,
        F16 = 305,
        F17 = 306,
        F18 = 307,
        F19 = 308,
        F20 = 309,
        F21 = 310,
        F22 = 311,
        F23 = 312,
        F24 = 313,
        F25 = 314,
        Numpad0 = 320,
        Numpad1 = 321,
        Numpad2 = 322,
        Numpad3 = 323,
        Numpad4 = 324,
        Numpad5 = 325,
        Numpad6 = 326,
        Numpad7 = 327,
        Numpad8 = 328,
        Numpad9 = 329,
        NumpadDecimal = 330,
        NumpadDivide = 331,
        NumpadMultiply = 332,
        NumpadSubtract = 333,
        NumpadAdd = 334,
        NumpadEnter = 335,
        NumpadEqual = 336,
        LeftShift = 340,
        LeftControl = 341,
        LeftAlt = 342,
        LeftSuper = 343,
        RightShift = 344,
        RightControl = 345,
        RightAlt = 346,
        RightSuper = 347,
        Menu = 348
    }

    /// <summary>
    /// Modifier key flags.
    /// </summary>
    [Flags]
    public enum KeyMods
    {
        /// <summary>
        /// If this bit is set one or more Shift keys were held down.
        /// </summary>
        Shift = 0x0001,

        /// <summary>
        /// If this bit is set one or more Control keys were held down.
        /// </summary>
        Control = 0x0002,

        /// <summary>
        /// If this bit is set one or more Alt keys were held down.
        /// </summary>
        Alt = 0x0004,

        /// <summary>
        /// If this bit is set one or more Super keys were held down.
        /// </summary>
        Super = 0x0008
    }

    /// <summary>
    /// Mouse buttons.
    /// </summary>
    public enum MouseButton
    {
        Button1 = 0,
        Button2 = 1,
        Button3 = 2,
        Button4 = 3,
        Button5 = 4,
        Button6 = 5,
        Button7 = 6,
        Button8 = 7,
        ButtonLast = Button8,
        ButtonLeft = Button1,
        ButtonRight = Button2,
        ButtonMiddle = Button3
    }
}