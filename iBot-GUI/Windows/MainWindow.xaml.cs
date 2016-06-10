using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using Locale = iBot_GUI.Resources;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using TabControl = System.Windows.Controls.TabControl;

namespace iBot_GUI.Windows
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public delegate void OnClipboardChangeEventHandler(ClipboardFormat format, object data);

        private static readonly string[] Formats = Enum.GetNames(typeof(ClipboardFormat));
        private readonly DispatcherTimer _statusUpdateTimer = new DispatcherTimer();

        /// <summary>
        ///     Next clipboard viewer window
        /// </summary>
        private IntPtr _hWndNextViewer;

        /// <summary>
        ///     The <see cref="HwndSource" /> for this window.
        /// </summary>
        private HwndSource _hWndSource;

        public bool IsViewing;

        public MainWindow()
        {
            InitializeComponent();
            SourceInitialized += win_SourceInitialized;
            Loaded += (_, __) => InitCbViewer();
        }

        public static event OnClipboardChangeEventHandler OnClipboardChange;

        private void InitCbViewer()
        {
            var wih = new WindowInteropHelper(this);
            _hWndSource = HwndSource.FromHwnd(wih.Handle);

            if (_hWndSource != null)
            {
                _hWndSource.AddHook(WinProc); // start processing window messages
                _hWndNextViewer = Win32.SetClipboardViewer(_hWndSource.Handle); // set this window as a viewer
            }
            IsViewing = true;
        }

        private void CloseCbViewer()
        {
            // remove this window from the clipboard viewer chain
            Win32.ChangeClipboardChain(_hWndSource.Handle, _hWndNextViewer);

            _hWndNextViewer = IntPtr.Zero;
            _hWndSource.RemoveHook(WinProc);
            IsViewing = false;
        }

        private IntPtr WinProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (msg)
            {
                case Win32.WmChangecbchain:
                    if (wParam == _hWndNextViewer)
                    {
                        // clipboard viewer chain changed, need to fix it.
                        _hWndNextViewer = lParam;
                    }
                    else if (_hWndNextViewer != IntPtr.Zero)
                    {
                        // pass the message to the next viewer.
                        Win32.SendMessage(_hWndNextViewer, msg, wParam, lParam);
                    }
                    break;
                case Win32.WmDrawclipboard:
                    // clipboard content changed
                    ClipChanged();
                    // pass the message to the next viewer.
                    Win32.SendMessage(_hWndNextViewer, msg, wParam, lParam);
                    break;
            }

            return IntPtr.Zero;
        }

        private void ClipChanged()
        {
            if(!IsViewing) return;

            var iData = Clipboard.GetDataObject();            
            ClipboardFormat? format = null;

            foreach (var f in Formats)
            {
                if (iData == null || !iData.GetDataPresent(f)) continue;

                format = (ClipboardFormat) Enum.Parse(typeof(ClipboardFormat), f);
                break;
            }

            var data = iData?.GetData(format.ToString());

            if (data == null || format == null)
                return;

            OnClipboardChange?.Invoke((ClipboardFormat) format, data);
        }

        public void UpdateTitleText(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                Title = Locale.Main.app_name;
                MainTitle.Text = Locale.Main.app_name;
                return;
            }

            Title = string.Format(Locale.Main.app_title_format, title.Trim());
            MainTitle.Text = string.Format(Locale.Main.app_title_format, title.Trim());
        }

        private void HomeTabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tab = sender as TabControl;
            var i = tab?.SelectedItem as TabItem;

            UpdateTitleText(i?.Header.ToString());
        }

        public void UpdateStatusText(string status)
        {
            if (Dispatcher.CheckAccess())
            {
                Status.Text = status;
            }
            else
            {
                Dispatcher.Invoke(() => Status.Text = status);
            }

            _statusUpdateTimer.Stop();
            _statusUpdateTimer.Interval = new TimeSpan(0, 0, 0, 4);
            _statusUpdateTimer.Tick += statusUpdateCleaner_Clear;
            _statusUpdateTimer.Start();
        }

        private void statusUpdateCleaner_Clear(object sender, EventArgs e)
        {
            Status.Text = Locale.Main.status_base;
        }

        #region Window Handle Stuff

        private void win_SourceInitialized(object sender, EventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;
            var handleSource = HwndSource.FromHwnd(handle);
            handleSource?.AddHook(WindowProc);
        }

        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != 0x0024 && msg != 0x0046)
                return IntPtr.Zero;

            if (msg == 0x0046)
            {
                var pos = (Windowpos) Marshal.PtrToStructure(lParam, typeof(Windowpos));
                if ((pos.flags & 0x0002) != 0)
                {
                    return IntPtr.Zero;
                }

                var hwndSource = HwndSource.FromHwnd(hwnd);
                if (hwndSource != null)
                {
                    var wnd = (Window) hwndSource.RootVisual;
                    if (wnd == null)
                    {
                        return IntPtr.Zero;
                    }
                }

                var changedPos = false;
                if (pos.cx < MinWidth)
                {
                    pos.cx = (int) MinWidth;
                    changedPos = true;
                }
                if (pos.cy < MinHeight)
                {
                    pos.cy = (int) MinHeight;
                    changedPos = true;
                }
                if (!changedPos)
                {
                    return IntPtr.Zero;
                }

                Marshal.StructureToPtr(pos, lParam, true);
                handled = true;
                return IntPtr.Zero;
            }

            WmGetMinMaxInfo(hwnd, lParam);
            handled = true;
            return IntPtr.Zero;
        }

        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            var mmi = (Minmaxinfo) Marshal.PtrToStructure(lParam, typeof(Minmaxinfo));

            var currentScreen = Screen.FromHandle(hwnd);
            var workArea = currentScreen.WorkingArea;
            var monitorArea = currentScreen.Bounds;
            mmi.ptMaxPosition.X = Math.Abs(workArea.Left - monitorArea.Left);
            mmi.ptMaxPosition.Y = Math.Abs(workArea.Top - monitorArea.Top);
            mmi.ptMaxSize.X = Math.Abs(workArea.Right - workArea.Left);
            mmi.ptMaxSize.Y = Math.Abs(workArea.Bottom - workArea.Top);

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            switch (WindowState)
            {
                case WindowState.Normal:
                    // BorderFrame.BorderThickness = new Thickness(1, 1, 1, 23);

                    BtnActionRestore.Visibility = Visibility.Collapsed;
                    BtnActionMaxamize.Visibility = Visibility.Visible;
                    break;
                case WindowState.Maximized:
                    // BorderFrame.BorderThickness = new Thickness(0, 0, 0, 23);

                    BtnActionRestore.Visibility = Visibility.Visible;
                    BtnActionMaxamize.Visibility = Visibility.Collapsed;
                    break;
                case WindowState.Minimized:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void btnActionMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnActionRestore_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Normal;
        }

        private void btnActionMaximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
        }

        private void btnActionClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void SwitchWindowState()
        {
            switch (WindowState)
            {
                case WindowState.Normal:
                {
                    WindowState = WindowState.Maximized;
                    break;
                }
                case WindowState.Maximized:
                {
                    WindowState = WindowState.Normal;
                    break;
                }
                case WindowState.Minimized:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool _mRestoreIfMove;

        private void headerThumb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if ((ResizeMode == ResizeMode.CanResize) || (ResizeMode == ResizeMode.CanResizeWithGrip))
                {
                    SwitchWindowState();
                }

                return;
            }
            if (WindowState == WindowState.Maximized)
            {
                _mRestoreIfMove = true;
                return;
            }

            DragMove();
        }

        private void headerThumb_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _mRestoreIfMove = false;
        }

        private void headerThumb_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_mRestoreIfMove)
                return;

            _mRestoreIfMove = false;

            var percentHorizontal = e.GetPosition(this).X/ActualWidth;
            var targetHorizontal = RestoreBounds.Width*percentHorizontal;

            var percentVertical = e.GetPosition(this).Y/ActualHeight;
            var targetVertical = RestoreBounds.Height*percentVertical;

            WindowState = WindowState.Normal;

            Point lMousePosition;
            GetCursorPos(out lMousePosition);

            Left = lMousePosition.X - targetHorizontal;
            Top = lMousePosition.Y - targetVertical;

            DragMove();
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out Point lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public int X;
            public int Y;

            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Minmaxinfo
        {
            public Point ptReserved;
            public Point ptMaxSize;
            public Point ptMaxPosition;
            public Point ptMinTrackSize;
            public Point ptMaxTrackSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Windowpos
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int flags;
        }

        #endregion
    }

    /// <summary>
    ///     This static class holds the Win32 function declarations and constants needed by
    ///     this sample application.
    /// </summary>
    internal static class Win32
    {
        /// <summary>
        ///     The WM_DRAWCLIPBOARD message notifies a clipboard viewer window that
        ///     the content of the clipboard has changed.
        /// </summary>
        internal const int WmDrawclipboard = 0x0308;

        /// <summary>
        ///     A clipboard viewer window receives the WM_CHANGECBCHAIN message when
        ///     another window is removing itself from the clipboard viewer chain.
        /// </summary>
        internal const int WmChangecbchain = 0x030D;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
    }

    public enum ClipboardFormat : byte
    {
        /// <summary>
        ///     Specifies the standard ANSI text format. This static field is read-only.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        Text,

        /// <summary>
        ///     Specifies the standard Windows Unicode text format. This static field
        ///     is read-only.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        UnicodeText,

        /// <summary>
        ///     Specifies the Windows device-independent bitmap (DIB) format. This static
        ///     field is read-only.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        Dib,

        /// <summary>Specifies a Windows bitmap format. This static field is read-only.</summary>
        /// <filterpriority>1</filterpriority>
        Bitmap,

        /// <summary>
        ///     Specifies the Windows enhanced metafile format. This static field is
        ///     read-only.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        EnhancedMetafile,

        /// <summary>
        ///     Specifies the Windows metafile format, which Windows Forms does not
        ///     directly use. This static field is read-only.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        MetafilePict,

        /// <summary>
        ///     Specifies the Windows symbolic link format, which Windows Forms does
        ///     not directly use. This static field is read-only.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        SymbolicLink,

        /// <summary>
        ///     Specifies the Windows Data Interchange Format (DIF), which Windows Forms
        ///     does not directly use. This static field is read-only.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        Dif,

        /// <summary>
        ///     Specifies the Tagged Image File Format (TIFF), which Windows Forms does
        ///     not directly use. This static field is read-only.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        Tiff,

        /// <summary>
        ///     Specifies the standard Windows original equipment manufacturer (OEM)
        ///     text format. This static field is read-only.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        OemText,

        /// <summary>
        ///     Specifies the Windows palette format. This static field is read-only.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        Palette,

        /// <summary>
        ///     Specifies the Windows pen data format, which consists of pen strokes
        ///     for handwriting software, Windows Forms does not use this format. This static
        ///     field is read-only.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        PenData,

        /// <summary>
        ///     Specifies the Resource Interchange File Format (RIFF) audio format,
        ///     which Windows Forms does not directly use. This static field is read-only.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        Riff,

        /// <summary>
        ///     Specifies the wave audio format, which Windows Forms does not directly
        ///     use. This static field is read-only.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        WaveAudio,

        /// <summary>
        ///     Specifies the Windows file drop format, which Windows Forms does not
        ///     directly use. This static field is read-only.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        FileDrop,

        /// <summary>
        ///     Specifies the Windows culture format, which Windows Forms does not directly
        ///     use. This static field is read-only.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        Locale,

        /// <summary>
        ///     Specifies text consisting of HTML data. This static field is read-only.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        Html,

        /// <summary>
        ///     Specifies text consisting of Rich Text Format (RTF) data. This static
        ///     field is read-only.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        Rtf,

        /// <summary>
        ///     Specifies a comma-separated value (CSV) format, which is a common interchange
        ///     format used by spreadsheets. This format is not used directly by Windows Forms.
        ///     This static field is read-only.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        CommaSeparatedValue,

        /// <summary>
        ///     Specifies the Windows Forms string class format, which Windows Forms
        ///     uses to store string objects. This static field is read-only.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        StringFormat,

        /// <summary>
        ///     Specifies a format that encapsulates any type of Windows Forms object.
        ///     This static field is read-only.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        Serializable
    }
}