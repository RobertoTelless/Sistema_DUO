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
    
    public partial class CENTRO_CUSTO
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CENTRO_CUSTO()
        {
            this.CONTA_PAGAR = new HashSet<CONTA_PAGAR>();
            this.CONTA_PAGAR_RATEIO = new HashSet<CONTA_PAGAR_RATEIO>();
            this.CONTA_RECEBER = new HashSet<CONTA_RECEBER>();
            this.CONTA_RECEBER_RATEIO = new HashSet<CONTA_RECEBER_RATEIO>();
            this.PEDIDO_COMPRA = new HashSet<PEDIDO_COMPRA>();
        }
    
        public int CECU_CD_ID { get; set; }
        public int ASSI_CD_ID { get; set; }
        public string CECU_NR_NUMERO { get; set; }
        public string CECU_NM_NOME { get; set; }
        public int CECU_IN_ATIVO { get; set; }
        public Nullable<int> GRUP_CD_ID { get; set; }
        public Nullable<int> SUBG_CD_ID { get; set; }
        public Nullable<int> CECU_IN_TIPO { get; set; }
        public Nullable<int> CECU_IN_MOVTO { get; set; }
        public string CECU_NM_EXIBE { get; set; }
    
        public virtual ASSINANTE ASSINANTE { get; set; }
        public virtual GRUPO GRUPO { get; set; }
        public virtual SUBGRUPO SUBGRUPO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CONTA_PAGAR> CONTA_PAGAR { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CONTA_PAGAR_RATEIO> CONTA_PAGAR_RATEIO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CONTA_RECEBER> CONTA_RECEBER { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CONTA_RECEBER_RATEIO> CONTA_RECEBER_RATEIO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PEDIDO_COMPRA> PEDIDO_COMPRA { get; set; }
    }
}
