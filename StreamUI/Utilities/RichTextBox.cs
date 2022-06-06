using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

public static class RichTextBoxExtensions
{
    public static void AppendText(this RichTextBox box, string text, Color color)
    {
        Run run = new Run(text);
        run.Foreground = new SolidColorBrush(color);
        Paragraph paragraph = new Paragraph(run);
        paragraph.Margin = new Thickness(0);
        box.Document.Blocks.Add(paragraph);
        box.ScrollToEnd();
        
        paragraph = new Paragraph(new Run());
        paragraph.Margin = new Thickness(0);
        box.Document.Blocks.Add(paragraph);
        box.ScrollToEnd();
    }

    public static void ErrorText(this RichTextBox box, string text)
    {
        box.AppendText(text, Colors.Red);
    }
}
