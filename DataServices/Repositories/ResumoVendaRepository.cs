using System;
using System.Collections.Generic;
using EntitiesServices.Model;  
using ModelServices.Interfaces.Repositories;
using System.Linq;
using EntitiesServices.Work_Classes;
using System.Data.Entity;

namespace DataServices.Repositories
{
    public class ResumoVendaRepository : RepositoryBase<RESUMO_VENDA>, IResumoVendaRepository
    {
        public List<RESUMO_VENDA> GetAllItens(Int32 idAss)
        {
            IQueryable<RESUMO_VENDA> query = Db.RESUMO_VENDA;
            query = query.Where(p => p.ASSI_CD_ID == idAss);
            return query.ToList();
        }
    }
}
