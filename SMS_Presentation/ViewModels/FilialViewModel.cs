using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EntitiesServices.Attributes;
using EntitiesServices.Model;

namespace SMS_Solution.ViewModels
{
    public class FilialViewModel
    {
        [Key]
        public int FILI_CD_ID { get; set; }
        [Required(ErrorMessage = "Campo NOME obrigatorio")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "O NOME deve conter no minimo 1 caracteres e no m�ximo 50.")]
        public string FILI_NM_NOME { get; set; }
        [StringLength(100, ErrorMessage = "A RAZ�O SOCIAL deve conter no m�ximo 100.")]
        public string FILI_NM_RAZAO { get; set; }
        [StringLength(20, MinimumLength = 14, ErrorMessage = "O CNPJ deve conter no minimo 14 caracteres e no m�ximo 20.")]
        [CustomValidationCNPJ(ErrorMessage = "CNPJ inv�lido")]
        public string FILI_NR_CNPJ { get; set; }
        [StringLength(100, MinimumLength = 1, ErrorMessage = "O E-MAIL deve conter no minimo 1 caracteres e no m�ximo 100.")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Deve ser um e-mail v�lido")]
        public string FILI_NM_EMAIL { get; set; }
        [StringLength(50, ErrorMessage = "O CONTATO deve conter no m�ximo 50.")]
        public string FILI_NM_CONTATOS { get; set; }
        [StringLength(50, ErrorMessage = "O TELEFONE deve conter no m�ximo 50.")]
        public string FILI_NM_TELEFONES { get; set; }
        [StringLength(50, ErrorMessage = "O ENDERE�O deve conter no m�ximo 50.")]
        public string FILI_NM_ENDERECO { get; set; }
        [StringLength(50, ErrorMessage = "O BAIRRO deve conter no m�ximo 50.")]
        public string FILI_NM_BAIRRO { get; set; }
        [StringLength(50, ErrorMessage = "A CIDADE deve conter no m�ximo 50.")]
        public string FILI_NM_CIDADE { get; set; }
        public Nullable<int> UF_CD_ID { get; set; }
        [StringLength(10, ErrorMessage = "O CEP deve conter no m�ximo 10.")]
        public string FILI_NR_CEP { get; set; }
        public System.DateTime FILI_DT_CADASTRO { get; set; }
        public int FILI_IN_ATIVO { get; set; }
        public Nullable<int> TIPE_CD_ID { get; set; }
        public Nullable<int> CRTR_CD_ID { get; set; }
        [StringLength(50, ErrorMessage = "A INSCRI��O ESTADUAL deve conter no m�ximo 50.")]
        public string FILI_NR_INSCRICAO_ESTADUAL { get; set; }
        [StringLength(50, ErrorMessage = "A INSCRI��O MUNICIAPAL deve conter no m�ximo 50.")]
        public string FILI_NR_INSCRICAO_MUNICIPAL { get; set; }
        public Nullable<int> FILI_IN_IE_ISENTO { get; set; }
        [StringLength(50, ErrorMessage = "O CNAE deve conter no m�ximo 50.")]
        public string FILI_NR_CNAE { get; set; }
        [StringLength(100, ErrorMessage = "O WEBSITE deve conter no m�ximo 100.")]
        public string FILI_NM_WEBSITE { get; set; }
        [StringLength(50, ErrorMessage = "O CELURAR deve conter no m�ximo 50.")]
        public string FILI_NR_CELULAR { get; set; }
        [StringLength(250, ErrorMessage = "O LOGOTIPO deve conter no m�ximo 250.")]
        public string FILI_AQ_LOGOTIPO { get; set; }

        public virtual ASSINANTE ASSINANTE { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CLIENTE> CLIENTE { get; set; }
        public virtual UF UF { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FORNECEDOR> FORNECEDOR { get; set; }

    }
}