using emp_ferias.lib.Interfaces;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace emp_ferias.Services
{
    public class ServiceLogin : IServiceLogin
    {
        public string GetUserID()
        {
            return HttpContext.Current.User.Identity.GetUserId();
        }
    }
}