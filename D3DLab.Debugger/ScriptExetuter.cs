using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using System;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;

namespace D3DLab.Debugger {


    public class ScriptEnvironment {
        public dynamic CurrentWatch { get; set; }
    }
    public interface IPrimitiveDrawer {
        void DrawPolygon(Vector3[] points);
        void DrawPoint(Vector3 point);
    }
    public class ScriptTypesMatching {
        class Variable {
            public ScriptVariable variable;
        }

        IPrimitiveDrawer drawer;

        public ScriptTypesMatching(IPrimitiveDrawer drawer) {
            this.drawer = drawer;
        }

        public void GetPrimitive(ScriptVariable variable) {
            var var = new Variable {
                variable = variable
            };
            var type = variable.Type;
            if (type.IsArray) {
                AnalizeCollection(type, variable);
            }
            Analize(type, variable);
        }

        void Analize(Type type, ScriptVariable variable) {
            if (type.IsValueType) {
                if (NumericsTypes.IsVector3(type)) {
                    drawer.DrawPoint((Vector3)variable.Value);
                }
            }
        }

        void AnalizeCollection(Type type, ScriptVariable variable) {
            var itemtype = type.GetElementType();
            if (itemtype.IsValueType) {
                if (NumericsTypes.IsVector3(itemtype)) {
                    drawer.DrawPolygon((Vector3[])variable.Value);
                }
            }
        }
    }

    public sealed class ScriptExetuter {
        const string namaspaces = "using System;using System.Numerics;";
        private ScriptTypesMatching sc;

        public event Action<string> Output;

        public ScriptExetuter(IPrimitiveDrawer drawer) {
            sc = new ScriptTypesMatching(drawer);
        }
        public Task<object> Execute(ScriptEnvironment environment, string code) {
            return CSharpScript.EvaluateAsync(code,
             ScriptOptions.Default.WithImports("System.Dynamic")
               .AddReferences(
                   Assembly.GetAssembly(typeof(System.Dynamic.DynamicObject)),  // System.Code
                   Assembly.GetAssembly(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo)),  // Microsoft.CSharp
                   Assembly.GetAssembly(typeof(System.Dynamic.ExpandoObject))  // System.Dynamic
               ),
               globals: environment);
        }
        public void Execute1(ScriptEnvironment environment, string code) {
            
            var imports = new[] {
                "System",
            };
            var asemblyis = new[] {
                typeof(System.Numerics.Vector2).Assembly
            }; 
            code = namaspaces + "var sqrt = System.Math.Sqrt(2);var point = new []{ new Vector3(), new Vector3()};";
            CSharpScript.RunAsync(code, ScriptOptions.Default.WithImports(imports).WithReferences(asemblyis))
                .ContinueWith(x=> {
                    var vars = x.Result.Variables;
                    //var v = vars[0].Value;
                    foreach(var v in vars) {
                        sc.GetPrimitive(v);
                    }
                    //var res = x.Result.ReturnValue;
                  //  var str = res.ToString();
                });
        }
        public void Execute2(ScriptEnvironment environment, string code) {
            //IEntityComponent propery = new VisualProperty();

            //            Clipboard.SetDataObject()
            //CSharpScript

            // CSharpScript.RunAsync(,)
            //var r = CSharpScript.EvaluateAsync("1+1").Result;

            //var environment = new ScriptEnvironment {
            //    CurrentWatch = ((Control)propery.GetPropertyObject())
            //};
            //Microsoft.CSharp
            //
            //ScriptOptions.Default.WithSourceResolver().WithReferences(typeof(System.Dynamic).Assembly);

            // CodeDomProvider.CreateProvider("CSharp").;

            var scriptOptions = ScriptOptions.Default;
            var mscorlib = typeof(object).GetTypeInfo().Assembly;
            var systemCore = typeof(System.Linq.Enumerable).GetTypeInfo().Assembly;
            /*
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


                var c = ((D3DLab.Core.Test.Component)environment.CurrentWatch);
                // Initialize script with custom interactive assembly loader
                var script = CSharpScript.Create(@"var propery = c; propery.Name", scriptOptions, typeof(D3DLab.Core.Test.Component), interactiveLoader);

                var task = script.RunAsync(globals: c);

                //c.Visibility = System.Windows.Visibility.Hidden;

                var state = task.Result;

                var val = state.ReturnValue;

                //state = state.ContinueWithAsync(@"var x = new List<int>(){1,2,3,4,5};").Result;
                //state = state.ContinueWithAsync("var y = x.Take(3).ToList();").Result;

                //var y = state.Variables[1];
                //yList = (List<int>)y.Value;
            }
            */

            var result = CSharpScript.EvaluateAsync(
                "var propery = CurrentWatch; propery.RenderTechnique",
              ScriptOptions.Default.WithImports("System.Dynamic")
                .AddReferences(
                    Assembly.GetAssembly(typeof(System.Dynamic.DynamicObject)),  // System.Code
                    Assembly.GetAssembly(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo)),  // Microsoft.CSharp
                    Assembly.GetAssembly(typeof(System.Dynamic.ExpandoObject))  // System.Dynamic
                ),
                globals: environment).Result;


            using (var aloader = new InteractiveAssemblyLoader()) {
                //aloader.RegisterDependency(typeof(D3DLab.Core.Test.Component).Assembly);
                aloader.RegisterDependency(typeof(System.Math).Assembly);

                var script = CSharpScript.Create("System.Math.Sqrt(2)", assemblyLoader: aloader);
                script.RunAsync().ContinueWith(x => {
                    var res = x.Result.ReturnValue;
                    var str = res.ToString();
                });
            }
        }
    }
}
