using System.IO;
using WpfApp1.Models;


namespace WpfApp1.Data
{
    public class Context
    {
        public IRepository<PessoaModel> Pessoas { get; }
        public IRepository<ProdutoModel> Produtos { get; }
        public IRepository<PedidoModel> Pedidos { get; }

        public Context(IRepository<PessoaModel> pessoas,
                       IRepository<ProdutoModel> produtos,
                       IRepository<PedidoModel> pedidos)
        {
            Pessoas = pessoas;
            Produtos = produtos;
            Pedidos = pedidos;
        }

        public Context(string baseFolder)
        {
            Pessoas = new Repository<PessoaModel>(Path.Combine(baseFolder, "pessoas.json"));
            Produtos = new Repository<ProdutoModel>(Path.Combine(baseFolder, "produtos.json"));
            Pedidos = new Repository<PedidoModel>(Path.Combine(baseFolder, "pedidos.json"));
        }
    }
}
