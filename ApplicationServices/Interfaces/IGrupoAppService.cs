using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ApplicationServices.Interfaces
{
    public interface IGrupoAppService : IAppServiceBase<GRUPO>
    {
        Int32 ValidateCreate(GRUPO item, USUARIO usuario);
        Int32 ValidateEdit(GRUPO item, GRUPO itemAntes, USUARIO usuario);
        Int32 ValidateEdit(GRUPO item, GRUPO itemAntes);
        Int32 ValidateDelete(GRUPO item, USUARIO usuario);
        Int32 ValidateReativar(GRUPO item, USUARIO usuario);

        GRUPO CheckExist(GRUPO obj, Int32 idAss);
        List<GRUPO> GetAllItens(Int32 idAss);
        List<GRUPO> GetAllItensAdm(Int32 idAss);
        GRUPO GetItemById(Int32 id);
    }
}
