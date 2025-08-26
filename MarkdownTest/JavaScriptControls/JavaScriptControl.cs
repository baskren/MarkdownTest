using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Uno.Foundation;

#if xBROWSERWASM
using Uno.Foundation.Interop;
using Uno.UI.NativeElementHosting;
#endif

namespace MarkdownTest;

[Microsoft.UI.Xaml.Data.Bindable]
public abstract partial class JavaScriptControl : UserControl
{

#if xBROWSERWASM
    private BrowserHtmlElement _element;
#else
    private readonly WebView2 internalWebView;    
#endif

    public virtual string StyleSheet { get; } = string.Empty;

    public JavaScriptControl()
    {
#if xBROWSERWASM
        _element = BrowserHtmlElement.CreateHtmlElement("div");
        _element.SetHtmlContent(HtmlBody);
        Content = _element;

#else
        Content = internalWebView = new WebView2
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        internalWebView.DefaultBackgroundColor = Colors.Transparent;
        internalWebView.NavigationCompleted += OnNavigationCompleted;
#endif
        Loaded += OnJavaScriptControlLoaded;
    }

//#if xBROWSERWASM
//    protected string HtmlContentId => _element.GetHtmlId();
//#else
    protected readonly string HtmlContentId = "content-" + Guid.NewGuid().ToString();
//#endif

    protected virtual string HtmlBody => $@"<div class=""markdown-body"" id = ""{HtmlContentId}""><p>CONTENT GOES HERE</p></ div>";

    protected virtual async void OnJavaScriptControlLoaded(object sender, RoutedEventArgs e)
    {
#if xBROWSERWASM
        //var script = $@"document.getElementById('{_element.GetHtmlId()}').innerHTML = '{HtmlBody}';";
        //Console.WriteLine(script);
        //await InvokeScriptAsync(script, false);
        await LoadJavaScript();  
#else
        var html = $@"<html>
    <head>
        <style type=""text/css"" >{StyleSheet}</style>
    </head>
     <body>
        <div class=""markdown-body"">
         {HtmlBody}
        </div>
    </ body>
    </ html>
";
        await internalWebView.EnsureCoreWebView2Async();

        System.Diagnostics.Debug.WriteLine($"HTML: {html}");

        internalWebView.NavigateToString(html);
#endif
    }

    protected async Task LoadEmbeddedJavaScriptFile(string filename)
    {
        var markdownScript = (await GetEmbeddedFileStreamAsync(GetType(), filename)).ReadToEnd();

        await InvokeScriptAsync(markdownScript, false);
        //await InvokeScriptAsync(markdownScript);
    }

    protected abstract Task LoadJavaScript();

    private void OnNavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
#if __ANDROID__
        var wv = internalWebView.GetType().GetField("_webView", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(internalWebView) as Android.Webkit.WebView;
        wv.SetBackgroundColor(Android.Graphics.Color.Transparent);
#endif

        LoadJavaScript();
    }

    protected async Task UpdateHtmlFromScript(string contentScript)
    {
        if (Foreground is SolidColorBrush colorBrush)
        {
            var color = colorBrush.Color;
            // This is required because default tostring on wasm doesn't come out in the format #RRGGBB or even #AARRGGBB
            var colorString = $"#{color.R.ToString("X")}{color.G.ToString("X")}{color.B.ToString("X")}";
            Console.WriteLine($"Color {colorString}");
            var colorScript = $@"document.getElementById('{HtmlContentId}').style.color = '{colorString}';";
            await InvokeScriptAsync(colorScript);
        }

        if (Background is SolidColorBrush background)
        {
            var color = background.Color;
            // This is required because default tostring on wasm doesn't come out in the format #RRGGBB or even #AARRGGBB
            var colorString = $"#{color.R.ToString("X")}{color.G.ToString("X")}{color.B.ToString("X")}";
            Console.WriteLine($"Color {colorString}");
            var colorScript = $@"document.getElementById('{HtmlContentId}').style.background-color = '{colorString}';";
            await InvokeScriptAsync(colorScript);
        }

        /*
        contentScript = $@"`<html>
     <body style=""background-color: green"">
        <p>TEST TEXT</p>
         {contentScript}
    </ body>
    </ html>`
";
        */
#if xBROWSERWASM
        //contentScript = await InvokeScriptAsync(contentScript);

        
        contentScript = $"`{contentScript.Trim()}`";


        /*
        contentScript = $@"`
<html>
    <body style=""background-color: green"">
        <p>TEST TEXT</p>
        <div class=""markdown-body"">
            {contentScript}
        </div>
    </ body>
</ html>`
";
        */
        
        contentScript = $@"`<html>
    <head>
        <style type=""text/css"" >{StyleSheet}</style>
    </head>
     <body>
        <div class=""markdown-body"">
         {contentScript}
        </div>
    </ body>
    </ html>`
";
        
#else
#endif
        var script = $@"document.getElementById('{HtmlContentId}').innerHTML = {contentScript};";
        await InvokeScriptAsync(script);

    }

    public async Task<string> InvokeScriptAsync(string scriptToRun, bool resizeAfterScript = true)
    {
        //scriptToRun = ReplaceLiterals(scriptToRun);

#if xBROWSERWASM
        //scriptToRun = ReplaceLiterals(scriptToRun);
        //var script = $"javascript:eval(\"{scriptToRun}\");";
        var script = scriptToRun;
        Console.WriteLine($"SCRIPT: {script}");

        try
        {
            var result = WebAssemblyRuntime.InvokeJS(script);
            Console.WriteLine($"Result: {result}");

            if (resizeAfterScript)
            {
                await ResizeToContent();
            }

            return result;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("FAILED " + e);
            return string.Empty;
        }
#else
        //var source = new CancellationTokenSource();
        System.Diagnostics.Debug.WriteLine($"SCRIPT: {scriptToRun}");
        var result = await internalWebView.ExecuteScriptAsync(scriptToRun).AsTask();
        if (resizeAfterScript)
        {
            await ResizeToContent();
        }
        System.Diagnostics.Debug.WriteLine($"RESULT: {Regex.Unescape(result)}");
        return result ?? string.Empty;
#endif
    }

    private static Func<string, string> ReplaceLiterals = txt =>
        txt
    .Replace("\\", "\\\\")
    .Replace("\n", "\\n")
    .Replace("\r", "\\r")
    .Replace("\"", "\\\"")
    .Replace("\'", "\\\'")
    .Replace("`", "\\`")
    .Replace("^", "\\^");


    public static async Task<Stream> GetEmbeddedFileStreamAsync(Type assemblyType, string fileName)
    {
        await Task.Yield();

        var manifestName = assemblyType.GetTypeInfo().Assembly
            .GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(fileName.Replace(" ", "_").Replace("/", ".").Replace("\\", "."), StringComparison.OrdinalIgnoreCase));

        if (manifestName == null)
            throw new InvalidOperationException($"Failed to find resource [{fileName}]");


        return assemblyType.GetTypeInfo().Assembly.GetManifestResourceStream(manifestName);
    }

    public async Task ResizeToContent()
    {
        var documentRoot = $"document.getElementById('{HtmlContentId}')";
        var heightString = await InvokeScriptAsync($"{documentRoot}.scrollHeight.toString()",
            false);
        int height;
        if (int.TryParse(heightString, out height))
            this.Height = height + 100;

    }

}
