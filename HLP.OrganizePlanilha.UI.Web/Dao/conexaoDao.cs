using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Web;

namespace HLP.OrganizePlanilha.UI.Web.Dao
{
    public sealed class conexaoDao
    {
        private OleDbConnection _olecon;
        private String _fileLocation = "";
        private String _stringConexao = string.Empty;


        public conexaoDao(String _fileLocation)
        {
            this._fileLocation = _fileLocation;
            FileInfo info = new FileInfo(_fileLocation);

            _stringConexao = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + _fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";

            if (info.Extension == ".xls")
            {
                _stringConexao = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + _fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
            }
            else if (info.Extension == ".xlsx")
            {
                _stringConexao = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + _fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
            }

            this._olecon = new OleDbConnection(string.Format(_stringConexao, _fileLocation));
        }


        public OleDbConnection OpenConexao()
        {
            if (_olecon.State == System.Data.ConnectionState.Closed)
            {
                _olecon.Open();
            }
            return _olecon;
        }

        public void CloseConexao()
        {
            if (_olecon.State == System.Data.ConnectionState.Open)
            {
                _olecon.Close();
            }
        }
    }
}