Vamos criar as regras de negócio.
Vamos criar uma pasta chamada Biz na rais do projeto Core
Para cada Usecase devemos criar uma biz com o mesmo padrão de nome,
odede serão colocados as regras de negócio.
Eemplo:
AdicionarUsuarioUseCase.cs -> AdicionarUsuarioBiz.cs
Regras de negócio: 
1) O nome do usuario não pode ter menos de 3 caracteres e não pode ser vazio
2) O email deve ser um email válido.
Então teremos
public bool NomeNotNull
public bool ValidMail
Depois fazeremos um método IsValid para validar as regras de negocio
Assim teremos AdicionarUsuarioBiz.Isvalid(Model entity)
Dentro dessa Isvalid ela deve chamar NomeNOtNull e ValidMail 
Para validar as regras
Ela pode rtornar assim: return NomeNotNull() && ValidMail()

Resumo:
1 - Deve Criar a pasta Biz
2 - Criar uma biz para AdicionarUsuarioUseCase dentro da pasta biz
3 - Criar uma validação para cada regra
4 - Criar um IsValid na Biz
5 - Fazer a chamada no Usecase

Primeiro faça só no AdicionarUsuarioUseCase


 Regras as serem Implementadas:
1. O nome do usuario não pode ter menos de 3 caracteres e não pode ser vazio
2. O email deve ser um email válido.
4. A senha do usuário deve ter pelo menos 8 caracteres, incluindo pelo menos uma letra maiúscula, uma letra minúscula e um número.
5. O nome do credor não pode ter menos de 3 caracteres e não pode ser vazio.
6. O nome da categoria não pode ter menos de 3 caracteres e não pode ser vazio
7. O nome da conta não pode ter menos de 3 caracteres e não pode ser vazio
8. O nome da divida não pode ter menos de 3 caracteres e não pode ser vazio
9. O valor da divida não pode ser negativo ou zero.
10. A data de vencimento da divida não pode ser uma data passada.
11. O valor da transação não pode ser negativo ou zero.
12. O dia_vencimento da transação deve se um dia entre 1 e 31.
