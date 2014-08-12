using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HLP.OrganizePlanilha.UI.Web.Models
{
    public sealed class ParametrosModel
    {
        public int id { get; set; }

        private string _COD_D;
        /// <summary>
        /// 2-AUTOMÁTICO - Y - MANUAL
        /// </summary>
        //public string COD_D
        //{
        //    get { return _COD_D.ToUpper() == "Y" ? "Manual" : "Automático"; }
        //    set { _COD_D = value == "Manual" ? "Y" : "2"; }
        //}
        public string COD_D
        {
            get { return _COD_D.ToUpper() != "Y" ? "2" : "Y"; }
            set { _COD_D = value; }
        }

        /// <summary>
        /// TERMINAL LADO ESQUERDO
        /// </summary>
        public string TERM { get; set; }

        /// <summary>
        /// SELO LADO ESQUERDO
        /// </summary>
        /// 
        public string ACC_01 { get; set; }

        /// <summary>
        /// Parametro que irá diferenciará os terminais repetidos.
        /// </summary>
        public string COD_01 { get; set; }


    }
}