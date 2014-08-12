using HLP.OrganizePlanilha.UI.Web.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Web;
using System.IO;

using System.Diagnostics;
using System.ComponentModel;

namespace HLP.OrganizePlanilha.UI.Web.Dao
{
    public class funcoesExcelDao
    {
        private conexaoDao conn;

        private string sTable = string.Empty;

        public funcoesExcelDao(string sPathArquivo)
        {
            conn = new conexaoDao(sPathArquivo);
        }

        public List<PlanilhaModel> GetAllInfo()
        {
            try
            {
                OleDbCommand command = new OleDbCommand();
                command.Connection = conn.OpenConexao();
                command.CommandType = CommandType.Text;
                string sCampos = "";
                foreach (var coluna in lCampos)
                {
                    sCampos += (sCampos.Equals("") ? "" : ", ") + coluna;
                }
                command.CommandText = string.Format("SELECT "
                        + sCampos
                        + " FROM [{0}] ", this.sTable);
                OleDbDataReader reader = command.ExecuteReader();

                List<PlanilhaModel> lDados = new List<PlanilhaModel>();
                int idCount = 0;
                decimal dValida = 0;
                while (reader.Read())
                {
                    if (reader.GetValue(3).ToString() != "" && reader.GetValue(2).ToString() != "")
                    {

                        decimal.TryParse(reader.GetValue(3).ToString(), out dValida);
                        if (dValida > 0)
                        {
                            lDados.Add(new PlanilhaModel
                            {
                                PLANTA = reader.GetValue(0).ToString(),
                                //MAQUINA = reader.GetValue(1).ToString(),
                                TIPO = reader.GetValue(2).ToString(),
                                CALIBRE = reader.GetValue(3).ToString(),
                                CANTIDAD = reader.GetValue(4).ToString(),
                                TERM_IZQ = reader.GetValue(6).ToString(),
                                TERM_DER = reader.GetValue(8).ToString(),
                                COD_DI = reader.GetValue(5).ToString() != "2" ? "Y" : "2",
                                COD_DD = reader.GetValue(7).ToString() != "2" ? "Y" : "2",
                                COD_01_I = reader.GetValue(9).ToString(),
                                COD_01_D = reader.GetValue(10).ToString(),
                                ACC_01_I = (lSelosAtivos.Where(c => reader.GetValue(11).ToString().Contains(c)).Count() > 0) ? reader.GetValue(11).ToString() : "",
                                ACC_01_D = (lSelosAtivos.Where(c => reader.GetValue(12).ToString().Contains(c)).Count() > 0) ? reader.GetValue(12).ToString() : "",
                                idPLANILHA = idCount
                            });

                            idCount = idCount + 1;
                        }
                    }
                }


                //int iCount = lDados.Count();
                //var teste = lDados.Where(c => c.ACC_01_I == "7157-3035-60");

                //teste = lDados.Where(c => c.ACC_01_I == "7157-3035-60" && c.CALIBRE == "0.35");

                //teste = lDados.Where(c => c.ACC_01_I == "7157-3035-60" && c.CALIBRE == "0.5");


                var dadosAgrupados = Util.GroupList(lDados);

                //teste = dadosAgrupados.Where(c => c.ACC_01_I == "7157-3035-60");

                var dados = dadosAgrupados.Where(c => c.COD_DI == "2"
                                                 && c.COD_DD == "Y");

                foreach (var item in dados)
                {
                    Util.InverteLado(item);
                }
                return Util.GroupList(dadosAgrupados);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally { conn.CloseConexao(); }
        }

        public List<string> lCampos { get; set; }

        private string[] lSelosAtivos = { "7157", "7158", "7172" };

        public List<string> ValidaCamposPlanilha()
        {
            try
            {
                List<string> lCamposAnalise = new List<string>();

                lCamposAnalise.Add("PLANTA");
                lCamposAnalise.Add("MAQUINA");
                lCamposAnalise.Add("TIPO");
                lCamposAnalise.Add("CALIBRE");
                lCamposAnalise.Add("CANTIDAD");
                lCamposAnalise.Add("COD_DI");
                lCamposAnalise.Add("TERM_IZQ");
                lCamposAnalise.Add("COD_DD");
                lCamposAnalise.Add("TERM_DER");
                lCamposAnalise.Add("COD_01_I");
                lCamposAnalise.Add("COD_01_D");
                lCamposAnalise.Add("ACC_01_I");
                lCamposAnalise.Add("ACC_01_D");

                OleDbCommand command = new OleDbCommand(string.Format("SELECT * FROM [{0}A1:BA2] ", this.sTable), conn.OpenConexao());
                DataSet ds = new DataSet();
                OleDbDataAdapter da = new OleDbDataAdapter(command);
                da.Fill(ds);
                DataTable dtRet = ds.Tables[0];
                lCampos = new List<string>();
                List<string> lcamposInvalid = new List<string>();
                foreach (var coluna in lCamposAnalise)
                {
                    if (!dtRet.Columns.Contains(coluna))
                    {
                        lcamposInvalid.Add(coluna);
                        lCampos.Add("''as " + coluna);
                    }
                    else
                        lCampos.Add(coluna);
                }
                return lcamposInvalid;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally { conn.CloseConexao(); }
        }

        public bool SetFirstTable(string sParameter)
        {
            try
            {
                DataTable dt = new DataTable();
                dt = conn.OpenConexao().GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                List<string> lSheets = new List<string>();
                if (dt != null)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        lSheets.Add(row["TABLE_NAME"].ToString());
                    }
                }
                this.sTable = lSheets.Distinct().FirstOrDefault(c => c.ToUpper().Contains(sParameter.ToUpper()));
                if (this.sTable == "")
                    return false;
                else
                    return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



    }
}