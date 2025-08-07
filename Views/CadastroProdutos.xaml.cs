using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WpfApp1.Data;
using WpfApp1.Models;

namespace WpfApp1
{
    public partial class CadastroProdutos : Window
    {
        private readonly Context _context;
        public CadastroProdutos()
        {
            InitializeComponent();
            _context = App.Db;
        }

        private async Task AtualizarGridAsync()
        {
            var produtos = (await _context.Produtos.GetAllAsync())
                          .OrderBy(p => p.Nome)
                          .ToList();
            dtGridProdutos.ItemsSource = produtos;
        }

        private void LimparFormulario()
        {
            txtId.Text = "";
            txtCodigo.Text = "";
            txtNome.Text = "";
            txtValor.Text = "";
            dtGridProdutos.SelectedIndex = -1;
        }

        private async void btnGravar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text))
            {
                MessageBox.Show("Nome é obrigatório.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtCodigo.Text))
            {
                MessageBox.Show("Codigo é obrigatório.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtValor.Text))
            {
                MessageBox.Show("Valor é obrigatório.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (int.TryParse(txtId.Text, out int id) && id > 0)
                {
                    var produto = await _context.Produtos.GetByIdAsync(id);
                    if (produto != null)
                    {
                        produto.Codigo = Convert.ToInt32(txtCodigo.Text);
                        produto.Nome = txtNome.Text.Trim();
                        produto.Preco = Convert.ToDecimal(txtValor.Text);
                        await _context.Produtos.UpdateAsync(produto);
                    }
                }
                else
                {
                    var novoProduto = new ProdutoModel
                    {
                        Codigo = Convert.ToInt32(txtCodigo.Text),
                        Nome = txtNome.Text.Trim(),
                        Preco = Convert.ToDecimal(txtValor.Text)
                    };
                    await _context.Produtos.AddAsync(novoProduto);
                    txtId.Text = novoProduto.Id.ToString();
                }

                await AtualizarGridAsync();
                LimparFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnExcluir_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtId.Text, out int id) && id > 0)
            {
                var res = MessageBox.Show("Confirma exclusão?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _context.Produtos.DeleteAsync(id);
                        await AtualizarGridAsync();
                        LimparFormulario();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erro ao excluir: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Selecione um Produto para excluir.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnNovo_Click(object sender, RoutedEventArgs e)
        {
            LimparFormulario();
        }

        private async void btnPesquisar_Click(object sender, RoutedEventArgs e)
        {
            string pesquisa = !string.IsNullOrWhiteSpace(txtNome.Text)
                ? txtNome.Text.Trim()
                : txtCodigo.Text?.Trim();

            var produtos = (await _context.Produtos.GetAllAsync())
                  .Where(p =>
                      string.IsNullOrWhiteSpace(pesquisa) ||
                      p.Nome.Contains(pesquisa) ||
                      new string(p.Codigo.ToString().Where(char.IsDigit).ToArray())
                          .Contains(new string(pesquisa.Where(char.IsDigit).ToArray()))
                  )
                  .OrderBy(p => p.Nome)
                  .ToList();

            dtGridProdutos.ItemsSource = produtos;
        }

        private void dtGridProdutos_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dtGridProdutos.SelectedItem is ProdutoModel produto)
            {
                txtId.Text = produto.Id.ToString();
                txtNome.Text = produto.Nome;
                txtCodigo.Text = produto.Codigo.ToString();
                txtValor.Text = produto.Preco.ToString();
            }
            else
            {
                LimparFormulario();
            }
        }
    }
}

