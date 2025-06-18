using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BetterStepsRecorder.WPF.Components.PropertyGrid
{
    public class PropertyGridItem : INotifyPropertyChanged
    {
        public PropertyGridItem(string name, object value)
        {
            Name = name;
            _value = value;
        }

        public string Name { get; set; }

        private object _value;
        public object Value
        {
            get { return _value; }
            set
            {
                if (!Equals(_value, value))
                {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
