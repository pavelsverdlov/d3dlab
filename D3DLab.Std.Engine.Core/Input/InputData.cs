using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace D3DLab.Std.Engine.Core.Input {
    [Flags]
    public enum GeneralMouseButtons {
        None = 0,
        Left = 1,
        Right = 2,
        Middle = 3,
        XButton1 = 4,
        XButton2 = 5
    }

    public struct WindowPoint {
        public readonly int X;
        public readonly int Y;
        public WindowPoint(int x, int y) {
            X = x;
            Y = y;
        }
    }

    #region input data

    public struct ButtonsState {
        public Vector2 PointDown { get; set; }
        public WindowPoint CursorPointDown { get; set; }
    }
    public class InputStateData {
        public GeneralMouseButtons Buttons { get; set; }
        public Vector2 CurrentPosition { get; set; }//=> control.PointToClient(CursorCurrentPosition).ToVector2();
        public WindowPoint CursorCurrentPosition { get; set; }
        public int Delta { get; set; }
        public Dictionary<GeneralMouseButtons, ButtonsState> ButtonsStates {
            get {
                return buttonsStates;
            }
        }

        readonly Dictionary<GeneralMouseButtons, ButtonsState> buttonsStates;
        public bool IsPressed(GeneralMouseButtons button) {
            return (Buttons & button) == button;
        }

        public InputStateData(Dictionary<GeneralMouseButtons, ButtonsState> buttonsStates) {
            this.buttonsStates = buttonsStates;
            Delta = 0;
            CurrentPosition = Vector2.Zero;
            CursorCurrentPosition = new WindowPoint();
            Buttons = GeneralMouseButtons.None;
        }

        public static InputStateData Create() {
            var buttonsStates = new Dictionary<GeneralMouseButtons, ButtonsState> {
                { GeneralMouseButtons.Right, new ButtonsState() },
                { GeneralMouseButtons.Left, new ButtonsState() },
                { GeneralMouseButtons.Middle, new ButtonsState() }
            };
            return new InputStateData(buttonsStates);
        }

        public InputStateData Clone() {
            var buttonsStates = new Dictionary<GeneralMouseButtons, ButtonsState>();
            foreach (var i in this.buttonsStates) {
                buttonsStates.Add(i.Key, i.Value);
            }
            var cloned = new InputStateData(buttonsStates);
            cloned.Buttons = Buttons;
            cloned.CurrentPosition = CurrentPosition;
            cloned.CursorCurrentPosition = CursorCurrentPosition;
            cloned.Delta = Delta;

            return cloned;
        }
    }

    #endregion
}
