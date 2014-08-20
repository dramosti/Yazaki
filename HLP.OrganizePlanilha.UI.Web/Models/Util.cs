using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace HLP.OrganizePlanilha.UI.Web.Models
{
    public enum Lado { ESQUERDO, DIREITO };

    public class Util
    {
        /// <summary>
        /// Método  que salva a planilha em excel e retorna o nome do arquivo.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string WriteTsv<T>(List<T> data)
        {
            List<PlanilhaModel> ldata = data as List<PlanilhaModel>;

            object cl;

            string fileLocation = string.Format("{0}/{1}", HttpContext.Current.Server.MapPath("~/App_Data/ExcelFiles")
                                                        , string.Format("ArquivoYazaki_{0}.xls", DateTime.Now.ToString("ddMMyyHHmmss")));

            if (System.IO.File.Exists(fileLocation))
            {
                System.IO.File.Delete(fileLocation);
            }

            using (TextWriter output = File.CreateText(fileLocation))
            {
                List<PropertyInfo> lProperties = typeof(T).GetProperties().ToList();
                PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
                foreach (PropertyDescriptor prop in props)
                {
                    cl = lProperties.FirstOrDefault(c => c.Name == prop.DisplayName).GetCustomAttributes(true).FirstOrDefault(i => i.GetType() == typeof(Coluna));
                    bool isColuna = false;
                    if (cl != null)
                        isColuna = (cl as Coluna).isColuna;
                    if (isColuna)
                    {
                        output.Write(prop.DisplayName); // header
                        output.Write("\t");
                    }
                }
                //output.Write("TOTAL");
                //output.Write("\t");
                output.WriteLine();


                foreach (var G in ldata.Select(c => c.G).Distinct())
                {
                    foreach (var item in ldata.Where(c => c.G == G))
                    {
                        foreach (PropertyDescriptor prop in props)
                        {
                            cl = lProperties.FirstOrDefault(c => c.Name == prop.DisplayName).GetCustomAttributes(true).FirstOrDefault(i => i.GetType() == typeof(Coluna));
                            bool isColuna = false;
                            if (cl != null)
                                isColuna = (cl as Coluna).isColuna;
                            if (isColuna)
                            {
                                output.Write(prop.Converter.ConvertToString(
                                     prop.GetValue(item)));
                                output.Write("\t");
                            }
                        }
                        output.WriteLine();
                    }

                    //foreach (PropertyDescriptor prop in props)
                    //{
                    //    cl = lProperties.FirstOrDefault(c => c.Name == prop.DisplayName).GetCustomAttributes(true).FirstOrDefault(i => i.GetType() == typeof(Coluna));
                    //    bool isColuna = false;
                    //    if (cl != null)
                    //        isColuna = (cl as Coluna).isColuna;
                    //    if (isColuna)
                    //    {
                    //        output.Write("");
                    //        output.Write("\t");
                    //    }
                    //}

                    //String sValue = "";
                    //sValue = ldata.Where(c => c.G == G).Sum(c => Convert.ToDecimal(c.CANTIDAD.Replace(".", ","))).ToString("#0.00");
                    //output.Write(sValue);
                    //output.Write("\t");

                    //output.WriteLine();
                }
            }
            return fileLocation;
        }

        public static List<PlanilhaModel> GroupList(List<PlanilhaModel> dados)
        {
            try
            {
                // agrupamos os registros.
                return (from c in dados
                        group c by new
                        {
                            c.PLANTA,
                            c.ACC_01_D,
                            c.ACC_01_I,
                            c.CALIBRE,
                            c.COD_DD,
                            c.COD_DI,
                            c.TERM_DER,
                            c.TERM_IZQ,
                            c.TIPO,
                            c.COD_01_I,
                            c.COD_01_D
                        } into grupo
                        orderby grupo.Sum(o => Convert.ToDecimal(o.CANTIDAD.Replace(".", ",")))
                        select new PlanilhaModel
                        {
                            PLANTA = grupo.Key.PLANTA,
                            ACC_01_D = grupo.Key.ACC_01_D,
                            ACC_01_I = grupo.Key.ACC_01_I,
                            CALIBRE = grupo.Key.CALIBRE,
                            COD_DD = grupo.Key.COD_DD,
                            COD_DI = grupo.Key.COD_DI,
                            TERM_DER = grupo.Key.TERM_DER,
                            TERM_IZQ = grupo.Key.TERM_IZQ,
                            TIPO = grupo.Key.TIPO,
                            COD_01_D = grupo.Key.COD_01_D,
                            COD_01_I = grupo.Key.COD_01_I,
                            CANTIDAD = grupo.Sum(o => Convert.ToDecimal(o.CANTIDAD.Replace(".", ","))).ToString("#0.00")
                        }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public static List<PlanilhaModel> GroupListYY(List<PlanilhaModel> dados)
        {
            try
            {
                // agrupamos os registros.
                return (from c in dados
                        group c by new
                        {
                            c.PLANTA,
                            c.ACC_01_D,
                            c.ACC_01_I,
                            c.CALIBRE,
                            c.COD_DD,
                            c.COD_DI,
                            c.TERM_DER,
                            c.TERM_IZQ,
                            c.TIPO,
                            c.COD_01_I,
                            c.COD_01_D,
                            c.G,
                            c.id,
                        } into grupo
                        orderby grupo.Sum(o => Convert.ToDecimal(o.CANTIDAD.Replace(".", ",")))
                        select new PlanilhaModel
                        {
                            PLANTA = grupo.Key.PLANTA,
                            ACC_01_D = grupo.Key.ACC_01_D,
                            ACC_01_I = grupo.Key.ACC_01_I,
                            CALIBRE = grupo.Key.CALIBRE,
                            COD_DD = grupo.Key.COD_DD,
                            COD_DI = grupo.Key.COD_DI,
                            TERM_DER = grupo.Key.TERM_DER,
                            TERM_IZQ = grupo.Key.TERM_IZQ,
                            TIPO = grupo.Key.TIPO,
                            COD_01_D = grupo.Key.COD_01_D,
                            COD_01_I = grupo.Key.COD_01_I,
                            id = grupo.Key.id,
                            G = grupo.Key.G,
                            CANTIDAD = grupo.Sum(o => Convert.ToDecimal(o.CANTIDAD.Replace(".", ","))).ToString("#0.00")
                        }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public static PlanilhaModel InverteLado(PlanilhaModel c)
        {
            string td = c.TERM_DER; // terminal direito
            string sd = c.ACC_01_D; // selo direito
            string te = c.TERM_IZQ; // terminal esquerdo                             
            string se = c.ACC_01_I; // selo esquerdo
            string codDI = c.COD_DI; // tipo de aplicador
            string codDD = c.COD_DD; // tipo de aplicador


            // muda-se os lados.
            c.TERM_DER = te;
            c.ACC_01_D = se;
            c.TERM_IZQ = td;
            c.ACC_01_I = sd;
            c.COD_DI = codDD;
            c.COD_DD = codDI;

            return c;
        }

        public static bool bAtivaRegraModel { get; set; }
        public static string fileLocation { get; set; }
    }
}
