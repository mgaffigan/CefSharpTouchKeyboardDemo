# Example of using `CefSharp.Wpf.HwndHost` with w10 touch keyboard
General considerations:
1. Only applies/tested with Windows 10 Enterprise 20H2 (10.0.19042) and higher.  Not tested on W11
2. No hardware keyboard should be present (or GPIO should indicate that the keyboard is disabled - ex: Lenovo Yoga in tablet mode)
3. System must be configured to use touch keyboard in desktop mode (unclear on requirement)
4. Behavior varies based on touch input device and system state when the app is started
5. Behavior varies based on `BorderStyle`

### Things that don't work
- `disable-usb-keyboard-detect` does not by itself produce reliable SIP activation
- `CefSharp.Wpf.HwndHost` by itself does not produce reliable SIP activation, but does sometimes work and does produce correct SIP type for numeric et al.
- `CefSharp.Wpf` does not produce any SIP activation (in box), and produces GPU artifacts and performance issues on many machines
- `CefSharp.Wpf` + [Touch keyboard sample](https://github.com/cefsharp/CefSharp/tree/master/CefSharp.Wpf.Example/Controls/TouchKeyboard) does produce reliable SIP activation, but does not control SIP keyboard type (e.g.: numeric pad not shown on `<input type="number"`) and still suffers from GPU and performance issues
- This solution without debouncing/200ms delay does not produce reliable activation

### Overview of solution
1. Listen to focus changes (`FocusedNodeChangedEnabled` + `OnFocusedNodeChanged`)
2. Based on the focused element, decide whether the SIP should be shown.  Example considers only `<input>` elements to be applicable, which is likely oversimplified.
3. Reactive `.Throttle` which is poorly named and actually represents a debouncing is used to discard transitory states
4. Call `TryShow` or `TryHide` as appropriate, lazy creating `IInputPane2` for the top-level window