using System.Windows;

namespace WpfApp1
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

        private void mnuPessoas_Click(object sender, RoutedEventArgs e)
        {
            Window w = new CadastroPessoas();
            w.Show();
        }

        private void mnuProdutos_Click(object sender, RoutedEventArgs e)
        {
            Window w = new CadastroProdutos();
            w.Show();
        }

        private void mnuPedidos_Click(object sender, RoutedEventArgs e)
        {
            Window w = new CadastroPedidos();
            w.Show();
        }
    }
}
