using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Search;
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
using System.Xml;

namespace avaedit
{
    /// <summary>
    /// AvaEditor.xaml 的交互逻辑
    /// </summary>
    public partial class AvaEditor : UserControl
    {

        private static readonly ICommand validateCommand = new RoutedUICommand("验证XML", "Validate", typeof(AvaEditor),
       new InputGestureCollection { new KeyGesture(Key.V, ModifierKeys.Control | ModifierKeys.Shift) });

        public static ICommand ValidateCommand
        {
            get { return validateCommand; }
        }

        public AvaEditor()
        {
            InitializeComponent();
            Loaded += AvaEditor_Loaded;
        }

        private ToolTip toolTip;
        private TextMarkerService textMarkerService;

        private void AvaEditor_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= AvaEditor_Loaded;


            textMarkerService = new TextMarkerService(textEditor);
            var textView = textEditor.TextArea.TextView;
            textView.BackgroundRenderers.Add(textMarkerService);
            textView.LineTransformers.Add(textMarkerService);
            textView.Services.AddService(typeof(TextMarkerService), textMarkerService);

            textView.MouseHover += MouseHover;
            textView.MouseHoverStopped += TextEditorMouseHoverStopped;
            textView.VisualLinesChanged += VisualLinesChanged;

            SearchPanel.Install(textEditor.TextArea);


            string name = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".lua.xshd";
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using (System.IO.Stream s = assembly.GetManifestResourceStream(name))
            {
                using (XmlTextReader reader = new XmlTextReader(s))
                {
                    var xshd = HighlightingLoader.LoadXshd(reader);
                    textEditor.SyntaxHighlighting = HighlightingLoader.Load(xshd, HighlightingManager.Instance);
                }
            }

            //右键菜单
            //ContextMenu aMenu = new ContextMenu();
            //MenuItem copyMenuItem = new MenuItem();
            //copyMenuItem.Header = "复制(C)";
            //copyMenuItem.Click += (sender1, eventArgs) => textEditor.Copy();
            //aMenu.Items.Add(copyMenuItem);
            //MenuItem selectAllItem = new MenuItem();
            //selectAllItem.Header = "全选(A)";
            //selectAllItem.Click += (sender1, eventArgs) => textEditor.SelectAll();
            //aMenu.Items.Add(selectAllItem);
            //textEditor.ContextMenu = aMenu;
        }

        private void MouseHover(object sender, MouseEventArgs e)
        {
            var pos = textEditor.TextArea.TextView.GetPositionFloor(e.GetPosition(textEditor.TextArea.TextView) + textEditor.TextArea.TextView.ScrollOffset);
            bool inDocument = pos.HasValue;
            if (inDocument)
            {
                TextLocation logicalPosition = pos.Value.Location;
                int offset = textEditor.Document.GetOffset(logicalPosition);

                var markersAtOffset = textMarkerService.GetMarkersAtOffset(offset);
                TextMarker markerWithToolTip = markersAtOffset.FirstOrDefault(marker => marker.ToolTip != null);

                if (markerWithToolTip != null)
                {
                    if (toolTip == null)
                    {
                        toolTip = new ToolTip();
                        toolTip.Closed += ToolTipClosed;
                        toolTip.PlacementTarget = this;
                        toolTip.Content = new TextBlock
                        {
                            Text = markerWithToolTip.ToolTip,
                            TextWrapping = TextWrapping.Wrap
                        };
                        toolTip.IsOpen = true;
                        e.Handled = true;
                    }
                }
            }
        }

        void ToolTipClosed(object sender, RoutedEventArgs e)
        {
            toolTip = null;
        }

        void TextEditorMouseHoverStopped(object sender, MouseEventArgs e)
        {
            if (toolTip != null)
            {
                toolTip.IsOpen = false;
                e.Handled = true;
            }
        }

        private void VisualLinesChanged(object sender, EventArgs e)
        {
            if (toolTip != null)
            {
                toolTip.IsOpen = false;
            }
        }

        private void Validate(object sender, ExecutedRoutedEventArgs e)
        {
            IServiceProvider sp = textEditor;
            var markerService = (TextMarkerService)sp.GetService(typeof(TextMarkerService));
            markerService.Clear();

            try
            {
                var document = new XmlDocument { XmlResolver = null };
                document.LoadXml(textEditor.Document.Text);
            }
            catch (XmlException ex)
            {
                DisplayValidationError(ex.Message, ex.LinePosition, ex.LineNumber);
            }
        }

