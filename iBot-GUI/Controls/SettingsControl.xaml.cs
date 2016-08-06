﻿using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using iBot_GUI.Pages.Settings;
using iBot_GUI.Resources;
using IBot.Facades.Core;
using IBot.Facades.Core.Settings;

namespace iBot_GUI.Controls
{
    /// <summary>
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl : UserControl
    {
        public SettingsBase Settings { get; set; }

        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register(nameof(Settings),
                                        typeof(SettingsBase),
                                        typeof(SettingsControl),
                                        new PropertyMetadata(new SettingsBase(),
                                                             PropertyChangedCallback));

        public SettingsControl()
        {
            InitializeComponent();
        }

        private static void PropertyChangedCallback(DependencyObject dpObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            if (!(dpObject is SettingsControl))
                return;

            var instance = (SettingsControl) dpObject;
            var value = dpObject.GetValue(SettingsProperty) as SettingsBase;

            instance.Settings = value;
            instance.MakeScaffolding();
        }

        private void MakeScaffolding()
        {
            MainStack.Children.Clear();
            MainStack.Children.Add(MakeScaffolding(Settings));
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

                if (propType.IsValueType
                    || value is string
                    || value is IList
                    || value is IDictionary)
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
                    else if (value is IList)
                    {
                        var box = new ListBox
                        {
                            ItemsSource = (value as IList),
                        };

                        box.SetBinding(ListBox.SelectedItemProperty, binding);

                        valueElement = box;
                    }
                    else if (value is IDictionary)
                    {
                        var box = new ListBox
                        {
                            ItemsSource = (value as IDictionary),
                        };

                        box.SetBinding(ListBox.SelectedItemProperty, binding);

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
