using System.Windows;
using System.Windows.Controls;
using iBot_GUI.Utilities;
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

        private static void PropertyChangedCallback(DependencyObject dpObject,
                                                    DependencyPropertyChangedEventArgs eventArgs)
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

            foreach (var uiElement in EditorElementGenerator.GenerateUiElementRecursive(Settings))
                MainStack.Children.Add(uiElement);
        }
    }
}