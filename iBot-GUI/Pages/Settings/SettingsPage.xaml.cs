using System;
using System.Collections.Generic;
using System.Linq;
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

namespace iBot_GUI.Pages.Settings
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : UserControl
    {
        public SettingsPage()
        {
            InitializeComponent();

            MainTabControl.Items.Add(new TabItem
            {
                Content = MakeScaffolding(new TestSettings())
            });
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

                if (propType.IsValueType || value is string)
                {
                    Label label;
                    UIElement valueElement;

                    if (value is char || value is string)
                    {
                        label = new Label { Content = $"Char or String: {prop.Name}" };
                        valueElement = new TextBox { Text = value.ToString() };
                    }
                    else if (value is bool)
                    {
                        label = new Label { Content = $"Bool: {prop.Name}" };
                        valueElement = new CheckBox { IsChecked = (bool) value };
                    }
                    else if (value is short || value is int || value is long || value is ushort || value is uint || value is ulong)
                    {
                        label = new Label { Content = $"Short, Int, Long, UShort, UInt, ULong: {prop.Name}" };
                        valueElement = new TextBox() { Text = value.ToString() };
                    }
                    else if (value is float || value is double)
                    {
                        label = new Label { Content = $"Float or Double: {prop.Name}" };
                        valueElement = new TextBox { Text = value.ToString() };
                    }
                    else if (value is byte || value is sbyte)
                    {
                        label = new Label { Content = $"Byte or SByte: {prop.Name}" };
                        valueElement = new TextBox { Text = value.ToString() };
                    }
                    else if (value is Enum)
                    {
                        label = new Label { Content = $"Enum: {prop.Name}" };
                        var combo = new ComboBox();

                        foreach (var enumValue in Enum.GetValues(propType))
                        {
                            combo.Items.Add(enumValue);
                        }
                        combo.SelectedValue = value;

                        valueElement = combo;
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
                    var label = new Label { Content = prop.Name };
                    var list = new ListBox();
                    list.Items.Add(MakeScaffolding(prop.GetValue(settings)));

                    DockPanel.SetDock(label, Dock.Left);
                    DockPanel.SetDock(list, Dock.Right);

                    element.Children.Add(label);
                    element.Children.Add(list);

                    stack.Children.Add(element);
                }
            }

            return stack;
        }
    }

    internal enum TestEnum
    {
        EOne,
        ETwo,
        EThree,
        EFour,
        EFive,
        ESix
    }

    internal class SettingsBase {}

    internal class TestSettings : SettingsBase
    {
        public ushort UShortField { get; set; } = 10;

        public short ShortField { get; set; } = 10;

        public uint UIntegerField { get; set; } = 10;

        public int IntegerField { get; set; } = 10;

        public ulong ULongField { get; set; } = 10;

        public long LongField { get; set; } = 10;

        public decimal DecimalField { get; set; } = 10.0m;

        public double DoubleField { get; set; } = 10.0d;

        public float FloatField { get; set; } = 10.0f;

        public bool BoolField { get; set; } = false;

        public char CharField { get; set; } = 'h';

        public string StringField { get; set; } = "Hello, World";

        public TestEnum EnumField { get; set; } = TestEnum.ESix;

        public Nested NestedField { get; set; } = new Nested();
    }

    internal class Nested
    {
        public ushort UShortField { get; set; } = 10;

        public short ShortField { get; set; } = 10;

        public uint UIntegerField { get; set; } = 10;

        public int IntegerField { get; set; } = 10;

        public ulong ULongField { get; set; } = 10;

        public long LongField { get; set; } = 10;

        public decimal DecimalField { get; set; } = 10.0m;

        public double DoubleField { get; set; } = 10.0d;

        public float FloatField { get; set; } = 10.0f;

        public bool BoolField { get; set; } = false;

        public char CharField { get; set; } = 'h';

        public string StringField { get; set; } = "Hello, World";

        public TestEnum EnumField { get; set; } = TestEnum.ESix;
    }
}
