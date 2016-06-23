using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using iBot_GUI.Pages.Settings;
using iBot_GUI.Resources;

namespace iBot_GUI.Controls
{
    /// <summary>
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl : UserControl
    {
        public SettingsControl()
        {
            InitializeComponent();
        }

        internal SettingsControl(SettingsBase settings) : this()
        {
            MainStack.Children.Add(MakeScaffolding(settings));
        }

        private UIElement MakeScaffolding<T>(T settings)
        {
            var stack = new StackPanel();

            var type = settings.GetType();

            var props = type.GetProperties();

            foreach (var prop in props)
            {
                var element = new DockPanel();

                var propType = prop.PropertyType;
                var value = prop.GetValue(settings);

                var settingsDescriptionIdentifier = Attribute.GetCustomAttributes(prop)
                                                             .OfType<DescriptionAttribute>()
                                                             .FirstOrDefault()
                                                            ?.Description;

                var description = string.IsNullOrWhiteSpace(settingsDescriptionIdentifier)
                                      ? ""
                                      : SettingsDescriptions.ResourceManager.GetString(settingsDescriptionIdentifier);

                if (propType.IsValueType || value is string)
                {
                    UIElement valueElement;

                    var label = new Label
                    {
                        Content = prop.Name,
                        ToolTip = description
                    };

                    var binding = new Binding(prop.Name)
                    {
                        Source = settings,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                        Mode = BindingMode.TwoWay,
                        Delay = 250
                    };

                    if (value is char || value is string)
                    {
                        var box = new TextBox { Text = value.ToString() };
                        box.SetBinding(TextBox.TextProperty, binding);

                        valueElement = box;
                    }
                    else if (value is bool)
                    {
                        var box = new CheckBox { IsChecked = (bool) value };
                        box.SetBinding(CheckBox.IsCheckedProperty, binding);

                        valueElement = box;
                    }
                    else if (value is short || value is int || value is long || value is ushort || value is uint || value is ulong)
                    {
                        var box = new TextBox { Text = value.ToString() };
                        box.SetBinding(TextBox.TextProperty, binding);

                        valueElement = box;
                    }
                    else if (value is float || value is double)
                    {
                        var box = new TextBox { Text = value.ToString() };
                        box.SetBinding(TextBox.TextProperty, binding);

                        valueElement = box;
                    }
                    else if (value is byte || value is sbyte)
                    {
                        var box = new TextBox { Text = value.ToString() };
                        box.SetBinding(TextBox.TextProperty, binding);

                        valueElement = box;
                    }
                    else if (value is Enum)
                    {
                        var box = new ComboBox
                        {
                            SelectedValue = value,
                            ItemsSource = Enum.GetValues(propType)
                        };

                        box.SetBinding(ComboBox.SelectedItemProperty, binding);

                        valueElement = box;
                    }
                    else
                    {
                        // structs? fuck it...
                        continue;
                    }

                    DockPanel.SetDock(label, Dock.Left);
                    DockPanel.SetDock(valueElement, Dock.Right);

                    element.Children.Add(label);
                    element.Children.Add(valueElement);

                    stack.Children.Add(element);
                }
                else
                {
                    var group = new GroupBox
                    {
                        Header = prop.Name,
                        Content = MakeScaffolding(value)
                    };

                    stack.Children.Add(group);

                    //var label = new Label { Content = prop.Name };
                    //var list = MakeScaffolding(prop.GetValue(settings));

                    //DockPanel.SetDock(label, Dock.Left);
                    //DockPanel.SetDock(list, Dock.Right);

                    //element.Children.Add(label);
                    //element.Children.Add(list);

                    //stack.Children.Add(element);
                }
            }

            return stack;
        }
    }
}
