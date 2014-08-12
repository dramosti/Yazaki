using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HLP.OrganizePlanilha.UI.Web.Models
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class Coluna : System.Attribute
    {
        public bool isColuna { get; set; }
    }
}