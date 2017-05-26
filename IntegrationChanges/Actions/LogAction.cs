using System;
using System.Collections.Generic;
using System.Web;
using nz.govt.moe.idp.saml.client.Actions;
using nz.govt.moe.idp.saml.client.identity;
using nz.govt.moe.idp.saml.client.Logging;
using nz.govt.moe.idp.saml.client.protocol;

namespace CurriculumTool.Actions
{
    public class LogAction: IAction
    {
        #region IAction Members

        public void LoginAction(nz.govt.moe.idp.saml.client.protocol.AbstractEndpointHandler handler, HttpContext context, nz.govt.moe.idp.saml.client.Saml20Assertion assertion)
        {
            // Since FormsAuthentication is used in this sample, the user name to log can be found in context.User.Identity.Name.
            // This user will not be set until after a new redirect, so unfortunately we cannot just log it here,
            // but will have to do in MyPage.Load in order to log the local user id
        }

        public void LogoutAction(nz.govt.moe.idp.saml.client.protocol.AbstractEndpointHandler handler, HttpContext context, bool IdPInitiated)
        {
            // Example of logging required by the requirements SLO1 ("Id of internal user account")
            // Since FormsAuthentication is used in this sample, the user name to log can be found in context.User.Identity.Name
            // The login will be not be cleared until next redirect due to the way FormsAuthentication works, so we will have to check Saml20Identity.IsInitialized() too
            AuditLogging.logEntry(Direction.IN, Operation.LOGOUT, "ServiceProvider logout",
                "SP local user id: " + (context.User.Identity.IsAuthenticated ? context.User.Identity.Name : "none") + " login status: " + Saml20Identity.IsInitialized());
        }

        /// <summary>
        /// <see cref="IAction.SoapLogoutAction"/>
        /// </summary>
        public void SoapLogoutAction(AbstractEndpointHandler handler, HttpContext context, string userId)
        {
            AuditLogging.logEntry(Direction.IN, Operation.LOGOUT, "ServiceProvider SOAP logout",
                "IdP user id: " + userId + " login status: " + Saml20Identity.IsInitialized());
        }

        public string Name
        {
            get { return "LogAction"; }
            set
            {
                
            }
        }

        #endregion
    }
}
