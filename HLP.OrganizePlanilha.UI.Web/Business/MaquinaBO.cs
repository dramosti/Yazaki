using HLP.OrganizePlanilha.UI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HLP.OrganizePlanilha.UI.Web.Business
{
    /// <summary>
    /// Classe de negócio, onde será feita todas as regras para inserir os cabos na máquina e a assignação
    /// </summary>
    public sealed class MaquinaBO
    {
        #region Propriedades Publicas
        /// <summary>
        /// Lista que será o resultado final da análise e será convertida em planilha
        /// </summary>
        public YazakiList resultado { get; set; }

        /// <summary>
        /// Lista de todos os cabos que contem selos
        /// </summary>
        public List<PlanilhaModel> lSelos { get; set; }
        /// <summary>
        /// Lista que terá todos os cabos utilizados, sem nenhum agrupamento, e só será usada para saber quais cabos
        /// estão na contidos na máquina para posteriormente serem desvinculados.
        /// </summary>
        public List<PlanilhaModel> lUtilizadosSemAgrupamento { get; set; }
        /// <summary>
        /// Objeto que representa a máquina
        /// </summary>
        public MaquinaModel _maquina { get; set; }
        /// <summary>
        /// Local onde o arquivo xml assignado foi salvo.
        /// </summary>
        public string fileLocation { get; set; }

        #endregion
        #region Propriedades privadas
        /// <summary>
        /// Lista que irá manter todos os cabos possíveis de acordo com a parametrização de calibre.
        /// O Sistema irá buscar dessa lista os cabos para inserir na máquina.
        /// </summary>
        private List<PlanilhaModel> _lDadosPlanilha { get; set; }
        /// <summary>
        /// Lista que irá conter sómente os cabos já analisados e prontos para a assignação.
        /// </summary>
        private List<PlanilhaModel> _lDadosParaAssignacao { get; set; }

        /// <summary>
        /// Contador q será convertido em Letras para separar os Grupos
        /// </summary>
        private int iContador { get; set; }

        /// <summary>
        /// Objeto utilizado na assignação, para saber qual foi o último registro analisado.
        /// </summary>
        private PlanilhaModel objUltimoRegistro = null;

        #endregion
        #region Construtor
        public MaquinaBO(MaquinaModel maquina_)
        {
            this._maquina = maquina_;
            this.resultado = new YazakiList();
            this.lUtilizadosSemAgrupamento = new List<PlanilhaModel>();
        }
        #endregion
        #region Metodos
        /// <summary>
        /// Método que irá chamar irá fazer a organização e salvar o arquivo em excel.
        /// </summary>
        /// <param name="lDadosPlanilha_"></param>
        public void IniciaOrganizacao(List<PlanilhaModel> lDadosPlanilha_)
        {
            try
            {
                this.SetParametros();


                this._lDadosPlanilha = new List<PlanilhaModel>();
                //Inserimos todos os cabos possíveis a serem analisados.
                foreach (var item in lDadosPlanilha_.Where(c => (Convert.ToDecimal(c.CALIBRE.Replace(".", ",")) >= this.resultado.param.bitolaMin
                                                            && Convert.ToDecimal(c.CALIBRE.Replace(".", ",")) <= this.resultado.param.bitolaMax)
                                                            && c.bUtilizado == false).ToList())
                {
                    item.bUtilizado = false;
                    item.id = null;
                    this._lDadosPlanilha.Add(item);
                }

                // caso haja selos, é a primeira coisa que fazemos.
                if (this.resultado.param.IsSelos)
                {
                    // incluir selos
                    this.IncludeSelos();
                    // nesse momento, nós deixamos registrado todos os terminais que acompanham os selos inseridos para não serem contabilizados na qtde de terminais parametrizados na maquina.
                    this.resultado.SetTerminaisComSelos();
#warning verificar se os terminais inclusos juntamente com os cabos que fazem combinação com cabos que tem selos serão validos, para contagem de terminais.

                    Util.bAtivaRegraModel = true;

                    // INCLUI CABOS QUE FAZEM REFERENCIA COM CABOS QUE TEM SELOS.
                    if (this.resultado.param.lseloEsq.Count() > 0 && this.resultado.param.lseloDir.Count() > 0)
                        if (!this.resultado.Ultrapassou())
                            this.IncludeAutomaticosBySelos();
                }
                else
                    Util.bAtivaRegraModel = true;


                if (this.resultado.TotalTerminalDireitoFaltante > 0 && this.resultado.TotalTerminalEsquerdoFaltante > 0)
                {
                    // AUTOMÁTICOS.
                    this.IncludeAutomaticosLado_A_B();
                }

                // Incluimos os Terminais que são Y-2 ou 2-Y onde o Terminal já se encontra na Lista
                IncludeManualAutimatico_AutomaticoManual();


                if (this.resultado.TotalTerminalDireitoFaltante > 0)
                {
                    this.IncludeManualAutimatico(); // Y-2
                }
                if (this.resultado.TotalTerminalEsquerdoFaltante > 0)
                {
                    this.IncludeAutomaticoManual(); // 2-Y
                }

                if (this.resultado.Ultrapassou())
                    this.AnalisedeQuantidade();

                this._lDadosParaAssignacao = new List<PlanilhaModel>();
                foreach (var item in this.resultado)
                {
                    this._lDadosParaAssignacao.Add(this.DesvinculaItemCollectionGenerica(item));
                }

                //crio uma Lista para saber quais itens estão sendo utilizados nessa máquina, para posteriormente desvincular.
                foreach (PlanilhaModel item in this._lDadosParaAssignacao)
                {
                    lUtilizadosSemAgrupamento.Add(item.Clone() as PlanilhaModel);
                }

                this._lDadosParaAssignacao = Util.GroupList(this._lDadosParaAssignacao);

                YazakiList.ParametrosLista param = this.resultado.GetParametro();
                this.resultado = new YazakiList();
                this.resultado.param = param;
                BeginAssignacao();

                // MANUAIS - MANUAIS
                this.IncludeCabosManuais();

                var dadosYY = this.resultado.Where(c => c.COD_DD == "Y" && c.COD_DI == "Y").ToList();
                foreach (var item in dadosYY)
                {
                    lUtilizadosSemAgrupamento.Add(item.Clone() as PlanilhaModel); // insiro os manuais que até então nao se encontravam na lista.
                    this.resultado.Remove(item);
                }
                dadosYY = Util.GroupListYY(dadosYY);
                foreach (var itemYYtoAdd in dadosYY)
                {
                    this.resultado.Add(itemYYtoAdd);
                }

                // metodo que escreve os dados em xls e salva.
                this.fileLocation = Util.WriteOne<PlanilhaModel>(this.resultado.ToList());
                this._maquina.bAssigacao = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Método que faz a assignação de todos os registros que recebe como parametro, sem analisar nenhum parametro de máquina
        /// </summary>
        /// <param name="lDadosPlanilha_"></param>
        public void OrganizacaoRestante(List<PlanilhaModel> lDadosPlanilha_)
        {
            try
            {
                this.SetParametros();
                this._lDadosParaAssignacao = new List<PlanilhaModel>();
                foreach (var item in lDadosPlanilha_.Where(c => c.bUtilizado == false))
                {
                    this.lUtilizadosSemAgrupamento.Add(item);
                    this._lDadosParaAssignacao.Add(item.Clone() as PlanilhaModel);
                }
                BeginAssignacao();


                foreach (var item in lDadosPlanilha_.Where(c => c.bUtilizado == false && c.COD_DI == "Y" && c.COD_DD == "Y").ToList())
                {
                    this.resultado.Add(item);
                }
                // metodo que escreve os dados em xls e salva.
                this.fileLocation = Util.WriteOne<PlanilhaModel>(this.resultado.ToList());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



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

                //this.resultado.param.termDirMax += this.resultado.param.lseloDir.Where(c=> c != "").Count();
                //this.resultado.param.termDirMin += this.resultado.param.lseloDir.Where(c => c != "").Count();

                //this.resultado.param.termEsqMax += this.resultado.param.lseloEsq.Where(c => c != "").Count();
                //this.resultado.param.termEsqMin += this.resultado.param.lseloEsq.Where(c => c != "").Count();


            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Método responsável por incluir todos os selos parametrizados na máquina.
        /// </summary>
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
        /// <summary>
        /// Método recursivo, responsavel por encontrar possíveis combinações entre terminais com selos e sem selos.
        /// O método varre a lista de terminais com selos, verificando se existe o mesmo terminal, só que sem selo.
        /// </summary>
        private void IncludeAutomaticosBySelos()
        {
            try
            {
                int icountAtual = this.resultado.Count();
                var dadosPesquisa = new List<PlanilhaModel>();
                foreach (var itemSelo in this.lSelos)
                {

                    dadosPesquisa = new List<PlanilhaModel>();

                    // PRIMEIRAMENTE ANALISAMOS OS TERMINAIS COM SELO DO LADO ESQUERDO.                   
                    if (itemSelo.COD_DI == "2")
                    {
                        // 2-2  (Automático - Automático)
                        if (this.resultado.TotalTerminalDireitoFaltante > 0)
                        {
                            // resultado para ser invertido o lado. (Automático - Automático)
                            dadosPesquisa = (from c in this._lDadosPlanilha
                                             where c.COD_DI == "2"
                                             && c.COD_DD == "2"
                                             && c.ACC_01_D == ""
                                             && c.ACC_01_I == ""
                                             && c.TERM_DER == itemSelo.TERM_IZQ
                                             && c.bUtilizado == false
                                             select c).ToList();

                            foreach (var itemInvert in dadosPesquisa)
                            {
                                Util.InverteLado(itemInvert);
                            }
                            dadosPesquisa = new List<PlanilhaModel>();
                            // reultado de Automático - Automático.
                            dadosPesquisa = (from c in this._lDadosPlanilha
                                             where c.COD_DI == "2"
                                             && c.COD_DD == "2"
                                             && c.ACC_01_D == ""
                                             && c.ACC_01_I == ""
                                             && c.TERM_IZQ == itemSelo.TERM_IZQ
                                             && c.bUtilizado == false
                                             select c).ToList();
                        }
                        if (dadosPesquisa.Count() == 0)
                        {
                            // resultado para ser invertido ( Automático - Manual )
                            dadosPesquisa = (from c in this._lDadosPlanilha
                                             where
                                             c.COD_DI == "Y"
                                             && c.COD_DD == "2"
                                             && c.ACC_01_D == ""
                                             && c.TERM_DER == itemSelo.TERM_IZQ
                                             && c.bUtilizado == false
                                             select c).ToList();

                            foreach (var itemInvert in dadosPesquisa)
                            {
                                Util.InverteLado(itemInvert);
                            }
                            dadosPesquisa = new List<PlanilhaModel>();
                            // Resultado Automático - Manual

                            dadosPesquisa = (from c in this._lDadosPlanilha
                                             where
                                             c.COD_DI == "2"
                                             && c.ACC_01_I == ""
                                             && c.COD_DD == "Y"
                                             && c.TERM_IZQ == itemSelo.TERM_IZQ
                                             && c.bUtilizado == false
                                             select c).ToList();
                        }
                    }

                    // ANÁLISE DE TERMINAIS COM SELO DO LADO DIREITO .
                    if (dadosPesquisa.Count() == 0)
                    {
                        if (itemSelo.COD_DD == "2")
                        {
                            // 2-2  (Automático - Automático)
                            if (this.resultado.TotalTerminalEsquerdoFaltante > 0)
                            {
                                // resultado para ser invertido o lado. (Automático - Automático)
                                dadosPesquisa = (from c in this._lDadosPlanilha
                                                 where c.COD_DI == "2"
                                                 && c.COD_DD == "2"
                                                 && c.ACC_01_D == ""
                                                 && c.ACC_01_I == ""
                                                 && c.TERM_IZQ == itemSelo.TERM_DER
                                                 && c.bUtilizado == false
                                                 select c).ToList();

                                foreach (var itemInvert in dadosPesquisa)
                                {
                                    Util.InverteLado(itemInvert);
                                }
                                dadosPesquisa = new List<PlanilhaModel>();
                                // reultado de Automático - Automático.
                                dadosPesquisa = (from c in this._lDadosPlanilha
                                                 where c.COD_DI == "2"
                                                 && c.COD_DD == "2"
                                                 && c.ACC_01_D == ""
                                                 && c.ACC_01_I == ""
                                                 && c.TERM_DER == itemSelo.TERM_DER
                                                 && c.bUtilizado == false
                                                 select c).ToList();
                            }
                            if (dadosPesquisa.Count() == 0)
                            {
                                // resultado para ser invertido ( Automático - Manual )
                                dadosPesquisa = (from c in this._lDadosPlanilha
                                                 where
                                                 c.COD_DD == "Y"
                                                 && c.COD_DI == "2"
                                                 && c.ACC_01_I == ""
                                                 && c.TERM_IZQ == itemSelo.TERM_DER
                                                 && c.bUtilizado == false
                                                 select c).ToList();

                                foreach (var itemInvert in dadosPesquisa)
                                {
                                    Util.InverteLado(itemInvert);
                                }
                                dadosPesquisa = new List<PlanilhaModel>();
                                // Resultado Automático - Manual

                                dadosPesquisa = (from c in this._lDadosPlanilha
                                                 where
                                                 c.COD_DD == "2"
                                                 && c.ACC_01_D == ""
                                                 && c.COD_DI == "Y"
                                                 && c.TERM_DER == itemSelo.TERM_DER
                                                 && c.bUtilizado == false
                                                 select c).ToList();
                            }
                        }
                    }
                    if (dadosPesquisa.Count() > 0)
                    {
                        foreach (var item in dadosPesquisa)
                        {
                            if (this.resultado.CombinacaoComSeloDireito(item) && this.resultado.CombinacaoComSeloEsquerdo(item))
                            {
                                item.bUtilizado = true;
                                this.resultado.Add(item);
                            }
                            else
                            {
                                if (this.resultado.PodeAddCaboAnalisandoTerminais(item))
                                {
                                    item.bUtilizado = true;
                                    this.resultado.Add(item);
                                }
                            }
                        }
                    }
                }

                if (this.resultado.Count() > icountAtual)
                    this.IncludeAutomaticosBySelos();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// Método responsável por buscar as combinações de terminais até atingir o parametro de terminais maquina.
        /// Método A->B
        /// Regra de negócio simples: 
        /// a.	(1 – 5) um do lado A para no máximo 5 do lado B
        /// b.	(3 – 1) um do lado B para no máximo 3 do lado A, até terminar as combinações anteriores do anterior “1-5”
        /// c.	Terminando as combinações anteriores, verificar no Lado A, o que menos se repete e trazer para incluir na máquina, e voltar nas regras iniciais desse bloco.
        /// </summary>
        /// <param name="sTerm"></param>
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
                                                c.ACC_01_D == ""
                                                && c.ACC_01_I == ""
                                                && c.COD_DI == "2"
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
                                                c.ACC_01_D == ""
                                                && c.ACC_01_I == ""
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
                                       && c.ACC_01_D == ""
                                       && c.ACC_01_I == ""
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
                                                        && c.ACC_01_D == ""
                                                        && c.ACC_01_I == ""
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
                                       && c.ACC_01_D == ""
                                       && c.ACC_01_I == ""
                                     && c.bUtilizado == false
                                     select c).OrderBy(c => c.CALIBRE).Take(5);

                        List<PlanilhaModel> lDadosIncluosos = new List<PlanilhaModel>();
                        foreach (var item in dados)
                        {
                            if (this.resultado.TotalTerminalDireitoFaltante > 0)
                                //if (item.COD_DD == "2")
                                    this.IncludeTerminalDireito_B_A(item, lDadosIncluosos);
                        }


                        var TotalPorTerminal = (from c in this._lDadosPlanilha
                                                where lDadosIncluosos.Select(o => o.TERM_IZQ).Contains(c.TERM_IZQ)
                                                && c.ACC_01_D == ""
                                                && c.ACC_01_I == ""
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
        /// <summary>
        /// Método B <- A
        /// </summary>
        /// <param name="item"></param>
        /// <param name="lDadosIncluosos"></param>
        private void IncludeTerminalDireito_B_A(PlanilhaModel item, List<PlanilhaModel> lDadosIncluosos)
        {
            try
            {
                // verifico dados que podem ser invertidos.
                var lInvert = (from c in this._lDadosPlanilha
                               where
                               c.TERM_IZQ == item.TERM_DER
                               && c.ACC_01_D == ""
                               && c.ACC_01_I == ""
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
                                                && c.ACC_01_D == ""
                                                && c.ACC_01_I == ""
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
                             && c.ACC_01_D == ""
                             && c.ACC_01_I == ""
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

        /// <summary>
        /// Método responsável por buscar a quantidade de cabos manuais parametrizados na máquina.
        /// </summary>
        private void IncludeCabosManuais()
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
                                        && c.CANTIDAD != "0"
                                        && c.bUtilizado == false
                                        && cabosAtuais.Contains(new { c.CALIBRE, c.TIPO })
                                  select c;

                    if (dadosYY.Count() == 0)
                    {
                        dadosYY = from c in this._lDadosPlanilha
                                  where c.COD_DI == "Y"
                                        && c.COD_DD == "Y"
                                        && c.CANTIDAD != "0"
                                        && c.bUtilizado == false
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
                        this.IncludeCabosManuais();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Inclui 2-Y
        /// </summary>
        private void IncludeAutomaticoManual()
        {
            int iCountValidador = this.resultado.Count();
            var dados = (from c in this._lDadosPlanilha
                         where c.COD_DI == "2"
                         && c.ACC_01_I == ""
                         && c.COD_DD == "Y"
                         && c.bUtilizado == false
                         select c).ToList();
            if (dados.Count() > 0)
            {
                PlanilhaModel item = dados.FirstOrDefault();
                item.bUtilizado = true;
                this.resultado.Add(item);
            }
            else
            {
                dados = (from c in this._lDadosPlanilha
                         where c.COD_DI == "Y"
                         && c.COD_DD == "2"
                         && c.bUtilizado == false
                         select c).ToList();

                if (dados.Count() > 0)
                {
                    PlanilhaModel item = dados.FirstOrDefault();
                    Util.InverteLado(item);
                    item.bUtilizado = true;
                    int iCount = this.resultado.Where(c => c.TERM_IZQ == item.TERM_IZQ).Count();
                    this.resultado.Add(item);
                }
            }
            if (this.resultado.Count() > iCountValidador)
            {
                if (this.resultado.TotalTerminalEsquerdoFaltante > 0)
                {
                    this.IncludeAutomaticoManual();
                }

            }
        }


        private void IncludeManualAutimatico_AutomaticoManual()
        {
            var lTermEsq = this.resultado.Where(c => c.COD_DI == "2").Select(c => c.TERM_IZQ).Distinct().ToArray();
            var lTermDir = this.resultado.Where(c => c.COD_DD == "2").Select(c => c.TERM_DER).Distinct().ToArray();

            bool bInvertLado = false;

            foreach (var item in lTermEsq)
            {
                if (!this.resultado.Ultrapassou())
                {
                    bInvertLado = false;
                    var objResult = (from c in this._lDadosPlanilha
                                     where c.TERM_IZQ == item.ToString()
                                     && c.ACC_01_I == ""
                                     && c.COD_DD == "Y"
                                     && c.bUtilizado == false
                                     select c);
                    if (objResult.Count() == 0)
                    {
                        objResult = from c in this._lDadosPlanilha
                                    where c.TERM_DER == item.ToString()
                                    && c.ACC_01_D == ""
                                    && c.COD_DI == "Y"
                                    && c.bUtilizado == false
                                    select c;
                        if (objResult.Count() > 0)
                        {
                            bInvertLado = true;
                        }
                    }

                    foreach (var itemAdd in objResult)
                    {
                        if (!this.resultado.Ultrapassou())
                        {
                            if (bInvertLado)
                                Util.InverteLado(itemAdd);
                            itemAdd.bUtilizado = true;
                            this.resultado.Add(itemAdd);
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            foreach (var item in lTermDir)
            {
                if (!this.resultado.Ultrapassou())
                {
                    bInvertLado = false;

                    var objResult = from c in this._lDadosPlanilha
                                    where c.TERM_DER == item.ToString()
                                    && c.ACC_01_D == ""
                                    && c.COD_DI == "Y"
                                    && c.bUtilizado == false
                                    select c;

                    if (objResult.Count() == 0)
                    {
                        objResult = (from c in this._lDadosPlanilha
                                     where c.TERM_IZQ == item.ToString()
                                     && c.ACC_01_I == ""
                                     && c.COD_DD == "Y"
                                     && c.bUtilizado == false
                                     select c);
                        if (objResult.Count() > 0)
                        {
                            bInvertLado = true;
                        }
                    }

                    foreach (var itemAdd in objResult)
                    {
                        if (!this.resultado.Ultrapassou())
                        {
                            if (bInvertLado)
                                Util.InverteLado(itemAdd);
                            itemAdd.bUtilizado = true;
                            this.resultado.Add(itemAdd);
                        }
                    }
                }
                else
                {
                    break;
                }
            }
        }



        /// <summary>
        /// inclui Y-2
        /// </summary>
        private void IncludeManualAutimatico()
        {
            int iCountValidador = this.resultado.Count();
            var dados = (from c in this._lDadosPlanilha
                         where c.COD_DD == "2"
                         && c.ACC_01_D == ""
                         && c.COD_DI == "Y"
                         && c.bUtilizado == false
                         select c).ToList();
            if (dados.Count() > 0)
            {
                PlanilhaModel item = dados.FirstOrDefault();
                item.bUtilizado = true;
                this.resultado.Add(item);
            }
            else
            {
                dados = (from c in this._lDadosPlanilha
                         where c.COD_DD == "Y"
                         && c.COD_DI == "2"
                         && c.bUtilizado == false
                         select c).ToList();

                if (dados.Count() > 0)
                {
                    PlanilhaModel item = dados.FirstOrDefault();
                    Util.InverteLado(item);
                    item.bUtilizado = true;
                    this.resultado.Add(item);
                }
            }
            if (this.resultado.Count() > iCountValidador)
            {
                if (this.resultado.TotalTerminalDireitoFaltante > 0)
                {
                    this.IncludeManualAutimatico();
                }

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


        /// <summary>
        /// Método recursivo que analisa o volume total da máquina e se necessário remove dos maiores até dar o volume parametrizado.
        /// </summary>
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

                //verifica se da para inverter algum item.
                foreach (var item in cabos)
                {
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