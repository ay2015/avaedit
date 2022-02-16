using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
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

namespace avaedit
{
    //public class CompletionData : ICompletionData
    //{
    //    public CompletionData(string text)
    //    {
    //        Text = text;
    //    }

    //    public ImageSource Image => null;

    //    public string Text { get; }

    //    public object Content => Text;

    //    public object Description =>this.Text +" 的描述";

    //    public double Priority { get; }

    //    public void Complete(TextArea textArea, ISegment completionSegment,
    //        EventArgs insertionRequestEventArgs)
    //    {
    //        textArea.Document.Replace(completionSegment, Text);
    //    }
    //}

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Console.WriteLine("ay 2022 www.ayjs.net");
            //Loaded += MainWindow_Loaded;


            //textEditor.TextArea.TextEntered += TextAreaOnTextEntered;
        }

        //private CompletionWindow _completionWindow;

        //private void TextAreaOnTextEntered(object sender, TextCompositionEventArgs e)
        //{
        //    if (e.Text == ".")
        //    {
        //        _completionWindow = new CompletionWindow(textEditor.TextArea);

        //        var completionData = _completionWindow.CompletionList.CompletionData;
        //        completionData.Add(new CompletionData("Name"));
        //        completionData.Add(new CompletionData("Gender"));
        //        completionData.Add(new CompletionData("Class"));
        //        _completionWindow.Show();

        //        _completionWindow.Closed += (o, args) => _completionWindow = null;
        //    }
        //}

    

        //private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        //{
        //    Loaded -= MainWindow_Loaded;

        //}
        //private static FoldingManager foldingManager = null;
        //private static XmlFoldingStrategy foldingStrategy = new XmlFoldingStrategy();



        //private void zhedie_Click(object sender, RoutedEventArgs e)
        //{
        //    //默认语法高亮规则
        //    textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".xml");
        //    //折叠
        //    if (foldingManager != null)
        //    {
        //        FoldingManager.Uninstall(foldingManager);
        //        foldingManager.Clear();
        //    }
        //    foldingManager = FoldingManager.Install(textEditor.TextArea);

        //    //textEditor.Text = "";
        //    foldingStrategy.UpdateFoldings(foldingManager, textEditor.Document);
        //}


    }
}
