using ChocoVersionSelect.Model;
using System.Reflection;

namespace ChocoVersionSelect.View;

public partial class Syntax : Window
{
    public Syntax()
    {
        InitializeComponent();
        AssemblyName name = Assembly.GetExecutingAssembly().GetName();
        Title = $"{name.Name} command line syntax";
        txtVersion.Text = $"{name.Name} {name.Version}";
        txtSyntax.Text = $"{name.Name} {CommandLineParser.Syntax}";
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
