using HLP.OrganizePlanilha.UI.Web.Business;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HLP.OrganizePlanilha.UI.Web.Models
{
    public sealed class MaquinaModel
    {

        public MaquinaModel()
        {
            BusinessMaquina = new MaquinaBO(maquina_: this);
        }

        public MaquinaBO BusinessMaquina { get; set; }

        public int idMAQUINA { get; set; }
        public int idPROJETO { get; set; }

        [Required(ErrorMessage = "Campo é obrigatório.")]
        [Display(Name = "Maquina")]
        public string xMAQUINA { get; set; }


        public string _SELOS_ESQUERDO = "";
        //[Required(ErrorMessage = "Campo é obrigatório.")]
        [Display(Name = "Selo esquerdo")]
        public string SELOS_ESQUERDO
        {
            get { return _SELOS_ESQUERDO; }
            set { _SELOS_ESQUERDO = value; }
        }


        public string _SELOS_DIREITO = "";
        //[Required(ErrorMessage = "Campo é obrigatório.")]
        [Display(Name = "Selo direito")]
        public string SELOS_DIREITO
        {
            get { return _SELOS_DIREITO; }
            set { _SELOS_DIREITO = value; }
        }

        [Required(ErrorMessage = "Campo é obrigatório.")]
        [Display(Name = "Terminal esquerdo")]
        public string QTDE_TERM_ESQUERDO { get; set; }

        [Required(ErrorMessage = "Campo é obrigatório.")]
        [Display(Name = "Terminal direito")]
        public string QTDE_TERM_DIREITO { get; set; }

        [Required(ErrorMessage = "Campo é obrigatório.")]
        [Display(Name = "Y-Y")]
        public string YY { get; set; }

        [Required(ErrorMessage = "Campo é obrigatório.")]
        [Display(Name = "Bitola")]
        public string CALIBRE { get; set; }

        [Required(ErrorMessage = "Campo é obrigatório.")]
        [Display(Name = "Volume")]
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        public string QTDE_CAPACIDADE { get; set; }

        [Required(ErrorMessage = "Campo é obrigatório.")]
        [Display(Name = "Tolerância")]
        public string QTDE_TOLERANCIA { get; set; }       

    }
}