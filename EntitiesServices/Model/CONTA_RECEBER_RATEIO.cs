//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EntitiesServices.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class CONTA_RECEBER_RATEIO
    {
        public int CRRA_CD_ID { get; set; }
        public int CARE_CD_ID { get; set; }
        public int CECU_CD_ID { get; set; }
        public Nullable<int> CRRA_NR_PERCENTUAL { get; set; }
        public Nullable<int> CRRA_IN_ATIVO { get; set; }
    
        public virtual CENTRO_CUSTO CENTRO_CUSTO { get; set; }
        public virtual CONTA_RECEBER CONTA_RECEBER { get; set; }
    }
}
