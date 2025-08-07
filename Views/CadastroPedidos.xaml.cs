using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Data;
using WpfApp1.Models;

namespace WpfApp1
{
    public partial class CadastroPedidos : Window
    {
        private readonly Context _context;
        private readonly ObservableCollection<ItemPedidoView> _itensView = new ObservableCollection<ItemPedidoView>();
        public PessoaModel PessoaSelecionada { get; set; }

        public CadastroPedidos()
        {
            InitializeComponent();
            _context = App.Db;

            dtGridPedidos.ItemsSource = _itensView;

            this.Loaded += CadastroPedidos_Loaded;
            btnNovo.Click += btnNovo_Click;
            btnPesquisar.Click += btnPesquisar_Click;
            btnExcluir.Click += btnExcluir_Click;
            btnGravar.Click += btnGravar_Click;
        }

        private async void CadastroPedidos_Loaded(object sender, RoutedEventArgs e)
        {
            var pessoas = (await _context.Pessoas.GetAllAsync()).OrderBy(p => p.Nome).ToList();
            cbCliente.ItemsSource = pessoas;
            cbCliente.DisplayMemberPath = "Nome";
            cbCliente.SelectedValuePath = "Id";

            if (PessoaSelecionada != null)
            {
                cbCliente.SelectedValue = PessoaSelecionada.Id;
            }

            var produtos = (await _context.Produtos.GetAllAsync()).OrderBy(p => p.Nome).ToList();
            cbProduto.ItemsSource = produtos;

            dpData.SelectedDate = DateTime.Now;
            AtualizaTotal();
            txtStatus.Text = "Pendente";
            HabilitaEdicao(true);
        }


        private void AtualizaTotal()
        {
            var total = _itensView.Sum(i => i.Subtotal);
            txtTotal.Text = total.ToString("F2");
        }

        private void cbProduto_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var prod = cbProduto.SelectedItem as ProdutoModel;
            txtPreco.Text = prod != null ? prod.Preco.ToString("F2") : string.Empty;
        }

        private void btnAdicionarItem_Click(object sender, RoutedEventArgs e)
        {
            var prod = cbProduto.SelectedItem as ProdutoModel;
            if (prod == null)
            {
                MessageBox.Show("Selecione um produto.");
                return;
            }

            int qtd;
            if (!int.TryParse(txtQtd.Text, out qtd) || qtd <= 0)
            {
                MessageBox.Show("Quantidade inválida.");
                return;
            }

            var existente = _itensView.FirstOrDefault(i => i.ProdutoId == prod.Id);
            if (existente != null)
            {
                existente.Quantidade += qtd;
                existente.NotifyChanged();
            }
            else
            {
                _itensView.Add(new ItemPedidoView
                {
                    ProdutoId = prod.Id,
                    ProdutoNome = prod.Nome,
                    Quantidade = qtd,
                    PrecoUnitario = prod.Preco
                });
            }

            txtQtd.Text = "";
            AtualizaTotal();
        }

        private void btnRemoverItem_Click(object sender, RoutedEventArgs e)
        {
            var sel = dtGridPedidos.SelectedItem as ItemPedidoView;
            if (sel == null)
            {
                MessageBox.Show("Selecione um item para remover.");
                return;
            }
            _itensView.Remove(sel);
            AtualizaTotal();
        }

