//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HLP.OrganizePlanilha.UI.Web.Dao.Contexts
{
    using System;
    using System.Collections.Generic;
    
    public partial class TB_PROJETO
    {
        public TB_PROJETO()
        {
            this.TB_MAQUINA = new HashSet<TB_MAQUINA>();
            this.TB_PLANILHA = new HashSet<TB_PLANILHA>();
        }
    
        public int idPROJETO { get; set; }
        public string xPROJETO { get; set; }
        public System.DateTime dtCADASTRO { get; set; }
    
        public virtual ICollection<TB_MAQUINA> TB_MAQUINA { get; set; }
        public virtual ICollection<TB_PLANILHA> TB_PLANILHA { get; set; }
    }
}
