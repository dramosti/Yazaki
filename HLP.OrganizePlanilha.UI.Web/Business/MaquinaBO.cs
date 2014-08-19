using HLP.OrganizePlanilha.UI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HLP.OrganizePlanilha.UI.Web.Business
{
    public sealed class MaquinaBO
    {

        /// <summary>
        /// Lista que será o resultado final da análise e será convertida em planilha
        /// </summary>
        public YazakiList resultado { get; set; }
        public List<PlanilhaModel> lSelos { get; set; }
        public MaquinaModel _maquina { get; set; }
        public string fileLocation { get; set; }

        #region Propriedades privadas
        /// <summary>
        /// Lista de cabos que serão manipulados.
        /// </summary>
        private List<PlanilhaModel> _lDadosPlanilha { get; set; }
        private List<PlanilhaModel> _lDadosParaAssignacao { get; set; }

        /// <summary>
        /// Contador q será convertido em Letras para separar os Grupos
        /// </summary>
        private int iContador { get; set; }

        private PlanilhaModel objUltimoRegistro = null;
        #endregion

        public MaquinaBO(MaquinaModel maquina_)
        {
            this._maquina = maquina_;
            this.resultado = new YazakiList();
        }


        #region Metodos

        /// <summary>
        /// Método responsável por inserir os parametros necessários para a organização dos dados.
        /// </summary>
        private void SetParametros()
        {
            try
            {
                this.resultado.param.MAQUINA = this._maquina.xMAQUINA;
                this.resultado.param.bitolaMin = Convert.ToDecimal(this._maquina.CALIBRE.Split('-')[0].ToString().Replace(".", ","));
                if (this._maquina.CALIBRE.Split('-').Count() > 1)
                    this.resultado.param.bitolaMax = Convert.ToDecimal(this._maquina.CALIBRE.Split('-')[1].ToString().Replace(".", ","));
                else
                    this.resultado.param.bitolaMax = this.resultado.param.bitolaMin;

                if (this._maquina.QTDE_TERM_ESQUERDO != null)
                {
                    this.resultado.param.termEsqMin = Convert.ToInt32(this._maquina.QTDE_TERM_ESQUERDO.Split('-')[0].ToString());
                    if (this._maquina.QTDE_TERM_ESQUERDO.Split('-').Count() > 1)
                        this.resultado.param.termEsqMax = Convert.ToInt32(this._maquina.QTDE_TERM_ESQUERDO.Split('-')[1].ToString());
                    else
                        this.resultado.param.termEsqMax = this.resultado.param.termEsqMin;
                }

                if (this._maquina.QTDE_TERM_DIREITO != null)
                {
                    this.resultado.param.termDirMin = Convert.ToInt32(this._maquina.QTDE_TERM_DIREITO.Split('-')[0].ToString());
                    if (this._maquina.QTDE_TERM_ESQUERDO.Split('-').Count() > 1)
                        this.resultado.param.termDirMax = Convert.ToInt32(this._maquina.QTDE_TERM_ESQUERDO.Split('-')[1].ToString());
                    else
                        this.resultado.param.termDirMax = this.resultado.param.termDirMin;
                }

                if (this._maquina.SELOS_ESQUERDO != null)
                {
                    this.resultado.param.lseloEsq = this._maquina.SELOS_ESQUERDO.Trim().Split(',').ToList();
                    this.resultado.param.lseloEsq.Add("");
                }
                else
                {
                    this.resultado.param.lseloEsq = new List<string>();
                    this.resultado.param.lseloEsq.Add("");
                }

                if (this._maquina.SELOS_DIREITO != null)
                {
                    this.resultado.param.lseloDir = this._maquina.SELOS_DIREITO.Trim().Split(',').ToList();
                    this.resultado.param.lseloDir.Add("");
                }
                else
                {
                    this.resultado.param.lseloDir = new List<string>();
                    this.resultado.param.lseloDir.Add("");
                }

                if (this._maquina.QTDE_CAPACIDADE != null)
                    this.resultado.param.volumeTotal = Convert.ToDecimal(this._maquina.QTDE_CAPACIDADE.Replace(".", ""));
                if (this._maquina.YY != null)
                    this.resultado.param.volumeYY = Convert.ToDecimal(this._maquina.YY.Replace(".", ""));

                if (this._maquina.QTDE_TOLERANCIA != null)
                {
                    //this.resultado.param.toleranciaMin = this.resultado.param.volumeTotal - Convert.ToDecimal(this._maquina.QTDE_TOLERANCIA.Split('-')[0].ToString().Replace(".", ","));
                    //if (this._maquina.QTDE_TOLERANCIA.Split('-').Count() > 1)
                    //    this.resultado.param.toleranciaMax = this.resultado.param.volumeTotal + Convert.ToDecimal(this._maquina.QTDE_TOLERANCIA.Split('-')[1].ToString().Replace(".", ","));
                    //else
                    //    this.resultado.param.toleranciaMax = this.resultado.param.volumeTotal;

                    this.resultado.param.tolerancia = this._maquina.QTDE_TOLERANCIA.ToDecimal();
                }

                this.resultado.param.termDirMax += this.resultado.param.lseloDir.Count();
                this.resultado.param.termDirMin += this.resultado.param.lseloDir.Count();

                this.resultado.param.termEsqMax += this.resultado.param.lseloEsq.Count();
                this.resultado.param.termEsqMin += this.resultado.param.lseloEsq.Count();


            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void IncludeSelos()
        {
            try
            {
                this.lSelos = new List<PlanilhaModel>();



                // Verifica se existe algum selo no outro lado para mudar de lado.
                foreach (var seloe in this.resultado.param.lseloEsq)
                {
                    if (!this.resultado.param.lseloDir.Contains(seloe))
                        foreach (var item in this._lDadosPlanilha.Where(c => seloe == c.ACC_01_D))
                        {
                            Util.InverteLado(item);
                        }
                }
                // Verifica se existe algum selo no outro lado para mudar de lado.
                foreach (var selod in this.resultado.param.lseloDir)
                {
                    if (!this.resultado.param.lseloEsq.Contains(selod))
                        foreach (var item in this._lDadosPlanilha.Where(c => selod == c.ACC_01_I))
                        {
                            Util.InverteLado(item);
                        }
                }

                // verifica se existe o selo e do outro lado tenha tb selo constando na lista de parametros
                foreach (var selo in this.resultado.param.lseloEsq)
                {
                    if (selo != "")
                    {
                        foreach (var itemSelo in this._lDadosPlanilha.Where(c =>
                            c.ACC_01_I == selo
                            && this.resultado.param.lseloDir.Contains(c.ACC_01_D)))
                        {
                            itemSelo.bUtilizado = true;
                            this.lSelos.Add(itemSelo);
                        }
                    }
                }

                // verifica se existe o selo e do outro lado tenha tb selo constando na lista de parametros
                foreach (var selo in this.resultado.param.lseloDir)
                {
                    if (selo != "")
                    {
                        var itens = this._lDadosPlanilha.Where(c => c.ACC_01_D == selo);

                        itens = this._lDadosPlanilha.Where(c => c.ACC_01_D != "");

                        foreach (var itemSelo in this._lDadosPlanilha.Where(c =>
                            c.ACC_01_D == selo
                            && this.resultado.param.lseloEsq.Contains(c.ACC_01_I)))
                        {
                            itemSelo.bUtilizado = true;
                            this.lSelos.Add(itemSelo);
                        }
                    }
                }

                foreach (var item in this.lSelos)
                {
                    this.resultado.Add(item);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void IncludeAutomaticosBySelos()
        {
            try
            {
                int icountAtual = this.resultado.Count();
                var dadosPesquisa = new List<PlanilhaModel>();
                foreach (var itemSelo in this.lSelos)
                {

                    dadosPesquisa = new List<PlanilhaModel>();
                    //if (this.resultado.TotalTerminalDireitoFaltante > 0 && this.resultado.TotalTerminalEsquerdoFaltante > 0)
                    {
                        // verifico primeiro o lado esquerdo.
                        if (itemSelo.COD_DI == "2")
                        {

                            dadosPesquisa = (from c in this._lDadosPlanilha
                                             where c.COD_DI == "2"
                                             && c.COD_DD == "2"
                                             && c.ACC_01_D == ""
                                             && c.ACC_01_I == ""
                                             && c.TERM_IZQ == itemSelo.TERM_IZQ
                                             && c.bUtilizado == false
                                             select c).ToList();
                        }
                        // se não encontrou nada, eu verifico na direita.                        
                        if (dadosPesquisa.Count() == 0 && itemSelo.COD_DD == "2")
                        {
                            dadosPesquisa = (from c in this._lDadosPlanilha
                                             where c.COD_DI == "2"
                                             && c.COD_DD == "2"
                                             && c.ACC_01_D == ""
                                             && c.ACC_01_I == ""
                                             && c.TERM_DER == itemSelo.TERM_DER
                                             && c.bUtilizado == false
                                             select c).ToList();
                        }
                    }


                    if (dadosPesquisa.Count() == 0)
                    {
                        //VERIFICO MANUAL E AUTOMÁTICO. PRIMEIRO LADO ESQUERDO
                     //   if (this.resultado.TotalTerminalEsquerdoFaltante > 0 && itemSelo.COD_DI == "2")
                        {
                            dadosPesquisa = (from c in this._lDadosPlanilha
                                             where c.COD_DI == "2"
                                             && c.ACC_01_I == ""
                                             && c.COD_DD == "Y"
                                             && c.TERM_IZQ == itemSelo.TERM_IZQ
                                             && c.bUtilizado == false
                                             select c).ToList();
                        }
                    }


                    if (dadosPesquisa.Count() == 0)
                    {
                        //VERIFICO MANUAL E AUTOMÁTICO. LADO DIREITO.
                        if (this.resultado.TotalTerminalDireitoFaltante > 0 && itemSelo.COD_DD == "2")
                        {
                            dadosPesquisa = (from c in this._lDadosPlanilha
                                             where c.COD_DD == "2"
                                             && c.ACC_01_D == ""
                                             && c.COD_DI == "Y"
                                             && c.TERM_DER == itemSelo.TERM_DER
                                             && c.bUtilizado == false
                                             select c).ToList();
                        }
                    }

                    if (dadosPesquisa.Count() > 0)
                    {

                        var itemSelecionado = dadosPesquisa.FirstOrDefault();
                        itemSelecionado.bUtilizado = true;
                        this.resultado.Add(itemSelecionado);
                    }
                }

                if (this.resultado.Count() > icountAtual)
                    if (this.resultado.TotalTerminalDireitoFaltante > 0 || this.resultado.TotalTerminalEsquerdoFaltante > 0)
                        this.IncludeAutomaticosBySelos();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }



        private void IncludeAutomaticosLado_A_B(string sTerm = "")
        {
            try
            {
                if (this.resultado.TotalTerminalEsquerdoFaltante > 0 && this.resultado.TotalTerminalDireitoFaltante > 0)
                {
                    if (sTerm == "")
                    {
                        // verifico no lado esquerdo os que ja constam na maquina com selos.
                        var TotalPorTerminal = (from c in this._lDadosPlanilha
                                                where
                                                c.ACC_01_D != ""
                                                && c.ACC_01_I != ""
                                                && this.resultado.Select(o => c.TERM_IZQ).Contains(c.TERM_IZQ)
                                                && c.bUtilizado == false
                                                group c by new { c.TERM_IZQ, c.CALIBRE } into grupoPlanilha
                                                select new
                                                {
                                                    TERMINAL = grupoPlanilha.Key.TERM_IZQ,
                                                    CALIBRE = grupoPlanilha.Key.CALIBRE,
                                                    TOTAL = grupoPlanilha.Count()
                                                }).OrderByDescending(c => c.TOTAL);

                        if (TotalPorTerminal.Count() == 0)
                        {
                            // verifico na lista inteira.
                            TotalPorTerminal = (from c in this._lDadosPlanilha
                                                where
                                                c.ACC_01_D != ""
                                                && c.ACC_01_I != ""
                                                && c.COD_DI == "2"
                                                && c.bUtilizado == false
                                                group c by new { c.TERM_IZQ, c.CALIBRE } into grupoPlanilha
                                                select new
                                                {
                                                    TERMINAL = grupoPlanilha.Key.TERM_IZQ,
                                                    CALIBRE = grupoPlanilha.Key.CALIBRE,
                                                    TOTAL = grupoPlanilha.Count()
                                                }).OrderByDescending(c => c.TOTAL);
                        }
                        if (TotalPorTerminal.Count() > 0)
                            sTerm = TotalPorTerminal.FirstOrDefault().TERMINAL;
                    }

                    if (sTerm != "")
                    {
                        // verifico dados que podem ser invertidos.
                        var lInvert = (from c in this._lDadosPlanilha
                                       where
                                       c.TERM_DER == sTerm
                                       && c.ACC_01_D != ""
                                       && c.ACC_01_I != ""
                                       && c.bUtilizado == false
                                       select c).OrderBy(c => c.CALIBRE).ToList();

                        foreach (var itemToInvert in lInvert)
                        {
                            Util.InverteLado(itemToInvert);
                        }

                        // incluo primeiro os 2 - Y
                        var dadosAutomaticos_Manuais = (from c in this._lDadosPlanilha
                                                        where
                                                        c.TERM_IZQ == sTerm
                                                        && c.ACC_01_D != ""
                                                        && c.ACC_01_I != ""
                                                        && c.COD_DD == "Y "
                                                        && c.bUtilizado == false
                                                        select c).OrderBy(c => c.CALIBRE).ToList();

                        foreach (var item in dadosAutomaticos_Manuais)
                        {
                            item.bUtilizado = true;
                            this.resultado.Add(item);
                        }

                        var dados = (from c in this._lDadosPlanilha
                                     where
                                     c.TERM_IZQ == sTerm
                                       && c.ACC_01_D != ""
                                       && c.ACC_01_I != ""
                                     && c.bUtilizado == false
                                     select c).OrderBy(c => c.CALIBRE).Take(5);

                        List<PlanilhaModel> lDadosIncluosos = new List<PlanilhaModel>();
                        foreach (var item in dados)
                        {
                            if (this.resultado.TotalTerminalDireitoFaltante > 0)
                                this.IncludeTerminalDireito_B_A(item, lDadosIncluosos);
                        }


                        var TotalPorTerminal = (from c in this._lDadosPlanilha
                                                where lDadosIncluosos.Select(o => o.TERM_IZQ).Contains(c.TERM_IZQ)
                                                && c.ACC_01_D != ""
                                                && c.ACC_01_I != ""
                                                && c.bUtilizado == false
                                                group c by new { c.TERM_IZQ, c.CALIBRE } into grupoPlanilha
                                                select new
                                                {
                                                    TERMINAL = grupoPlanilha.Key.TERM_IZQ,
                                                    CALIBRE = grupoPlanilha.Key.CALIBRE,
                                                    TOTAL = grupoPlanilha.Count()
                                                }).OrderBy(c => c.TOTAL);

                        if (TotalPorTerminal.Count() > 0)
                            this.IncludeAutomaticosLado_A_B(TotalPorTerminal.FirstOrDefault().TERMINAL);
                        else
                            this.IncludeAutomaticosLado_A_B();

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        private void IncludeTerminalDireito_B_A(PlanilhaModel item, List<PlanilhaModel> lDadosIncluosos)
        {
            try
            {
                // verifico dados que podem ser invertidos.
                var lInvert = (from c in this._lDadosPlanilha
                               where
                               c.TERM_IZQ == item.TERM_DER
                               && c.ACC_01_D != ""
                               && c.ACC_01_I != ""
                               && c.bUtilizado == false
                               select c).OrderBy(c => c.CALIBRE).ToList();

                foreach (var itemToInvert in lInvert)
                {
                    Util.InverteLado(itemToInvert);
                }

                // incluo primeiro os 2 - Y
                var dadosAutomaticos_Manuais = (from c in this._lDadosPlanilha
                                                where
                                                c.TERM_DER == item.TERM_DER
                                                && c.COD_DD == "Y "
                                                && c.ACC_01_D != ""
                                                && c.ACC_01_I != ""
                                                && c.bUtilizado == false
                                                select c).OrderBy(c => c.CALIBRE).ToList();

                foreach (var i in dadosAutomaticos_Manuais)
                {
                    i.bUtilizado = true;
                    this.resultado.Add(i);
                }

                var dados = (from c in this._lDadosPlanilha
                             where
                             c.TERM_DER == item.TERM_DER
                             && c.ACC_01_D != ""
                             && c.ACC_01_I != ""
                             && c.bUtilizado == false
                             select c).OrderBy(c => c.CALIBRE).Take(3);

                foreach (var itemAincluir in dados)
                {
                    if (this.resultado.TotalTerminalEsquerdoFaltante > 0)
                    {
                        itemAincluir.bUtilizado = true;
                        this.resultado.Add(itemAincluir);
                        lDadosIncluosos.Add(itemAincluir);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        //private void IncludeAutomaticosAteCompletarTerminais()
        //{
        //    try
        //    {
        //        int iCountAtual = this.resultado.Count();
        //        var resultPesquisa = new List<PlanilhaModel>();
        //        if (this.resultado.TotalTerminalDireitoFaltante > 0 && this.resultado.TotalTerminalEsquerdoFaltante > 0)
        //        {
        //            resultPesquisa = (from c in this._lDadosPlanilha
        //                              where c.COD_DI == "2"
        //                              && c.COD_DD == "2"
        //                              && c.ACC_01_D == ""
        //                              && c.ACC_01_I == ""
        //                              && c.bUtilizado == false
        //                              select c).OrderBy(c => c.CALIBRE).ToList();
        //            if (resultPesquisa.Count() > 0)
        //            {
        //                var item = resultPesquisa.FirstOrDefault();
        //                item.bUtilizado = true;
        //                this.resultado.Add(item);

        //                if (this.resultado.TotalTerminalEsquerdoFaltante > 0)
        //                {
        //                    var itemEsquerdo = (from c in this._lDadosPlanilha
        //                                        where c.COD_DI == "2"
        //                                        && c.ACC_01_I == ""
        //                                        && c.COD_DD == "Y"
        //                                        && c.TERM_IZQ == item.TERM_IZQ
        //                                        && c.bUtilizado == false
        //                                        select c).OrderBy(c => c.CALIBRE).FirstOrDefault();

        //                    if (itemEsquerdo != null)
        //                    {
        //                        itemEsquerdo.bUtilizado = true;
        //                        this.resultado.Add(itemEsquerdo);
        //                    }
        //                }
        //                if (this.resultado.TotalTerminalDireitoFaltante > 0)
        //                {
        //                    var itemDireito = (from c in this._lDadosPlanilha
        //                                       where c.COD_DD == "2"
        //                                       && c.ACC_01_D == ""
        //                                       && c.COD_DI == "Y"
        //                                       && c.TERM_DER == item.TERM_DER
        //                                       && c.bUtilizado == false
        //                                       select c).OrderBy(c => c.CALIBRE).FirstOrDefault();

        //                    if (itemDireito != null)
        //                    {
        //                        itemDireito.bUtilizado = true;
        //                        this.resultado.Add(itemDireito);
        //                    }
        //                }
        //            }
        //        }
        //        if (resultPesquisa.Count() == 0)
        //        {
        //            if (this.resultado.TotalTerminalEsquerdoFaltante > 0)
        //            {
        //                // contador para verificar se existe cabo automático na esquerda
        //                int icountTotalAutomaticoLadoEsquerdo = (from c in this._lDadosPlanilha
        //                                                         where c.COD_DI == "2"
        //                                                         && c.COD_DD == "Y"
        //                                                         && c.bUtilizado == false
        //                                                         select c).Count();

        //                PlanilhaModel itemEsquerdo = null;
        //                if (icountTotalAutomaticoLadoEsquerdo > 0)
        //                {
        //                    // caso exista eu busco um para inserir na maquina
        //                    itemEsquerdo = (from c in this._lDadosPlanilha
        //                                    where c.COD_DI == "2"
        //                                    && c.ACC_01_I == ""
        //                                    && c.COD_DD == "Y"
        //                                    && c.bUtilizado == false
        //                                    select c).OrderBy(c => c.CALIBRE).FirstOrDefault();
        //                }
        //                if (itemEsquerdo == null)
        //                {
        //                    // caso não tenha cabo na esquerda, eu verifico na direita e mudo ele de lado.
        //                    itemEsquerdo = (from c in this._lDadosPlanilha
        //                                    where c.COD_DD == "2"
        //                                    && c.ACC_01_D == ""
        //                                    && c.COD_DI == "Y"
        //                                    && c.bUtilizado == false
        //                                    select c).OrderBy(c => c.CALIBRE).FirstOrDefault();
        //                    if (itemEsquerdo != null)
        //                        itemEsquerdo = Util.InverteLado(itemEsquerdo);
        //                }



        //                if (itemEsquerdo != null)
        //                {
        //                    itemEsquerdo.bUtilizado = true;
        //                    this.resultado.Add(itemEsquerdo);
        //                }
        //            }
        //            if (this.resultado.TotalTerminalDireitoFaltante > 0)
        //            {
        //                var itemDireito = (from c in this._lDadosPlanilha
        //                                   where c.COD_DD == "2"
        //                                   && c.ACC_01_D == ""
        //                                   && c.COD_DI == "Y"
        //                                   && c.bUtilizado == false
        //                                   select c).OrderBy(c => c.CALIBRE).FirstOrDefault();

        //                if (itemDireito != null)
        //                {
        //                    itemDireito.bUtilizado = true;
        //                    this.resultado.Add(itemDireito);
        //                }
        //            }
        //        }

        //        if (this.resultado.Count > iCountAtual)
        //            if (this.resultado.TotalTerminalDireitoFaltante > 0 || this.resultado.TotalTerminalEsquerdoFaltante > 0)
        //                this.IncludeAutomaticosAteCompletarTerminais2();


        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }

        //}


        private void IncludeSelosManuais()
        {
            try
            {
                if (this.resultado.GetVolumeTotalManualByLista() < this.resultado.param.volumeYY)
                {

                    var cabosAtuais = (from c in this.resultado
                                       select new
                                       {
                                           CALIBRE = c.CALIBRE,
                                           TIPO = c.TIPO
                                       }).Distinct().ToList();

                    var dadosYY = from c in this._lDadosPlanilha
                                  where c.COD_DI == "Y"
                                        && c.COD_DD == "Y"
                                        && cabosAtuais.Contains(new { c.CALIBRE, c.TIPO })
                                  select c;

                    if (dadosYY.Count() == 0)
                    {
                        dadosYY = from c in this._lDadosPlanilha
                                  where c.COD_DI == "Y"
                                        && c.COD_DD == "Y"
                                  select c;
                    }

                    decimal dTotalYYparaAnalise = this.resultado.GetVolumeTotalManualByLista();
                    foreach (var item in dadosYY)
                    {
                        if ((item.CANTIDAD.ToDecimal() + this.resultado.GetVolumeTotalManualByLista()) > this.resultado.param.volumeYY)
                        {
                            decimal dvalorToRemove = (item.CANTIDAD.ToDecimal() + this.resultado.GetVolumeTotalManualByLista()) - this.resultado.param.volumeYY;
                            item.SubtraiValor(dvalorToRemove);
                        }

                        var PosicaoItem = this.resultado.FirstOrDefault(c => c.CALIBRE == item.CALIBRE && c.TIPO == item.TIPO);
                        if (PosicaoItem != null)
                        {
                            item.G = PosicaoItem.G;
                            item.id = PosicaoItem.id;
                        }
                        this.resultado.Add(this.DesvinculaItemCollectionGenerica(item));

                        if (this.resultado.GetVolumeTotalManualByLista() >= this.resultado.param.volumeYY)
                        {
                            break;
                        }
                    }

                    if ((this.resultado.GetVolumeTotalManualByLista() < this.resultado.param.volumeYY) && (this.resultado.GetVolumeTotalManualByLista() != dTotalYYparaAnalise))
                        this.IncludeSelosManuais();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// Método que irá chamar irá fazer a organização e salvar o arquivo em excel.
        /// </summary>
        /// <param name="lDadosPlanilha_"></param>
        public void IniciaOrganizacao(List<PlanilhaModel> lDadosPlanilha_)
        {
            try
            {
                this.SetParametros();
                if (_lDadosPlanilha != null)
                    foreach (var item in this._lDadosPlanilha)
                    {
                        item.bUtilizado = false;
                        item.id = null;
                    }

                this._lDadosPlanilha = new List<PlanilhaModel>();

                foreach (var item in lDadosPlanilha_.Where(c => (Convert.ToDecimal(c.CALIBRE.Replace(".", ",")) >= this.resultado.param.bitolaMin
                                                            && Convert.ToDecimal(c.CALIBRE.Replace(".", ",")) <= this.resultado.param.bitolaMax)
                                                            && c.bUtilizado == false).ToList())
                {
                    item.bUtilizado = false;
                    item.id = null;
                    this._lDadosPlanilha.Add(item);
                }

                if (this.resultado.param.IsSelos)
                {
                    // INCLUI SELOS
                    this.IncludeSelos();
                    Util.bAtivaRegraModel = true;

                    // INCLUI CABOS QUE FAZEM REFERENCIA COM OS SELOS.
                    //if (!this.resultado.isCompleted)
                    if (this.resultado.param.lseloEsq.Count() > 0 && this.resultado.param.lseloDir.Count() > 0)
                        this.IncludeAutomaticosBySelos();
                }
                Util.bAtivaRegraModel = true;
                Util.bAtivaRegraModel = false;

                if (this.resultado.TotalTerminalDireitoFaltante > 0 && this.resultado.TotalTerminalEsquerdoFaltante > 0)
                {
                    // AUTOMÁTICOS.
                    this.IncludeAutomaticosLado_A_B();
                }

                if (!this.resultado.ValidaPorcentagemGeral())
                    this.AnalisedeQuantidade();

                this._lDadosParaAssignacao = new List<PlanilhaModel>();
                foreach (var item in this.resultado)
                {
                    this._lDadosParaAssignacao.Add(this.DesvinculaItemCollectionGenerica(item));
                }

                this._lDadosParaAssignacao = Util.GroupList(this._lDadosParaAssignacao);

                YazakiList.ParametrosLista param = this.resultado.GetParametro();
                this.resultado = new YazakiList();
                this.resultado.param = param;
                BeginAssignacao();

                // MANUAIS - MANUAIS
                this.IncludeSelosManuais();

                var dadosYY = this.resultado.Where(c => c.COD_DD == "Y" && c.COD_DI == "Y").ToList();
                foreach (var item in dadosYY)
                {
                    this.resultado.Remove(item);
                }
                dadosYY = Util.GroupListYY(dadosYY);
                foreach (var itemYYtoAdd in dadosYY)
                {
                    this.resultado.Add(itemYYtoAdd);
                }

                // metodo que escreve os dados em xls e salva.
                this.fileLocation = Util.WriteTsv<PlanilhaModel>(this.resultado.ToList());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// Retorna o objeto clonado
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private PlanilhaModel DesvinculaItemCollectionGenerica(PlanilhaModel item)
        {
            try
            {
                PlanilhaModel itemClone = item.Clone() as PlanilhaModel;
                if (item.dRestante > 0)
                {
                    item.CANTIDAD = item.dRestante.ToString();
                    item.dRestante = 0;
                    item.bUtilizado = false;
                }
                else
                {
                    item.CANTIDAD = "0";
                    item.bUtilizado = true;
                }
                itemClone.bUtilizado = false;
                return itemClone;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private void AnalisedeQuantidade()
        {
            try
            {

                var item = this.resultado.OrderByDescending(c => c.PERCENTUAL).FirstOrDefault();

                item.SubtraiPercentual(1);
                bool bValida = this.resultado.ValidaPorcentagemGeral();
                if (bValida == false)
                {
                    AnalisedeQuantidade();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        /// <summary>
        /// Método responsável por selecionar o lado e o terminal que será analisádo.
        /// </summary>
        /// <param name="lado"></param>
        private void BeginAssignacao(Lado lado = Lado.ESQUERDO)
        {
            try
            {
                // selecionamos os teminais que são automáticos pelo lado esquerdo / caso seja a vez do lado direito valerá a condição abaixo.
                var TotalPorTerminal = (from c in this._lDadosParaAssignacao
                                        where c.COD_DI != "Y"
                                        && c.bUtilizado == false
                                        group c by new { c.TERM_IZQ, c.CALIBRE } into grupoPlanilha
                                        select new
                                        {
                                            TERMINAL = grupoPlanilha.Key.TERM_IZQ,
                                            CALIBRE = grupoPlanilha.Key.CALIBRE,
                                            TOTAL = grupoPlanilha.Count()
                                        }).OrderBy(c => c.CALIBRE);

                if (lado == Lado.DIREITO)
                    TotalPorTerminal = (from c in this._lDadosParaAssignacao
                                        where c.COD_DD != "Y"
                                        && c.bUtilizado == false
                                        group c by new { c.TERM_DER, c.CALIBRE } into grupoPlanilha
                                        select new
                                        {
                                            TERMINAL = grupoPlanilha.Key.TERM_DER,
                                            CALIBRE = grupoPlanilha.Key.CALIBRE,
                                            TOTAL = grupoPlanilha.Count()
                                        }).OrderBy(c => c.CALIBRE);

                if (TotalPorTerminal.Count() == 0 && lado == Lado.ESQUERDO)
                {
                    BeginAssignacao(Lado.DIREITO);
                }

                if (TotalPorTerminal.Count() > 0)
                {
                    // Selecionamos um Terminal automático aleatório
                    //Random alatoryIndex = new Random();
                    //int index = alatoryIndex.Next(0, TotalPorTerminal.Count() - 1);
                    //var caboSelecionado = TotalPorTerminal.ToList()[index];
                    //if (TotalPorTerminal != null)
                    //{
                    //    PercorreDadosCabo(caboSelecionado.TERMINAL, lado);
                    //}
                    var caboSelecionado = TotalPorTerminal.FirstOrDefault();
                    if (TotalPorTerminal != null)
                    {
                        PercorreDadosCabo(caboSelecionado.TERMINAL, lado);
                    }
                }
                else
                {
                    // Hora de tratar os Y-Y
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Método recursivo que faz toda a organização dos cabos com terminais automáticos 2 ou 4
        /// </summary>
        /// <param name="TERM"></param>
        /// <param name="lado"></param>
        private void PercorreDadosCabo(string TERM, Lado lado)
        {
            try
            {
                iContador = iContador + 1;

                var cabos = this._lDadosParaAssignacao.Where(c =>
                                        c.COD_DI != "Y"
                                        && c.TERM_IZQ == TERM
                                        && c.bUtilizado == false);
                if (lado == Lado.DIREITO)
                    cabos = this._lDadosParaAssignacao.Where(c =>
                                        c.COD_DD != "Y"
                                        && c.TERM_DER == TERM
                                        && c.bUtilizado == false);


                var cabosInlcusos = new List<PlanilhaModel>();
                foreach (var itemToAdd in cabos)
                {
                    itemToAdd.bUtilizado = true;
                    cabosInlcusos.Add(itemToAdd);
                }
                // cabos = Util.GroupList(cabos.ToList());

                //verifica se da para inverter algum item.
                foreach (var item in cabos)
                {
                    //if (item.bUtilizado == false)
                    //{
                    //    item.bUtilizado = true;
                    //    cabosInlcusos.Add(item);
                    //}

                    // verificamos se na planilha original tem o item invertido, caso tenha nós alteramos e incluimos na lista de cabosInclusos.
                    var TotalInvertido = this._lDadosParaAssignacao.Where(c => c.TERM_DER == item.TERM_IZQ && c.TERM_IZQ == item.TERM_DER
                                                    && c.bUtilizado == false);

                    if (lado == Lado.DIREITO)
                        TotalInvertido = this._lDadosParaAssignacao.Where(c => c.TERM_IZQ == item.TERM_DER && c.TERM_DER == item.TERM_IZQ
                                                    && c.bUtilizado == false);

                    if (TotalInvertido.Count() > 0)
                    {
                        foreach (var c in TotalInvertido)
                        {
                            cabosInlcusos.Add(Util.InverteLado(c));
                        }
                    }
                }

                // agrupamos os registros.
                //cabosInlcusos = Util.GroupList(cabosInlcusos.ToList());


                // inverte o lado da comparação.
                lado = lado == Lado.ESQUERDO ? Lado.DIREITO : Lado.ESQUERDO;

                List<string> terminais = cabosInlcusos.Where(c => c.COD_DI != "Y")
                    .Select(c => c.TERM_IZQ).Distinct().ToList();

                if (lado == Lado.DIREITO)
                    terminais = cabosInlcusos.Where(c => c.COD_DD != "Y")
                        .Select(c => c.TERM_DER).Distinct().ToList();

                // buscamos qual o terminal que mais se repete.
                var terminalMaisRepete = (from t in this._lDadosParaAssignacao
                                          where terminais.Contains(t.TERM_IZQ) && t.COD_DI != "Y" && t.bUtilizado == false
                                          group t.idPLANILHA by t.TERM_IZQ into term
                                          select new
                                          {
                                              TERMINAL = term.Key,
                                              TOTAL = term.Count()
                                          }).OrderByDescending(c => c.TOTAL).FirstOrDefault();

                if (lado == Lado.DIREITO)
                    terminalMaisRepete = (from t in this._lDadosParaAssignacao
                                          where terminais.Contains(t.TERM_DER) && t.COD_DD != "Y" && t.bUtilizado == false
                                          group t.idPLANILHA by t.TERM_DER into term
                                          select new
                                          {
                                              TERMINAL = term.Key,
                                              TOTAL = term.Count()
                                          }).OrderByDescending(c => c.TOTAL).FirstOrDefault();



                if (terminalMaisRepete != null)
                {
                    cabosInlcusos = cabosInlcusos.OrderBy(c => c.TERM_IZQ == terminalMaisRepete.TERMINAL).ToList();
                    if (lado == Lado.DIREITO)
                        cabosInlcusos = cabosInlcusos.OrderBy(c => c.TERM_DER == terminalMaisRepete.TERMINAL).ToList();

                    // Organizamos o restante.
                    // pegamos todos os cabos menos o que ficará por último ...
                    var dadosParaOrganizar = cabosInlcusos.Where(c => c.TERM_IZQ != terminalMaisRepete.TERMINAL);
                    if (lado == Lado.DIREITO)
                        dadosParaOrganizar = cabosInlcusos.Where(c => c.TERM_DER != terminalMaisRepete.TERMINAL);


                    int iCount = 0; // contador para organizar o grupo.
                    // organizamos pelo terminal, calibre e tipo.   

                    if (this.objUltimoRegistro != null)
                    {
                        try
                        {
                            // esses dados são os que vão ficar acima ( por primeiro ).
                            var dadosIniciais = from c in dadosParaOrganizar
                                                //orderby c.TERM_IZQ, c.CALIBRE, c.TIPO
                                                where c.CALIBRE == objUltimoRegistro.CALIBRE && c.TIPO == objUltimoRegistro.TIPO
                                                select c;

                            foreach (var item in dadosIniciais)
                            {
                                item.id = iCount;
                                iCount++;
                            }
                        }
                        catch (Exception)
                        {

                            throw;
                        }

                    }

                    dadosParaOrganizar = from c in dadosParaOrganizar
                                         orderby c.TERM_IZQ, c.CALIBRE, c.TIPO
                                         where c.id == null
                                         select c;

                    foreach (var item in dadosParaOrganizar)
                    {
                        item.id = iCount;
                        iCount++;
                    }

                    if (cabosInlcusos.Count() > 1)
                    {
                        var ultimoCaboIncluso = cabosInlcusos.Where(c => c.id != null).OrderBy(c => c.id).LastOrDefault();

                        if (ultimoCaboIncluso != null)
                        {
                            var dadosRestantes = cabosInlcusos.Where(c => c.id == null).OrderByDescending(c => c.CALIBRE == ultimoCaboIncluso.CALIBRE);
                            // os cabos que ficaram por último não são organizados dessa maneira abaixo pois devem seguir o último calibre que vem na sequencia.
                            //dadosRestantes = from c in dadosRestantes
                            //                 orderby c.TERM_IZQ, c.CALIBRE, c.TIPO
                            //                 select c;
                            foreach (var item in dadosRestantes)
                            {
                                item.id = iCount;
                                iCount++;
                            }
                        }

                    }


                    // guardamos o último registro, para análise na próxima passada.
                    objUltimoRegistro = cabosInlcusos.LastOrDefault();
                }

                try
                {
                    foreach (var item in cabosInlcusos.OrderBy(c => c.id))
                    {
                        item.G = Convert.ToChar(64 + iContador).ToString();
                        this.resultado.Add(item);
                    }
                }
                catch (Exception)
                {

                    throw;
                }

                if (terminalMaisRepete != null)
                    PercorreDadosCabo(terminalMaisRepete.TERMINAL.ToString(), lado);
                else
                    BeginAssignacao();

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        #endregion

    }





}