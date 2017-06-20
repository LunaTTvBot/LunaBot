using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using iBot_GUI.Annotations;
using iBot_GUI.Resources;

namespace iBot_GUI.Utilities
{
    // @TODO: not too happy with the name... needs something more concise yet still telling
    /// <summary>
    /// Class that generates a <see cref="UIElement"/> to edit named values.
    /// </summary>
    public static class EditorElementGenerator
    {
        /// <summary>
        /// Generate a <see cref="UIElement"/> for a specific <see cref="Type"/>, using all possible Properties of the given type recursively
        /// </summary>
        /// <typeparam name="T">Type that determines which <see cref="UIElement"/> will be generated</typeparam>
        /// /// <returns><see cref="UIElement"/> that can be injected into some other Control</returns>
        public static IEnumerable<UIElement> GenerateUiElementRecursive<T>([NotNull] T baseObject)
        {
            var list = new List<UIElement>();

            foreach (var prop in baseObject.GetType()
                                           .GetProperties())
            {
                var name = prop.Name;

                var value = prop.GetValue(baseObject);

                var settingsDescriptionIdentifier = Attribute.GetCustomAttributes(prop)
                                                             .OfType<DescriptionAttribute>()
                                                             .FirstOrDefault()
                                                             ?.Description;

                var description = string.IsNullOrWhiteSpace(settingsDescriptionIdentifier)
                                      ? ""
                                      : SettingsDescriptions.ResourceManager
                                                            .GetString(settingsDescriptionIdentifier);

                var binding = new Binding(prop.Name)
                {
                    Source = baseObject,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    Mode = BindingMode.TwoWay,
                    Delay = 250
                };

                var element = GenerateUiElement(name, value, description, binding);

                list.Add(element);
            }

            return list;
        }

        /// <summary>
        /// Generate a <see cref="UIElement"/> for a specific <see cref="Type"/>
        /// </summary>
        /// <typeparam name="T">Type that determines which <see cref="UIElement"/> will be generated</typeparam>
        /// <param name="name">Name of this Value</param>
        /// <param name="value">Value for the desired <see cref="UIElement"/></param>
        /// <param name="description">Optional description for this Key / Value Pair</param>
        /// <param name="binding">Binding to use while binding the Value to the UIElement</param>
        /// <returns><see cref="UIElement"/> that can be injected into some other Control</returns>
        [NotNull]
        private static UIElement GenerateUiElement<T>([NotNull] string name,
                                                      [NotNull] T value,
                                                      [CanBeNull] string description,
                                                      [CanBeNull] Binding binding = null)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var result = HandleValueType<T>(name, value, description, binding)
                         ?? (HandleSpecialTypes<T>(name, value, description, binding)
                             ?? HandleGenericType<T>(name, value, description, binding));

            return result;
        }

        /// <summary>
        /// Handle the creation of an <see cref="UIElement"/> for some ValueType
        /// </summary>
        /// <typeparam name="T">Type that determines which <see cref="UIElement"/> will be generated</typeparam>
        /// <param name="name">Name of this Value</param>
        /// <param name="value">Value for the desired <see cref="UIElement"/></param>
        /// <param name="description">Optional description for this Key / Value Pair</param>
        /// <param name="binding">Binding to use while binding the Value to the UIElement</param>
        /// <returns>A valid <see cref="UIElement"/> or null when no UIElement could be created</returns>
        [CanBeNull]
        private static UIElement HandleValueType<T>([NotNull] string name,
                                                    [NotNull] T value,
                                                    [CanBeNull] string description = null,
                                                    [CanBeNull] Binding binding = null)
        {
            var type = value.GetType();

            var element = new DockPanel();

            Func<UIElement> genericElementCreator = () =>
            {
                var box = new TextBox {Text = value.ToString()};

                if (binding != null)
                    box.SetBinding(TextBox.TextProperty, binding);

                return box;
            };

            UIElement valueElement;

            if (type == typeof(char) || type == typeof(string))
            {
                valueElement = genericElementCreator.Invoke();
            }
            else if (type == typeof(bool))
            {
                // double cast because direct cast is not currently possible in C# - ugly but it works for now
                var box = new CheckBox {IsChecked = (bool) (object) value};

                if (binding != null)
                    box.SetBinding(ToggleButton.IsCheckedProperty, binding);

                valueElement = box;
            }
            else if (type == typeof(short)
                     || type == typeof(int)
                     || type == typeof(long)
                     || type == typeof(ushort)
                     || type == typeof(uint)
                     || type == typeof(ulong))
            {
                valueElement = genericElementCreator.Invoke();
            }
            else if (type == typeof(float) || type == typeof(double))
            {
                valueElement = genericElementCreator.Invoke();
            }
            else if (type == typeof(byte) || type == typeof(sbyte))
            {
                valueElement = genericElementCreator.Invoke();
            }
            else if (type == typeof(Enum))
            {
                var box = new ComboBox
                {
                    SelectedValue = value,
                    ItemsSource = Enum.GetValues(type)
                };

                if (binding != null)
                    box.SetBinding(Selector.SelectedItemProperty, binding);

                valueElement = box;
            }
            else
            {
                // type is not supported in this function
                return null;
            }

            var label = new Label
            {
                Content = name,
                ToolTip = description ?? ""
            };

            DockPanel.SetDock(label, Dock.Left);
            DockPanel.SetDock(valueElement, Dock.Right);

            element.Children.Add(label);
            element.Children.Add(valueElement);

            return element;
        }

