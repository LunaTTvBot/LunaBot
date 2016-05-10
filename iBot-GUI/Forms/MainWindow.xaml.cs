using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using IBot.Events;
using IBot.Events.CustomEventArgs;
using IBot.Misc;
using Application = System.Windows.Application;
using Locale = iBot_GUI.Resources;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using TabControl = System.Windows.Controls.TabControl;

namespace iBot_GUI.Forms
{
    /// <summary>
    ///     Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly DispatcherTimer _statusUpdateTimer = new DispatcherTimer();
        private bool _connected;

        public MainWindow()
        {
            InitializeComponent();
            SourceInitialized += win_SourceInitialized;

            var red = new Uri("pack://Application:,,,/Template/Themes/Color/Red.xaml", UriKind.RelativeOrAbsolute);
            Application.Current.Resources.MergedDictionaries.Remove(new ResourceDictionary {Source = red});

            ConnectionManager.BotConnectedEvent += ConnectionManagerOnBotConnectedEvent;
            ConnectionManager.BotDisconnectedEvent += ConnectionManagerOnBotDisconnectedEvent;
            ErrorManager.UserLoginErrorEvent += OnUserLoginErrorEvent;
        }

        private static void OnUserLoginErrorEvent(object sender, ErrorEventArgs eArgs)
        {
            MessageBox.Show(eArgs.Message);

            ConnectionManager.DisconnectFromBotAccount();
        }

        private void ConnectionManagerOnBotConnectedEvent(object sender, ConnectionEventArgs connectedEventArgs)
        {
            var green = new Uri("pack://Application:,,,/Template/Themes/Color/Green.xaml", UriKind.RelativeOrAbsolute);
            var red = new Uri("pack://Application:,,,/Template/Themes/Color/Red.xaml", UriKind.RelativeOrAbsolute);
            var b = ConnectButton;

            if (_connected) return;

            Application.Current.Resources.MergedDictionaries.Remove(new ResourceDictionary {Source = red});
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary {Source = green});
            if (b != null) b.Content = Locale.Main.disconnect;
            UpdateStatusText(Locale.Main.connected);
            _connected = true;

            ToggleConnectionInputs();
        }

        private void ConnectionManagerOnBotDisconnectedEvent(object sender, ConnectionEventArgs connectedEventArgs)
        {
            var green = new Uri("pack://Application:,,,/Template/Themes/Color/Green.xaml", UriKind.RelativeOrAbsolute);
            var red = new Uri("pack://Application:,,,/Template/Themes/Color/Red.xaml", UriKind.RelativeOrAbsolute);
            var b = ConnectButton;

            if (!_connected) return;

            Application.Current.Resources.MergedDictionaries.Remove(new ResourceDictionary {Source = green});
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary {Source = red});
            if (b != null)
            {
                if(Dispatcher.CheckAccess()) {
                    b.Content = Locale.Main.connect;
                } else
                {
                    Dispatcher.Invoke(() => b.Content = Locale.Main.connect);
                }
            }
            UpdateStatusText(Locale.Main.disconnected);
            _connected = false;

            ToggleConnectionInputs();
        }

        private void ToggleConnectionInputs()
        {
            var en = _connected == false;

            var page = TheStartPage;

            if(Dispatcher.CheckAccess()) {
                page.ChannelList.IsEnabled = en;
                page.ChannelTextBox.IsEnabled = en;
                page.AddChannelButton.IsEnabled = en;
                page.RemoveChannelButton.IsEnabled = en;
                page.TokenBox.IsEnabled = en;
                page.ApplyTokenButton.IsEnabled = en;
                page.NickBox.IsEnabled = en;
            } else {
                Dispatcher.Invoke(() =>
                {
                    page.ChannelList.IsEnabled = en;
                    page.ChannelTextBox.IsEnabled = en;
                    page.AddChannelButton.IsEnabled = en;
                    page.RemoveChannelButton.IsEnabled = en;
                    page.TokenBox.IsEnabled = en;
                    page.ApplyTokenButton.IsEnabled = en;
                    page.NickBox.IsEnabled = en;
                });
            }
        }

        public void UpdateTitleText(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                Title = string.Format(Locale.Main.bot_name);
                MainTitle.Text = string.Format(Locale.Main.bot_name);
                return;
            }

            Title = string.Format(Locale.Main.window_title, title.Trim());
            MainTitle.Text = string.Format(Locale.Main.window_title, title.Trim());
        }

        public void UpdateStatusText(string status)
        {
            if(Dispatcher.CheckAccess()) {
                Status.Text = status;
            } else {
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

        private void HomeTabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tab = sender as TabControl;
            var i = tab?.SelectedItem as TabItem;

            UpdateTitleText(i?.Header.ToString());
        }

        private void ConnectButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_connected == false)
            {
                try
                {
                    ConnectionManager.ConnectToBotAccount();
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show(ex.Message);
                }

                return;
            }

            ConnectionManager.DisconnectFromBotAccount();
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
                    BorderFrame.BorderThickness = new Thickness(1, 1, 1, 23);

                    BtnActionRestore.Visibility = Visibility.Collapsed;
                    BtnActionMaxamize.Visibility = Visibility.Visible;
                    break;
                case WindowState.Maximized:
                    BorderFrame.BorderThickness = new Thickness(0, 0, 0, 23);

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
            if (!_mRestoreIfMove) return;

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
}