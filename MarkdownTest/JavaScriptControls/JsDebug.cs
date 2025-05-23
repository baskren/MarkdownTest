using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownTest;
public partial class JsDebug : JavaScriptControl
{
    protected override async Task LoadJavaScript()
    {
        var script = $@"document.getElementById('{HtmlContentId}').innerHTML = ""DEBUG_CONTROL"";";

        await InvokeScriptAsync(script);
    }
}
