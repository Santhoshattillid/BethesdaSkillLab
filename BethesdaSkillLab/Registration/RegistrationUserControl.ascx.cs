using System;
using System.Web.UI;
using Microsoft.Office.Server.UserProfiles;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace BethesdaSkillLab.Registration
{
    public partial class RegistrationUserControl : UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack && SPContext.Current != null)
                {
                    Txtname.Text = SPContext.Current.Web.CurrentUser.Name;
                    Txtmail.Text = SPContext.Current.Web.CurrentUser.Email;
                    SPSecurity.RunWithElevatedPrivileges(delegate
                                                             {
                                                                 using (var site = new SPSite(SPContext.Current.Site.Url))
                                                                 {
                                                                     var context = SPServiceContext.GetContext(site);
                                                                     var profileManager = new UserProfileManager(context);
                                                                     var userProfile =
                                                                         profileManager.GetUserProfile(
                                                                             SPContext.Current.Web.CurrentUser.LoginName);
                                                                     TxtContact.Text = userProfile.Properties.GetPropertyByName(PropertyConstants.WorkPhone) != null ? userProfile[PropertyConstants.WorkPhone].Value.ToString() : string.Empty;
                                                                 }
                                                             });
                    TxtContact.Text = SPContext.Current.Web.CurrentUser.Name;
                }
            }
            catch (Exception ex)
            {
                LblError.Text = ex.Message;
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
            }
        }

        protected void BtnRegister_Click(object sender, EventArgs e)
        {
        }
    }
}