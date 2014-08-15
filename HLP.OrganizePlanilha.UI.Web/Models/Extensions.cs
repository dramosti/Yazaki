using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HLP.OrganizePlanilha.UI.Web.Models
{
    public static class Extensions
    {

        public static decimal ToDecimal(this string valor)
        {
            try
            {
                decimal dValor;
                Decimal.TryParse(valor.Replace('.', ','), out dValor);
                return dValor;

            }
            catch (Exception ex)
            {
                return 0;
            }
        }
    }
}