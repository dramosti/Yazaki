using HLP.OrganizePlanilha.UI.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HLP.OrganizePlanilha.UI.Web.Models
{
    public sealed class ProjetoModel
    {

        public int idProjeto { get; set; }

        [Display(Name = "Nome do Projeto")]
        [Required(ErrorMessage = "Nome para o Projeto é obrigatório")]
        public string xPROJETO { get; set; }
        [Display(Name = "Nome da aba")]
        [Required(ErrorMessage = "Nome da aba é obrigatório.")]
        public string xNomeAba { get; set; }
        public DateTime dtCADASTRO { get; set; }
        public decimal qTotal { get; set; }

        public ProjetoModel()
        {
            this.ldadosMaquina = new List<MaquinaModel>();
            this.ldadosPlanilhaFinal = new List<PlanilhaModel>();
            this.ldadosPlanilhaOriginal = new List<PlanilhaModel>();
            painel = new PainelModel();
        }
                
        public List<ParametrosModel> ldadosParametroTempDistinct { get; set; }
        public List<MaquinaModel> ldadosMaquina { get; set; }
        public List<PlanilhaModel> ldadosPlanilhaOriginal { get; set; }
        public List<PlanilhaModel> ldadosPlanilhaFinal { get; set; }
        public PainelModel painel { get; set; }
        public List<BitolaModel> bitolas { get; set; }

        public string fileLocationCompleted { get; set; }
      

     



    }
}