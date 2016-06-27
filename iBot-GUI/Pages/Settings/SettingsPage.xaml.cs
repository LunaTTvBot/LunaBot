using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using iBot_GUI.Annotations;
using iBot_GUI.Controls;

namespace iBot_GUI.Pages.Settings
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : UserControl, INotifyPropertyChanged
    {
        private Dictionary<string, SettingsBase> _settings;
        public Dictionary<string, SettingsBase> Settings
        {
            get { return _settings; }
            set
            {
                if (Equals(value, _settings))
                    return;
                _settings = value;
                OnPropertyChanged(nameof(Settings));
            }
        }

        public SettingsPage()
        {
            Settings = new Dictionary<string, SettingsBase>();

            InitializeComponent();

            Settings.Add("Test Settings", new TestSettings());
            OnPropertyChanged(nameof(Settings));

            //MainTabControl.Items.Add(new TabItem()
            //{
            //    Header = "TestSettings",
            //    Content = new SettingsControl(new TestSettings())
            //});
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum TestEnum
    {
        EOne,
        ETwo,
        EThree,
        EFour,
        EFive,
        ESix
    }

    public class BindingBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected virtual void SetField<T>(ref T field, T value, string name)
        {
            var prev = field;
            field = value;
            OnPropertyChanged(name);
            System.Console.WriteLine($"Property {name} changed from {prev} to {value}");
        }
    }

    public class SettingsBase : BindingBase {}

    public class TestSettings : SettingsBase
    {
        private ushort _uShortField = 10;
        private short _shortField = 10;
        private uint _uIntegerField = 10;
        private NestedOne _nestedOneField = new NestedOne();
        private TestEnum _enumField = TestEnum.ESix;
        private string _stringField = "Hello, World";
        private char _charField = 'h';
        private bool _boolField = false;
        private float _floatField = 10.0f;
        private double _doubleField = 10.0d;
        private decimal _decimalField = 10.0m;
        private long _longField = 10;
        private ulong _uLongField = 10;
        private int _integerField = 10;

        [Description("settings_description" + nameof(_uShortField))]
        public ushort UShortField
        {
            get { return _uShortField; }
            set { SetField(ref _uShortField, value, nameof(UShortField)); }
        }

        [Description("settings_description" + nameof(_shortField))]
        public short ShortField
        {
            get { return _shortField; }
            set { SetField(ref _shortField, value, nameof(ShortField)); }
        }

        [Description("settings_description" + nameof(_uIntegerField))]
        public uint UIntegerField
        {
            get { return _uIntegerField; }
            set { SetField(ref _uIntegerField, value, nameof(UIntegerField)); }
        }

        [Description("settings_description" + nameof(_uIntegerField))]
        public int IntegerField
        {
            get { return _integerField; }
            set { SetField(ref _integerField, value, nameof(IntegerField)); }
        }

        [Description("settings_description" + nameof(_uLongField))]
        public ulong ULongField
        {
            get { return _uLongField; }
            set { SetField(ref _uLongField, value, nameof(ULongField)); }
        }

        [Description("settings_description" + nameof(_longField))]
        public long LongField
        {
            get { return _longField; }
            set { SetField(ref _longField, value, nameof(LongField)); }
        }

        [Description("settings_description" + nameof(_decimalField))]
        public decimal DecimalField
        {
            get { return _decimalField; }
            set { SetField(ref _decimalField, value, nameof(DecimalField)); }
        }

        [Description("settings_description" + nameof(_doubleField))]
        public double DoubleField
        {
            get { return _doubleField; }
            set { SetField(ref _doubleField, value, nameof(DoubleField)); }
        }

        [Description("settings_description" + nameof(_floatField))]
        public float FloatField
        {
            get { return _floatField; }
            set { SetField(ref _floatField, value, nameof(FloatField)); }
        }

        [Description("settings_description" + nameof(_boolField))]
        public bool BoolField
        {
            get { return _boolField; }
            set { SetField(ref _boolField, value, nameof(BoolField)); }
        }

        [Description("settings_description" + nameof(_charField))]
        public char CharField
        {
            get { return _charField; }
            set { SetField(ref _charField, value, nameof(CharField)); }
        }

        [Description("settings_description" + nameof(_stringField))]
        public string StringField
        {
            get { return _stringField; }
            set { SetField(ref _stringField, value, nameof(StringField)); }
        }

        [Description("settings_description" + nameof(_enumField))]
        public TestEnum EnumField
        {
            get { return _enumField; }
            set { SetField(ref _enumField, value, nameof(EnumField)); }
        }

        [Description("settings_description" + nameof(_nestedOneField))]
        public NestedOne NestedOneField
        {
            get { return _nestedOneField; }
            set { SetField(ref _nestedOneField, value, nameof(NestedOneField)); }
        }
    }

    public class NestedOne : BindingBase
    {
        private ushort _uShortField = 10;
        private short _shortField = 10;
        private uint _uIntegerField = 10;
        private NestedTwo _nestedTwoField = new NestedTwo();
        private TestEnum _enumField = TestEnum.ESix;
        private string _stringField = "Hello, World";
        private char _charField = 'h';
        private bool _boolField = false;
        private float _floatField = 10.0f;
        private double _doubleField = 10.0d;
        private decimal _decimalField = 10.0m;
        private long _longField = 10;
        private ulong _uLongField = 10;
        private int _integerField = 10;

        public ushort UShortField
        {
            get { return _uShortField; }
            set { SetField(ref _uShortField, value, nameof(UShortField)); }
        }

        public short ShortField
        {
            get { return _shortField; }
            set { SetField(ref _shortField, value, nameof(ShortField)); }
        }

        public uint UIntegerField
        {
            get { return _uIntegerField; }
            set { SetField(ref _uIntegerField, value, nameof(UIntegerField)); }
        }

        public int IntegerField
        {
            get { return _integerField; }
            set { SetField(ref _integerField, value, nameof(IntegerField)); }
        }

        public ulong ULongField
        {
            get { return _uLongField; }
            set { SetField(ref _uLongField, value, nameof(ULongField)); }
        }

        public long LongField
        {
            get { return _longField; }
            set { SetField(ref _longField, value, nameof(LongField)); }
        }

        public decimal DecimalField
        {
            get { return _decimalField; }
            set { SetField(ref _decimalField, value, nameof(DecimalField)); }
        }

        public double DoubleField
        {
            get { return _doubleField; }
            set { SetField(ref _doubleField, value, nameof(DoubleField)); }
        }

        public float FloatField
        {
            get { return _floatField; }
            set { SetField(ref _floatField, value, nameof(FloatField)); }
        }

        public bool BoolField
        {
            get { return _boolField; }
            set { SetField(ref _boolField, value, nameof(BoolField)); }
        }

        public char CharField
        {
            get { return _charField; }
            set { SetField(ref _charField, value, nameof(CharField)); }
        }

        public string StringField
        {
            get { return _stringField; }
            set { SetField(ref _stringField, value, nameof(StringField)); }
        }

        public TestEnum EnumField
        {
            get { return _enumField; }
            set { SetField(ref _enumField, value, nameof(EnumField)); }
        }

        public NestedTwo NestedTwoField
        {
            get { return _nestedTwoField; }
            set { SetField(ref _nestedTwoField, value, nameof(NestedTwoField)); }
        }
    }

    public class NestedTwo : BindingBase
    {
        private ushort _uShortField = 10;
        private short _shortField = 10;
        private uint _uIntegerField = 10;
        private TestEnum _enumField = TestEnum.ESix;
        private string _stringField = "Hello, World";
        private char _charField = 'h';
        private bool _boolField = false;
        private float _floatField = 10.0f;
        private double _doubleField = 10.0d;
        private decimal _decimalField = 10.0m;
        private long _longField = 10;
        private ulong _uLongField = 10;
        private int _integerField = 10;

        public ushort UShortField
        {
            get { return _uShortField; }
            set { SetField(ref _uShortField, value, nameof(UShortField)); }
        }

        public short ShortField
        {
            get { return _shortField; }
            set { SetField(ref _shortField, value, nameof(ShortField)); }
        }

        public uint UIntegerField
        {
            get { return _uIntegerField; }
            set { SetField(ref _uIntegerField, value, nameof(UIntegerField)); }
        }

        public int IntegerField
        {
            get { return _integerField; }
            set { SetField(ref _integerField, value, nameof(IntegerField)); }
        }

        public ulong ULongField
        {
            get { return _uLongField; }
            set { SetField(ref _uLongField, value, nameof(ULongField)); }
        }

        public long LongField
        {
            get { return _longField; }
            set { SetField(ref _longField, value, nameof(LongField)); }
        }

        public decimal DecimalField
        {
            get { return _decimalField; }
            set { SetField(ref _decimalField, value, nameof(DecimalField)); }
        }

        public double DoubleField
        {
            get { return _doubleField; }
            set { SetField(ref _doubleField, value, nameof(DoubleField)); }
        }

        public float FloatField
        {
            get { return _floatField; }
            set { SetField(ref _floatField, value, nameof(FloatField)); }
        }

        public bool BoolField
        {
            get { return _boolField; }
            set { SetField(ref _boolField, value, nameof(BoolField)); }
        }

        public char CharField
        {
            get { return _charField; }
            set { SetField(ref _charField, value, nameof(CharField)); }
        }

        public string StringField
        {
            get { return _stringField; }
            set { SetField(ref _stringField, value, nameof(StringField)); }
        }

        public TestEnum EnumField
        {
            get { return _enumField; }
            set { SetField(ref _enumField, value, nameof(EnumField)); }
        }
    }
}
