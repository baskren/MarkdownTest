using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownTest;

public partial class PdfControl : JavaScriptControl
{
    public event EventHandler PDFReady;

    //public bool IsMarkedReady { get; private set; }


    //public string MarkdownText
    //{
    //    get { return (string)GetValue(MarkdownTextProperty); }
    //    set { SetValue(MarkdownTextProperty, value); }
    //}

    //// Using a DependencyProperty as the backing store for MarkdownText.  This enables animation, styling, binding, etc...
    //public static readonly DependencyProperty MarkdownTextProperty =
    //    DependencyProperty.Register("MarkdownText", typeof(string), typeof(MarkedControl), new PropertyMetadata(null, MarkdownTextChanged));

    //private static async void MarkdownTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //{
    //    await (d as MarkedControl).DisplayMarkdownText();
    //}

    //private async Task DisplayMarkdownText()
    //{
    //    if (IsMarkedReady && !string.IsNullOrWhiteSpace(MarkdownText))
    //    {
    //        await DisplayMarkdown(MarkdownText);
    //    }
    //}

    public string PdfEmbeddedJavaScriptFile { get; set; } = "Resources.pdf.min.js";

    //protected override string HtmlBody => $@"<canvas id=""{HtmlContentId}""></canvas>";


    protected override async Task LoadJavaScript()
    {
        await LoadEmbeddedJavaScriptFile("Resources.pdf.worker.min.js");
        await LoadEmbeddedJavaScriptFile(PdfEmbeddedJavaScriptFile);


        var js = (await GetEmbeddedFileStreamAsync(GetType(), "Resources.contentpdf.js")).ReadToEnd();

        var pdfBytes = (await GetEmbeddedFileStreamAsync(GetType(), "sampledocpage1.pdf")).ReadBytes();
        var pdfAsString = Convert.ToBase64String(pdfBytes);
        js = js.Replace("the-canvas", HtmlContentId);
        js = js.Replace("##CONTENT##", pdfAsString);
        await InvokeScriptAsync(js, false);

        //await LoadEmbeddedJavaScriptFile("Resources.simplepdf.js");
        //IsMarkedReady = true;

        //await DisplayMarkdownText();

        //MarkedReady?.Invoke(this, EventArgs.Empty);
    }

    //public async Task DisplayMarkdown(string markdown)
    //{
    //    markdown = markdown.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\"", "\\\"").Replace("\'", "\\\'");//.Replace("\t","\\t").Replace("`","");
    //    System.Diagnostics.Debug.WriteLine(markdown);
    //    await UpdateHtmlFromScript($"marked('{markdown}')");
    //}

    //public async Task LoadMarkdownFromFile(string embeddedFileName)
    //{
    //    var markdown = (await GetEmbeddedFileStreamAsync(GetType(), embeddedFileName)).ReadToEnd();
    //    await DisplayMarkdown(markdown);
    //}
}
