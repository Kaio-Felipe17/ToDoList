# ToDoList API

API de tarefas em .NET 9, com Minimal APIs, MediatR e EF Core / SQL Server.

## Organização

- **Domain:** entidades, estado inicial e regras de criação de itens.
- **Application:** casos de uso em Items/Create e Items/GetAll, DTOs, mapeamentos e contratos dos repositórios.
- **Infrastructure:** contexto, configurações de persistência, repositórios e migrations.
- **API:** endpoints HTTP, configuração dos serviços e tratamento centralizado de erros.
- **Tests:** regras de criação, handlers, persistência relacional e respostas de erro.

Application referencia Domain; Infrastructure referencia Application; API compõe os serviços.

## Comportamento

- POST /items cria um item e retorna 201 com o DTO.
- GET /items lista os itens não excluídos.
- Nome obrigatório, com até 100 caracteres; descrição opcional, com até 200.
- Nome e descrição são aparados na criação. Itens começam ativos e não concluídos.
- Nomes são únicos entre itens não excluídos. A comparação segue a collation do SQL Server.
- Dados inválidos retornam 400; nome duplicado retorna 409; erros inesperados retornam 500 sem detalhes internos. Essas exceções são retornadas como ProblemDetails.
- O repositório faz exclusão lógica, preservando o registro e preenchendo DeletedAt. Consultas usuais filtram os excluídos; o nome pode ser reutilizado depois da exclusão.
- Não há endpoint de exclusão ou atualização nesta versão.
- CancellationToken é propagado do endpoint até o EF Core.

## Execução

Configure ConnectionStrings:Database via secrets de desenvolvimento ou pela variável de ambiente ConnectionStrings__Database. Não versione credenciais.

~~~powershell
dotnet build ToDoList/ToDoList.sln -m:1
dotnet test ToDoList/ToDoList.sln -m:1
dotnet run --project ToDoList/ToDoList.API
~~~

A documentação Scalar está disponível em /scalar/v1 no ambiente Development.

## Banco de dados

Instale o dotnet-ef 9.0.20, caso ainda não esteja disponível:

~~~powershell
dotnet tool install --global dotnet-ef --version 9.0.20
dotnet ef database update --project ToDoList/ToDoList.Infrastructure --startup-project ToDoList/ToDoList.API
~~~

A migration ApplyItemRulesAndSoftDelete aplica limites de tamanho e um índice único filtrado por IsDeleted = 0. Ela interrompe a execução se houver valores acima dos limites ou nomes duplicados entre itens não excluídos, para permitir correção dos dados sem truncamento ou exclusão automáticos.

Os testes de persistência usam SQLite em memória e não acessam seu banco SQL Server. A interpretação de nomes pela collation e a tradução de erros específicos do SQL Server devem também ser verificadas nesse provedor antes de uma implantação.