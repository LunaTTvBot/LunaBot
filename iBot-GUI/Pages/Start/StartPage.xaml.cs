using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using iBot_GUI.Windows;
using IBot.Facades.Connection;
using IBot.Facades.Core;
using IBot.Facades.Events;
using IBot.Facades.Events.Args.User;

namespace iBot_GUI.Pages.Start
{
    public partial class StartPage
    {
        private readonly DispatcherTimer _userListTimer;
        private bool _copied;
        private Paragraph _paragraph;

        public StartPage()
        {
            InitializeComponent();
            UserEvents.UserPublicMessageEvent += ExtentInformationBox;

            InfoBox.Document = new FlowDocument();
            InfoBox.IsDocumentEnabled = true;

            CenterText();

            DataContext = this;

            MainWindow.OnClipboardChange += MainWindowOnOnClipboardChange;

            _userListTimer = new DispatcherTimer {Interval = new TimeSpan(0, 0, 10)};
            _userListTimer.Tick += UserListTimerOnTick;
        }

        private void UserListTimerOnTick(object sender, EventArgs eventArgs)
        {
            if (Dispatcher.CheckAccess())
            {
                ChatterList.ItemsSource =
                    ChatterList.ItemsSource =
                        UserList.GetUsers(SettingsManager.GetConnectionSettings().Value.ChannelList.First())
                            .Select(user => user.Name)
                            .ToList();
            }
            else
            {
                Dispatcher.Invoke(
                    () =>
                        ChatterList.ItemsSource =
                            UserList.GetUsers(SettingsManager.GetConnectionSettings().Value.ChannelList.First())
                                .Select(user => user.Name)
                                .ToList());
            }

            ChatterList.SelectedItem = null;
        }

        private static bool IsUserVisible(FrameworkElement element, FrameworkElement container)
        {
            if (!element.IsVisible)
                return false;

            var bounds =
                element.TransformToAncestor(container)
                    .TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
            var rect = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);
            return rect.Contains(bounds.TopLeft) || rect.Contains(bounds.BottomRight);
        }

