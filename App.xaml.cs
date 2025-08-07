using System;
using System.IO;
using System.Windows;
using WpfApp1.Data;
using WpfApp1.Models;

namespace WpfApp1
{
    public partial class App : Application
    {
        public static Context Db { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var baseFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WpfApp1");

            if (!Directory.Exists(baseFolder))
                Directory.CreateDirectory(baseFolder);

            Db = new Context(
                new Repository<PessoaModel>(Path.Combine(baseFolder, "pessoas.json")),
                new Repository<ProdutoModel>(Path.Combine(baseFolder, "produtos.json")),
                new Repository<PedidoModel>(Path.Combine(baseFolder, "pedidos.json"))
            );

            var main = new MainWindow();
            main.Show();
        }
    }
}