        private void DisplayValidationError(string message, int linePosition, int lineNumber)
        {
            if (lineNumber >= 1 && lineNumber <= textEditor.Document.LineCount)
            {
                int offset = textEditor.Document.GetOffset(new TextLocation(lineNumber, linePosition));
                int endOffset = TextUtilities.GetNextCaretPosition(textEditor.Document, offset, System.Windows.Documents.LogicalDirection.Forward, CaretPositioningMode.WordBorderOrSymbol);
                if (endOffset < 0)
                {
                    endOffset = textEditor.Document.TextLength;
                }
                int length = endOffset - offset;

                if (length < 2)
                {
                    length = Math.Min(2, textEditor.Document.TextLength - offset);
                }

                textMarkerService.Create(offset, length, message);
            }
        }
    }





    public sealed class TextMarker : TextSegment
    {
        public TextMarker(int startOffset, int length)
        {
            StartOffset = startOffset;
            Length = length;
        }

        public Color? BackgroundColor { get; set; }
        public Color MarkerColor { get; set; }
        public string ToolTip { get; set; }
    }

    public class TextMarkerService : IBackgroundRenderer, IVisualLineTransformer
    {
        private readonly TextEditor textEditor;
        private readonly TextSegmentCollection<TextMarker> markers;

        public TextMarkerService(TextEditor textEditor)
        {
            this.textEditor = textEditor;
            markers = new TextSegmentCollection<TextMarker>(textEditor.Document);
        }

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (markers == null || !textView.VisualLinesValid)
            {
                return;
            }
            var visualLines = textView.VisualLines;
            if (visualLines.Count == 0)
            {
                return;
            }
            int viewStart = visualLines.First().FirstDocumentLine.Offset;
            int viewEnd = visualLines.Last().LastDocumentLine.EndOffset;
            foreach (TextMarker marker in markers.FindOverlappingSegments(viewStart, viewEnd - viewStart))
            {
                if (marker.BackgroundColor != null)
                {
                    var geoBuilder = new BackgroundGeometryBuilder { AlignToWholePixels = true, CornerRadius = 3 };
                    geoBuilder.AddSegment(textView, marker);
                    Geometry geometry = geoBuilder.CreateGeometry();
                    if (geometry != null)
                    {
                        Color color = marker.BackgroundColor.Value;
                        var brush = new SolidColorBrush(color);
                        brush.Freeze();
                        drawingContext.DrawGeometry(brush, null, geometry);
                    }
                }
                foreach (Rect r in BackgroundGeometryBuilder.GetRectsForSegment(textView, marker))
                {
                    Point startPoint = r.BottomLeft;
                    Point endPoint = r.BottomRight;

                    var usedPen = new Pen(new SolidColorBrush(marker.MarkerColor), 1);
                    usedPen.Freeze();
                    const double offset = 2.5;

                    int count = Math.Max((int)((endPoint.X - startPoint.X) / offset) + 1, 4);

                    var geometry = new StreamGeometry();

                    using (StreamGeometryContext ctx = geometry.Open())
                    {
                        ctx.BeginFigure(startPoint, false, false);
                        ctx.PolyLineTo(CreatePoints(startPoint, endPoint, offset, count).ToArray(), true, false);
                    }

                    geometry.Freeze();

                    drawingContext.DrawGeometry(Brushes.Transparent, usedPen, geometry);
                    break;
                }
            }
        }

        public KnownLayer Layer
        {
            get { return KnownLayer.Selection; }
        }

        public void Transform(ITextRunConstructionContext context, IList<VisualLineElement> elements)
        { }

        private IEnumerable<Point> CreatePoints(Point start, Point end, double offset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new Point(start.X + (i * offset), start.Y - ((i + 1) % 2 == 0 ? offset : 0));
            }
        }

        public void Clear()
        {
            foreach (TextMarker m in markers)
            {
                Remove(m);
            }
        }

        private void Remove(TextMarker marker)
        {
            if (markers.Remove(marker))
            {
                Redraw(marker);
            }
        }

        private void Redraw(ISegment segment)
        {
            textEditor.TextArea.TextView.Redraw(segment);
        }

        public void Create(int offset, int length, string message)
        {
            var m = new TextMarker(offset, length);
            markers.Add(m);
            m.MarkerColor = Colors.Red;
            m.BackgroundColor = Colors.Yellow;
            m.ToolTip = message;
            Redraw(m);
        }

        public IEnumerable<TextMarker> GetMarkersAtOffset(int offset)
        {
            return markers == null ? Enumerable.Empty<TextMarker>() : markers.FindSegmentsContaining(offset);
        }
    }
}
