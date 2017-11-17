namespace D3DLab.Core.Input.EventArgs {
    public class CancelSwitchToStateEventArgs : System.EventArgs {
        public CancelSwitchToStateEventArgs(object fromState, object toState) {
            FromState = fromState;
            ToState = toState;
        }
        public object FromState { get; private set; }
        public object ToState { get; private set; }
        public bool Cancel { get; set; }
    }
}
