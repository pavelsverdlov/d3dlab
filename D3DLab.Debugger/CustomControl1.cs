using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;



namespace D3DLab.Debugger {
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:D3DLab.Debugger"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:D3DLab.Debugger;assembly=D3DLab.Debugger"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    public class CustomControl1 : Control {
        static CustomControl1() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomControl1), new FrameworkPropertyMetadata(typeof(CustomControl1)));
        }

        public interface IVisualProperty {
            string Name { get; }
            string Value { get; }

            Func<object> GetPropertyObject();
        }

        class VisualProperty : IVisualProperty {
            public string Name => throw new NotImplementedException();

            public string Value => throw new NotImplementedException();


            object internalObject;
            public VisualProperty() {
                internalObject = new CustomControl1();
            }

            public Func<object> GetPropertyObject() {
                return () => internalObject;
            }
        }

        public interface IVisualTreeItem {
            string Header { get; }
            ObservableCollection<IVisualProperty> Properties { get; }
        }

        public interface IImmediateWindow { }
        public interface ICommand { }

        public interface IVisual {
            void Execute();
        }


        public interface IVisualEntity {

        }

        public class VisualDebbugProxy : IVisualEntity {
            public VisualDebbugProxy() {

            }
        }


        public class ImmediateWindowEnvironment {
            public dynamic CurrentWatch { get; set; }
        }

        public static class Test {

            public static int Global = 10;
            
            public static void test1() {
                IVisualProperty propery = new VisualProperty();

                //            Clipboard.SetDataObject()
                //CSharpScript

                // CSharpScript.RunAsync(,)
                //var r = CSharpScript.EvaluateAsync("1+1").Result;

                var environment = new ImmediateWindowEnvironment {
                    CurrentWatch = ((Control)propery.GetPropertyObject()())
                };
                //Microsoft.CSharp
                //
                //ScriptOptions.Default.WithSourceResolver().WithReferences(typeof(System.Dynamic).Assembly);

                // CodeDomProvider.CreateProvider("CSharp").;

                var scriptOptions = ScriptOptions.Default;
                var mscorlib = typeof(object).GetTypeInfo().Assembly;
                var systemCore = typeof(System.Linq.Enumerable).GetTypeInfo().Assembly;

                var references = new[] { mscorlib, systemCore, typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly };
                scriptOptions = scriptOptions.AddReferences(references);

                List<int> yList;
                using (var interactiveLoader = new InteractiveAssemblyLoader()) {
                    foreach (var reference in references) {
                        interactiveLoader.RegisterDependency(reference);
                    }

                    // Add namespaces
                    scriptOptions = scriptOptions.AddImports("System");
                    scriptOptions = scriptOptions.AddImports("Microsoft");
                    scriptOptions = scriptOptions.AddImports("Microsoft.CSharp");
                    scriptOptions = scriptOptions.AddImports("System.Linq");
                    scriptOptions = scriptOptions.AddImports("System.Dynamic");
                    scriptOptions = scriptOptions.AddImports("System.Collections.Generic");


                    var c = ((Control)propery.GetPropertyObject()());
                    // Initialize script with custom interactive assembly loader
                    var script = CSharpScript.Create(@"var propery = c; propery.Visibility", scriptOptions, typeof(Control), interactiveLoader);

                    var task = script.RunAsync(globals: c);

                    c.Visibility = Visibility.Hidden;

                    var state = task.Result;

                    var val = state.ReturnValue;

                    //state = state.ContinueWithAsync(@"var x = new List<int>(){1,2,3,4,5};").Result;
                    //state = state.ContinueWithAsync("var y = x.Take(3).ToList();").Result;

                    //var y = state.Variables[1];
                    //yList = (List<int>)y.Value;
                }
                

                var result = CSharpScript.EvaluateAsync(
                    "var propery = CurrentWatch; propery.Visibility",
                    //ScriptOptions.Default.WithImports("System.Math", "Microsoft.CSharp", "System.Dynamic"), 
                    globals: environment).Result;

                using (var aloader = new InteractiveAssemblyLoader()) {
                    aloader.RegisterDependency(typeof(System.Math).Assembly);

                    var script = CSharpScript.Create("Math.Sqrt(2)", assemblyLoader: aloader);
                    script.RunAsync().ContinueWith(x => {
                        var res = x.Result.ReturnValue;
                        var str = res.ToString();
                    });
                }
            }
            
        }
    }
}
