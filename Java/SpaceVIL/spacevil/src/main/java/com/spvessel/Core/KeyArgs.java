package com.spvessel.Core;

import com.spvessel.Flags.InputState;
import com.spvessel.Flags.KeyCode;
import com.spvessel.Flags.KeyMods;

public final class KeyArgs extends InputEventArgs
{
    public KeyCode key;
    public int scancode;
    public InputState state;
    public KeyMods mods;

    @Override
    public void clear() {
        key = KeyCode.UNKNOWN;
        scancode = -1;
        state = InputState.RELEASE;
        mods = KeyMods.NO;
    }
}