using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace D3DLab.Core2 {
    public abstract class Component : IDisposable {
        public abstract void Dispose();
    }
    public interface IPosComponent {
        object POsion { get; set; }
    }
    public class PosComponent : Component, IPosComponent {
        public object POsion { get; set; }

        public override void Dispose() {
            throw new NotImplementedException();
        }
    }

    public abstract class ComponentSystem {
        public void Execute(IScene main) {
            foreach (var en in main.GetEntities()) {
                var position = en.GetComponent<PosComponent>();


                //


            }
        }
    }

    public abstract class Entity {
        private readonly List<Component> components;

        protected Entity() {
            components = new List<Component>();
        }
        public T GetComponent<T>() {
            return default(T);
        }

        public void AddComponent<T>(T component) where T : Component {
            components.Add(component);
        }
    }

    public interface IMatrixComp {
        object Matrix { get; }
    }
    public class TransformComponent : Component, IMatrixComp {
        public object Matrix => throw new NotImplementedException();

        public override void Dispose() {
            throw new NotImplementedException();
        }
    }
    public class GroupTransformEntityComponent : Component, IMatrixComp {
        private readonly List<Entity> entities;
        private object matrix;

        public object Matrix {
            get {
                return matrix;
            }
            set {
                //
                matrix = value;
                foreach (var en in entities) {
                    var position = en.GetComponent<GroupTransformEntityComponent>();
                    position.Matrix = value;
                }
            }
        }


        public override void Dispose() {
            throw new NotImplementedException();
        }
    }
    
    public struct InputStates {
        public MouseButtons Buttons { get; set; }
    }

    public interface IScene {
        IEnumerable<Entity> GetEntities();
        InputStates InputState { get; }
    }

    public class Main : IScene {
        public InputStates InputState => throw new NotImplementedException();

        List<Entity> entities = new List<Entity>();
        List<ComponentSystem> systems = new List<ComponentSystem>();
        InputStates state = new InputStates();

        class Arrow : Entity {

        }
        class Support : Entity {

        }

        public void Test() {
            foreach (var sys in systems) {
                sys.Execute(this);
            }

        }

        public IEnumerable<Entity> GetEntities() {
            throw new NotImplementedException();
        }
    }
}
