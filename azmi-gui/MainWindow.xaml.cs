using azmi_main;
using System.Windows;

namespace azmi_gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TokenTextBlock.Text = (new GetToken()).Execute("management");
        }
    }
}
