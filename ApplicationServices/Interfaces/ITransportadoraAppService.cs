using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ApplicationServices.Interfaces
{
    public interface ITransportadoraAppService : IAppServiceBase<TRANSPORTADORA>
    {
        Int32 ValidateCreate(TRANSPORTADORA perfil, USUARIO usuario);
        Int32 ValidateEdit(TRANSPORTADORA perfil, TRANSPORTADORA perfilAntes, USUARIO usuario);
        Int32 ValidateEdit(TRANSPORTADORA item, TRANSPORTADORA itemAntes);
        Int32 ValidateDelete(TRANSPORTADORA perfil, USUARIO usuario);
        Int32 ValidateReativar(TRANSPORTADORA perfil, USUARIO usuario);
        List<UF> GetAllUF();
        UF GetUFbySigla(String sigla);
        List<TRANSPORTADORA> GetAllItens();
        List<TRANSPORTADORA> GetAllItensAdm();
        TRANSPORTADORA GetItemById(Int32 id);
        TRANSPORTADORA GetByEmail(String email);
        TRANSPORTADORA CheckExist(TRANSPORTADORA conta);
        List<FILIAL> GetAllFilial();
        TRANSPORTADORA_ANEXO GetAnexoById(Int32 id);
        Int32 ExecuteFilter(String nome, String cnpj, String email, String cidade, String uf, out List<TRANSPORTADORA> objeto);
        List<TIPO_VEICULO> GetAllTipoVeiculo();
        List<TIPO_TRANSPORTE> GetAllTipoTransporte();
    }
}
