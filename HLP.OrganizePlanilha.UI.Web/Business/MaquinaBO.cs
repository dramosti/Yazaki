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
        public YazakiList<PlanilhaModel> resultado { get; set; }
        public List<PlanilhaModel> lSelos { get; set; }
        public MaquinaModel _maquina { get; set; }
        public string fileLocation { get; set; }

        #region Propriedades privadas
        /// <summary>
        /// Lista de cabos que serão manipulados.
        /// </summary>
        private List<PlanilhaModel> _lDadosPlanilha { get; set; }

        /// <summary>
        /// Contador q será convertido em Letras para separar os Grupos
        /// </summary>
        private int iContador { get; set; }

        private PlanilhaModel objUltimoRegistro = null;
        #endregion

        public MaquinaBO(MaquinaModel maquina_)
        {
            this._maquina = maquina_;
            this.resultado = new YazakiList<PlanilhaModel>();
        }


        #region Metodos

        /// <summary>
        /// Método responsável por inserir os parametros necessários para a organização dos dados.
        /// </summary>
        private void SetParametros()
        {
            try
            {
                this.resultado.bitolaMin = Convert.ToDecimal(this._maquina.CALIBRE.Split('-')[0].ToString().Replace(".", ","));
                if (this._maquina.CALIBRE.Split('-').Count() > 1)
                    this.resultado.bitolaMax = Convert.ToDecimal(this._maquina.CALIBRE.Split('-')[1].ToString().Replace(".", ","));
                else
                    this.resultado.bitolaMax = this.resultado.bitolaMin;

                if (this._maquina.QTDE_TERM_ESQUERDO != null)
                {
                    this.resultado.termEsqMin = Convert.ToInt32(this._maquina.QTDE_TERM_ESQUERDO.Split('-')[0].ToString());
                    if (this._maquina.QTDE_TERM_ESQUERDO.Split('-').Count() > 1)
                        this.resultado.termEsqMax = Convert.ToInt32(this._maquina.QTDE_TERM_ESQUERDO.Split('-')[1].ToString());
                    else
                        this.resultado.termEsqMax = this.resultado.termEsqMin;
                }

                if (this._maquina.QTDE_TERM_DIREITO != null)
                {
                    this.resultado.termDirMin = Convert.ToInt32(this._maquina.QTDE_TERM_DIREITO.Split('-')[0].ToString());
                    if (this._maquina.QTDE_TERM_ESQUERDO.Split('-').Count() > 1)
                        this.resultado.termDirMax = Convert.ToInt32(this._maquina.QTDE_TERM_ESQUERDO.Split('-')[1].ToString());
                    else
                        this.resultado.termDirMax = this.resultado.termDirMin;
                }

                if (this._maquina.SELOS_ESQUERDO != null)
                {
                    this.resultado.lseloEsq = this._maquina.SELOS_ESQUERDO.Trim().Split(',').ToList();
                    this.resultado.lseloEsq.Add("");
                }
                else
                {
                    this.resultado.lseloEsq = new List<string>();
                    this.resultado.lseloEsq.Add("");
                }

                if (this._maquina.SELOS_DIREITO != null)
                {
                    this.resultado.lseloDir = this._maquina.SELOS_DIREITO.Trim().Split(',').ToList();
                    this.resultado.lseloDir.Add("");
                }
                else
                {
                    this.resultado.lseloDir = new List<string>();
                    this.resultado.lseloDir.Add("");
                }

                if (this._maquina.QTDE_CAPACIDADE != null)
                    this.resultado.volumeTotal = Convert.ToDecimal(this._maquina.QTDE_CAPACIDADE.Replace(".", ""));
                if (this._maquina.YY != null)
                    this.resultado.volumeYY = Convert.ToDecimal(this._maquina.YY.Replace(".", ""));

                if (this._maquina.QTDE_TOLERANCIA != null)
                {
                    this.resultado.toleranciaMin = this.resultado.volumeTotal - Convert.ToDecimal(this._maquina.QTDE_TOLERANCIA.Split('-')[0].ToString().Replace(".", ","));
                    if (this._maquina.QTDE_TOLERANCIA.Split('-').Count() > 1)
                        this.resultado.toleranciaMax = this.resultado.volumeTotal + Convert.ToDecimal(this._maquina.QTDE_TOLERANCIA.Split('-')[1].ToString().Replace(".", ","));
                    else
                        this.resultado.toleranciaMax = this.resultado.volumeTotal;
                }


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
                foreach (var seloe in this.resultado.lseloEsq)
                {
                    if (!this.resultado.lseloDir.Contains(seloe))
                        foreach (var item in this._lDadosPlanilha.Where(c => seloe == c.ACC_01_D))
                        {
                            Util.InverteLado(item);
                        }
                }

                foreach (var selod in this.resultado.lseloDir)
                {
                    if (!this.resultado.lseloEsq.Contains(selod))
                        foreach (var item in this._lDadosPlanilha.Where(c => selod == c.ACC_01_I))
                        {
                            Util.InverteLado(item);
                        }
                }
                foreach (var selo in this.resultado.lseloEsq)
                {
                    if (selo != "")
                    {
                        foreach (var itemSelo in this._lDadosPlanilha.Where(c =>
                            c.ACC_01_I == selo
                            && this.resultado.lseloDir.Contains(c.ACC_01_D)))
                        {
                            // itemSelo.bUtilizado = true;
                            this.lSelos.Add(itemSelo);
                        }
                    }
                }
                foreach (var selo in this.resultado.lseloDir)
                {
                    if (selo != "")
                        foreach (var itemSelo in this._lDadosPlanilha.Where(c =>
                            c.ACC_01_D == selo
                            && this.resultado.lseloEsq.Where(o => o.Equals(c.ACC_01_I)).Count() > 0))
                        {
                            //itemSelo.bUtilizado = true;
                            this.lSelos.Add(itemSelo);
                        }
                }

                this.lSelos = Util.GroupList(this.lSelos);

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

        private void IncludeAutomaticos()
        {

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

                foreach (var item in lDadosPlanilha_.Where(c => (Convert.ToDecimal(c.CALIBRE.Replace(".", ",")) >= this.resultado.bitolaMin
                                                            && Convert.ToDecimal(c.CALIBRE.Replace(".", ",")) <= this.resultado.bitolaMax)
                                                            && c.bUtilizado == false).ToList())
                {
                    item.id = null;
                    this._lDadosPlanilha.Add(item);
                }
                // SELOS
                this.IncludeSelos();





                this._lDadosPlanilha = new List<PlanilhaModel>();
                this._lDadosPlanilha.AddRange(this.lSelos);



                //Util.bAtivaRegraModel = true;
                BeginAssignacao();

                // metodo que escreve os dados em xls e salva.
                this.fileLocation = Util.WriteTsv<PlanilhaModel>(this.resultado.ToList());

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
                var TotalPorTerminal = (from c in this._lDadosPlanilha
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
                    TotalPorTerminal = (from c in this._lDadosPlanilha
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

                var cabos = this._lDadosPlanilha.Where(c =>
                                        c.COD_DI != "Y"
                                        && c.TERM_IZQ == TERM
                                        && c.bUtilizado == false);
                if (lado == Lado.DIREITO)
                    cabos = this._lDadosPlanilha.Where(c =>
                                        c.COD_DD != "Y"
                                        && c.TERM_DER == TERM
                                        && c.bUtilizado == false);


                var cabosInlcusos = new List<PlanilhaModel>();
                foreach (var itemToAdd in cabos)
                {
                    itemToAdd.bUtilizado = true;
                    cabosInlcusos.Add(itemToAdd);
                }
                cabos = Util.GroupList(cabos.ToList());

                //verifica se da para inverter algum item.
                foreach (var item in cabos)
                {
                    //if (item.bUtilizado == false)
                    //{
                    //    item.bUtilizado = true;
                    //    cabosInlcusos.Add(item);
                    //}

                    // verificamos se na planilha original tem o item invertido, caso tenha nós alteramos e incluimos na lista de cabosInclusos.
                    var TotalInvertido = this._lDadosPlanilha.Where(c => c.TERM_DER == item.TERM_IZQ && c.TERM_IZQ == item.TERM_DER
                                                    && c.bUtilizado == false);

                    if (lado == Lado.DIREITO)
                        TotalInvertido = this._lDadosPlanilha.Where(c => c.TERM_IZQ == item.TERM_DER && c.TERM_DER == item.TERM_IZQ
                                                    && c.bUtilizado == false);

                    if (TotalInvertido.Count() > 0)
                    {
                        foreach (var c in TotalInvertido)
                        {
                            //string td = c.TERM_DER; // terminal direito
                            //string sd = c.ACC_01_D; // selo direito
                            //string te = c.TERM_IZQ; // terminal esquerdo                             
                            //string se = c.ACC_01_I; // selo esquerdo

                            //// muda-se os lados.
                            //c.TERM_DER = te;
                            //c.ACC_01_D = se;
                            //c.TERM_IZQ = td;
                            //c.ACC_01_I = sd;
                            //c.bUtilizado = true;
                            cabosInlcusos.Add(Util.InverteLado(c));
                        }
                    }
                }

                // agrupamos os registros.
                cabosInlcusos = Util.GroupList(cabosInlcusos.ToList());


                // inverte o lado da comparação.
                lado = lado == Lado.ESQUERDO ? Lado.DIREITO : Lado.ESQUERDO;

                List<string> terminais = cabosInlcusos.Where(c => c.COD_DI != "Y")
                    .Select(c => c.TERM_IZQ).Distinct().ToList();

                if (lado == Lado.DIREITO)
                    terminais = cabosInlcusos.Where(c => c.COD_DD != "Y")
                        .Select(c => c.TERM_DER).Distinct().ToList();

                // buscamos qual o terminal que mais se repete.
                var terminalMaisRepete = (from t in this._lDadosPlanilha
                                          where terminais.Contains(t.TERM_IZQ) && t.COD_DI != "Y" && t.bUtilizado == false
                                          group t.idPLANILHA by t.TERM_IZQ into term
                                          select new
                                          {
                                              TERMINAL = term.Key,
                                              TOTAL = term.Count()
                                          }).OrderByDescending(c => c.TOTAL).FirstOrDefault();

                if (lado == Lado.DIREITO)
                    terminalMaisRepete = (from t in this._lDadosPlanilha
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