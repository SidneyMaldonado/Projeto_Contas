# commit d0ba7c7

- **Data:** 2026-07-10
- **Autor:** miltec\smaldonado
- **Mensagem original:** Termino do backend

## Resumo

Primeiro backend real do projeto. Cria os Models de EF Core em `Contas_Db/Model` (`Categoria`, `Conta`, `Credor`, `Divida`, `Parcela`, `Usuario`, mais `ISoftDelete` como marcador de exclusão lógica) e o `ContasDbContext`. Introduz o padrão de repositório: `IRepository<T>` genérico (`Contas_Db/Repository/IRepository.cs`) com `Repository<T>` como implementação base, mais um repositório especializado (`IParcelaRepository`/`ParcelaRepository`) para a operação extra de pagar parcela. Atualiza o script SQL (`script_create.sql`) para refletir esse schema.

Adiciona o projeto `Contas_Test` (MSTest) com testes de repositório para todas as entidades (`CategoriaRepositoryTests`, `ContaRepositoryTests`, `CredorRepositoryTests`, `DividaRepositoryTests`, `ParcelaRepositoryTests`, `UsuarioRepositoryTests`) e um teste de conectividade (`ConexaoTest`).

É o commit que estabelece a base de dados e a camada de acesso a dados que todo o resto do projeto (UseCases, API) vai construir por cima.

## Arquivos afetados

- `Contas_Db/Model/*.cs` (Categoria, Conta, ContasDbContext, Credor, Divida, ISoftDelete, Parcela, Usuario — novos)
- `Contas_Db/Repository/IParcelaRepository.cs`, `IRepository.cs`, `ParcelaRepository.cs`, `Repository.cs` (novos)
- `Contas_Db/Contas_Db.csproj` (referências ao EF Core)
- `Contas_Db/md/criar_model.md`, `Contas_Db/md/script_create.sql` (atualizados)
- `Contas_Test/*RepositoryTests.cs`, `Contas_Test/ConexaoTest.cs`, `Contas_Test/Contas_Test.csproj`, `Contas_Test/MSTestSettings.cs` (novo projeto de testes)
- `Projeto_Contas.slnx`

26 arquivos alterados, 1381 inserções, 24 remoções.
