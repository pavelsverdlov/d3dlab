using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Shaders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace D3DLab.Debugger.Windows {
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

    public class ShaderParser {
        public enum Tokens {
            None,
            Break,
            NewLine,
            Operator,
            Comments,
        }
        readonly string text;

        public ShaderParser(string text) {
            this.text = text;
        }

        public void Analyze(Action<string,Tokens> analyzed) {
            var str = new StringBuilder();
            var token = Tokens.None;
            foreach (char ch in text) {                
                switch (ch) {
                    case '\t':
                    case ' ':
                    case ';':
                    case ':':
                    case '=':
                    case '\r':
                    case '\n':
                    case '{':
                    case '}':                        
                    case ')':                        
                    case '(':                        
                    case '.':
                    case ',':
                    case '[':
                    case ']':
                    case '>':
                    case '<':
                        if (str.Length > 0 && token != Tokens.Break) {
                            analyzed(str.ToString(), token);
                            str.Clear();
                        }
                        token = Tokens.Break;
                        //str.Append(ch);
                        //continue;
                        break;
                    default:
                        switch (token) {
                            case Tokens.Break:
                                analyzed(str.ToString(), token);
                                str.Clear();
                                token = Tokens.None;
                                break;
                        }
                        break;
                }
                str.Append(ch);
                switch (str.ToString()) {
                    case "struct":
                    case "cbuffer":
                    case "float2":
                    case "float3":
                    case "float4":
                    case "int":
                    //
                    case "if":
                    case "return":
                    case "register":
                    case "void":
                    //
                    case "triangle":
                    case "line":
                    case "TriangleStream":

                        //case "in":
                        token = Tokens.Operator;
                        analyzed(str.ToString(), token);
                        str.Clear();
                        token = Tokens.None;
                        break;
                    case "\r\n":
                        if (token == Tokens.Comments) {
                            analyzed(str.ToString(), token);
                            str.Clear();
                            token = Tokens.None;
                        } else {
                            token = Tokens.NewLine;
                            analyzed(str.ToString(), token);
                            str.Clear();
                        }
                        break;
                    case "//":
                        token = Tokens.Comments;
                        break;
                }
            }
            analyzed(str.ToString(), token);
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
        public class EditorWordSelectedCommand : ICommand {
            readonly Dictionary<string, List<Run>> words;
            string prev;
            public EditorWordSelectedCommand(Dictionary<string, List<Run>> words) {
                this.words = words;
            }
            public event EventHandler CanExecuteChanged = (x, o) => { };
            public bool CanExecute(object parameter) { return true; }
            public void Execute(object parameter) {
                var world = parameter.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(prev)) {
                    words[prev].ForEach(x => x.Background = Brushes.Transparent);
                    prev = null;
                }
                if (words.TryGetValue(world, out var item)) {
                    item.ForEach(x => x.Background = Brushes.LightBlue);
                    prev = world;
                }
            }
        }

        public string Header { get { return Info.Stage; } }
        public FlowDocument ShaderDocument { get; set; }
        public ICommand WordSelected { get; }

        public ShaderInfo Info { get; }

        private readonly Dictionary<string, List<Run>> words;

        public ShaderTabEditor(ShaderInfo info) {
            this.Info = info;
            ShaderDocument = new FlowDocument();
            words = new Dictionary<string, List<Run>>();
            WordSelected = new EditorWordSelectedCommand(words);            
        }
        public void LoadShaderAsync() {
            var text = System.IO.File.ReadAllText(Info.Path);

            var parser = new ShaderParser(text);

            var par = new Paragraph();

            parser.Analyze((txt, token) => {
                if(!words.TryGetValue(txt, out var item)) {
                    words.Add(txt, new List<Run>());
                }

                Run run = new Run(txt);
                switch (token) {
                    case ShaderParser.Tokens.Operator:
                        run.Foreground = Brushes.Blue;
                        break;
                    case ShaderParser.Tokens.Comments:
                        run.Foreground = Brushes.Green;
                        break;
                    default:
                        break;
                }
                par.Inlines.Add(run);
                words[txt].Add(run);
            });

            //foreach (var word in text.Split(' ')) {
            //    var run = new Run(word + " ");
            //    switch (word) {
            //        case "struct":
            //        case "cbuffer":
            //        case "register":
            //        case "float3":
            //        case "float2":
            //        case "float4":
            //        case "float4x4":
            //        case "return":
            //            run.Foreground = Brushes.Blue;
            //            break;
            //        case "mul":
            //            run.Foreground = Brushes.Green;
            //            break;
            //        default:
            //            break;
            //    }
            //    if (Info.EntryPoint + "(" == word) {
            //        run.Foreground = Brushes.Green;
            //    }
            //    par.Inlines.Add(run);
            //}
            ShaderDocument.Blocks.Add(par);
            OnPropertyChanged(nameof(ShaderDocument));
        }


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