        private void MainWindowOnOnClipboardChange(ClipboardFormat format, object data)
        {
            try
            {
                if (_copied)
                {
                    _copied = false;
                    return;
                }

                if (Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive) == null)
                    return;
                if (!IsUserVisible(this, Window.GetWindow(this)))
                    return;

                var ms = new MemoryStream();
                var fd = new FlowDocument();

                if (!string.IsNullOrEmpty(InfoBox.Selection.Text))
                    InfoBox.Selection.Save(ms, DataFormats.XamlPackage);

                var finalPara = new Paragraph();
                var fdRange = new TextRange(fd.ContentStart, fd.ContentEnd);
                fdRange.Load(ms, DataFormats.XamlPackage);

                var count = 0;
                while (fd.Blocks.Count != 0)
                {
                    if (count > 0)
                    {
                        finalPara.Inlines.Add("\r\n");
                    }
                    var block = fd.Blocks.FirstBlock;
                    var blockPara = block as Paragraph;
                    if (blockPara != null)
                    {
                        var paragraph = blockPara;
                        var thePara = paragraph;
                        while (thePara.Inlines.Count != 0)
                        {
                            var inline = thePara.Inlines.FirstInline;
                            var container = inline as InlineUIContainer;
                            if (container != null)
                            {
                                var uiContainer = container;
                                var child = uiContainer.Child as Image;
                                if (child != null)
                                {
                                    var image = child;
                                    finalPara.Inlines.Add(image.ToolTip.ToString());
                                    thePara.Inlines.Remove(container);
                                }
                                else
                                {
                                    finalPara.Inlines.Add(container);
                                }
                            }
                            else
                            {
                                finalPara.Inlines.Add(inline);
                            }
                        }

                        fd.Blocks.Remove(blockPara);
                    }
                    else
                    {
                        fd.Blocks.Remove(block);
                    }

                    count++;
                }

                var newRange = new TextRange(finalPara.ContentStart, finalPara.ContentEnd);
                Clipboard.SetText(newRange.Text);
                _copied = true;
            }
            catch (Exception e)
            {
                Trace.WriteLine("Copy Error: " + e.Message);
            }
        }

        public void CenterText()
        {
            var text = new TextRange(InfoBox.Document.ContentStart, InfoBox.Document.ContentEnd);
            text.ApplyPropertyValue(Inline.BaselineAlignmentProperty, BaselineAlignment.Center);
        }

        private void ExtentInformationBox(object sender, PublicMessageEventArgs e)
        {
            if (Dispatcher.CheckAccess())
            {
                AppendText(e);
            }
            else
            {
                Dispatcher.Invoke(() => AppendText(e));
            }
        }

        private void AppendText(PublicMessageEventArgs publicMessageEventArgsFacade)
        {
            var from = !string.IsNullOrEmpty(publicMessageEventArgsFacade.Tags?.DisplayName)
                ? publicMessageEventArgsFacade.Tags.DisplayName
                : publicMessageEventArgsFacade.UserName;
            var text = publicMessageEventArgsFacade.Message;
            var brush = !string.IsNullOrEmpty(publicMessageEventArgsFacade.Tags?.Color)
                ? (SolidColorBrush) new BrushConverter().ConvertFrom(publicMessageEventArgsFacade.Tags?.Color)
                : Brushes.Red;
            _paragraph = new Paragraph();
            _paragraph.Inlines.Add(new Run(text)
            {
                BaselineAlignment = BaselineAlignment.Center
            });

            Paragraph parsedPara = null;
            if (!string.IsNullOrEmpty(publicMessageEventArgsFacade.Tags?.Emotes))
                parsedPara = ParseSmileys(_paragraph, publicMessageEventArgsFacade.Tags.Emotes);

            var p = new Paragraph();

            p.Inlines.Add(new Run(DateTime.Now.ToString("HH:mm:ss") + " | ")
            {
                FontFamily = new FontFamily("Consolas"),
                BaselineAlignment = BaselineAlignment.Center
            });

            p.Inlines.Add(new Bold(new Run(from + ": "))
            {
                Foreground = brush,
                BaselineAlignment = BaselineAlignment.Center
            });

            if (parsedPara != null)
            {
                while (parsedPara.Inlines.Count != 0)
                {
                    p.Inlines.Add(parsedPara.Inlines.FirstInline);
                }
            }

            InfoBox.Document.Blocks.Add(p);
            Thread.Sleep(100);

            if (InfoBox.VerticalOffset + InfoBox.ActualHeight > InfoBox.ExtentHeight)
                InfoBox.ScrollToEnd();
        }

        private static Paragraph ParseSmileys(TextElement element, string emoteTag)
        {
            var fullRange = new TextRange(element.ContentStart, element.ContentEnd);

            var builder = new Paragraph();

            var emotes = ParseEmotes(emoteTag);
            if (emotes.Count <= 0) return null;

            emotes = emotes.OrderBy(e => e.Start).ToList();

            var messageIndex = 0;
            for (var emoteIndex = 0; emoteIndex < emotes.Count(); emoteIndex++)
            {
                var emote = emotes[emoteIndex];

                var start = fullRange.Start.GetPositionAtOffset(messageIndex);
                if (start == null)
                    continue;
                var end = fullRange.Start.GetPositionAtOffset(emote.Start);
                if (end == null)
                    continue;

                var range = new TextRange(start, end);

                var ms = new MemoryStream();
                range.Save(ms, DataFormats.XamlPackage);
                var flowDocumentCopy = new FlowDocument();
                var copyRange = new TextRange(flowDocumentCopy.ContentStart, flowDocumentCopy.ContentEnd);
                copyRange.Load(ms, DataFormats.XamlPackage);

                builder.Inlines.Add(new Run(copyRange.Text)
                {
                    BaselineAlignment = BaselineAlignment.Center
                });

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(@"http://static-cdn.jtvnw.net/emoticons/v1/" + emote.Id + @"/1.0",
                    UriKind.Absolute);
                bitmap.EndInit();

                builder.Inlines.Add(new Image
                {
                    Source = bitmap,
                    ToolTip = range.Text,
                    Tag = range.Text,
                    MinWidth = 20,
                    MinHeight = 20,
                    MaxWidth = 32,
                    MaxHeight = 32,
                    Stretch = Stretch.None
                });

                messageIndex = emote.End + 1;
            }

            if (messageIndex == fullRange.Text.Length) return builder;
            {
                var start = fullRange.Start.GetPositionAtOffset(messageIndex);
                var end = fullRange.Start.GetPositionAtOffset(fullRange.Text.Length);

                var range = new TextRange(start, end);

                var ms = new MemoryStream();
                range.Save(ms, DataFormats.XamlPackage);
                var flowDocumentCopy = new FlowDocument();
                var copyRange = new TextRange(flowDocumentCopy.ContentStart, flowDocumentCopy.ContentEnd);
                copyRange.Load(ms, DataFormats.XamlPackage);

                builder.Inlines.Add(new Run(copyRange.Text) {
                    BaselineAlignment = BaselineAlignment.Center
                });
            }

            return builder;
        }

        /// <summary>
        ///     Replacing the text range with image.
        /// </summary>
        /// <param name="textRange">The smiley text range.</param>
        /// <param name="image">The smiley icon</param>
        public void ReplaceTextRangeWithImage(TextRange textRange, Image image)
        {
            if (!(textRange.Start.Parent is Run)) return;
            var run = (Run) textRange.Start.Parent;

            var runBefore =
                new Run(new TextRange(run.ContentStart, textRange.Start).Text)
                {
                    BaselineAlignment = BaselineAlignment.Center
                };
            var runAfter =
                new Run(new TextRange(textRange.End, run.ContentEnd).Text)
                {
                    BaselineAlignment = BaselineAlignment.Center
                };

            if (textRange.Start.Paragraph == null) return;
            textRange.Start.Paragraph.Inlines.Add(runBefore);
            textRange.Start.Paragraph.Inlines.Add(image);
            textRange.Start.Paragraph.Inlines.Add(runAfter);
            textRange.Start.Paragraph.Inlines.Remove(run);
        }

        private static List<Emote> ParseEmotes(string tagEmotes)
        {
            var emotes = tagEmotes.Split('/');
            var list = new List<Emote>();

            foreach (var emote in emotes)
            {
                var parts = emote.Split(',');
                var lastId = 0;

                foreach (var part in parts)
                {
                    string[] startEnd;
                    int id;
                    if (part.Contains(":"))
                    {
                        var elements = part.Split(':');
                        id = Convert.ToInt32(elements[0]);
                        startEnd = elements[1].Split('-');
                        lastId = id;
                    }
                    else
                    {
                        id = Convert.ToInt32(lastId);
                        startEnd = part.Split('-');
                    }
                    var start = Convert.ToInt32(startEnd[0]);
                    var end = Convert.ToInt32(startEnd[1]);
                    list.Add(new Emote(id, start, end));
                }
            }

            return list;
        }

        private void UIElement_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) return;

            var text = InputBox.Text;
            if (!string.IsNullOrEmpty(text))
            {
                Connection.WritePublicChatMessage(new string(text.ToCharArray()),
                    SettingsManager.GetConnectionSettings().Value.ChannelList.First());
                InputBox.Text = "";
            }
        }

        private void StartPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            _userListTimer.Start();
        }
    }

    internal class Emote
    {
        public Emote(int id, int start, int end)
        {
            Id = id;
            Start = start;
            End = end;
        }

        public int Id { get; }
        public int Start { get; }
        public int End { get; }
    }
}