using HLP.OrganizePlanilha.UI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HLP.OrganizePlanilha.UI.Web.Business
{
    public sealed class ProjetoBO
    {

        // Carrega parametros dos terminais e selos.
        public static void OrganizeDadosParaParametroInicial(ProjetoModel objProjetoModel)
        {
            try
            {
                List<ParametrosModel> ldadosParametroTemp = new List<ParametrosModel>();
                var dados = (from c in objProjetoModel.ldadosPlanilhaOriginal
                             select new
                             {
                                 c.COD_DI,
                                 c.TERM_IZQ,
                                 c.ACC_01_I,
                                 c.COD_01_I,
                                 c.COD_DD,
                                 c.TERM_DER,
                                 c.ACC_01_D,
                                 c.COD_01_D,
                                 c.CANTIDAD
                             }).Distinct();

                foreach (var item in dados)
                {
                    ldadosParametroTemp.Add(new ParametrosModel
                    {
                        COD_D = item.COD_DI,
                        TERM = item.TERM_IZQ,
                        ACC_01 = item.ACC_01_I,
                        COD_01 = item.COD_01_I
                    });
                    ldadosParametroTemp.Add(new ParametrosModel
                    {
                        COD_D = item.COD_DD,
                        TERM = item.TERM_DER,
                        ACC_01 = item.ACC_01_D,
                        COD_01 = item.COD_01_D
                    });

                    decimal dqtde = 0;
                    decimal.TryParse(item.CANTIDAD, out dqtde);
                    objProjetoModel.qTotal += dqtde;
                }
                string[] param = { "D", "U", "PU" };
                objProjetoModel.ldadosParametroTempDistinct = (from p in ldadosParametroTemp
                                                               where param.Contains(p.COD_01)
                                                               group p by new
                                                               {
                                                                   p.TERM,
                                                                   p.ACC_01,
                                                                   p.COD_01,
                                                                   p.COD_D

                                                               } into v
                                                               select new ParametrosModel
                                                               {
                                                                   ACC_01 = v.Key.ACC_01,
                                                                   COD_01 = v.Key.COD_01,
                                                                   COD_D = v.Key.COD_D,
                                                                   TERM = v.Key.TERM
                                                               }).ToList();

                int icount = 1;
                foreach (var item in objProjetoModel.ldadosParametroTempDistinct)
                {
                    item.id = icount;
                    icount++;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Método que efetiva a alteração do parametro de Automático x Manual.
        /// </summary>
        /// <param name="objProjetoModel"></param>
        /// <param name="id"></param>
        public static void AlteraParametroTerminal(ProjetoModel objProjetoModel, int id)
        {
            ParametrosModel param = objProjetoModel.ldadosParametroTempDistinct.FirstOrDefault(c => c.id == id);

            if (param != null)
            {
                var resultado = objProjetoModel.ldadosPlanilhaOriginal.Where(c =>
                                c.TERM_IZQ == param.TERM
                                && c.COD_DI == param.COD_D
                                && c.COD_01_I == param.COD_01
                                && c.ACC_01_I == param.ACC_01);

                foreach (var item in resultado)
                {
                    item.COD_DI = item.COD_DI == "2" ? "Y" : "2";
                }

                resultado = objProjetoModel.ldadosPlanilhaOriginal.Where(
                        c => c.TERM_DER == param.TERM
                            && c.COD_DD == param.COD_D
                            && c.COD_01_D == param.COD_01
                            && c.ACC_01_D == param.ACC_01);

                foreach (var item in resultado)
                {
                    item.COD_DD = item.COD_DD == "2" ? "Y" : "2";
                }
                param.COD_D = param.COD_D == "2" ? "Y" : "2";
            }
        }

        public static PainelModel CarregarDadosPainel(ProjetoModel objProjetoModel)
        {
            try
            {
                objProjetoModel.painel = new PainelModel();
                objProjetoModel.painel.SelosE = objProjetoModel.ldadosPlanilhaOriginal.Where(c => c.bUtilizado == false).Select(c => c.ACC_01_I).Where(c => c != "").Distinct().Count();
                objProjetoModel.painel.SelosD = objProjetoModel.ldadosPlanilhaOriginal.Where(c => c.bUtilizado == false).Select(c => c.ACC_01_D).Where(c => c != "").Distinct().Count();
                objProjetoModel.painel.TerminaisE = objProjetoModel.ldadosPlanilhaOriginal.Where(c => c.bUtilizado == false && c.COD_DI != "Y").Select(c => c.TERM_IZQ).Where(c => c != "").Distinct().Count();
                objProjetoModel.painel.TerminaisD = objProjetoModel.ldadosPlanilhaOriginal.Where(c => c.bUtilizado == false && c.COD_DD != "Y").Select(c => c.TERM_DER).Where(c => c != "").Distinct().Count();
                objProjetoModel.painel.VolumeYY = objProjetoModel.ldadosPlanilhaOriginal.Where(c => c.bUtilizado == false && c.CANTIDAD != "" && c.COD_DI == "Y" && c.COD_DD == "Y").Select(c => Convert.ToDecimal(c.CANTIDAD)).Sum();
                objProjetoModel.painel.VolumeTotal = objProjetoModel.ldadosPlanilhaOriginal.Where(c => c.CANTIDAD != "" && c.bUtilizado == false).Select(c => Convert.ToDecimal(c.CANTIDAD)).Sum();
                return objProjetoModel.painel;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<BitolaModel> CarregarDadosBitola(ProjetoModel objProjetoModel)
        {
            try
            {

                var dados = from c in objProjetoModel.ldadosPlanilhaOriginal
                            where c.CANTIDAD != "" && c.bUtilizado == false && c.COD_DI == "Y" && c.COD_DD == "Y"
                            group Convert.ToDecimal(c.CANTIDAD) by c.CALIBRE into grupo
                            select new
                            {
                                calibre = grupo.Key,
                                volume = grupo.Sum()
                            };

                objProjetoModel.bitolas = new List<BitolaModel>();
                foreach (var item in dados)
                {
                    objProjetoModel.bitolas.Add(new BitolaModel
                    {
                        CALIBRE = item.calibre,
                        VOLUME = item.volume
                    });
                }
                return objProjetoModel.bitolas = objProjetoModel.bitolas.OrderBy(c => c.CALIBRE).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}