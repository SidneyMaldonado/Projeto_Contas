using System.Linq;
using Contas_Db.Repository.Interface;

namespace Contas_Core.Biz;

public class AdicionarUsuarioBiz
{
    private readonly IUsuarioRepository _repository;
    private Contas_Db.Model.Usuario _entity = null!;

    public AdicionarUsuarioBiz(IUsuarioRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> IsValidAsync(Contas_Db.Model.Usuario entity)
    {
        _entity = entity;
        return NomeNotNull() && ValidMail() && SenhaValida() && await EmailNotDuplicadoAsync();
    }

    public bool NomeNotNull() =>
        !string.IsNullOrWhiteSpace(_entity.Nome) && _entity.Nome.Trim().Length >= 3;

    public bool ValidMail()
    {
        try
        {
            _ = new System.Net.Mail.MailAddress(_entity.Email);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public bool SenhaValida() =>
        !string.IsNullOrEmpty(_entity.Senha) &&
        _entity.Senha.Length >= 8 &&
        _entity.Senha.Any(char.IsUpper) &&
        _entity.Senha.Any(char.IsLower) &&
        _entity.Senha.Any(char.IsDigit);

    public async Task<bool> EmailNotDuplicadoAsync() => !await _repository.EmailExisteAsync(_entity.Email);
}
