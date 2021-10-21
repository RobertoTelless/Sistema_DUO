using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;
using EntitiesServices.Work_Classes;

namespace ModelServices.Interfaces.EntitiesServices
{
    public interface ITransportadoraService : IServiceBase<TRANSPORTADORA>
    {
        Int32 Create(TRANSPORTADORA perfil, LOG log);
        Int32 Create(TRANSPORTADORA perfil);
        Int32 Edit(TRANSPORTADORA perfil, LOG log);
        Int32 Edit(TRANSPORTADORA perfil);
        Int32 Delete(TRANSPORTADORA perfil, LOG log);
        TRANSPORTADORA CheckExist(TRANSPORTADORA conta);
        List<UF> GetAllUF();
        UF GetUFbySigla(String sigla);
        TRANSPORTADORA GetItemById(Int32 id);
        TRANSPORTADORA GetByEmail(String email);
        List<TRANSPORTADORA> GetAllItens();
        List<TRANSPORTADORA> GetAllItensAdm();
        List<FILIAL> GetAllFilial();
        TRANSPORTADORA_ANEXO GetAnexoById(Int32 id);
        List<TRANSPORTADORA> ExecuteFilter(String nome, String cnpj, String email, String cidade, String uf);
        List<TIPO_VEICULO> GetAllTipoVeiculo();
        List<TIPO_TRANSPORTE> GetAllTipoTransporte();
    }
}
