using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Markdig;

namespace MarkdownTest;

[Microsoft.UI.Xaml.Data.Bindable]
public partial class MarkedControl : JavaScriptControl
{
    public event EventHandler MarkedReady;

    public bool IsMarkedReady { get; private set; }

    MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseEmojiAndSmiley()
        .UseAutoLinks()
        .UseListExtras()
        .Build();

    public string MarkdownText
    {
        get { return (string)GetValue(MarkdownTextProperty); }
        set { SetValue(MarkdownTextProperty, value); }
    }

    string? _style;
    public override string StyleSheet => _style!;

    // Using a DependencyProperty as the backing store for MarkdownText.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty MarkdownTextProperty =
        DependencyProperty.Register("MarkdownText", typeof(string), typeof(MarkedControl), new PropertyMetadata(null, MarkdownTextChanged));

    private static async void MarkdownTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        await (d as MarkedControl).DisplayMarkdownText();
    }

    private async Task DisplayMarkdownText()
    {
        if (IsMarkedReady && !string.IsNullOrWhiteSpace(MarkdownText))
        {
            await DisplayMarkdown(MarkdownText);
        }
    }

    protected override async void OnJavaScriptControlLoaded(object sender, RoutedEventArgs e)
    {
        await LoadStyleSheetAsync();
        base.OnJavaScriptControlLoaded(sender, e);
    }

    async Task LoadStyleSheetAsync()
    {
        _style ??= (await GetEmbeddedFileStreamAsync(GetType(), "github-markdown.css")).ReadToEnd();
    }

    public string MarkedEmbeddedJavaScriptFile { get; set; } = "Resources.marked.min.js";

    protected override async Task LoadJavaScript()
    {
        await InnerLoadJavaScriptAsync();
    }

    public async Task InnerLoadJavaScriptAsync()
    {

#if !xBROWSERWASM
        // use Platforms/WebAssembly/WasmScripts, instead
        // await LoadEmbeddedJavaScriptFile(MarkedEmbeddedJavaScriptFile);
        // Using MarkDig instead!
#endif

        IsMarkedReady = true;

        await DisplayMarkdownText();

        MarkedReady?.Invoke(this, EventArgs.Empty);
    }

    public async Task DisplayMarkdown(string markdown)
    {

        System.Diagnostics.Debug.WriteLine(markdown);
        var html = Markdig.Markdown.ToHtml(markdown, pipeline);
#if xBROWSERWASM
        await UpdateHtmlFromScript(html);
#else
        html = html.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\"", "\\\"").Replace("\'", "\\\'");//.Replace("\t","\\t").Replace("`","");
        await UpdateHtmlFromScript($"`{html}`");
#endif
    }

    public async Task LoadMarkdownFromResource(string embeddedFileName)
    {
        var markdown = (await GetEmbeddedFileStreamAsync(GetType(), embeddedFileName)).ReadToEnd();
        await DisplayMarkdown(markdown);
    }
}
