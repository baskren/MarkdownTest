
using System.Threading;

namespace MarkdownTest;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
    }

    private async void markedCtrl_MarkedReady(object sender, EventArgs args)
    {
        //await markedCtrl.LoadMarkdownFromFile("SharedAssets.md");
    }

    private async void button_Click(object sender, RoutedEventArgs e)
    {
        //await markCtrl1.LoadMarkdownFromResource("markdown-it.md");
    }
}
