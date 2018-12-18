using D3DLab.Debugger;
using D3DLab.Debugger.Presentation.ScriptConsole;
using D3DLab.Debugger.Presentation.SystemList;
using D3DLab.Debugger.Windows;
using D3DLab.Parser;
using D3DLab.Plugin.Contracts.Parsers;
using D3DLab.Plugins;
using D3DLab.Std.Engine.Core;
using D3DLab.Std.Engine.Core.Ext;
using D3DLab.Visualization;
using D3DLab.Wpf.Engine.App;
using D3DLab.Wpf.Engine.App.Host;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace D3DLab {
    public interface IDropFiles {
        void Dropped(string[] files);
    }

    class TraceOutputListener : System.Diagnostics.TraceListener {
        private ObservableCollection<string> output;
        const int maxlines = 100;
        public TraceOutputListener(ObservableCollection<string> consoleOutput) {
            this.output = consoleOutput;
        }

        public override void Write(string message) {

        }

        public override void WriteLine(string message) {
            App.Current.Dispatcher.InvokeAsync(() => {
                output.Insert(0, $"[{DateTime.Now.TimeOfDay}] {message.Trim()}");
                if(output.Count > maxlines) {
                    output.RemoveAt(maxlines);
                }
            });
        }
    }

    public sealed class MainWindowViewModel : IDropFiles, IFileLoader {

        #region commands
        class ClearConsoleOutputCommand : Debugger.BaseWPFCommand {
            readonly MainWindowViewModel main;
            public ClearConsoleOutputCommand(MainWindowViewModel main) { this.main = main; }
            public override void Execute(object parameter) {
                main.ClearConsole();
            }
        }

        class ShowAxisCommand : Debugger.BaseWPFCommand {
            MainWindowViewModel main;
            CoordinateSystemLinesGameObject gameObj;

            bool isShowed;
            public ShowAxisCommand(MainWindowViewModel main) {
                this.main = main;
                isShowed = true;
            }

            public override void Execute(object parameter) {
                if (gameObj == null) {
                    gameObj = CoordinateSystemLinesGameObject.Build(main.context.GetEntityManager());
                    return;
                }
                if (isShowed) {
                    gameObj.Hide(main.context.GetEntityManager());
                } else {
                    gameObj.Show(main.context.GetEntityManager());
                }
                isShowed = !isShowed;
                main.ForceRender();
            }
        }
        class MoveToCenterWorldCommand : Debugger.BaseWPFCommand {
            private MainWindowViewModel main;

            public MoveToCenterWorldCommand(MainWindowViewModel main) {
                this.main = main;
            }

            public override void Execute(object parameter) {
                main.scene.Window.InputManager.PushCommand(new Std.Engine.Core.Input.Commands.ToCenterWorldCommand());
            }
        }
        class LoadCommand : ICommand {
            private MainWindowViewModel main;

            public LoadCommand(MainWindowViewModel mainWindowViewModel) {
                this.main = mainWindowViewModel;
            }

            public event EventHandler CanExecuteChanged = (s, r) => { };


            public bool CanExecute(object parameter) {
                return true;
            }

            public void Execute(object parameter) {
                //var bl = new EntityBuilder(main.context.GetEntityManager());

                //var file = this.GetType().Assembly.GetManifestResourceStream("D3DLab.Resources.ducky.obj");

                //main.plugins.Import();

                //var tag = bl.Build(file, main.plugins.ParserPlugins.First());


                // main.items.Add(item);
                var obj = TerrainGameObject.Create(main.context.GetEntityManager());

                main.items.Add(new LoadedItem(main, obj));
            }
        }
        public class RenderModeSwitherCommand : Debugger.BaseWPFCommand<Debugger.Infrastructure.IVisualTreeEntityItem> {
            readonly IContextState context;

            public RenderModeSwitherCommand(IContextState context) {
                this.context = context;
            }
            public override void Execute(Debugger.Infrastructure.IVisualTreeEntityItem item) {
                var tag = item.Name;
                //var sw = new EntityRenderModeSwither(context.GetEntityManager().GetEntity(tag));
                //sw.TurnOn(EntityRenderModeSwither.Modes.BoundingBox);
            }
        }

        #endregion

        #region items

        public class LoadedItem {
            public ICommand VisiblityChanged { get; }
            public ICommand ShowDebuggingVisualization { get; }
            public ICommand LookAt { get; }
            public string Header { get { return gobj.Description; } }

            public LoadedItem() { }

            protected GameObject gobj;
            protected readonly MainWindowViewModel main;

            public LoadedItem(MainWindowViewModel main, GameObject gobj) {
                this.main = main;
                this.gobj = gobj;
                VisiblityChanged = new Command(this);
                ShowDebuggingVisualization = new WpfActionCommand<bool?>(OnShowDebugVisualization);
                LookAt = new WpfActionCommand(OnLookAt);
            }

            private void OnLookAt() {
                gobj.LookAtSelf(main.context.GetEntityManager());
                main.ForceRender();
            }

            void OnShowDebugVisualization(bool? ischecked) {
                if (ischecked.HasValue && ischecked.Value) {
                    gobj.ShowDebugVisualization(main.context.GetEntityManager());
                } else {
                    gobj.HideDebugVisualization(main.context.GetEntityManager());
                }
                main.ForceRender();
            }

            public override string ToString() {
                return gobj.ToString();
            }

            private void Hide() {
                gobj.Hide(main.context.GetEntityManager());
                main.ForceRender();
            }

            private void Show() {
                gobj.Show(main.context.GetEntityManager());
                main.ForceRender();
            }

            class Command : Debugger.BaseWPFCommand<bool> {
                private LoadedItem item;

                public Command(LoadedItem item) {
                    this.item = item;
                }

                public override void Execute(bool _checked) {
                    if (_checked) {
                        item.Show();
                    } else {
                        item.Hide();
                    }

                }
            }
        }

        public class ImportFileLoadedItem : LoadedItem {
            readonly ImportFileInfo info;
            readonly FileSystemWatcher watcher;
            readonly ElementTag tag;
            readonly ManualResetEventSlim reset;
            readonly string tempFileName;
            public ImportFileLoadedItem(MainWindowViewModel main, ImportFileInfo info) : base(main, null) {
                this.info = info;
                reset = new ManualResetEventSlim(true);
                tempFileName = Path.ChangeExtension(info.File.FullName, ".temp");
                File.Delete(tempFileName);
                //
                var bl = new EntityBuilder(main.context.GetEntityManager());
                tag = bl.Build(info.File, info.Parser);
                base.gobj = new SingleGameObject(tag, info.File.Name);
                //
                watcher = new FileSystemWatcher(info.File.DirectoryName, "*" + Path.GetExtension(info.File.Name));
                watcher.EnableRaisingEvents = true;
                watcher.Changed += OnFileChanged;
            }
            DateTime lastWriteTime;
            private void OnFileChanged(object sender, FileSystemEventArgs e) {
                var lastTime = File.GetLastWriteTime(info.File.FullName);
                if (e.FullPath == info.File.FullName && lastWriteTime < lastTime) {                    
                    Thread.Sleep(100);
                    //reset.Wait();
                    //reset.Reset();
                    Stream stream = null;
                    var tries = 3;
                    while (tries --> 0) {
                        try {
                            File.Copy(info.File.FullName, tempFileName, true);
                            stream = File.OpenRead(tempFileName);
                            //using (var readed = File.OpenRead(temp)) {
                            //stream = new MemoryStream();
                            //readed.CopyTo(stream);
                            //stream.Position = 0;
                            //}                            
                            lastWriteTime = lastTime;
                            break;
                        } catch {
                            Thread.Sleep(100);
                        }
                    }
                    if (stream.IsNull()) {
                        return;
                    }

                    System.Diagnostics.Trace.WriteLine($"Reload [{info.File.FullName}]");

                    try {
                        var rbl = new EntityReBuilder(tag, main.context.GetEntityManager());
                        rbl.ReBuildGeometry(stream, info.Parser);
                    } catch (Exception ex) {
                        System.Diagnostics.Trace.WriteLine($"Error [{ex.Message}]");
                    } finally {
                        stream.Dispose();
                        //reset.Set();
                    }
                }
            }
        }

        #endregion

        private SceneView scene;
        private readonly EngineNotificator notificator;
        ContextStateProcessor context;

        public VisualTreeviewerViewModel VisualTreeviewer { get; }
        public SystemViewPresenter SystemsView { get; }
        public ScriptsConsoleVM ScriptsConsole { get; }

        public ICommand LoadDuck { get; set; }
        public ICommand MoveToCenterWorld { get; }
        public ICommand ShowAxis { get; }
        public ICommand ClearConsoleOutput { get; }


        public ICollectionView Items { get; set; }
        public ObservableCollection<string> ConsoleOutput { get; }
        readonly ObservableCollection<LoadedItem> items;
        readonly PluginImporter plugins;
        readonly PrimitiveDrawer primitiveDrawer;

        public MainWindowViewModel() {
            LoadDuck = new LoadCommand(this);
            MoveToCenterWorld = new MoveToCenterWorldCommand(this);
            ShowAxis = new ShowAxisCommand(this);
            ClearConsoleOutput = new ClearConsoleOutputCommand(this);

            primitiveDrawer = new PrimitiveDrawer();

            VisualTreeviewer = new VisualTreeviewerViewModel();
            SystemsView = new SystemViewPresenter();
            ScriptsConsole = new ScriptsConsoleVM(primitiveDrawer);

            items = new ObservableCollection<LoadedItem>();
            Items = CollectionViewSource.GetDefaultView(items);
            notificator = new EngineNotificator();

            notificator.Subscribe(new ViewportSubscriber(this));

            plugins = new PluginImporter();
            ConsoleOutput = new ObservableCollection<string>();
            System.Diagnostics.Trace.Listeners.Add(new TraceOutputListener(ConsoleOutput));
        }

        public void Init(FormsHost host, FrameworkElement overlay) {
            context = new ContextStateProcessor();
            context.AddState(0, x => new GenneralContextState(x, notificator));

            context.SwitchTo(0);

            scene = new SceneView(host, overlay, context, notificator);
            scene.RenderStarted += OnRenderStarted;

            primitiveDrawer.SetContext(context);

            VisualTreeviewer.RenderModeSwither = new RenderModeSwitherCommand(context);
        }

        private void OnRenderStarted() {
            VisualTreeviewer.GameWindow = scene.Window;
            SystemsView.GameWindow = scene.Window;

            var manager = context.GetEntityManager();
            //
            items.Add(new LoadedItem(this, CameraGameObject.Create(context)));
            items.Add(new LoadedItem(this, LightGameObject.CreateAmbientLight(manager)));
            items.Add(new LoadedItem(this, LightGameObject.CreatePointLight(manager, Vector3.Zero + Vector3.UnitZ * 1000)));
            items.Add(new LoadedItem(this, LightGameObject.CreateDirectionLight(manager, new Vector3(1, 4, 4).Normalize())));

        }

        public void Dropped(string[] files) {
            plugins.Import();
            var win = new ChooseParseWindow();
            win.ViewModel.AddFiles(files);
            win.ViewModel.AddParsers(plugins.ParserPlugins);
            win.ViewModel.SetLoader(this);
            win.ShowDialog();
        }

        public void Load(ImportFileInfo info) {
            items.Add(new ImportFileLoadedItem(this, info));
        }

        void ForceRender() {
            scene.Window.InputManager.PushCommand(new Std.Engine.Core.Input.Commands.ForceRenderCommand());
        }

        void ClearConsole() {
            ConsoleOutput.Clear();
        }
    }

    public sealed class GenneralContextState : BaseContextState {
        public GenneralContextState(ContextStateProcessor processor, EngineNotificator notificator) : base(processor, new ManagerContainer(notificator)) {
        }
    }

    public sealed class ViewportSubscriber :
        IManagerChangeSubscriber<GraphicEntity>,
        IManagerChangeSubscriber<IComponentSystem>,
        IEntityRenderSubscriber {
        private readonly MainWindowViewModel mv;

        public ViewportSubscriber(MainWindowViewModel mv) {
            this.mv = mv;
        }

        public void Change(GraphicEntity entity) {
            App.Current.Dispatcher.BeginInvoke(new Action(() => {
                mv.VisualTreeviewer.Add(entity);
            }));
        }

        public void Change(IComponentSystem sys) {
            App.Current.Dispatcher.BeginInvoke(new Action(() => {
                mv.SystemsView.AddSystem(sys);
            }));
        }

        public void Render(IEnumerable<GraphicEntity> entities) {
            if (App.Current == null) { return; }
            App.Current.Dispatcher.BeginInvoke(new Action(() => {
                mv.VisualTreeviewer.Refresh(entities);
            }));
        }
    }



    public sealed class SceneView : Wpf.Engine.App.Scene {

        public SceneView(FormsHost host, FrameworkElement overlay, ContextStateProcessor context, IEntityRenderNotify notify)
            : base(host, overlay, context, notify) {


            //try {
            //    Fwk.ImageSharp.ImagePr.Load(Path.Combine(AppContext.BaseDirectory, "Textures", "spnza_bricks_a_diff.png"));
            //} catch (Exception ex) {
            //    ex.ToString();
            //}

            //var center = new Vector3();
            //var point = new Vector3(10, 10, 10);
            //var res = point + center;

            //var v = new Vector3(10, 10, 10) + new Vector3(5, 20, 0);
            //var v = new Vector3(5, 20, 0) - new Vector3(10, 10, 10);
            //var normal = v;
            //normal.Normalize();

            //var point1 = new Vector3(5, 20, 0) - normal * v.Length()/2;
            //var point2 = new Vector3(10, 10, 10) + normal * v.Length() / 2;



        }

        public class LineBuilder {
            private readonly List<Vector3> positions;
            private readonly List<int> lineListIndices;

            public LineBuilder() {
                positions = new List<Vector3>();
                lineListIndices = new List<int>();
            }

            public void Build(IEnumerable<Vector3> points, bool closed = false) {
                var first = positions.Count;
                positions.AddRange(points);
                var lineCount = positions.Count - first - 1;

                for (var i = 0; i < lineCount; i++) {
                    lineListIndices.Add(first + i);
                    lineListIndices.Add(first + i + 1);
                }

                if (closed) {
                    lineListIndices.Add(positions.Count - 1);
                    lineListIndices.Add(first);
                }
            }

        }



    }


}
