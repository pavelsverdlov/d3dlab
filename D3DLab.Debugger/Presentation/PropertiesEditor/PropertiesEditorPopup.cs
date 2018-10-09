using D3DLab.Debugger.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace D3DLab.Debugger.Presentation.PropertiesEditor {
    public class PropertiesEditorPopup {
        private PropertiesEditorWindow win;
        public PropertiesEditorWindowViewModel ViewModel { get; }

        public PropertiesEditorPopup() {
            win = new PropertiesEditorWindow();
            ViewModel = (PropertiesEditorWindowViewModel)win.DataContext;
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

    public abstract class ViewProperty {
        public string Key { get; set; }
        public string Title { get; set; }
        
        public bool IsChanged { get; set; }
        public abstract void UpdateValue(string val);
    }


    public abstract class ViewProperty<TVal> : ViewProperty, System.ComponentModel.INotifyPropertyChanged {
        public TVal Value { get; set; }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged = (x, y) => { };

        public override void UpdateValue(string val) {
            IsChanged = true;
            PropertyChanged.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(IsChanged)));
        }

        protected void OnPropertyChanged(string name) {
            PropertyChanged.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
    }

    public class FloatViewProperty : ViewProperty<float> {
        public override void UpdateValue(string val) {
            Value = float.Parse(val);
            base.UpdateValue(val);
        }
    }
    public class DependViewProperty<TVal> : ViewProperty<TVal> {
        private readonly Action<string> change;

        public DependViewProperty(Action<string> change) {
            this.change = change;
        }
        public override void UpdateValue(string val) {
            change(val);
            base.UpdateValue(val);
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
            dictionary.Add(key, new DependViewProperty<T>(change) {
                Title = key,
                Value = val
            });

        }
    }

    public class GroupViewProperty : ViewProperty<ObservableCollection<ViewProperty>> {
        protected readonly Dictionary<string, ViewProperty> dictionary;

        public GroupViewProperty() {
            dictionary = new Dictionary<string, ViewProperty>();
            Value = new ObservableCollection<ViewProperty>();
        }

        public void AddPrimitive<T>(string key, T val, Action<string> change) {
            var pr = new DependViewProperty<T>(change) {
                Title = key,
                Value = val
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
                }
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
                if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) {
                    var value = $"{type.GenericTypeArguments.First().Name}[{((System.Collections.ICollection)val).Count}]";
                    property.AddPrimitive(name, value, x=> { });
                    return;
                }
                var group = new GroupViewProperty();                
                group.Title = name;
                if (type.IsValueType) {
                    group.AnalyzeValueType(val, group, type);
                } else {
                    group.Analyze(val, group, type);
                }
                if (group.Value.Any()) {
                    Value.Add(group);
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
        IVisualComponentItem item;
        public PropertiesEditorWindowViewModel() {
        }

        public void Analyze(IVisualComponentItem item) {
            this.item = item;
            var com = item.GetOriginComponent();
            Title = item.Name;

            item.PropertyChanged += OnAllPropertiesChanged;

            Analyze(com);

            OnPropertyChanged(nameof(Title));
        }

        public void Dispose() {
            Value.Clear();
            dictionary.Clear();
            item.PropertyChanged -= OnAllPropertiesChanged;
        }

        void OnAllPropertiesChanged(object sender, PropertyChangedEventArgs e) {
            Value.Clear();
            dictionary.Clear();
            Analyze(item.GetOriginComponent());
        }
    }
}
