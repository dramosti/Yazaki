using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace HLP.OrganizePlanilha.UI.Web.Models
{
    public class YazakiList<T> : Collection<T>
    {

        public decimal bitolaMin { get; set; }
        public decimal bitolaMax { get; set; }
        public int termEsqMin { get; set; }
        public int termEsqMax { get; set; }
        public int termDirMin { get; set; }
        public int termDirMax { get; set; }
        public List<string> lseloEsq { get; set; }
        public List<string> lseloDir { get; set; }
        public decimal volumeAtual { get; set; }
        public decimal volumeTotal { get; set; }
        public decimal volumeYY { get; set; }
        public decimal toleranciaMin { get; set; }
        public decimal toleranciaMax { get; set; }



        public List<string> ltermEsq = new List<string>();
        public List<string> ltermDir = new List<string>();

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            this.volumeAtual += Convert.ToDecimal((item as PlanilhaModel).CANTIDAD.Replace(".", ","));
        }

    }
}