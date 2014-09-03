using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HLP.OrganizePlanilha.UI.Web.Models
{
    public sealed class PlanilhaModel : ICloneable
    {
        public PlanilhaModel()
        {

        }

        [Coluna(isColuna = false)]
        public int idPLANILHA { get; set; }

        [Coluna(isColuna = false)]
        public int idProjeto { get; set; }

        [Coluna(isColuna = true)]
        public string PLANTA { get; set; }

        [Coluna(isColuna = true)]
        public string MAQUINA { get; set; }

        [Coluna(isColuna = true)]
        public string TIPO { get; set; }

        [Coluna(isColuna = true)]
        public string CALIBRE { get; set; }

        [Coluna(isColuna = false)]
        public string LONG_CORT { get; set; }

        private string _CANTIDAD;
        [Coluna(isColuna = true)]
        public string CANTIDAD
        {
            get { return (_CANTIDAD.ToDecimal() - this.dRestante).ToString(); }
            set { _CANTIDAD = value; }
        }

        private string _COD_DI;
        [Coluna(isColuna = true)]

        public string COD_DI
        {
            get { return _COD_DI; }
            set
            {
                _COD_DI = value;

            }
        }

        private string _TERM_IZQ;
        [Coluna(isColuna = true)]
        public string TERM_IZQ
        {
            get { return Util.bAtivaRegraModel ? (this.COD_DI != "Y" ? _TERM_IZQ : "") : _TERM_IZQ; }
            set { _TERM_IZQ = value; }
        }


        private string _ACC_01_I;
        /// <summary>
        /// SELO ESQUERDO.
        /// </summary>
        [Coluna(isColuna = true)]
        public string ACC_01_I
        {
            get { return Util.bAtivaRegraModel ? (this.COD_DI != "Y" ? _ACC_01_I : "") : _ACC_01_I; }
            set
            {
                _ACC_01_I = value;
                //if (value != "")
                //    this.COD_DI = "2";
            }
        }


        private string _COD_DD;
        [Coluna(isColuna = true)]
        public string COD_DD
        {
            get { return _COD_DD; }
            set
            {
                _COD_DD = value;

            }
        }


        private string _TERM_DER;
        [Coluna(isColuna = true)]
        public string TERM_DER
        {
            get { return Util.bAtivaRegraModel ? (this.COD_DD != "Y" ? _TERM_DER : "") : _TERM_DER; }
            set
            {
                _TERM_DER = value;
            }
        }


        private string _ACC_01_D;
        /// <summary>
        /// SELO DIREITO
        /// </summary>
        [Coluna(isColuna = true)]
        public string ACC_01_D
        {
            get { return Util.bAtivaRegraModel ? (this.COD_DD != "Y" ? _ACC_01_D : "") : _ACC_01_D; }
            set
            {
                _ACC_01_D = value;
                //if (value != "")
                //    this.COD_DD = "2";
            }
        }


        private string _COD_01_I;
        [Coluna(isColuna = true)]
        public string COD_01_I
        {
            get { return _COD_01_I; }
            set { _COD_01_I = (value == "D" || value == "U" || value == "PU") ? value : ""; }
        }

        private string _COD_01_D;
        [Coluna(isColuna = true)]
        public string COD_01_D
        {
            get { return _COD_01_D; }
            set { _COD_01_D = (value == "D" || value == "U" || value == "PU") ? value : ""; }
        }

        [Coluna(isColuna = false)]
        public bool bUtilizado { get; set; }

        [Coluna(isColuna = false)]
        public string G { get; set; }

        private int? _id = null;
        [Coluna(isColuna = false)]
        public int? id
        {
            get { return _id; }
            set { _id = value; }
        }

        [Coluna(isColuna = false)]
        public decimal dRestante { get; set; }

        [Coluna(isColuna = false)]
        public decimal PERCENTUAL { get; set; }

        public void SubtraiPercentual(decimal dPercentual)
        {
            decimal valorRelativo = Math.Round(((this.CANTIDAD.ToDecimal() * dPercentual) / this.PERCENTUAL), 2);
            this.dRestante += valorRelativo;
        }

        public void SubtraiValor(decimal dValor)
        {
            this.dRestante += dValor;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}