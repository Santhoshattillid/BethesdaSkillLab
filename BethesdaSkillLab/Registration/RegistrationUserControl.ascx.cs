using System;
using System.Collections;
using System.Web.UI;
using System.Web.UI.WebControls;
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
                    var convertedDate = SPUtility.CreateISO8601DateTimeFromSystemDateTime(DateTime.Now.AddDays(1));
                    SPSecurity.RunWithElevatedPrivileges(delegate
                                                             {
                                                                 using (var site = new SPSite(SPContext.Current.Site.Url))
                                                                 {
                                                                     using (var web = site.OpenWeb())
                                                                     {
                                                                         var context = SPServiceContext.GetContext(site);
                                                                         var profileManager = new UserProfileManager(context);
                                                                         var userProfile = profileManager.GetUserProfile(SPContext.Current.Web.CurrentUser.LoginName);
                                                                         TxtContact.Text = userProfile.Properties.GetPropertyByName(PropertyConstants.WorkPhone) != null && userProfile[PropertyConstants.WorkPhone].Value != null ? userProfile[PropertyConstants.WorkPhone].Value.ToString() : "00000000000";

                                                                         // Loading skills here
                                                                         DdlSkill.Items.Clear();
                                                                         DdlSkill.Items.Add("Select Skill");
                                                                         var list =
                                                                             web.Lists.TryGetList(
                                                                                 Utilities.SkillLabConfigListName);
                                                                         if (list != null)
                                                                         {
                                                                             var query = new SPQuery
                                                                                             {
                                                                                                 Query = @" <Where>
                                                                                                          <Or>
                                                                                                             <Gt>
                                                                                                                <FieldRef Name='StartDate' />
                                                                                                                <Value IncludeTimeValue='TRUE' Type='DateTime'>" + convertedDate + @"</Value>
                                                                                                             </Gt>
                                                                                                             <Gt>
                                                                                                                <FieldRef Name='_EndDate' />
                                                                                                                <Value IncludeTimeValue='TRUE' Type='DateTime'>" + convertedDate + @"</Value>
                                                                                                             </Gt>
                                                                                                          </Or>
                                                                                                       </Where>"
                                                                                             };
                                                                             foreach (SPListItem listItem in list.GetItems(query))
                                                                             {
                                                                                 string skill =
                                                                                     listItem[Utilities.SkillColumnName]
                                                                                         .ToString();
                                                                                 if (!DdlSkill.Items.Contains(new ListItem(skill)))
                                                                                     DdlSkill.Items.Add(skill);
                                                                             }
                                                                         }
                                                                         DdlSkill.SelectedIndex = 0;
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
                                                                     using (var site = new SPSite(SPContext.Current.Site.Url))
                                                                     {
                                                                         using (var web = site.OpenWeb())
                                                                         {
                                                                             var list = web.Lists.TryGetList(Utilities.SkillLabListName);
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
                                                                                         timeSlots[listItem[Utilities.TimeColumnName]] = false;
                                                                                     }
                                                                                 }
                                                                                 DdlTime.Items.Clear();
                                                                                 foreach (DictionaryEntry entry in timeSlots)
                                                                                 {
                                                                                     if (Convert.ToBoolean(entry.Value))
                                                                                         DdlTime.Items.Add(entry.Key.ToString());
                                                                                 }
                                                                                 if (DdlTime.Items.Count == 0)
                                                                                     LblError.Text = "There is no time slots available for this date, please select another.";
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

        protected void DdlSkill_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (SPContext.Current != null)
                {
                    if (DdlSkill.SelectedIndex > 0)
                    {
                        var convertedDate = SPUtility.CreateISO8601DateTimeFromSystemDateTime(DateTime.Now);
                        SPSecurity.RunWithElevatedPrivileges(delegate
                        {
                            using (var site = new SPSite(SPContext.Current.Site.Url))
                            {
                                using (var web = site.OpenWeb())
                                {
                                    var list = web.Lists.TryGetList(Utilities.SkillLabConfigListName);
                                    if (list != null)
                                    {
                                        var query = new SPQuery
                                        {
                                            Query =
                                                @"<Where>
                                                    <And>
                                                      <Or>
                                                         <Gt>
                                                            <FieldRef Name='StartDate' />
                                                            <Value IncludeTimeValue='FALSE' Type='DateTime'>" + convertedDate + @"</Value>
                                                         </Gt>
                                                         <Gt>
                                                            <FieldRef Name='_EndDate' />
                                                            <Value IncludeTimeValue='FALSE' Type='DateTime'>" + convertedDate + @"</Value>
                                                         </Gt>
                                                      </Or>
                                                      <Eq>
                                                           <FieldRef Name='Title' />
                                                            <Value Type='Text'>" + DdlSkill.SelectedValue + @"</Value>
                                                      </Eq>
                                                    </And>
                                                   </Where>"
                                        };

                                        DdlTime.Items.Clear();
                                        DdlDates.Items.Clear();
                                        DdlDates.Items.Add("Select Date");
                                        var collection = list.GetItems(query);
                                        if (collection.Count > 0)
                                        {
                                            var minDate = Convert.ToDateTime(collection[0][Utilities.StartDateColumnName]);
                                            var maxDate = Convert.ToDateTime(collection[0][Utilities.EndDateColumnName]);
                                            foreach (SPListItem listItem in collection)
                                            {
                                                var value = Convert.ToDateTime(listItem[Utilities.StartDateColumnName]);
                                                if (value < minDate)
                                                    minDate = value;
                                                if (value > maxDate)
                                                    maxDate = value;
                                            }
                                            if (minDate < DateTime.Now.AddDays(1))
                                                minDate = DateTime.Now.AddDays(1);
                                            if (minDate < maxDate)
                                            {
                                                while (minDate <= maxDate)
                                                {
                                                    DdlDates.Items.Add(minDate.ToString(Utilities.DateFormatString));
                                                    minDate = minDate.AddDays(1);
                                                }
                                            }
                                            if (DdlDates.Items.Count == 1)
                                                LblError.Text = "There is no slot defined for this skill";
                                        }
                                        else
                                            LblError.Text = "There is no slot defined for this skill";
                                        DdlDates.SelectedIndex = 0;
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
    }
}