using System;
using System.Collections.Generic;
using System.Linq;

namespace WpfApp1.Models
{
    public class PedidoModel
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }

        public string ClienteNome { get; set; }
        public string Status { get; set; }
        public string FormaPgto { get; set; }
        public DateTime Data { get; set; } = DateTime.Now;
        public List<ItemPedido> Itens { get; set; } = new List<ItemPedido>();
        public string ItensTexto { get; set; }

        public decimal Total => Itens.Sum(i => i.Quantidade * i.PrecoUnitario);
    }

    public class ItemPedido
    {
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
    }
}
