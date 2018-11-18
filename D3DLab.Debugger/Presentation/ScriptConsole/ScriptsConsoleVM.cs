using System;
using System.Numerics;
using System.Windows.Input;

namespace D3DLab.Debugger.Presentation.ScriptConsole {
    public class ScriptsConsoleVM : IPrimitiveDrawer {

        public class ScriptsViewState {
            private ScriptsConsoleVM presenter;

            public ScriptsViewState(ScriptsConsoleVM presenter) {
                this.presenter = presenter;
            }
        }

        public class ScriptsController {
            class EnterCommand : BaseWPFCommand<string> {
                ScriptsController controller;

                public EnterCommand(ScriptsController scriptsController) {
                    this.controller = scriptsController;
                }

                public override void Execute(string text) {
                    controller.OnTextChanged(text);
                }
            }

            public ICommand TextEnter { get; }

            readonly ScriptsConsoleVM presenter;
            public ScriptsController(ScriptsConsoleVM presenter) {
                TextEnter = new EnterCommand(this);
                this.presenter = presenter;
            }

            void OnTextChanged(string text) {
                presenter.ExecuteText(text);
            }
        }

        public ScriptsViewState ViewState { get; }
        public ScriptsController Controller { get; }

        readonly ScriptExetuter executer;

        public ScriptsConsoleVM() {
            ViewState = new ScriptsViewState(this);
            Controller = new ScriptsController(this);
            executer = new ScriptExetuter(this);
        }

        void ExecuteText(string text) {
            executer.Execute1(new ScriptEnvironment(), text);
        }



        public void DrawPolygon(Vector3[] points) {
            
        }

        public void DrawPoint(Vector3 point) {
            
        }
    }
}
