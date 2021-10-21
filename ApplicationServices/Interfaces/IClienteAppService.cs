using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ApplicationServices.Interfaces
{
    public interface IClienteAppService : IAppServiceBase<CLIENTE>
    {
        Int32 ValidateCreate(CLIENTE perfil, USUARIO usuario);
        Int32 ValidateEdit(CLIENTE perfil, CLIENTE perfilAntes, USUARIO usuario);
        Int32 ValidateEdit(CLIENTE item, CLIENTE itemAntes);
        Int32 ValidateDelete(CLIENTE perfil, USUARIO usuario);
        Int32 ValidateReativar(CLIENTE perfil, USUARIO usuario);
        List<CLIENTE> GetAllItens();
        List<CLIENTE> GetAllItensAdm();
        CLIENTE GetItemById(Int32 id);
        CLIENTE GetByEmail(String email);
        CLIENTE CheckExist(CLIENTE conta);
        List<CATEGORIA_CLIENTE> GetAllTipos();
        List<TIPO_PESSOA> GetAllTiposPessoa();
        CLIENTE_ANEXO GetAnexoById(Int32 id);
        CLIENTE_CONTATO GetContatoById(Int32 id);
        CLIENTE_REFERENCIA GetReferenciaById(Int32 id);
        Int32 ExecuteFilter(Int32? id,  Int32? catId, String razao, String nome, String cpf, String cnpj, String email, String cidade, Int32? uf, Int32? ativo, out List<CLIENTE> objeto);
        Int32 ExecuteFilterSemPedido(String nome, String cidade, Int32? uf, out List<CLIENTE> objeto);
        Int32 ValidateEditContato(CLIENTE_CONTATO item);
        Int32 ValidateCreateContato(CLIENTE_CONTATO item);
        Int32 ValidateEditReferencia(CLIENTE_REFERENCIA item);
        Int32 ValidateCreateReferencia(CLIENTE_REFERENCIA item);
        Int32 ValidateCreateTag(CLIENTE_TAG item);
        List<REGIME_TRIBUTARIO> GetAllRegimes();
        List<UF> GetAllUF();
        UF GetUFbySigla(String sigla);
    }
}
