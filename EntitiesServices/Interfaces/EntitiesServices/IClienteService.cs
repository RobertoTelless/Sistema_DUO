using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;
using EntitiesServices.Work_Classes;

namespace ModelServices.Interfaces.EntitiesServices
{
    public interface IClienteService : IServiceBase<CLIENTE>
    {
        Int32 Create(CLIENTE perfil, LOG log);
        Int32 Create(CLIENTE perfil);
        Int32 Edit(CLIENTE perfil, LOG log);
        Int32 Edit(CLIENTE perfil);
        Int32 Delete(CLIENTE perfil, LOG log);

        CLIENTE CheckExist(CLIENTE conta);
        CLIENTE GetItemById(Int32 id);
        CLIENTE GetByEmail(String email);
        List<CLIENTE> GetAllItens();
        List<CLIENTE> GetAllItensAdm();
        List<CATEGORIA_CLIENTE> GetAllTipos();
        List<TIPO_PESSOA> GetAllTiposPessoa();
        //List<SEXO> GetAllSexos();
        List<REGIME_TRIBUTARIO> GetAllRegimes();
        List<TIPO_CONTRIBUINTE> GetAllContribuinte();
        List<UF> GetAllUF();
        UF GetUFbySigla(String sigla);

        CLIENTE_ANEXO GetAnexoById(Int32 id);
        CLIENTE_CONTATO GetContatoById(Int32 id);
        CLIENTE_REFERENCIA GetReferenciaById(Int32 id);
        List<CLIENTE> ExecuteFilter(Int32? id, Int32? catId, String razao, String nome, String cpf, String cnpj, String email, String cidade, Int32? uf, Int32? ativo);
        List<CLIENTE> ExecuteFilterSemPedido(String nome, String cidade, Int32? uf);
        Int32 EditContato(CLIENTE_CONTATO item);
        Int32 CreateContato(CLIENTE_CONTATO item);
        Int32 EditReferencia(CLIENTE_REFERENCIA item);
        Int32 CreateReferencia(CLIENTE_REFERENCIA item);
        Int32 CreateTag(CLIENTE_TAG item);
        //List<CONTA_RECEBER> GetItensAtrasoCliente();
    }
}
