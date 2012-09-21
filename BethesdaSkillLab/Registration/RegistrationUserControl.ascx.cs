using System;
using System.Collections;
using System.Collections.Generic;
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
                                                                             var startDate = list.Fields[Utilities.StartDateColumnName];
                                                                             var endDate = list.Fields[Utilities.EndDateColumnName];
                                                                             var query = new SPQuery
                                                                                             {
                                                                                                 Query = @" <Where>
                                                                                                          <Or>
                                                                                                             <Gt>
                                                                                                                <FieldRef Name='" + startDate.InternalName + @"' />
                                                                                                                <Value IncludeTimeValue='FALSE' Type='DateTime'>" + convertedDate + @"</Value>
                                                                                                             </Gt>
                                                                                                             <Gt>
                                                                                                                <FieldRef Name='" + endDate.InternalName + @"' />
                                                                                                                <Value IncludeTimeValue='FALSE' Type='DateTime'>" + convertedDate + @"</Value>
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
                LblError.Text += "<br/>";
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
            }
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
                LblError.Text += "<br/>";
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
            }
        }

        protected void DdlSkill_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                LblError.Text = string.Empty;
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
                                        var startDate = list.Fields[Utilities.StartDateColumnName];
                                        var endDate = list.Fields[Utilities.EndDateColumnName];
                                        var query = new SPQuery
                                        {
                                            Query =
                                                @"<Where>
                                                    <And>
                                                      <Or>
                                                         <Gt>
                                                            <FieldRef Name='" + startDate.InternalName + @"' />
                                                            <Value IncludeTimeValue='FALSE' Type='DateTime'>" + convertedDate + @"</Value>
                                                         </Gt>
                                                         <Gt>
                                                            <FieldRef Name='" + endDate.InternalName + @"' />
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
                                            {
                                                LblError.Text = "There is no slot defined for this skill";
                                                LblError.Text += "<br/>";
                                            }
                                        }
                                        else
                                        {
                                            LblError.Text = "There is no slot defined for this skill";
                                            LblError.Text += "<br/>";
                                        }
                                        DdlDates.SelectedIndex = 0;
                                    }
                                }
                            }
                        });
                    }
                    else
                    {
                        DdlDates.Items.Clear();
                        DdlTime.Items.Clear();
                        LblError.Text = "Please select skill.";
                        LblError.Text += "<br/>";
                    }
                }
            }
            catch (Exception ex)
            {
                LblError.Text = ex.Message;
                LblError.Text += "<br/>";
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
            }
        }

        protected void DdlDates_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (SPContext.Current != null)
                {
                    LblError.Text = string.Empty;
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
                                                                             var list = web.Lists.TryGetList(Utilities.SkillLabConfigListName);
                                                                             var skillLabList = web.Lists.TryGetList(Utilities.SkillLabListName);
                                                                             if (list != null && skillLabList != null)
                                                                             {
                                                                                 var startDate = list.Fields[Utilities.StartDateColumnName];
                                                                                 var endDate = list.Fields[Utilities.EndDateColumnName];
                                                                                 var query = new SPQuery
                                                                                                 {
                                                                                                     Query = @"<Where>
                                                                                                                    <And>
                                                                                                                      <Or>
                                                                                                                         <Gt>
                                                                                                                            <FieldRef Name='" + startDate.InternalName + @"' />
                                                                                                                            <Value IncludeTimeValue='FALSE' Type='DateTime'>" + convertedDate + @"</Value>
                                                                                                                         </Gt>
                                                                                                                         <Gt>
                                                                                                                            <FieldRef Name='" + endDate.InternalName + @"' />
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
                                                                                 DdlTime.Items.Add("Select time");
                                                                                 var timeSlots = new List<string>();
                                                                                 foreach (SPListItem listItem in list.GetItems(query))
                                                                                 {
                                                                                     string slot = listItem[Utilities.TimeSlotStartTimeColumnName].ToString();
                                                                                     slot += " - " + listItem[Utilities.TimeSlotEndTimeColumnName];
                                                                                     if (!timeSlots.Contains(slot))
                                                                                         timeSlots.Add(slot);
                                                                                 }
                                                                                 var scheduleDatefield = skillLabList.Fields[Utilities.ScheduleDateColumnName];
                                                                                 var timeField = skillLabList.Fields[Utilities.TimeColumnName];
                                                                                 foreach (string timeSlot in timeSlots)
                                                                                 {
                                                                                     query = new SPQuery
                                                                                     {
                                                                                         Query = @"<Where>
                                                                                                    <And>
                                                                                                        <Eq>
                                                                                                            <FieldRef Name='" + scheduleDatefield.InternalName + @"' />
                                                                                                            <Value IncludeTimeValue='FALSE' Type='DateTime'>" + convertedDate + @"</Value>
                                                                                                        </Eq>
                                                                                                        <Eq>
                                                                                                            <FieldRef Name='" + timeField.InternalName + @"' />
                                                                                                            <Value Type='Text'>" + timeSlot + @"</Value>
                                                                                                        </Eq>
                                                                                                    </And>
                                                                                                   </Where>"
                                                                                     };
                                                                                     if (skillLabList.GetItems(query).Count == 0)
                                                                                     {
                                                                                         DdlTime.Items.Add(timeSlot);
                                                                                     }
                                                                                 }
                                                                                 if (DdlTime.Items.Count == 1)
                                                                                 {
                                                                                     LblError.Text =
                                                                                         "There is no time slots available for this date, please select another.";
                                                                                     LblError.Text += "<br/>";
                                                                                 }
                                                                             }
                                                                         }
                                                                     }
                                                                 });
                    }
                    else
                    {
                        DdlTime.Items.Clear();
                        LblError.Text = "Please select date";
                        LblError.Text += "<br/>";
                    }
                }
            }
            catch (Exception ex)
            {
                LblError.Text = ex.Message;
                LblError.Text += "<br/>";
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
            }
        }

        protected void BtnRegister_Click(object sender, EventArgs e)
        {
            try
            {
                if (SPContext.Current != null)
                {
                    LblError.Text = string.Empty;

                    // validation starts here
                    if (DdlSkill.SelectedIndex < 1)
                    {
                        LblError.Text = "Please select skill and fill all other fields.";
                        LblError.Text += "<br/>";
                        return;
                    }

                    if (DdlDates.SelectedIndex < 1)
                    {
                        LblError.Text = "Please select date for registration";
                        LblError.Text += "<br/>";
                        return;
                    }

                    if (DdlTime.SelectedIndex < 1)
                    {
                        LblError.Text = "Please select time slot for registration";
                        LblError.Text += "<br/>";
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
                                    var time = list.Fields[Utilities.TimeColumnName];
                                    var scheduleDate = list.Fields[Utilities.ScheduleDateColumnName];
                                    var query = new SPQuery
                                    {
                                        Query = @"<Where>
                                                        <And>
                                                            <Eq>
                                                                <FieldRef Name='" + time.InternalName + @"' /><Value Type='Text'>" + DdlTime.SelectedValue + @"</Value>
                                                             </Eq>
                                                            <Eq>
                                                                <FieldRef Name='" + scheduleDate.InternalName + @"' />
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
                                        ModifyItemPermissions(newItem);

                                        // creating calendar event
                                        var calendarList = web.Lists.TryGetList(Utilities.CalendarListName);
                                        if (calendarList != null)
                                        {
                                            var times = DdlTime.SelectedValue.Split('-');
                                            if (times.Length > 1)
                                            {
                                                var newCalItem = calendarList.Items.Add();
                                                newCalItem["Title"] = DdlSkill.SelectedValue;
                                                newCalItem[Utilities.EventOwnerColumnName] = SPContext.Current.Web.CurrentUser;
                                                var date = selectedDate;
                                                date = date.AddHours(date.Hour * -1);
                                                date = date.AddMinutes(date.Minute * -1);
                                                date = date.AddSeconds(date.Second * -1);
                                                int hour = 0;

                                                if (times[0].IndexOf("AM", StringComparison.Ordinal) > 0)
                                                    hour = Convert.ToInt16(times[0].Trim().Replace("AM", ""));
                                                else if (times[0].IndexOf("PM", StringComparison.Ordinal) > 0)
                                                    hour = Convert.ToInt16(times[0].Trim().Replace("PM", "")) + 12;

                                                date = date.AddHours(hour);
                                                newCalItem["Start Time"] = date;
                                                date = date.AddHours(hour * -1);
                                                hour = 0;
                                                if (times[1].IndexOf("AM", StringComparison.Ordinal) > 0)
                                                    hour = Convert.ToInt16(times[1].Trim().Replace("AM", ""));
                                                else if (times[1].IndexOf("PM", StringComparison.Ordinal) > 0)
                                                    hour = Convert.ToInt16(times[1].Trim().Replace("PM", "")) + 12;
                                                date = date.AddHours(hour);
                                                newCalItem["End Time"] = date;
                                                newCalItem["Location"] = DdlSkill.SelectedValue;
                                                newCalItem["Category"] = "Skill test";
                                                newCalItem["fAllDayEvent"] = false;
                                                newCalItem.Update();
                                                ModifyItemPermissions(newCalItem);
                                            }
                                        }

                                        web.AllowUnsafeUpdates = false;
                                        LblError.Text = "Your slot has been registered successfully.";
                                        LblError.Text += "<br/>";

                                        // sending notification to faculty for skill lab registration
                                        if (SPUtility.IsEmailServerSet(web))
                                        {
                                            try
                                            {
                                                var skillLabConfigList = web.Lists.TryGetList(Utilities.SkillLabConfigListName);
                                                if (skillLabConfigList != null)
                                                {
                                                    var startDate = list.Fields[Utilities.StartDateColumnName];
                                                    var endDate = list.Fields[Utilities.EndDateColumnName];
                                                    query = new SPQuery
                                                                {
                                                                    Query =
                                                                        @"<Where>
                                                                            <And>
                                                                              <Or>
                                                                                 <Gt>
                                                                                    <FieldRef Name='" + startDate.InternalName + @"' />
                                                                                    <Value IncludeTimeValue='FALSE' Type='DateTime'>" +
                                                                                                convertedDate + @"</Value>
                                                                                 </Gt>
                                                                                 <Gt>
                                                                                    <FieldRef Name='" + endDate.InternalName + @"' />
                                                                                    <Value IncludeTimeValue='FALSE' Type='DateTime'>" +
                                                                                                convertedDate + @"</Value>
                                                                                 </Gt>
                                                                              </Or>
                                                                              <Eq>
                                                                                   <FieldRef Name='Title' />
                                                                                    <Value Type='Text'>" +
                                                                                                DdlSkill.SelectedValue + @"</Value>
                                                                              </Eq>
                                                                            </And>
                                                                           </Where>"
                                                                };

                                                    foreach (SPListItem listItem in skillLabConfigList.GetItems(query))
                                                    {
                                                        var createdBy = new SPFieldUserValue(web, listItem["Author"].ToString());
                                                        if (!string.IsNullOrEmpty(createdBy.User.Email))
                                                        {
                                                            var mailBody = "Here is the details of registration.";
                                                            mailBody += "</br> User: " + SPContext.Current.Web.CurrentUser.Name;
                                                            mailBody += "</br> Date: " + DdlDates.SelectedValue;
                                                            mailBody += "</br> Skill: " + DdlSkill.SelectedValue;
                                                            mailBody += "</br> Time-Slot: " + DdlTime.SelectedValue;
                                                            mailBody += "</br> Registered at: " + DateTime.Now.ToShortDateString();
                                                            SPUtility.SendEmail(web, true, true, createdBy.User.Email, "New skill lab registration.", mailBody);
                                                        }
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                // write the error to event log
                                                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
                                            }
                                        }
                                        else
                                        {
                                            //LblError.Text +="</br> The email notifications cannot be send due to settings not configured.";
                                            LblError.Text += "<br/>";
                                        }
                                    }
                                    else
                                    {
                                        LblError.Text = "The selected slot is already registered, please try again.";
                                        LblError.Text += "<br/>";
                                    }
                                }
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                LblError.Text = ex.Message;
                LblError.Text += "<br/>";
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
            }
        }

        private void ModifyItemPermissions(SPListItem listItem)
        {
            // Modifying the permissions for the list to restrict the students for direct list items operations
            try
            {
                if (!listItem.HasUniqueRoleAssignments)
                {
                    var web = listItem.ParentList.ParentWeb;
                    listItem.BreakRoleInheritance(false, false);

                    var group = web.SiteGroups[Utilities.FacultyGroupName];
                    var roleAssignment = new SPRoleAssignment(group);
                    var spRole = web.RoleDefinitions["Full Control"];
                    roleAssignment.RoleDefinitionBindings.Add(spRole);
                    listItem.RoleAssignments.Add(roleAssignment);

                    roleAssignment = new SPRoleAssignment(SPContext.Current.Web.CurrentUser);
                    spRole = web.RoleDefinitions["Read"];
                    roleAssignment.RoleDefinitionBindings.Add(spRole);
                    listItem.RoleAssignments.Add(roleAssignment);

                    listItem.Update();
                }
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
            }
        }
    }
}