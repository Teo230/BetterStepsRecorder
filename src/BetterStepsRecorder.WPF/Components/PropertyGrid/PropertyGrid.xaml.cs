using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace BetterStepsRecorder.WPF.Components.PropertyGrid
{
    /// <summary>
    /// Interaction logic for PropertyGrid.xaml
    /// </summary>
    public partial class PropertyGrid : ContentControlEx, INotifyPropertyChanged
    {
        public PropertyGrid()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty SelectedObjectProperty =
            DependencyProperty.Register(
                "SelectedProperty",
                typeof(object),
                typeof(PropertyGrid),
                new PropertyMetadata(null, OnSelectedPropertyChanged));

        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register(
                "Item",
                typeof(object),
                typeof(PropertyGrid),
                new PropertyMetadata(null, OnItemChanged));

        public object Item
        {
            get { return GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public object SelectedObject
        {
            get { return GetValue(SelectedObjectProperty); }
            set { SetValue(SelectedObjectProperty, value); }
        }

        private ObservableCollection<PropertyGridItem> _propertyItems = new ObservableCollection<PropertyGridItem>();
        public ObservableCollection<PropertyGridItem> PropertyItems
        {
            get { return _propertyItems; }
            set
            {
                if (_propertyItems != value)
                {
                    _propertyItems = value;
                    NotifyPropertyChanged(nameof(PropertyItems));
                }
            }
        }

        private static void OnItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PropertyGrid control = (PropertyGrid)d;
            control.PopulateProperties(e.NewValue);
        }

        private static void OnSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PropertyGrid control = (PropertyGrid)d;
        }

        private void PopulateProperties(object obj)
        {
            //PropertyItems.Clear();
            if (obj == null) return;

            foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                // You'd add logic here to filter Browsable(false), handle ReadOnly, etc.
                var value = obj.GetType().GetProperty(prop.Name).GetValue(obj);
                PropertyItems.Add(new PropertyGridItem(prop.Name, value));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
