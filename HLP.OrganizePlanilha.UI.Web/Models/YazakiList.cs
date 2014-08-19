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
            this.TerminaisComSeloEsq = new List<string>();
            this.TerminaisComSeloDir = new List<string>();
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

        public bool isCompleted { get { return this.GetVolumeTotalByLista() >= this.param.volumeTotal; } }

        public int TotalTerminalEsquerdoFaltante
        {
            get
            {
                // desconsidera os terminais com selos.
                return this.param.termEsqMax - this.lista.Where(c => !(this.TerminaisComSeloEsq.Contains(c.TERM_IZQ))
                                                            && c.COD_DI == "2"
                    ).Select(c => c.TERM_IZQ.Trim()).Distinct().Count();
            }
        }
        public int TotalTerminalDireitoFaltante
        {
            get
            {
                // desconsidera os terminais com selos.
                var resultado = this.lista.Where(c => !(this.TerminaisComSeloDir.Contains(c.TERM_DER))
                                                        && c.COD_DD == "2").Select(c => c.TERM_DER.Trim()).Distinct();
                return this.param.termDirMax - resultado.Count();
            }
        }

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

        public bool Ultrapassou(bool bIncludeYY = false)
        {
            decimal totalPermitido = this.param.volumeTotal - (bIncludeYY ? 0 : this.param.volumeYY);
            foreach (var item in this.lista.Where(c => c.COD_DI == "2" || c.COD_DD == "2"))
            {
                item.PERCENTUAL = Math.Round(((item.CANTIDAD.ToDecimal() * 100) / totalPermitido), 2);
            }

            decimal porcentagemAtual = this.lista.Sum(c => c.PERCENTUAL);
            decimal porcentagemTolerancia = Math.Round(((this.param.tolerancia * 100) / totalPermitido), 2);

            if (porcentagemAtual >= (100 + porcentagemTolerancia))
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

            public bool IsSelos { get { return this.lseloEsq.Where(c => c != "").Count() > 0 || this.lseloDir.Where(c => c != "").Count() > 0; } }

        }



        private List<string> TerminaisComSeloEsq = new List<string>();
        private List<string> TerminaisComSeloDir = new List<string>();


        public void SetListaComSelos()
        {
            this.TerminaisComSeloEsq = this.lista.Where(c => c.TERM_IZQ != "").Select(c => c.TERM_IZQ).Distinct().ToList();
            this.TerminaisComSeloDir = this.lista.Where(c => c.TERM_DER != "").Select(c => c.TERM_DER).Distinct().ToList();
        }

        /// <summary>
        /// verifica se o terminal faz combinação com algum terminal com selo.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool CombinacaoComSeloEsquerdo(PlanilhaModel item)
        {
            bool bReturn = true;
            if (item.COD_DI == "2")
                if (!this.TerminaisComSeloEsq.Contains(item.TERM_IZQ))
                    bReturn = false;

            return bReturn;
        }

        /// <summary>
        /// verifica se o terminal faz combinação com algum terminal com selo.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool CombinacaoComSeloDireito(PlanilhaModel item)
        {
            bool bReturn = true;
            if (item.COD_DD == "2")
                if (!this.TerminaisComSeloDir.Contains(item.TERM_DER))
                    bReturn = false;
            return bReturn;
        }


        public bool PodeAddCaboAnalisandoTerminais(PlanilhaModel item)
        {
            bool bLadoE = true;
            bool bLadoD = true;
            if (item.COD_DI == "2")
            {
                if (this.lista.Where(c => c.TERM_IZQ == item.TERM_IZQ).Count() == 0)
                {
                    if (this.TotalTerminalEsquerdoFaltante == 0)
                    {
                        bLadoE = false;
                    }
                }
            }

            if (item.COD_DD == "2")
            {
                if (this.lista.Where(c => c.TERM_DER == item.TERM_DER).Count() == 0)
                {
                    if (this.TotalTerminalDireitoFaltante == 0)
                    {
                        bLadoD = false;
                    }
                }
            }

            if (bLadoE && bLadoD)
                return true;
            else
                return false;
        }

    }
}