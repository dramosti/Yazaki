﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HLP.OrganizePlanilha.UI.Web.Models
{
    public sealed class PlanilhaModel : IComparable
    {
        public PlanilhaModel()
        {
            maquina = new MaquinaModel();
        }

        [Coluna(isColuna = false)]
        public int idPLANILHA { get; set; }

        [Coluna(isColuna = false)]
        public int idPROJETO { get; set; }

        [Coluna(isColuna = true)]
        public string PLANTA { get; set; }

        [Coluna(isColuna = false)]
        public MaquinaModel maquina { get; set; }

        [Coluna(isColuna = true)]
        public string TIPO { get; set; }

        [Coluna(isColuna = true)]
        public string CALIBRE { get; set; }

        [Coluna(isColuna = false)]
        public string LONG_CORT { get; set; }

        [Coluna(isColuna = true)]
        public string CANTIDAD { get; set; }


        [Coluna(isColuna = true)]
        public string COD_DI { get; set; }


        public string _TERM_IZQ;
        [Coluna(isColuna = true)]
        public string TERM_IZQ
        {
            get { return Util.bAtivaRegraModel ? (this.COD_DI != "Y" ? _TERM_IZQ : "") : _TERM_IZQ; }
            set { _TERM_IZQ = value; }
        }


        private string _ACC_01_I;
        [Coluna(isColuna = true)]
        public string ACC_01_I
        {
            get { return Util.bAtivaRegraModel ? (this.COD_DI != "Y" ? _ACC_01_I : "") : _ACC_01_I; }
            set { _ACC_01_I = value; }
        }



        [Coluna(isColuna = true)]
        public string COD_DD { get; set; }


        public string _TERM_DER;
        [Coluna(isColuna = true)]
        public string TERM_DER
        {
            get { return Util.bAtivaRegraModel ? (this.COD_DD != "Y" ? _TERM_DER : "") : _TERM_DER; }
            set { _TERM_DER = value; }
        }


        private string _ACC_01_D;
        [Coluna(isColuna = true)]
        public string ACC_01_D
        {
            get { return Util.bAtivaRegraModel ? (this.COD_DD != "Y" ? _ACC_01_D : "") : _ACC_01_D; }
            set { _ACC_01_D = value; }
        }


        public string _COD_01_I;

        [Coluna(isColuna = false)]
        public string COD_01_I
        {
            get { return _COD_01_I; }
            set { _COD_01_I = (value == "D" || value == "U" || value == "PU") ? value : ""; }
        }

        public string _COD_01_D;
        [Coluna(isColuna = false)]
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

        public int CompareTo(object obj)
        {
            return this.CALIBRE.CompareTo(obj);
        }

        //public bool bAtivaRegra = false;
    }
}