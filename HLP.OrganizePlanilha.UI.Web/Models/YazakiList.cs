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


        public decimal GetVolumeTotal()
        {
            return this.lista.Sum(c => Convert.ToDecimal(c.CANTIDAD.Replace('.', ',')));
        }
        public decimal GetVolumeAutomaticos()
        {
            return this.lista.Where(c => c.COD_DI == "2" || c.COD_DD == "2").Sum(c => Convert.ToDecimal(c.CANTIDAD.Replace('.', ',')));
        }
        public int GetTotalTerminalEsquerdoFaltante()
        {
            return this.termEsqMax - this.lista.Where(c => c.COD_DI == "2").Select(c => c.TERM_IZQ.Trim()).Distinct().Count();
        }
        public int GetTotalTerminalDireitoFaltante()
        {
            return this.termDirMax - this.lista.Where(c => c.COD_DD == "2").Select(c => c.TERM_DER.Trim()).Distinct().Count();
        }


        private List<PlanilhaModel> lista { get { return (this.ToList() as List<PlanilhaModel>); } }





    }
}