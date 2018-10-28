using D3DLab.Debugger.Infrastructure;
using D3DLab.Debugger.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Input;

namespace D3DLab.Debugger.Presentation.PropertiesEditor {
    public class PropertiesEditorPopup {
        private PropertiesEditorWindow win;
        

        public PropertiesEditorWindowViewModel ViewModel { get; }

        public PropertiesEditorPopup(IRenderUpdater updater) {
            win = new PropertiesEditorWindow();
            ViewModel = (PropertiesEditorWindowViewModel)win.DataContext;
            ViewModel.RenderUpdater = updater;
            win.Closed += Win_Closed;
        }

        private void Win_Closed(object sender, EventArgs e) {
            win.Closed -= Win_Closed;
            ViewModel.Dispose();
        }

        public void Show() {
            win.Show();
        }
    }


    #region view properties

    public abstract class ViewProperty: System.ComponentModel.INotifyPropertyChanged {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged = (x, y) => { };

        public string Key { get; set; }
        public string Title { get; set; }

        public bool IsChanged { get; set; }
        public virtual void UpdateValue(string val) {
            IsChanged = true;
            PropertyChanged.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(IsChanged)));
        }

        protected void OnPropertyChanged(string name) {
            PropertyChanged.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
    }

    public abstract class ViewProperty<TVal> : ViewProperty{
        class ChangedCommand : BaseWPFCommand<string> {
            private ViewProperty<TVal> pr;

            public ChangedCommand(ViewProperty<TVal> viewProperty) {
                this.pr = viewProperty;
            }

            public override void Execute(string parameter) {
                pr.UpdateValue(parameter);
            }
        }
        public ICommand Changed { get; }
        public TVal Value { get; set; }

        readonly Action<string> change;

        public ViewProperty(Action<string> change) {
            Changed = new ChangedCommand(this);
            this.change = change;
        }

        public override void UpdateValue(string val) {
            change(val);
            base.UpdateValue(val);
        }        
    }

    public class TextBoxViewProperty : ViewProperty<string> {
        public TextBoxViewProperty(Action<string> change) : base(change) { }
    }

    public class ComboBoxViewProperty : ViewProperty<string> {
        public ICollectionView Values { get; }
        public ComboBoxViewProperty(Action<string> change, string current, string[] values) : base(change) {
            Value = current;
            var obs = new ObservableCollection<string>(values);
            Values = CollectionViewSource.GetDefaultView(obs);
            Values.MoveCurrentToPosition(obs.IndexOf(current));
            Values.CurrentChanged += OnValuesCurrentChanged;
        }

        void OnValuesCurrentChanged(object sender, EventArgs e) {
            UpdateValue(Values.CurrentItem.ToString());
        }
    }

    public class Vector3ViewProperty : ViewProperty<Vector3> {
        public Vector3ViewProperty(Vector3 val, Action<Vector3> change) : base(StringToVector3(change)) {
            Value = val;
        }

        static Action<string> StringToVector3(Action<Vector3> change) {
            return str => {
                var parts = str.Split(' ')
                .Select(x=>float.Parse(x, System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture))
                .ToArray();
                var v = new Vector3(parts[0], parts[1], parts[2]);
                change(v);
            };
        }
    }

    #endregion

    public abstract class ViewProperties {
        public ObservableCollection<ViewProperty> Properties {
            get {
                return new ObservableCollection<ViewProperty>(dictionary.Values);
            }
        }

        protected readonly Dictionary<string, ViewProperty> dictionary;
        public ViewProperties() {
            dictionary = new Dictionary<string, ViewProperty>();
        }

        public void AddPrimitive<T>(string key, T val, Action<string> change) {
            dictionary.Add(key, new TextBoxViewProperty(change) {
                Title = key,
                Value = val == null ? string.Empty : val.ToString()
            });
        }
        public void AddVector(string key, Vector3 val, Action<Vector3> change) {
            dictionary.Add(key, new Vector3ViewProperty(val, change) {
                Title = key
            });
        }
    }

    public class GroupViewProperty : ViewProperty<ObservableCollection<ViewProperty>> {
        protected readonly Dictionary<string, ViewProperty> dictionary;

        public GroupViewProperty() : base(x => { }) {
            dictionary = new Dictionary<string, ViewProperty>();
            Value = new ObservableCollection<ViewProperty>();
        }

        public void AddPrimitive<T>(string key, T val, Action<string> change) {
            var pr = new TextBoxViewProperty(change) {
                Title = key,
                Value = val == null ? string.Empty : val.ToString()
            };
            dictionary.Add(key, pr);
            Value.Add(pr);
        }
        public void AddEnum<T>(string key, T val, Action<string> change, string[] values) {
            var pr = new ComboBoxViewProperty(change, val.ToString(), values) {
                Title = key
            };
            dictionary.Add(key, pr);
            Value.Add(pr);
        }
        public void AddVector(string key, object obj, Action<Vector3> change) {
            var val = (System.Numerics.Vector3)obj;
            var pr = new Vector3ViewProperty(val, change) {
                Title = key
            };
            dictionary.Add(key, pr);
            Value.Add(pr);
        }


        public void Analyze(object com) {
            Analyze(com, this, com.GetType());
        }

        void Analyze(object com, GroupViewProperty property, Type type) {
            foreach (var pr in type.GetProperties()) {
                Analyze(com, property, pr);
            }
        }
        void AnalyzeValueType(object com, GroupViewProperty property, Type basetype) {
            foreach (var field in basetype.GetFields()) {
                var name = field.Name;
                var val = com.IsNull() ? null : field.GetValue(com);
                var type = field.FieldType;
                if (type.IsPrimitive || Type.GetTypeCode(type) == TypeCode.String) {
                    var converter = GetConverter(type);
                    void change(string x) => field.SetValue(com, converter(x));
                    property.AddPrimitive(name, val, change);
                } else if (type.IsEnum) {
                    var fields = type.GetFields().ToList();
                    fields.RemoveAt(0);
                    void change(string x) => field.SetValue(com, Enum.Parse(type, x));
                    var values = fields.Select(x => x.Name).ToArray();
                    property.AddEnum(name, val, change, values);
                }
            }
        }
        void Analyze(object val, string name, Type type,
            Action<object> setter, GroupViewProperty property) {
            try {
                if (type.IsPrimitive || Type.GetTypeCode(type) == TypeCode.String) {
                    var converter = GetConverter(type);
                    Action<string> change = x => setter(converter(x));
                    property.AddPrimitive(name, val, change);
                    return;
                }
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) {
                    var valuetext = val.IsNull() ? "null" : ((System.Collections.ICollection)val).Count.ToString();
                    var value = $"{type.GenericTypeArguments.First().Name}[{valuetext}]";
                    property.AddPrimitive(name, value, x => { });
                    return;
                }
                var group = new GroupViewProperty();
                group.Title = name;

                if (type.IsValueType && !type.IsEnum) {
                    group.AnalyzeValueType(val, group, type);
                } else {
                    group.Analyze(val, group, type);
                }
                if (group.Value.Any()) {
                    Value.Add(group);
                } else if (type.IsEnum) {
                    var fields = type.GetFields().ToList();
                    fields.RemoveAt(0);
                    void change(string x) => setter(Enum.Parse(type, x));
                    var values = fields.Select(x => x.Name).ToArray();
                    property.AddEnum(name, val, change, values);
                } else {
                    property.AddPrimitive(name, val, x => { });
                }

            } catch (Exception ex) {
                ex.ToString();
            }
        }
        void Analyze(object com, GroupViewProperty property, PropertyInfo pr) {
            var name = pr.Name;
            try {
                var val = com.IsNull() ? null : pr.GetValue(com);
                var type = pr.PropertyType;
                if (type.IsPrimitive || Type.GetTypeCode(type) == TypeCode.String) {
                    var converter = GetConverter(type);
                    Action<string> change = x => pr.SetValue(com, converter(x));
                    property.AddPrimitive(name, val, change);
                    return;
                }
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) {
                    var valuetext = val.IsNull() ? "null" : ((System.Collections.ICollection)val).Count.ToString();
                    var value = $"{type.GenericTypeArguments.First().Name}[{valuetext}]";
                    property.AddPrimitive(name, value, x => { });
                    return;
                }
                var group = new GroupViewProperty();
                group.Title = name;

                if (type.IsValueType && !type.IsEnum) {
                    switch (type.FullName) {
                        case "System.Numerics.Vector3":
                            group.AddVector(name, val, x => { pr.SetValue(com, x); });
                            break;
                        default:
                            group.AnalyzeValueType(val, group, type);
                            break;
                    }

                    
                } else {
                    group.Analyze(val, group, type);
                }
                if (group.Value.Any()) {
                    Value.Add(group);
                } else if (type.IsEnum) {
                    var fields = type.GetFields().ToList();
                    fields.RemoveAt(0);
                    void change(string x) => pr.SetValue(com, Enum.Parse(type, x));
                    var values = fields.Select(x => x.GetValue(com).ToString()).ToArray();
                    property.AddEnum(name, val, change, values);
                } else {
                    property.AddPrimitive(name, val, x => { });
                }

            } catch (Exception ex) {
                ex.ToString();
            }
        }

        protected Func<string, object> GetConverter(Type t) {
            switch (Type.GetTypeCode(t)) {
                case TypeCode.Boolean:
                    return _in => bool.Parse(_in);
                case TypeCode.String:
                    return _in => _in;
                case TypeCode.Single:
                    return _in => float.Parse(_in);
                default:
                    return x => null;
            }

        }
        protected bool IsDefaultValue(Type t, object val) {
            switch (Type.GetTypeCode(t)) {
                case TypeCode.String:
                    return string.IsNullOrWhiteSpace(val.ToString());
                case TypeCode.Single:
                    var fval = float.Parse(val.ToString());
                    return fval == 0 || float.NaN == fval;
                default:
                    return false;
            }

        }
    }

    //=====================================================

    public class PropertiesEditorWindowViewModel : GroupViewProperty, System.ComponentModel.INotifyPropertyChanged {

        public class ApplyCommand : BaseWPFCommand {
            private PropertiesEditorWindowViewModel _this;
            public ApplyCommand(PropertiesEditorWindowViewModel _this) {
                this._this = _this;
            }
            public override void Execute(object parameter) {
                _this.OnApply();
            }
        }

        public ICommand Apply { get; }
        public IRenderUpdater RenderUpdater { get; set; }

        IVisualComponentItem item;
        public PropertiesEditorWindowViewModel() {
            Apply = new ApplyCommand(this);
        }

        public void Analyze(IVisualComponentItem item) {
            this.item = item;
            var com = item.GetOriginComponent();
            Title = item.Name;

            Analyze(com);

            OnPropertyChanged(nameof(Title));
        }

        public void Dispose() {
            Value.Clear();
            dictionary.Clear();
        }

        void OnApply() {
            Value.Clear();
            dictionary.Clear();
            Analyze(item.GetOriginComponent());
            RenderUpdater.Update();
        }
    }
}
