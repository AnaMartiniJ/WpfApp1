using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WpfApp1.Data;
using WpfApp1.Models;

namespace WpfApp1.Services
{
    public class Service
    {
        public bool PodeMarcarComoPago(PedidoModel pedido)
            => pedido.Status == "Pendente";

        public bool PodeMarcarComoEnviado(PedidoModel pedido)
            => pedido.Status == "Pago";

        public bool PodeMarcarComoRecebido(PedidoModel pedido)
            => pedido.Status == "Enviado";

        public string GerarItensTexto(PedidoModel pedido, Dictionary<int, ProdutoModel> dictProdutos)
        {
            return string.Join(", ", pedido.Itens.Select(i =>
                dictProdutos.TryGetValue(i.ProdutoId, out var prod)
                    ? prod.Nome
                    : $"ID {i.ProdutoId}"));
        }

        public async Task<string> ObterNomeClienteAsync(int clienteId, IRepository<PessoaModel> repoPessoas)
        {
            var pessoa = await repoPessoas.GetByIdAsync(clienteId);
            return pessoa?.Nome ?? $"ID {clienteId}";
        }

        public List<PedidoModel> FiltrarPorStatus(List<PedidoModel> pedidos, string status)
        {
            return pedidos.Where(p => p.Status == status).ToList();
        }
    }
}
