namespace D3DLab.Core.Input.EventArgs {
    public class GetCustomRotateMarginsEventArgs : System.EventArgs {
        public GetCustomRotateMarginsEventArgs(System.Windows.Thickness rotateMargins) {
            RotateMargins = rotateMargins;
        }
        public System.Windows.Thickness RotateMargins { get; set; }
    }
}
