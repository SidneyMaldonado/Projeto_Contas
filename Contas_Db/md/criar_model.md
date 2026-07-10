Na pasta C:\Users\smaldonado\source\repos\Projeto_Contas\Contas_Db\md\ há um arquivo chamado script_create.sql
No projeto Conta_DB há uma pasta chamada Model (C:\Users\smaldonado\source\repos\Projeto_Contas\Contas_Db\Model\)
Dentro dessa pasta:
Para cada create dentro do script_create.sql crie uma model padrão C#, use Anottations para Definir chave primaria, 
tamanho e obrigatoriedade.
O Nome deve ser o Mesmo da tabela sem o tb_. E normalizado para o padrão C# com iniciais maúsculas.
Depois de criar as Models:
1 - Crie uma Interface IRepository<T> com os métodos padrão de repositório (Add, Update, Delete, GetById, GetAll).
2 - Crie uma classe Repository<T> que implementa a interface IRepository<T> e utilize Entity Framework para realizar as operações no banco de dados.
Coloque os dois em uma pasta chamada Repository dentro do projeto Conta_DB (C:\Users\smaldonado\source\repos\Projeto_Contas\Contas_Db\Repository\).
Depois crie um DBContext chamado ContasDbContext que herda de DbContext e configure as DbSets para cada model criada. Coloque o ContasDbContext na pasta Model.
Servidor: MS211,1434
Banco: test_fin
usuario: use SSPI
Coloque a string de conexao na onconfiguring do DBContext
Pode instalar o pacote Microsoft.EntityFrameworkCore.SqlServer via NuGet para utilizar o Entity Framework com SQL Server.
Cada chave estrangeira deve gerar DUAS propriedades na Model: a propriedade escalar do id (ex.: IdUsuario) e a propriedade de navegação correspondente (ex.: Usuario), seguindo o padrão convencional do Entity Framework Core.

Mapeamento de tipos especiais (confirmado):
  - varbinary(max) → byte[]
  - bit → bool
  - numeric(10,2) → decimal

As constraints CHECK e UNIQUE do script podem ser ignoradas (não precisam de anotação/Fluent API equivalente).

O IRepository deve ser assíncrono: todos os métodos retornam Task ou Task<T> e usam async/await para as operações de banco.

Nome do atributo de tabela — já que o nome da classe difere do nome da tabela real (tb_categoria vs Categoria), é necessário usar [Table("tb_categoria")] para o EF mapear corretamente.

A Class1.cs pode ser removida, pois não é necessária para o funcionamento do projeto.

## Regra de nomenclatura das colunas

1. Remova o prefixo da coluna e converta o restante para PascalCase, exceto id_ e dt_, que são sempre traduzidos para uma palavra semântica (ver regra 3):
   - nm_ → remove (fica só o restante)
   - ds_ → remove (fica só o restante)
   - dm_ → remove (fica só o restante)
   - nr_ → remove (fica só o restante)
   - img_ → remove (fica só o restante)
2. Se o nome resultante colidir com o nome da própria entidade (classe), substitua o prefixo por uma palavra semântica em vez de simplesmente removê-lo:
   - nm_ → Nome
   - ds_ → Descricao
   - img_ → Imagem
   (dm_ e nr_ não colidem em nenhuma tabela deste schema, mas seguem a mesma lógica se ocorrer.)
3. Prefixos sempre traduzidos, independentemente de colisão:
   - id_ → Id (colunas próprias) ou Id + nome da tabela referenciada, para FKs (ex.: id_usuario → IdUsuario), para não conflitar com a propriedade de navegação.
   - dt_ → Data + restante em PascalCase (ex.: dt_vencimento → DataVencimento, dt_pagamento → DataPagamento, dt_primeiro_vencimento → DataPrimeiroVencimento).
4. Colunas sem prefixo reconhecido (ex.: dia_vencimento) mantêm o nome completo, apenas convertido para PascalCase (DiaVencimento).

### Mapeamento final por tabela

**tb_categoria → Categoria**
- id_categoria → Id (PK)
- nm_categoria → Nome
- img_categoria → Imagem
- dm_ativo → Ativo

**tb_conta → Conta**
- id_conta → Id (PK)
- id_usuario → IdUsuario (FK) + navegação Usuario
- nm_conta → Nome
- img_conta → Imagem
- nr_saldo → Saldo
- dm_ativo → Ativo

**tb_credor → Credor**
- id_credor → Id (PK)
- nm_credor → Nome
- ds_observacoes → Observacoes
- img_logo → Logo
- dm_ativo → Ativo

**tb_divida → Divida**
- id_divida → Id (PK)
- id_usuario → IdUsuario (FK) + navegação Usuario
- id_credor → IdCredor (FK, nullable) + navegação Credor (nullable)
- nm_divida → Nome
- dia_vencimento → DiaVencimento
- dt_primeiro_vencimento → DataPrimeiroVencimento
- nr_parcelas → Parcelas
- nr_valor → Valor
- dm_ativo → Ativo

**tb_parcela → Parcela**
- id_parcela → Id (PK)
- id_divida → IdDivida (FK) + navegação Divida
- id_categoria → IdCategoria (FK) + navegação Categoria
- id_conta → IdConta (FK) + navegação Conta
- ds_parcela → Descricao
- nr_valor → Valor
- dt_vencimento → DataVencimento
- dt_pagamento → DataPagamento (nullable)
- dm_ativo → Ativo

**tb_usuario → Usuario**
- id_usuario → Id (PK)
- nm_usuario → Nome
- ds_email → Email
- ds_senha → Senha
- img_usuario → Imagem
- dm_ativo → Ativo

A connection string deve incluir `TrustServerCertificate=True` para evitar erro de certificado com o servidor MS211,1434.

As propriedades de navegação existem apenas no lado da FK (ex.: Parcela.Divida). Coleções inversas (ex.: Usuario.Contas, Divida.Parcelas) NÃO devem ser criadas.
