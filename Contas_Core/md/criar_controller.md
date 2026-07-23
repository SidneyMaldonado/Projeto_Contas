
!Faça tudo em C#
O Projeto tem as camadas: Contas_Api que usa restfull
Contas_Core que contem a aplicação em si e a pasta usecase onde ficam os casos de uso
Contas_Db que possui os repositórios e o contexto do banco de dados.
A api deve sempre chamar um usecase
O usecase depois de validar deve chamar o repository
o repository deve fazer gravações no banco de dados.
Na Contas_Core tem uma pasta biz. onde ficam as regras de negocio de cada entidade

Para cada use case crie um controller na Contas_Api que vai receber as requisições e chamar o usecase correspondente. 
O controller deve ser responsável por receber os dados da requisição, 
validar os dados básicos (como campos obrigatórios) e então chamar o usecase
. O usecase, por sua vez, deve conter a lógica de negócio e chamar o repository para realizar operações no banco de dados.
Agrupe ass controller por temas. exemplo: todos os controllers relacionados a contas devem ficar em umacontroller chamada "ContasController" dentro da pasta "Controllers" na Contas_Api.
Também para receber os dados da api voce deve criar um DTO para cada entidade que será manipulada. O DTO deve conter apenas os campos necessários para a operação específica e deve ser usado tanto no controller quanto no usecase.
Coloque os DTOs em uma pasta chamada "DTOs" dentro da Contas_Core.

Também será necessário criar os conversores de DTO para entidade e de entidade para DTO. 
Esses conversores devem ser implementados em uma pasta chamada
"Converters" dentro da Contas_Core. Cada conversor deve ter métodos estáticos 
para realizar a conversão entre os tipos.

Se faltar alguma informação me questione.

