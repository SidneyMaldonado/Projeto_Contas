# Projeto Contas

Aplicação para controle financeiro pessoal: cadastro de contas, credores,
categorias, dívidas e suas parcelas, com controle de saldo e vencimento.

## Estrutura da solução

| Projeto | Descrição |
|---|---|
| `Contas_Core` | Regras de negócio: UseCases (`UseCase/`) e validações (`Biz/`). |
| `Contas_Db` | Modelos EF Core (`Model/`), `DbContext`, repositórios (`Repository/`) e script de criação do banco (`Script/create.sql`). |
| `Contas_App` | App .NET MAUI (multiplataforma). Hoje é só o shell padrão do template — ainda não está integrado ao `Contas_Core`. |
| `Contas_Test` | Testes automatizados (MSTest) dos UseCases e repositórios. |

## Arquitetura

- **UseCase**: uma classe por operação (`AdicionarUsuarioUseCase`,
  `ObterTodosContaUseCase`, etc.), com um único método `ExecuteAsync`.
- **Biz**: uma classe de validação por UseCase de criação (`AdicionarUsuarioBiz`,
  `AdicionarDividaBiz`, ...), com um método `IsValid`/`IsValidAsync` que combina
  regras individuais (ex: `NomeNotNull`, `ValidMail`). O UseCase lança
  `ArgumentException` quando a entidade é inválida.
- **Repository**: `IRepository<T>` genérico (CRUD + soft delete) implementado por
  `Repository<T>`; entidades com operações específicas têm repositório próprio
  (`IUsuarioRepository`, `IContaRepository`, `IParcelaRepository`).
- **Soft delete**: entidades implementam `ISoftDelete` (flag `Ativo`); exclusão
  lógica via `SoftDeleteAsync`, sem remover o registro do banco.

## Domínio

- **Usuario** — dono das contas e dívidas. Senha é hasheada (PBKDF2) antes de
  persistir.
- **Conta** — conta bancária/carteira do usuário, com saldo.
- **Credor** — para quem uma dívida é devida.
- **Categoria** — classificação das parcelas (ex: Lazer, Mercado).
- **Divida** — um compromisso financeiro, dividido em parcelas.
- **Parcela** — cada parcela de uma dívida, associada a uma conta e categoria,
  com valor, vencimento e data de pagamento.

## Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- SQL Server acessível (para rodar a aplicação; os testes usam banco em
  memória e não precisam de SQL Server)
- Para compilar o `Contas_App`: workload do .NET MAUI
  (`dotnet workload install maui`)

## Banco de dados

O script de criação está em `Contas_Db/Script/create.sql` (banco `test_fin`).
A string de conexão está em `Contas_Db/Model/ContasDbContext.cs` — ajuste o
`Server`/`Database` conforme seu ambiente antes de rodar a aplicação.

## Build e testes

```bash
# compilar toda a solução
dotnet build Projeto_Contas.slnx

# rodar os testes
dotnet test Contas_Test/Contas_Test.csproj
```

## Regras de negócio implementadas

| Entidade | Regras |
|---|---|
| Usuario | Nome ≥ 3 caracteres; e-mail válido; e-mail não pode já estar cadastrado; senha ≥ 8 caracteres com maiúscula, minúscula e número. |
| Categoria / Credor / Conta | Nome ≥ 3 caracteres. |
| Conta | Saldo não pode ser negativo. |
| Divida | Nome ≥ 3 caracteres; valor > 0; número de parcelas ≥ 1; data do primeiro vencimento não pode ser passada; dia de vencimento entre 1 e 31 e consistente com a data do primeiro vencimento. |
| Parcela | Valor > 0. |
