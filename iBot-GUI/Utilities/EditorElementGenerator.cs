using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using iBot_GUI.Annotations;
using Newtonsoft.Json;

namespace iBot_GUI.Utilities
{
    // @TODO: not too happy with the name... needs something more concise yet still telling
    /// <summary>
    /// Class that generates a <see cref="UIElement"/> to edit named values.
    /// </summary>
    public static class EditorElementGenerator
    {
        /// <summary>
        /// Generate a <see cref="UIElement"/> for a specific <see cref="Type"/>
        /// </summary>
        /// <typeparam name="T">Type that determines which <see cref="UIElement"/> will be generated</typeparam>
        /// <param name="name">Name of this Value</param>
        /// <param name="value">Value for the desired <see cref="UIElement"/></param>
        /// <param name="binding">Binding to use while binding the Value to the UIElement</param>
        /// <returns><see cref="UIElement"/> that can be injected into some other Control</returns>
        [NotNull]
        public static UIElement GenerateUiElement<T>(string name, [NotNull] T value, [CanBeNull] Binding binding = null)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var type = typeof(T);

            var result = HandleValueType(type, value, binding)
                         ?? (HandleSpecialTypes(type, value, binding)
                             ?? HandleGenericType(type, value, binding));

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
            var type = typeof(T);

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
                ToolTip = description
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
            var type = typeof(T);

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
                ToolTip = description
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
            var element = new DockPanel();

            var box = new TextBox();

            try
            {
                box.Text = JsonConvert.SerializeObject(value, Formatting.Indented);
            }
            catch (JsonException)
            {
                box.Text = "";
            }

            if (binding != null)
                box.SetBinding(TextBox.TextProperty, binding);

            var valueElement = box;

            var label = new Label
            {
                Content = name,
                ToolTip = description
            };

            DockPanel.SetDock(label, Dock.Left);
            DockPanel.SetDock(valueElement, Dock.Right);

            element.Children.Add(label);
            element.Children.Add(valueElement);

            return element;
        }
    }
}