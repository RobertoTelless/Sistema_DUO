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
    
    public partial class TIPO_FAVORECIDO
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TIPO_FAVORECIDO()
        {
            this.CONTA_PAGAR = new HashSet<CONTA_PAGAR>();
            this.CONTA_RECEBER = new HashSet<CONTA_RECEBER>();
        }
    
        public int TIFA_CD_ID { get; set; }
        public string TIFA_NM_NOME { get; set; }
        public Nullable<int> TIFA_IN_ATIVO { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CONTA_PAGAR> CONTA_PAGAR { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CONTA_RECEBER> CONTA_RECEBER { get; set; }
    }
}
