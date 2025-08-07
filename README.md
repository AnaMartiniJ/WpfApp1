
# WpfApp1

Este é um projeto de aplicação desktop desenvolvido com **WPF (.NET Framework 4.6)** em C#. Ele simula um sistema de cadastro e gerenciamento de pedidos, produtos e pessoas, com persistência de dados em arquivos JSON.

## Funcionalidades

- Cadastro de pessoas, produtos e pedidos
- Associação de múltiplos itens a um pedido
- Filtros para visualizar pedidos:
  - Pagos
  - Enviados
  - Pendentes
- Ações rápidas:
  - Marcar como Pago
  - Marcar como Enviado
  - Marcar como Recebido
- Dados armazenados localmente em arquivos .json
- Utilização de repositório genérico assíncrono
- Visualização de dados em grids com WPF

## Estrutura do Projeto

- Models/ — Contém as classes PessoaModel, ProdutoModel, PedidoModel, etc.
- Views/ — Janelas do WPF (XAML) como CadastroPessoas, CadastroProdutos, CadastroPedidos.
- Services/ — Serviços auxiliares como geração de texto de itens e nome do cliente.
- Data/ — Repositório genérico (IRepository, Repository<T>) e Context com todos os repositórios.
- pessoas.json, produtos.json, pedidos.json — Arquivos que armazenam os dados da aplicação.

## Tecnologias Utilizadas

- C# com .NET Framework 4.6
- WPF (Windows Presentation Foundation)
- JSON para persistência de dados
- LINQ para consultas
- Programação assíncrona (async/await)
- Organização por camadas: Model, View, Repository, Services

## Como Executar

1. Abra a solução WpfApp1.sln no Visual Studio.
2. Compile o projeto.
3. Execute a aplicação (F5).

## Observações

- Para criar um pedido, é necessário já ter cadastrado uma pessoa e um produto.