        private async void btnGravar_Click(object sender, RoutedEventArgs e)
        {
            var cliente = cbCliente.SelectedItem as PessoaModel;
            if (cliente == null)
            {
                MessageBox.Show("Selecione o cliente.");
                return;
            }

            var formaPgtoItem = cbFormaPgto.SelectedItem as ComboBoxItem;
            var formaPgto = formaPgtoItem != null && formaPgtoItem.Content != null ? formaPgtoItem.Content.ToString() : null;
            if (string.IsNullOrWhiteSpace(formaPgto))
            {
                MessageBox.Show("Selecione a forma de pagamento.");
                return;
            }

            if (_itensView.Count == 0)
            {
                MessageBox.Show("Adicione pelo menos um item.");
                return;
            }

            try
            {
                foreach (var iv in _itensView)
                {
                    var prod = await _context.Produtos.GetByIdAsync(iv.ProdutoId);
                    if (prod == null) throw new Exception("Produto " + iv.ProdutoNome + " não encontrado.");
                }

                var pedido = new PedidoModel
                {
                    ClienteId = cliente.Id,
                    Status = "Pendente",
                    FormaPgto = formaPgto,
                    Data = dpData.SelectedDate ?? DateTime.Now,
                    Itens = _itensView.Select(iv => new ItemPedido
                    {
                        ProdutoId = iv.ProdutoId,
                        Quantidade = iv.Quantidade,
                        PrecoUnitario = iv.PrecoUnitario
                    }).ToList()
                };

                await _context.Pedidos.AddAsync(pedido);

                txtId.Text = pedido.Id.ToString();
                txtStatus.Text = "Pendente";
                MessageBox.Show("Pedido #" + pedido.Id + " salvo. Total: " + pedido.Total.ToString("F2"));
                HabilitaEdicao(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao finalizar: " + ex.Message);
            }
        }

        private void HabilitaEdicao(bool habilitar)
        {
            cbCliente.IsEnabled = habilitar;
            dpData.IsEnabled = habilitar;
            cbProduto.IsEnabled = habilitar;
            txtPreco.IsEnabled = false;
            txtQtd.IsEnabled = habilitar;
            btnAdicionarItem.IsEnabled = habilitar;
            btnRemoverItem.IsEnabled = habilitar;
            dtGridPedidos.IsReadOnly = !habilitar;
            cbFormaPgto.IsEnabled = habilitar;
            btnGravar.IsEnabled = habilitar;
        }

        private void btnNovo_Click(object sender, RoutedEventArgs e)
        {
            txtId.Text = "";
            cbCliente.SelectedIndex = -1;
            dpData.SelectedDate = DateTime.Now;
            cbProduto.SelectedIndex = -1;
            txtPreco.Text = "";
            txtQtd.Text = "";
            cbFormaPgto.SelectedIndex = -1;
            _itensView.Clear();
            AtualizaTotal();
            txtStatus.Text = "Pendente";
            HabilitaEdicao(true);
        }

        private async void btnPesquisar_Click(object sender, RoutedEventArgs e)
        {
            int idBusca;
            if (int.TryParse(txtPesquisaId.Text, out idBusca) && idBusca > 0)
            {
                var pedido = await _context.Pedidos.GetByIdAsync(idBusca);
                if (pedido != null)
                {
                    await CarregarPedidoNaTela(pedido);
                    return;
                }
                MessageBox.Show("Pedido #" + idBusca + " não encontrado.");
                return;
            }

            var clienteSel = cbCliente.SelectedItem as PessoaModel;
            if (clienteSel != null)
            {
                var pedidosCliente = (await _context.Pedidos.GetAllAsync())
                    .Where(p => p.ClienteId == clienteSel.Id)
                    .OrderByDescending(p => p.Data)
                    .ToList();

                if (pedidosCliente.Any())
                {
                    await CarregarPedidoNaTela(pedidosCliente.First());
                    return;
                }
                MessageBox.Show("Esse cliente não possui pedidos.");
                return;
            }

            var Nome = (cbCliente.Text ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(Nome))
            {
                var pessoas = (await _context.Pessoas.GetAllAsync())
                    .Where(p => (p.Nome ?? "").IndexOf(Nome, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                if (pessoas.Any())
                {
                    var pessoa = pessoas.First();
                    cbCliente.SelectedValue = pessoa.Id;

                    var pedidosCliente = (await _context.Pedidos.GetAllAsync())
                        .Where(p => p.ClienteId == pessoa.Id)
                        .OrderByDescending(p => p.Data)
                        .ToList();

                    if (pedidosCliente.Any())
                    {
                        await CarregarPedidoNaTela(pedidosCliente.First());
                        return;
                    }

                    MessageBox.Show("Cliente encontrado, mas não há pedidos para ele.");
                    return;
                }

                MessageBox.Show("Cliente não encontrado pelo nome informado.");
                return;
            }

            MessageBox.Show("Informe um Id do pedido, selecione um cliente ou digite um nome para pesquisar.");
        }

        private async Task CarregarPedidoNaTela(PedidoModel pedido)
        {
            txtId.Text = pedido.Id.ToString();
            cbCliente.SelectedValue = pedido.ClienteId;
            dpData.SelectedDate = pedido.Data;

            if (!string.IsNullOrWhiteSpace(pedido.FormaPgto))
            {
                for (int i = 0; i < cbFormaPgto.Items.Count; i++)
                {
                    var cbi = cbFormaPgto.Items[i] as ComboBoxItem;
                    if (cbi != null && cbi.Content != null &&
                        string.Equals(cbi.Content.ToString(), pedido.FormaPgto, StringComparison.OrdinalIgnoreCase))
                    {
                        cbFormaPgto.SelectedIndex = i;
                        break;
                    }
                }
            }

            _itensView.Clear();

            var produtos = await _context.Produtos.GetAllAsync();
            var dictProd = produtos.ToDictionary(p => p.Id, p => p);

            foreach (var it in pedido.Itens)
            {
                ProdutoModel prod;
                dictProd.TryGetValue(it.ProdutoId, out prod);

                _itensView.Add(new ItemPedidoView
                {
                    ProdutoId = it.ProdutoId,
                    ProdutoNome = prod != null ? prod.Nome : ("ID " + it.ProdutoId),
                    Quantidade = it.Quantidade,
                    PrecoUnitario = it.PrecoUnitario
                });
            }

            AtualizaTotal();
            txtStatus.Text = string.IsNullOrWhiteSpace(pedido.Status) ? "Finalizado" : pedido.Status;

            bool habilitar = string.Equals(pedido.Status, "Pendente", StringComparison.OrdinalIgnoreCase);
            HabilitaEdicao(habilitar);
        }

        private async void btnExcluir_Click(object sender, RoutedEventArgs e)
        {
            int id;
            if (!int.TryParse(txtId.Text, out id) || id <= 0)
            {
                MessageBox.Show("Carregue um pedido válido antes de excluir.");
                return;
            }

            var confirm = MessageBox.Show("Confirma a exclusão do pedido #" + id + "?",
                                          "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                var pedido = await _context.Pedidos.GetByIdAsync(id);
                if (pedido == null)
                {
                    MessageBox.Show("Pedido não encontrado.");
                    return;
                }

                await _context.Pedidos.DeleteAsync(id);

                MessageBox.Show("Pedido #" + id + " excluído com sucesso.");
                btnNovo_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao excluir: " + ex.Message);
            }
        }
    }

    public class ItemPedidoView : INotifyPropertyChanged
    {
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; }
        private int _quantidade;
        public int Quantidade
        {
            get { return _quantidade; }
            set { _quantidade = value; OnPropertyChanged(nameof(Quantidade)); OnPropertyChanged(nameof(Subtotal)); }
        }
        public decimal PrecoUnitario { get; set; }
        public decimal Subtotal { get { return Quantidade * PrecoUnitario; } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public void NotifyChanged() { OnPropertyChanged(nameof(Quantidade)); OnPropertyChanged(nameof(Subtotal)); }
    }
}
