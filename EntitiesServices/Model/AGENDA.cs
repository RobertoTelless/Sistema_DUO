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
    
    public partial class AGENDA
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public AGENDA()
        {
            this.AGENDA_ANEXO = new HashSet<AGENDA_ANEXO>();
            this.AGENDA_VINCULO = new HashSet<AGENDA_VINCULO>();
        }
    
        public int AGEN_CD_ID { get; set; }
        public int USUA_CD_ID { get; set; }
        public int ASSI_CD_ID { get; set; }
        public Nullable<int> CAAG_CD_ID { get; set; }
        public System.DateTime AGEN_DT_DATA { get; set; }
        public System.TimeSpan AGEN_HR_HORA { get; set; }
        public string AGEN_NM_TITULO { get; set; }
        public string AGEN_DS_DESCRICAO { get; set; }
        public int AGEN_IN_ATIVO { get; set; }
        public Nullable<int> AGEN_CD_USUARIO { get; set; }
        public string AGEN_TX_OBSERVACOES { get; set; }
        public Nullable<int> AGEN_IN_STATUS { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AGENDA_ANEXO> AGENDA_ANEXO { get; set; }
        public virtual ASSINANTE ASSINANTE { get; set; }
        public virtual CATEGORIA_AGENDA CATEGORIA_AGENDA { get; set; }
        public virtual USUARIO USUARIO { get; set; }
        public virtual USUARIO USUARIO1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AGENDA_VINCULO> AGENDA_VINCULO { get; set; }
    }
}
