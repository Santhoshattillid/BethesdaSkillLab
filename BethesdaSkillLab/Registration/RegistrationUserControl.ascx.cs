using System;
using System.Collections;
using System.Web.UI;
using Microsoft.Office.Server.UserProfiles;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;

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
                                                                     TxtContact.Text = userProfile.Properties.GetPropertyByName(PropertyConstants.WorkPhone) != null && userProfile[PropertyConstants.WorkPhone].Value != null ? userProfile[PropertyConstants.WorkPhone].Value.ToString() : "00000000000";
                                                                 }
                                                             });

                    // addding dates here
                    DdlDates.Items.Add("Select date");
                    var date = DateTime.Now;
                    for (int i = 0; i < 30; i++)
                    {
                        date = date.AddDays(1);
                        DdlDates.Items.Add(date.ToString(Utilities.DateFormatString));
                    }

                    DdlSkill.Items.Clear();
                    DdlSkill.Items.Add("Skill1");
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
            try
            {
                if (SPContext.Current != null)
                {
                    // validation starts here
                    if (DdlSkill.SelectedIndex < 0)
                    {
                        LblError.Text = "Please select skill and fill all other fields.";
                        return;
                    }

                    if (DdlDates.SelectedIndex < 0)
                    {
                        LblError.Text = "Please select date for registration";
                        return;
                    }

                    if (DdlTime.SelectedIndex < 0)
                    {
                        LblError.Text = "Please select time slot for registration";
                        return;
                    }

                    var selectedDate = Convert.ToDateTime(DdlDates.SelectedValue);
                    var convertedDate = SPUtility.CreateISO8601DateTimeFromSystemDateTime(selectedDate);
                    SPSecurity.RunWithElevatedPrivileges(delegate
                    {
                        using (var site = new SPSite(SPContext.Current.Site.Url))
                        {
                            using (var web = site.OpenWeb())
                            {
                                var list = web.Lists.TryGetList(Utilities.SkillLabListName);
                                if (list != null)
                                {
                                    var query = new SPQuery
                                    {
                                        Query = @"<Where>
                                                        <And>
                                                            <Eq>
                                                                <FieldRef Name='Time' /><Value Type='Text'>" + DdlTime.SelectedValue + @"</Value>
                                                             </Eq>
                                                            <Eq>
                                                                <FieldRef Name='Schedule_x0020_Date' />
                                                                <Value IncludeTimeValue='FALSE' Type='DateTime'>" + convertedDate + @"</Value>
                                                            </Eq>
                                                         </And>
                                                       </Where>"
                                    };
                                    if (list.GetItems(query).Count == 0)
                                    {
                                        web.AllowUnsafeUpdates = true;
                                        var newItem = list.Items.Add();
                                        newItem["Title"] = "New registration";
                                        newItem[Utilities.SkillColumnName] = DdlSkill.SelectedValue;
                                        newItem[Utilities.StudentColumnName] = SPContext.Current.Web.CurrentUser;
                                        newItem[Utilities.ScheduleDateColumnName] = selectedDate;
                                        newItem[Utilities.TimeColumnName] = DdlTime.SelectedValue;
                                        newItem.Update();
                                        web.AllowUnsafeUpdates = false;
                                        LblError.Text = "Your slot has been registered successfully.";
                                    }
                                    else
                                        LblError.Text = "The selected slot is already registered, please try again.";
                                }
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                LblError.Text = ex.Message;
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
            }
        }

        protected void DdlDates_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (SPContext.Current != null)
                {
                    if (DdlDates.SelectedIndex > 0)
                    {
                        var selectedDate = Convert.ToDateTime(DdlDates.SelectedValue);
                        var convertedDate = SPUtility.CreateISO8601DateTimeFromSystemDateTime(selectedDate);
                        SPSecurity.RunWithElevatedPrivileges(delegate
                                                                 {
                                                                     using (
                                                                         var site =
                                                                             new SPSite(SPContext.Current.Site.Url))
                                                                     {
                                                                         using (var web = site.OpenWeb())
                                                                         {
                                                                             var list =
                                                                                 web.Lists.TryGetList(
                                                                                     Utilities.SkillLabListName);
                                                                             if (list != null)
                                                                             {
                                                                                 var query = new SPQuery
                                                                                                 {
                                                                                                     Query =
                                                                                                         @"<Where>
                                                                                                                 <Eq>
                                                                                                                    <FieldRef Name='Schedule_x0020_Date' />
                                                                                                                    <Value IncludeTimeValue='FALSE' Type='DateTime'>" + convertedDate + @"</Value>
                                                                                                                 </Eq>
                                                                                                           </Where>"
                                                                                                 };
                                                                                 var timeSlots = GetTimeSlots();
                                                                                 foreach (SPListItem listItem in list.GetItems(query))
                                                                                 {
                                                                                     if (timeSlots.ContainsKey(listItem[Utilities.TimeColumnName]))
                                                                                     {
                                                                                         timeSlots[
                                                                                             listItem[
                                                                                                 Utilities.
                                                                                                     TimeColumnName]] =
                                                                                             false;
                                                                                     }
                                                                                 }
                                                                                 DdlTime.Items.Clear();
                                                                                 foreach (
                                                                                     DictionaryEntry entry in timeSlots)
                                                                                 {
                                                                                     if (Convert.ToBoolean(entry.Value))
                                                                                         DdlTime.Items.Add(
                                                                                             entry.Key.ToString());
                                                                                 }

                                                                                 if (DdlTime.Items.Count == 0)
                                                                                     LblError.Text =
                                                                                         "There is no time slots available for this date, please select another.";
                                                                             }
                                                                         }
                                                                     }
                                                                 });
                    }
                    else
                        LblError.Text = "Please select date";
                }
            }
            catch (Exception ex)
            {
                LblError.Text = ex.Message;
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
            }
        }

        private Hashtable GetTimeSlots()
        {
            var table = new Hashtable { { "09 AM - 10 AM", true },
                                        { "10 AM - 11 AM", true },
                                        { "11 AM - 12 PM", true },
                                        { "12 PM - 01 AM", true },
                                        { "01 PM - 02 AM", true },
                                        { "02 PM - 03 AM", true },
                                        { "03 PM - 04 AM", true },
                                        { "04 PM - 05 AM", true },
            };
            return table;
        }

        protected void BtnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                var list = SPContext.Current.Web.Lists.TryGetList(Utilities.SkillLabListName);
                Response.Redirect(list != null ? list.DefaultViewUrl : SPContext.Current.Web.Url);
            }
            catch (Exception ex)
            {
                LblError.Text = ex.Message;
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
            }
        }
    }
}