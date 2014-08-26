using HLP.OrganizePlanilha.UI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HLP.OrganizePlanilha.UI.Web.Controllers
{
    public class BaseController : Controller
    {
        public ProjetoModel SessionProjetoModel
        {
            get { return Session["objProjeto"] != null ? Session["objProjeto"] as ProjetoModel : new ProjetoModel(); }
            set { Session.Add("objProjeto", value); }
        }

                
        public string aviso
        {
            set
            {
                if (TempData.Keys.Contains("Aviso"))
                {
                    TempData["Aviso"] = value;
                }
                else
                {
                    TempData.Add("Aviso", value);
                }
            }
        }
    }
}