        /// <summary>
        /// Handle the creation of an <see cref="UIElement"/> for Special Classes
        /// </summary>
        /// <typeparam name="T">Type that determines which <see cref="UIElement"/> will be generated</typeparam>
        /// <param name="name">Name of this Value</param>
        /// <param name="value">Value for the desired <see cref="UIElement"/></param>
        /// <param name="description">Optional description for this Key / Value Pair</param>
        /// <param name="binding">Binding to use while binding the Value to the UIElement</param>
        /// <returns>A valid <see cref="UIElement"/> or null when no UIElement could be created</returns>
        [CanBeNull]
        private static UIElement HandleSpecialTypes<T>([NotNull] string name,
                                                       [NotNull] T value,
                                                       [CanBeNull] string description = null,
                                                       [CanBeNull] Binding binding = null)
        {
            var type = value.GetType();

            var element = new DockPanel();

            UIElement valueElement;

            if (typeof(IList).IsAssignableFrom(type))
            {
                var box = new ListBox
                {
                    ItemsSource = value as IList,
                };

                if (binding != null)
                    box.SetBinding(Selector.SelectedItemProperty, binding);

                valueElement = box;
            }
            else if (typeof(IDictionary).IsAssignableFrom(type))
            {
                var box = new ListBox
                {
                    ItemsSource = value as IDictionary,
                };

                if (binding != null)
                    box.SetBinding(Selector.SelectedItemProperty, binding);

                valueElement = box;
            }
            else
            {
                // type is not supported in this function
                return null;
            }

            var label = new Label
            {
                Content = name,
                ToolTip = description ?? ""
            };

            DockPanel.SetDock(label, Dock.Left);
            DockPanel.SetDock(valueElement, Dock.Right);

            element.Children.Add(label);
            element.Children.Add(valueElement);

            return element;
        }

        /// <summary>
        /// Handle the creation of an <see cref="UIElement"/> for all Types that are not covered by <see cref="HandleValueType{T}"/> and <see cref="HandleSpecialTypes{T}"/>
        /// </summary>
        /// <typeparam name="T">Type that determines which <see cref="UIElement"/> will be generated</typeparam>
        /// <param name="name">Name of this Value</param>
        /// <param name="value">Value for the desired <see cref="UIElement"/></param>
        /// <param name="description">Optional description for this Key / Value Pair</param>
        /// <param name="binding">Binding to use while binding the Value to the UIElement</param>
        /// <returns>A valid <see cref="UIElement"/></returns>
        [NotNull]
        private static UIElement HandleGenericType<T>([NotNull] string name,
                                                      [NotNull] T value,
                                                      [CanBeNull] string description = null,
                                                      [CanBeNull] Binding binding = null)
        {
            var stack = new StackPanel();

            foreach (var uiElement in GenerateUiElementRecursive(value))
                stack.Children.Add(uiElement);

            var box = new GroupBox
            {
                Header = name,
                Content = stack,
                ToolTip = description ?? ""
            };

            if (binding != null)
                box.SetBinding(TextBox.TextProperty, binding);

            return box;
        }
    }
}