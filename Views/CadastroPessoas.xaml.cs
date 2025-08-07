using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WpfApp1.Data;
using WpfApp1.Models;
using WpfApp1.Services;

namespace WpfApp1
{
    public partial class CadastroPessoas : Window
    {
        private readonly Context _context;
        private ObservableCollection<PedidoModel> _pedidosFiltrados = new ObservableCollection<PedidoModel>();
        private readonly Service service = new Service();

        public CadastroPessoas()
        {
            InitializeComponent();
            _context = App.Db;
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await AtualizarGridAsync();
        }

        private async Task AtualizarGridAsync()
        {
            var pessoas = (await _context.Pessoas.GetAllAsync())
                          .OrderBy(p => p.Nome)
                          .ToList();
            dtGridPessoas.ItemsSource = pessoas;
        }

        private void LimparFormulario()
        {
            txtId.Text = "";
            txtNome.Text = "";
            txtCpfCnpj.Text = "";
            txtEndereco.Text = "";
            txtComplemento.Text = "";
            txtCep.Text = "";
            txtCidade.Text = "";
            txtEstado.Text = "";
            dtGridPessoas.SelectedIndex = -1;
        }

        private async void btnGravar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text))
            {
                MessageBox.Show("Nome é obrigatório.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidaCpfOuCnpj(txtCpfCnpj.Text))
            {
                MessageBox.Show("Documento inválido (CPF ou CNPJ).", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (int.TryParse(txtId.Text, out int id) && id > 0)
                {
                    var pessoa = await _context.Pessoas.GetByIdAsync(id);
                    if (pessoa != null)
                    {
                        pessoa.Nome = txtNome.Text.Trim();
                        pessoa.CpfCnpj = txtCpfCnpj.Text.Trim();
                        pessoa.Endereco = txtEndereco.Text.Trim();
                        pessoa.Complemento = txtComplemento.Text.Trim();
                        pessoa.Cep = txtCep.Text.Trim();
                        pessoa.Cidade = txtCidade.Text.Trim();
                        pessoa.Estado = txtEstado.Text.Trim();
                        await _context.Pessoas.UpdateAsync(pessoa);
                    }
                }
                else
                {
                    var novaPessoa = new PessoaModel
                    {
                        Nome = txtNome.Text.Trim(),
                        CpfCnpj = txtCpfCnpj.Text.Trim(),
                        Endereco = txtEndereco.Text.Trim(),
                        Complemento = txtComplemento.Text.Trim(),
                        Cep = txtCep.Text.Trim(),
                        Cidade = txtCidade.Text.Trim(),
                        Estado = txtEstado.Text.Trim(),
                    };
                    await _context.Pessoas.AddAsync(novaPessoa);
                    txtId.Text = novaPessoa.Id.ToString();
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
                        await _context.Pessoas.DeleteAsync(id);
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
                MessageBox.Show("Selecione uma pessoa para excluir.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                : txtCpfCnpj.Text?.Trim();

            var pessoas = (await _context.Pessoas.GetAllAsync())
                  .Where(p =>
                      string.IsNullOrWhiteSpace(pesquisa) ||
                      p.Nome.Contains(pesquisa) ||
                      new string(p.CpfCnpj.Where(char.IsDigit).ToArray())
                          .Contains(new string(pesquisa.Where(char.IsDigit).ToArray()))
                  )
                  .OrderBy(p => p.Nome)
                  .ToList();

            dtGridPessoas.ItemsSource = pessoas;
        }

        private void dtGridPessoas_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dtGridPessoas.SelectedItem is PessoaModel pessoa)
            {
                txtId.Text = pessoa.Id.ToString();
                txtNome.Text = pessoa.Nome;
                txtCpfCnpj.Text = pessoa.CpfCnpj;
                txtEndereco.Text = pessoa.Endereco;
                txtComplemento.Text = pessoa.Complemento;
                txtCep.Text = pessoa.Cep;
                txtCidade.Text = pessoa.Cidade;
                txtEstado.Text = pessoa.Estado;
            }
            else
            {
                LimparFormulario();
            }
        }

        private bool ValidaCpfOuCnpj(string documento)
        {
            if (string.IsNullOrWhiteSpace(documento))
                return false;

            documento = new string(documento.Where(char.IsDigit).ToArray());

            if (documento.Length == 11)
                return ValidaCpf(documento);
            else if (documento.Length == 14)
                return ValidaCnpj(documento);

            return false;
        }

        private bool ValidaCpf(string cpf)
        {
            if (cpf.Length != 11) return false;
            if (new string(cpf[0], 11) == cpf) return false;

            int Calc(int len) =>
                (Enumerable.Range(0, len).Sum(i => (cpf[i] - '0') * (len + 1 - i)) * 10) % 11 % 10;

            return Calc(9) == (cpf[9] - '0') && Calc(10) == (cpf[10] - '0');
        }

        private bool ValidaCnpj(string cnpj)
        {
            if (cnpj.Length != 14) return false;
            if (new string(cnpj[0], 14) == cnpj) return false;

            int[] pesos1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] pesos2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            bool CalculaDigito(string num, int[] pesos, int digitoPos)
            {
                int soma = 0;
                for (int i = 0; i < pesos.Length; i++)
                    soma += (num[i] - '0') * pesos[i];

                int resto = soma % 11;
                int digito = resto < 2 ? 0 : 11 - resto;

                return digito == (num[digitoPos] - '0');
            }

            if (!CalculaDigito(cnpj, pesos1, 12)) return false;
            if (!CalculaDigito(cnpj, pesos2, 13)) return false;

            return true;
        }

        private void btnCriarPedido_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtNome.Text) &&
                !string.IsNullOrWhiteSpace(txtCpfCnpj.Text) &&
                !string.IsNullOrWhiteSpace(txtId.Text))
            {
                PessoaModel pessoaSelecionada = new PessoaModel
                {
                    Nome = txtNome.Text,
                    CpfCnpj = txtCpfCnpj.Text,
                    Id = int.Parse(txtId.Text)
                };

                CadastroPedidos cadastroPedidos = new CadastroPedidos();
                cadastroPedidos.PessoaSelecionada = pessoaSelecionada;
                cadastroPedidos.ShowDialog();
            }
            else
            {
                MessageBox.Show("Preencha os dados da pessoa antes de criar o pedido.");
            }
        }

        private PedidoModel pedidoSelecionado => dtGridPedidos.SelectedItem as PedidoModel;

        private async void BtnMarcarPago_Click(object sender, RoutedEventArgs e)
        {
            if (pedidoSelecionado == null)
            {
                MessageBox.Show("Selecione um pedido.");
                return;
            }

            if (service.PodeMarcarComoPago(pedidoSelecionado))
            {
                pedidoSelecionado.Status = "Pago";
                await _context.Pedidos.UpdateAsync(pedidoSelecionado);
                AtualizarGridPedidos();
            }
            else
            {
                MessageBox.Show("Somente pedidos pendentes podem ser marcados como pagos.");
            }
        }

        private async void BtnMarcarEnviado_Click(object sender, RoutedEventArgs e)
        {
            if (pedidoSelecionado == null)
            {
                MessageBox.Show("Selecione um pedido.");
                return;
            }

            if (service.PodeMarcarComoEnviado(pedidoSelecionado))
            {
                pedidoSelecionado.Status = "Enviado";
                await _context.Pedidos.UpdateAsync(pedidoSelecionado);
                AtualizarGridPedidos();
            }
            else
            {
                MessageBox.Show("Somente pedidos pagos podem ser enviados.");
                return;
            }
        }

        private async void BtnMarcarRecebido_Click(object sender, RoutedEventArgs e)
        {
            if (pedidoSelecionado == null)
            {
                MessageBox.Show("Selecione um pedido.");
                return;
            }

            if (service.PodeMarcarComoRecebido(pedidoSelecionado))
            {
                pedidoSelecionado.Status = "Recebido";
                await _context.Pedidos.UpdateAsync(pedidoSelecionado);
                AtualizarGridPedidos();
            }
            else
            {
                MessageBox.Show("Somente pedidos enviados podem ser marcados como recebidos.");
                return;
            }
        }

        private async void FiltrosPedido_Changed(object sender, RoutedEventArgs e)
        {
            var pessoa = txtId.Text;
            if (string.IsNullOrWhiteSpace(pessoa)) return;

            Dictionary<int, ProdutoModel> dictProd = (await _context.Produtos.GetAllAsync())
                .ToDictionary(p => p.Id);

            var pedidos = (await _context.Pedidos.GetAllAsync())
                .Where(p => string.IsNullOrWhiteSpace(pessoa) || p.ClienteId == Convert.ToInt32(pessoa))
                .OrderBy(p => p.Id)
                .ToList();

            if (chkMostrarPagos.IsChecked == true)
                pedidos = pedidos.Where(p => p.Status == "Pago").ToList();
            else if (chkMostrarEntregues.IsChecked == true)
                pedidos = pedidos.Where(p => p.Status == "Enviado").ToList();
            else if (chkMostrarPendentes.IsChecked == true)
                pedidos = pedidos.Where(p => p.Status == "Pendente").ToList();

            foreach (var pedido in pedidos)
            {
                pedido.ItensTexto = service.GerarItensTexto(pedido, dictProd);
                pedido.ClienteNome = await service.ObterNomeClienteAsync(pedido.ClienteId, _context.Pessoas);
            }

            dtGridPedidos.ItemsSource = pedidos;
        }

        private void AtualizarGridPedidos()
        {
            dtGridPedidos.Items.Refresh();
        }

        private async void chkMostrarTodos_Checked(object sender, RoutedEventArgs e)
        {
            Dictionary<int, ProdutoModel> dictProd = (await _context.Produtos.GetAllAsync())
                .ToDictionary(p => p.Id);
            Dictionary<int, PessoaModel> dictClientes = (await _context.Pessoas.GetAllAsync())
                .ToDictionary(c => c.Id);

            var pedidos = (await _context.Pedidos.GetAllAsync())
                .OrderBy(p => p.Id)
                .ToList();

            foreach (var p in pedidos)
            {
                p.ItensTexto = string.Join(", ", p.Itens.Select(i =>
                    dictProd.TryGetValue(i.ProdutoId, out var prod) ? prod.Nome : $"ID {i.ProdutoId}"));
                p.ClienteNome = dictClientes.TryGetValue(p.ClienteId, out var cliente) ? cliente.Nome : $"ID {p.ClienteId}";
            }

            dtGridPedidos.ItemsSource = pedidos;
        }
    }
}