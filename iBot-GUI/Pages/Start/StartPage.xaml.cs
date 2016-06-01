using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using iBot_GUI.Windows;
using IBot.Facades.Events;
using IBot.Facades.Events.Args.User;

namespace iBot_GUI.Pages.Start
{
    public partial class StartPage
    {
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
            if (_copied)
            {
                _copied = false;
                return;
            }

            if (Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive) == null) return;
            if (!IsUserVisible(this, Window.GetWindow(this))) return;

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

            if (!string.IsNullOrEmpty(publicMessageEventArgsFacade.Tags?.Emotes))
                ParseSmileys(_paragraph, publicMessageEventArgsFacade.Tags.Emotes);

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

            while (_paragraph.Inlines.Count != 0)
            {
                p.Inlines.Add(_paragraph.Inlines.FirstInline);
            }

            InfoBox.Document.Blocks.Add(p);
            Thread.Sleep(100);

            if (InfoBox.VerticalOffset + InfoBox.ActualHeight > InfoBox.ExtentHeight)
                InfoBox.ScrollToEnd();
        }

        private void ParseSmileys(TextElement element, string emoteTag)
        {
            var fullRange = new TextRange(element.ContentStart, element.ContentEnd);

            var emoteList = ParseEmotes(emoteTag);
            if (emoteList.Count > 0)
            {
                emoteList.ForEach(emote =>
                {
                    var start = fullRange.Start.GetPositionAtOffset(emote.Start);
                    if (start == null) return;
                    var end = fullRange.Start.GetPositionAtOffset(emote.End + 1);
                    if (end == null) return;

                    var range = new TextRange(start, end);

                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(@"http://static-cdn.jtvnw.net/emoticons/v1/" + emote.Id + @"/1.0",
                        UriKind.Absolute);
                    bitmap.EndInit();

                    ReplaceTextRangeWithImage(range,
                        new Image {Source = bitmap, ToolTip = range.Text, Tag = range.Text, Stretch = Stretch.None});
                });
            }
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
    }

    public static class WordBreaker
    {
        /// <summary>
        ///     Returns a TextRange covering a word containing or following this TextPointer.
        /// </summary>
        /// <remarks>
        ///     If this TextPointer is within a word or at start of word, the containing word range is returned.
        ///     If this TextPointer is between two words, the following word range is returned.
        ///     If this TextPointer is at trailing word boundary, the following word range is returned.
        /// </remarks>
        public static TextRange GetWordRange(TextPointer position)
        {
            TextRange wordRange = null;
            TextPointer wordStartPosition = null;

            // Go forward first, to find word end position.
            var wordEndPosition = GetPositionAtWordBoundary(position, /*wordBreakDirection*/LogicalDirection.Forward);

            if (wordEndPosition != null)
            {
                // Then travel backwards, to find word start position.
                wordStartPosition = GetPositionAtWordBoundary(wordEndPosition, /*wordBreakDirection*/
                    LogicalDirection.Backward);
            }

            if (wordStartPosition != null)
            {
                wordRange = new TextRange(wordStartPosition, wordEndPosition);
            }

            return wordRange;
        }

        /// <summary>
        ///     1.  When wordBreakDirection = Forward, returns a position at the end of the word,
        ///     i.e. a position with a wordBreak character (space) following it.
        ///     2.  When wordBreakDirection = Backward, returns a position at the start of the word,
        ///     i.e. a position with a wordBreak character (space) preceeding it.
        ///     3.  Returns null when there is no workbreak in the requested direction.
        /// </summary>
        private static TextPointer GetPositionAtWordBoundary(TextPointer position, LogicalDirection wordBreakDirection)
        {
            if (!position.IsAtInsertionPosition)
            {
                position = position.GetInsertionPosition(wordBreakDirection);
            }

            var navigator = position;
            while (navigator != null && !IsPositionNextToWordBreak(navigator, wordBreakDirection))
            {
                navigator = navigator.GetNextInsertionPosition(wordBreakDirection);
            }

            return navigator;
        }

        // Helper for GetPositionAtWordBoundary.
        // Returns true when passed TextPointer is next to a wordBreak in requested direction.
        private static bool IsPositionNextToWordBreak(TextPointer position, LogicalDirection wordBreakDirection)
        {
            var isAtWordBoundary = false;

            // Skip over any formatting.
            if (position.GetPointerContext(wordBreakDirection) != TextPointerContext.Text)
            {
                position = position.GetInsertionPosition(wordBreakDirection);
            }

            if (position.GetPointerContext(wordBreakDirection) == TextPointerContext.Text)
            {
                var oppositeDirection = wordBreakDirection == LogicalDirection.Forward
                    ? LogicalDirection.Backward
                    : LogicalDirection.Forward;

                var runBuffer = new char[1];
                var oppositeRunBuffer = new char[1];

                position.GetTextInRun(wordBreakDirection, runBuffer, /*startIndex*/0, /*count*/1);
                position.GetTextInRun(oppositeDirection, oppositeRunBuffer, /*startIndex*/0, /*count*/1);

                if (runBuffer[0] == ' ' && oppositeRunBuffer[0] != ' ')
                {
                    isAtWordBoundary = true;
                }
            }
            else
            {
                // If we're not adjacent to text then we always want to consider this position a "word break". 
                // In practice, we're most likely next to an embedded object or a block boundary.
                isAtWordBoundary = true;
            }

            return isAtWordBoundary;
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

    public static class FlowDocumentHelper
    {
        public static IEnumerable<TTextElement> WalkTextRange<TTextElement>(this FlowDocument doc, TextPointer start,
            TextPointer end) where TTextElement : TextElement
        {
            var lastVisited = new Dictionary<int, TTextElement>();
            foreach (var stack in doc.WalkTextHierarchy())
            {
                var element = stack.Peek() as TTextElement;
                if (element != null)
                {
                    TTextElement previous;
                    if (!lastVisited.TryGetValue(stack.Count - 1, out previous) || previous != element)
                    {
                        if (element.Overlaps(start, end))
                            yield return element;
                        lastVisited[stack.Count - 1] = element;
                    }
                }
            }
        }

        public static bool Overlaps(this TextElement element, TextPointer start, TextPointer end)
        {
            return element.ContentEnd.CompareTo(start) > 0 && element.ContentStart.CompareTo(end) < 0;
        }

        public static IEnumerable<Stack<DependencyObject>> WalkTextHierarchy(this FlowDocument doc)
        {
            if (doc == null)
                throw new ArgumentNullException();

            var stack = new Stack<DependencyObject>();

            // Keep a TextPointer for FlowDocument.ContentEnd handy, so we know when we're done.
            var docEnd = doc.ContentEnd;

            // Keep going until the TextPointer is equal to or greater than ContentEnd.
            for (var iterator = doc.ContentStart;
                (iterator != null) && (iterator.CompareTo(docEnd) < 0);
                iterator = iterator.GetNextContextPosition(LogicalDirection.Forward))
            {
                var parent = iterator.Parent;

                // Identify the type of content immediately adjacent to the text pointer.
                var context = iterator.GetPointerContext(LogicalDirection.Forward);

                switch (context)
                {
                    case TextPointerContext.ElementStart:
                    case TextPointerContext.EmbeddedElement:
                    case TextPointerContext.Text:
                        PushElement(stack, parent);
                        yield return stack;
                        break;

                    case TextPointerContext.ElementEnd:
                        break;

                    default:
                        throw new Exception("Unhandled TextPointerContext " + context);
                }

                switch (context)
                {
                    case TextPointerContext.ElementEnd:
                    case TextPointerContext.EmbeddedElement:
                    case TextPointerContext.Text:
                        PopToElement(stack, parent);
                        break;

                    case TextPointerContext.ElementStart:
                        break;

                    default:
                        throw new Exception("Unhandled TextPointerContext " + context);
                }
            }
        }

        private static int IndexOf<T>(Stack<T> source, T value)
        {
            var index = 0;
            var comparer = EqualityComparer<T>.Default;
            foreach (var item in source)
            {
                if (comparer.Equals(item, value))
                    return index;
                index++;
            }
            return -1;
        }

        private static void PopToElement<T>(Stack<T> stack, T item)
        {
            for (var index = IndexOf(stack, item); index >= 0; index--)
                stack.Pop();
        }

        private static void PushElement<T>(Stack<T> stack, T item)
        {
            PopToElement(stack, item);
            stack.Push(item);
        }
    }
}