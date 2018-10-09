using D3DLab.Debugger.IDE;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Shaders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace D3DLab.Debugger.Windows {
    public abstract class BaseCommand : ICommand {
        public event EventHandler CanExecuteChanged = (x, o) => { };
        public bool CanExecute(object parameter) { return true; }

        public abstract void Execute(object parameter);
    }
    public abstract class BaseCommand<T> : ICommand where T : class {
        public event EventHandler CanExecuteChanged = (x, o) => { };
        public bool CanExecute(object parameter) { return true; }

        public void Execute(object parameter) {
            Execute(parameter as T);
        }
        public abstract void Execute(T parameter);
    }

    public sealed class ShaderEditorPopup {
        private ShaderEditor win;
        public ShaderEditorViewModel ViewModel { get; }

        public ShaderEditorPopup() {
            win = new ShaderEditor();
            ViewModel = (ShaderEditorViewModel)win.DataContext;
        }
        public void Show() {
            win.Show();
        }
    }

    public class EditorHistory {
        public void Undo() {

        }
        public void Redo() {

        }

        public void Clear() {
        }
    }

    public class ShaderSaveCommand : ICommand {
        private ShaderEditorViewModel shaderEditorViewModel;

        public ShaderSaveCommand(ShaderEditorViewModel shaderEditorViewModel) {
            this.shaderEditorViewModel = shaderEditorViewModel;
        }

        public event EventHandler CanExecuteChanged = (x, o) => { };
        public bool CanExecute(object parameter) { return true; }
        public void Execute(object parameter) {
            shaderEditorViewModel.Save();
        }
    }
   
    public class ShaderTabEditor : INotifyPropertyChanged {

        public class EditorWordSelectedCommand : BaseCommand<TextPoiterSelectionChanged> {
            readonly ShaderDevelopmentEnvironment environment;

            string prev;
            public EditorWordSelectedCommand(ShaderDevelopmentEnvironment environment) {
                this.environment = environment;
            }
            public override void Execute(TextPoiterSelectionChanged selection) {
                var world = selection.CaretPointer.GetVariableName();
              
                environment.HighlightRelations(world, selection.GetRange());

                //if (!string.IsNullOrWhiteSpace(prev)) {
                //    words[prev].ForEach(x => x.Background = Brushes.Transparent);
                //    prev = null;
                //}
                //if (words.TryGetValue(world, out var item)) {
                //    item.ForEach(x => x.Background = Brushes.LightBlue);
                //    prev = world;
                //}
            }
        }

        public class IntellisenseInvokedCommand : BaseCommand {
            readonly ShaderDevelopmentEnvironment environment;

            public IntellisenseInvokedCommand(ShaderDevelopmentEnvironment environment) {
                this.environment = environment;
            }

            public override void Execute(object parameter) {
                var presenter = (IntellisensePopupPresenter)parameter;

                var name = presenter.TargetVariable.StartPointer.GetVariableName();

                var result = environment.IntelliSense(name, presenter.TargetVariable.GetRange());

                if (!result.Any()) {
                    environment.UnHighlightAll();
                    return;
                }

                presenter.AddRange(result);

                presenter.Show();
            }
        }

        public string Header { get { return Info.Stage; } }
        public FlowDocument ShaderDocument { get { return environment.ShaderDocument; } }
        public ICommand WordSelected { get; set; }
        public ICommand IntellisenseInvoked { get; set; }

        public IShaderInfo Info { get; }

        readonly ShaderDevelopmentEnvironment environment;
        public ShaderTabEditor(IShaderInfo info) {
            this.Info = info;
            environment = new ShaderDevelopmentEnvironment();
            IntellisenseInvoked = new IntellisenseInvokedCommand(environment);
            WordSelected = new EditorWordSelectedCommand(environment);
        }
        public void LoadShaderAsync() {
            var text = Info.ReadText();

            environment.Read(text);

            OnPropertyChanged(nameof(ShaderDocument));

            return;

            //var parser = new ShaderParser(text);

            //parser.Analyze();

            //ShaderDocument = parser.Document;
            

            //Interpreter = parser.Interpreter;

            //WordSelected = new EditorWordSelectedCommand(words, Interpreter);
        }

        //private void Fill(string txt, Tokens token) {
        //    if (!words.TryGetValue(txt, out var item)) {
        //        words.Add(txt, new List<Run>());
        //    }

        //    Run run = new Run(txt);
        //    //switch (token) {
        //    //    case Tokens.Operator:
        //    //        run.Foreground = Brushes.Blue;
        //    //        break;
        //    //    case Tokens.Comments:
        //    //        run.Foreground = Brushes.Green;
        //    //        break;
        //    //    default:
        //    //        break;
        //    //}
        //   // par.Inlines.Add(run);
        //    words[txt].Add(run);
        //}

        public event PropertyChangedEventHandler PropertyChanged = (x, y) => { };
        private void OnPropertyChanged(string name) {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }

    public sealed class ShaderEditorViewModel {
        public ICollectionView Tabs { get; }
        private readonly ObservableCollection<ShaderTabEditor> tabs;
        public ICommand SaveCommand { get; }
        public ObservableCollection<string> Errors { get; set; }


        readonly EditorHistory history;
        IShaderEditingComponent current;
        IShaderCompilator compilator;

        public ShaderEditorViewModel() {
            tabs = new ObservableCollection<ShaderTabEditor>();
            Tabs = CollectionViewSource.GetDefaultView(tabs);
            history = new EditorHistory();
            SaveCommand = new ShaderSaveCommand(this);
            Errors = new ObservableCollection<string>();
        }

        public void LoadShader(IShaderEditingComponent com) {
            current = com;
            compilator = com.GetCompilator();
            history.Clear();

            foreach (var sh in compilator.Infos) {
                var tab = new ShaderTabEditor(sh);
                tabs.Add(tab);
                tab.LoadShaderAsync();
            }
            Tabs.MoveCurrentToFirst();

        }

        private ShaderTabEditor GetSelected() {
            return (ShaderTabEditor)Tabs.CurrentItem;
        }

        public void Save() {
            var selected = GetSelected();
            try {
                var shaderDocument = selected.ShaderDocument;
                string text = new TextRange(shaderDocument.ContentStart, shaderDocument.ContentEnd).Text;

                compilator.Compile(selected.Info, text);
                current.ReLoad();
                Errors.Insert(0, $"Compile: {selected.Info.Stage} succeeded");
            } catch (Exception ex) {
                foreach(var line in Regex.Split(ex.Message, Environment.NewLine)){
                    Errors.Insert(0,line);
                }
            }
        }
    }
}
