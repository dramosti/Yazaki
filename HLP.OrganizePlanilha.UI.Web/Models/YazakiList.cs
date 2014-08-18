using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace HLP.OrganizePlanilha.UI.Web.Models
{
    public class YazakiList : Collection<PlanilhaModel>
    {
        public ParametrosLista param { get; set; }

        public YazakiList()
        {
            param = new ParametrosLista();
        }

        protected override void InsertItem(int index, PlanilhaModel item)
        {
            item.MAQUINA = this.param.MAQUINA;
            base.InsertItem(index, item);
        }

        public ParametrosLista GetParametro()
        {
            return this.param;
        }

        public decimal GetVolumeTotalByLista()
        {
            return this.lista.Sum(c => Convert.ToDecimal(c.CANTIDAD.Replace('.', ',')));
        }
        public decimal GetVolumeTotalManualByLista()
        {
            return this.lista.Where(c => c.COD_DD == "Y" && c.COD_DI == "Y").Sum(c => Convert.ToDecimal(c.CANTIDAD.Replace('.', ',')));
        }
        public decimal GetVolumeAutomaticosByLista()
        {
            return this.lista.Where(c => c.COD_DI == "2" || c.COD_DD == "2").Sum(c => Convert.ToDecimal(c.CANTIDAD.Replace('.', ',')));
        }
        private int GetVolumeTotalTerminalEsquerdoFaltanteByLista()
        {
            return this.param.termEsqMax - this.lista.Where(c => c.COD_DI == "2").Select(c => c.TERM_IZQ.Trim()).Distinct().Count();
        }
        private int GetVolumeTerminalDireitoFaltanteByLista()
        {
            return this.param.termDirMax - this.lista.Where(c => c.COD_DD == "2").Select(c => c.TERM_DER.Trim()).Distinct().Count();
        }



        public bool isCompleted { get { return this.GetVolumeTotalByLista() >= this.param.volumeTotal; } }

        public int TotalTerminalEsquerdoFaltante
        {
            get
            {
                int iValor = this.GetVolumeTotalTerminalEsquerdoFaltanteByLista() - this.param.lseloEsq.Count();
                return iValor <= 0 ? 0 : iValor;
            }
        }
        public int TotalTerminalDireitoFaltante { get { return this.GetVolumeTerminalDireitoFaltanteByLista(); } }

        private List<PlanilhaModel> lista { get { return (this.ToList() as List<PlanilhaModel>); } }

        public bool ValidaPorcentagemGeral(bool bIncludeYY = false)
        {
            decimal totalPermitido = this.param.volumeTotal - (bIncludeYY ? 0 : this.param.volumeYY);
            foreach (var item in this.lista.Where(c => c.COD_DI == "2" || c.COD_DD == "2"))
            {
                item.PERCENTUAL = Math.Round(((item.CANTIDAD.ToDecimal() * 100) / totalPermitido), 2);
            }

            decimal porcentagemAtual = this.lista.Sum(c => c.PERCENTUAL);
            decimal porcentagemTolerancia = Math.Round(((this.param.tolerancia * 100) / totalPermitido), 2);

            if (porcentagemAtual >= (100 - porcentagemTolerancia) && porcentagemAtual <= (100 + porcentagemTolerancia))
                return true;
            else return false;
        }


        public class ParametrosLista
        {
            public string MAQUINA { get; set; }
            public decimal bitolaMin { get; set; }
            public decimal bitolaMax { get; set; }
            public int termEsqMin { get; set; }
            public int termEsqMax { get; set; }
            public int termDirMin { get; set; }
            public int termDirMax { get; set; }
            public List<string> lseloEsq { get; set; }
            public List<string> lseloDir { get; set; }
            public decimal volumeTotal { get; set; }
            public decimal volumeYY { get; set; }
            public decimal tolerancia { get; set; }
            public List<string> ltermEsq = new List<string>();
            public List<string> ltermDir = new List<string>();

            public bool IsSelos { get { return this.lseloEsq.Count() > 0 || this.lseloDir.Count() > 0; } }

        }


    }
}