using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using Application = System.Windows.Application;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using TabControl = System.Windows.Controls.TabControl;
using Locale = iBot_GUI.Resources;

namespace iBot_GUI.Forms
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            SourceInitialized += win_SourceInitialized;
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

        private void HomeTabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var tab = sender as TabControl;
            var i = tab?.SelectedItem as TabItem;

            UpdateTitleText(i?.Header.ToString());
        }

        #region Window Handle Stuff

        private void win_SourceInitialized(object sender, EventArgs e) {
            var handle = new WindowInteropHelper(this).Handle;
            var handleSource = HwndSource.FromHwnd(handle);
            handleSource?.AddHook(WindowProc);
        }

        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            if(msg != 0x0024 && msg != 0x0046)
                return IntPtr.Zero;

            if(msg == 0x0046) {
                var pos = (Windowpos) Marshal.PtrToStructure(lParam, typeof(Windowpos));
                if((pos.flags & 0x0002) != 0) {
                    return IntPtr.Zero;
                }

                var hwndSource = HwndSource.FromHwnd(hwnd);
                if(hwndSource != null) {
                    var wnd = (Window) hwndSource.RootVisual;
                    if(wnd == null) {
                        return IntPtr.Zero;
                    }
                }

                var changedPos = false;
                if(pos.cx < MinWidth) {
                    pos.cx = (int) MinWidth;
                    changedPos = true;
                }
                if(pos.cy < MinHeight) {
                    pos.cy = (int) MinHeight;
                    changedPos = true;
                }
                if(!changedPos) {
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

        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam) {
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

        private void Window_StateChanged(object sender, EventArgs e) {
            switch(WindowState) {
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

        private void btnActionMinimize_Click(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Minimized;
        }

        private void btnActionRestore_Click(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Normal;
        }

        private void btnActionMaximize_Click(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Maximized;
        }

        private void btnActionClose_Click(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }

        private void SwitchWindowState() {
            switch(WindowState) {
                case WindowState.Normal: {
                        WindowState = WindowState.Maximized;
                        break;
                    }
                case WindowState.Maximized: {
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

        private void headerThumb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if(e.ClickCount == 2) {
                if((ResizeMode == ResizeMode.CanResize) || (ResizeMode == ResizeMode.CanResizeWithGrip)) {
                    SwitchWindowState();
                }

                return;
            }
            if(WindowState == WindowState.Maximized) {
                _mRestoreIfMove = true;
                return;
            }

            DragMove();
        }

        private void headerThumb_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            _mRestoreIfMove = false;
        }

        private void headerThumb_MouseMove(object sender, MouseEventArgs e) {
            if(!_mRestoreIfMove)
                return;

            _mRestoreIfMove = false;

            var percentHorizontal = e.GetPosition(this).X / ActualWidth;
            var targetHorizontal = RestoreBounds.Width * percentHorizontal;

            var percentVertical = e.GetPosition(this).Y / ActualHeight;
            var targetVertical = RestoreBounds.Height * percentVertical;

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
        public struct Point {
            public int X;
            public int Y;

            public Point(int x, int y) {
                X = x;
                Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Minmaxinfo {
            public Point ptReserved;
            public Point ptMaxSize;
            public Point ptMaxPosition;
            public Point ptMinTrackSize;
            public Point ptMaxTrackSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Windowpos {
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