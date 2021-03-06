using System;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace BethesdaSkillLab.Features.BethesdaSkillLab
{
    [Guid("560949b7-c3d2-49cd-ab21-074365f3d058")]
    public class BethesdaSkillLabEventReceiver : SPFeatureReceiver
    {
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            try
            {
                var site = (SPSite)properties.Feature.Parent;
                using (var web = site.OpenWeb())
                {
                    web.AllowUnsafeUpdates = true;

                    SPList skillLabConfiglist = null;
                    SPList skillLabList = null;
                    SPList calendar = null;

                    // Skill Lab list operations
                    try
                    {
                        skillLabList = web.Lists.TryGetList(Utilities.SkillLabListName);
                        if (skillLabList == null)
                        {
                            //creating new list
                            var listUID = web.Lists.Add(Utilities.SkillLabListName, string.Empty,
                                                        SPListTemplateType.GenericList);
                            skillLabList = web.Lists[listUID];
                            skillLabList.OnQuickLaunch = true;
                            skillLabList.Update();
                        }

                        // adding columns to the list
                        SPField textField;
                        if (!skillLabList.Fields.ContainsField(Utilities.SkillColumnName))
                        {
                            skillLabList.Fields.Add(Utilities.SkillColumnName, SPFieldType.Text, true);
                            textField = skillLabList.Fields[Utilities.SkillColumnName];
                            textField.StaticName = Utilities.SkillColumnName;
                            textField.Update();
                            AddFieldOnView(skillLabList, skillLabList.Fields[Utilities.SkillColumnName]);
                        }

                        SPFieldUser studentUserField = null;
                        if (!skillLabList.Fields.ContainsField(Utilities.StudentColumnName))
                        {
                            skillLabList.Fields.Add(Utilities.StudentColumnName, SPFieldType.User, true);
                            studentUserField = (SPFieldUser)skillLabList.Fields[Utilities.StudentColumnName];
                            if (studentUserField != null)
                            {
                                studentUserField.AllowMultipleValues = false;
                                studentUserField.Presence = true;
                                studentUserField.SelectionMode = SPFieldUserSelectionMode.PeopleOnly;
                                studentUserField.StaticName = Utilities.StudentColumnName;
                                studentUserField.Update();
                            }
                            AddFieldOnView(skillLabList, skillLabList.Fields[Utilities.StudentColumnName]);
                        }
                        else
                            studentUserField = (SPFieldUser)skillLabList.Fields[Utilities.StudentColumnName];

                        SPFieldDateTime scheduleDateField = null;
                        if (!skillLabList.Fields.ContainsField(Utilities.ScheduleDateColumnName))
                        {
                            skillLabList.Fields.Add(Utilities.ScheduleDateColumnName, SPFieldType.DateTime, true);
                            scheduleDateField = (SPFieldDateTime)skillLabList.Fields[Utilities.ScheduleDateColumnName];
                            if (scheduleDateField != null)
                            {
                                scheduleDateField.DisplayFormat = SPDateTimeFieldFormatType.DateOnly;
                                scheduleDateField.StaticName = Utilities.ScheduleDateColumnName;
                                scheduleDateField.Update();
                            }
                            AddFieldOnView(skillLabList, skillLabList.Fields[Utilities.ScheduleDateColumnName]);
                        }
                        else
                            scheduleDateField = (SPFieldDateTime)skillLabList.Fields[Utilities.ScheduleDateColumnName];

                        if (!skillLabList.Fields.ContainsField(Utilities.TimeColumnName))
                        {
                            skillLabList.Fields.Add(Utilities.TimeColumnName, SPFieldType.Text, true);
                            textField = skillLabList.Fields[Utilities.TimeColumnName];
                            textField.StaticName = Utilities.TimeColumnName;
                            textField.Update();
                            AddFieldOnView(skillLabList, skillLabList.Fields[Utilities.TimeColumnName]);
                        }

                        //try
                        //{
                        // creating the view for students
                        //var viewFound =skillLabList.Views.Cast<SPView>().Any(spView =>spView.Title.ToLower().Trim() == Utilities.StudentsViewName.ToLower().Trim());
                        //if (!viewFound)
                        //{
                        //var stringCollection = new StringCollection
                        //                           {
                        //                               "Title",
                        //                               Utilities.SkillColumnName,
                        //                               Utilities.ScheduleDateColumnName,
                        //                               Utilities.TimeColumnName
                        //                           };
                        //if (scheduleDateField != null && studentUserField != null)
                        //{
                        //                                    string query = @"
                        //                                <Where>
                        //                                    <And>
                        //                                        <Gt>
                        //                                            <FieldRef Name='" +
                        //                                                   scheduleDateField.InternalName + @"' />
                        //                                            <Value IncludeTimeValue='FALSE' Type='DateTime'>[Today]</Value>
                        //                                        </Gt>
                        //                                        <Eq>
                        //                                            <FieldRef Name='" +
                        //                                                   studentUserField.InternalName + @"' /><Value Type='User'>[Me]</Value>
                        //                                        </Eq>
                        //                                    </And>
                        //                                </Where>";
                        //skillLabList.Views.Add(Utilities.StudentsViewName, stringCollection, query, 50, true,true);
                        //}
                        //}

                        // finally update the list for saving view
                        //skillLabList.Update();
                        //}
                        //catch (Exception ex)
                        //{
                        //    SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Medium, EventSeverity.Information), TraceSeverity.Medium, ex.Message);
                        //}
                    }
                    catch (Exception ex)
                    {
                        SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Medium, EventSeverity.Information), TraceSeverity.Medium, ex.Message);
                    }

                    try
                    {
                        // now need to create skill lab config list
                        skillLabConfiglist = web.Lists.TryGetList(Utilities.SkillLabConfigListName);
                        if (skillLabConfiglist == null)
                        {
                            //create list first
                            var listUid = web.Lists.Add(Utilities.SkillLabConfigListName, string.Empty,
                                                        SPListTemplateType.GenericList);
                            skillLabConfiglist = web.Lists[listUid];
                            skillLabConfiglist.OnQuickLaunch = true;
                            skillLabConfiglist.Update();
                        }

                        // adding fields here
                        if (!skillLabConfiglist.Fields.ContainsField(Utilities.SkillColumnName))
                        {
                            var titleField = (SPFieldText)skillLabConfiglist.Fields["Title"];
                            titleField.StaticName = "Skill";
                            titleField.Update();
                        }

                        SPFieldDateTime dateField = null;
                        if (!skillLabConfiglist.Fields.ContainsField(Utilities.StartDateColumnName))
                        {
                            skillLabConfiglist.Fields.Add(Utilities.StartDateColumnName, SPFieldType.DateTime, true);
                            dateField = (SPFieldDateTime)skillLabConfiglist.Fields[Utilities.StartDateColumnName];
                            dateField.DisplayFormat = SPDateTimeFieldFormatType.DateOnly;
                            dateField.StaticName = Utilities.StartDateColumnName;
                            dateField.Update();
                            AddFieldOnView(skillLabConfiglist, dateField);
                        }

                        if (!skillLabConfiglist.Fields.ContainsField(Utilities.EndDateColumnName))
                        {
                            skillLabConfiglist.Fields.Add(Utilities.EndDateColumnName, SPFieldType.DateTime, true);
                            dateField = (SPFieldDateTime)skillLabConfiglist.Fields[Utilities.EndDateColumnName];
                            dateField.DisplayFormat = SPDateTimeFieldFormatType.DateOnly;
                            dateField.StaticName = Utilities.EndDateColumnName;
                            dateField.Update();
                            AddFieldOnView(skillLabConfiglist, dateField);
                        }

                        /*if (!skillLabConfiglist.Fields.ContainsField(Utilities.TimeSlotColumnName))
                        {
                            skillLabConfiglist.Fields.Add(Utilities.TimeSlotColumnName, SPFieldType.Text, true);
                            textField = skillLabConfiglist.Fields[Utilities.TimeSlotColumnName];
                            textField.StaticName = Utilities.TimeSlotColumnName;
                            textField.Update();
                            AddFieldOnView(skillLabConfiglist, skillLabConfiglist.Fields[Utilities.TimeSlotColumnName]);
                        }*/

                        if (!skillLabConfiglist.Fields.ContainsField(Utilities.TimeSlotStartTimeColumnName))
                        {
                            skillLabConfiglist.Fields.Add(Utilities.TimeSlotStartTimeColumnName, SPFieldType.Choice,
                                                          true);
                            var choiceField =
                                (SPFieldChoice)skillLabConfiglist.Fields[Utilities.TimeSlotStartTimeColumnName];
                            choiceField.StaticName = Utilities.TimeSlotStartTimeColumnName;
                            choiceField.EditFormat = SPChoiceFormatType.Dropdown;
                            for (int time = 9; time < 13; time++)
                                choiceField.Choices.Add(time + " AM");
                            for (int time = 1; time < 6; time++)
                                choiceField.Choices.Add(time + " PM");
                            choiceField.Update();
                            AddFieldOnView(skillLabConfiglist,
                                           skillLabConfiglist.Fields[Utilities.TimeSlotStartTimeColumnName]);
                        }

                        if (!skillLabConfiglist.Fields.ContainsField(Utilities.TimeSlotEndTimeColumnName))
                        {
                            skillLabConfiglist.Fields.Add(Utilities.TimeSlotEndTimeColumnName, SPFieldType.Choice, true);
                            var choiceField =
                                (SPFieldChoice)skillLabConfiglist.Fields[Utilities.TimeSlotEndTimeColumnName];
                            choiceField.StaticName = Utilities.TimeSlotEndTimeColumnName;
                            choiceField.EditFormat = SPChoiceFormatType.Dropdown;
                            for (int time = 9; time < 13; time++)
                                choiceField.Choices.Add(time + " AM");
                            for (int time = 1; time < 6; time++)
                                choiceField.Choices.Add(time + " PM");
                            choiceField.Update();
                            AddFieldOnView(skillLabConfiglist,
                                           skillLabConfiglist.Fields[Utilities.TimeSlotEndTimeColumnName]);
                        }

                        skillLabConfiglist.Update();
                    }
                    catch (Exception ex)
                    {
                        SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Medium, EventSeverity.Information), TraceSeverity.Medium, ex.Message);
                    }

                    // creating user groups here
                    try
                    {
                        web.SiteGroups.Add(Utilities.StudentsGroupName, web.AllUsers[0], web.AllUsers[0], string.Empty);
                        web.SiteGroups.Add(Utilities.FacultyGroupName, web.AllUsers[0], web.AllUsers[0], string.Empty);
                        web.Update();
                    }
                    catch (Exception ex)
                    {
                        SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Medium, EventSeverity.Information), TraceSeverity.Medium, ex.Message);
                    }

                    // Creating Calendar and a view for viewing schedules
                    try
                    {
                        calendar = web.Lists.TryGetList(Utilities.CalendarListName);
                        if (calendar == null)
                        {
                            var uid = web.Lists.Add(Utilities.CalendarListName, string.Empty, SPListTemplateType.Events);
                            calendar = web.Lists[uid];
                            calendar.Fields.Add(Utilities.EventOwnerColumnName, SPFieldType.User, true);
                            var userField = (SPFieldUser)calendar.Fields[Utilities.EventOwnerColumnName];
                            userField.AllowMultipleValues = false;
                            userField.SelectionMode = SPFieldUserSelectionMode.PeopleOnly;
                            userField.Update();
                            calendar.OnQuickLaunch = true;
                            calendar.Update();

                            // creating a view for users
                            var viewFound = calendar.Views.Cast<SPView>().Any(spView => spView.Title.ToLower().Trim() == Utilities.StudentsViewName.ToLower().Trim());
                            if (!viewFound)
                            {
                                string query = @"
                                <Where>
                                        <Eq>
                                            <FieldRef Name='" + userField.InternalName + @"' /><Value Type='User'>[Me]</Value>
                                        </Eq>
                                </Where>";
                                SPView view = calendar.Views.Add(Utilities.StudentsViewName, calendar.DefaultView.ViewFields.ToStringCollection(), query, 50, true, true, SPViewCollection.SPViewType.Calendar, false);
                                view.Update();
                            }

                            // finally updating the list
                            calendar.Update();
                        }
                    }
                    catch (Exception ex)
                    {
                        SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Medium, EventSeverity.Information), TraceSeverity.Medium, ex.Message);
                    }

                    // modifying list permissions here
                    try
                    {
                        ModifyListPermissions(skillLabList);
                        ModifyListPermissions(skillLabConfiglist);
                        ModifyListPermissions(calendar);
                    }
                    catch (Exception ex)
                    {
                        SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Medium, EventSeverity.Information), TraceSeverity.Medium, ex.Message);
                    }

                    web.AllowUnsafeUpdates = false;
                }
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
            }

            // creating timer job for notification sending
            try
            {
                var site = (SPSite)properties.Feature.Parent;
                bool timerJobFound = site.WebApplication.JobDefinitions.Any(jobDefinition => jobDefinition.Title == Utilities.TimerJobName);
                if (!timerJobFound)
                {
                    var notificationJob = new NotificationTimerJob(Utilities.TimerJobName, site.WebApplication);
                    var dailySchedule = new SPDailySchedule
                                            {
                                                BeginHour = 0,
                                                BeginMinute = 0,
                                                BeginSecond = 0,
                                                EndHour = 1,
                                                EndMinute = 59,
                                                EndSecond = 59
                                            };
                    notificationJob.Schedule = dailySchedule;
                    notificationJob.Update();
                }
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
            }
        }

        // Uncomment the method below to handle the event raised before a feature is deactivated.
        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            // removing notification timerjob here
            try
            {
                var site = (SPSite)properties.Feature.Parent;
                foreach (SPJobDefinition spJobDefinition in site.WebApplication.JobDefinitions)
                {
                    if (spJobDefinition.Title == Utilities.TimerJobName)
                    {
                        spJobDefinition.Delete();
                        break;
                    }
                }
                site.WebApplication.Update();
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
            }
        }

        private void ModifyListPermissions(SPList list)
        {
            // Modifying the permissions for the list to restrict the students for direct list items operations
            try
            {
                if (!list.HasUniqueRoleAssignments)
                {
                    var web = list.ParentWeb;

                    list.BreakRoleInheritance(false, false);
                    var group = web.SiteGroups[Utilities.StudentsGroupName];
                    var roleAssignment = new SPRoleAssignment(group);
                    SPRoleDefinition spRole = web.RoleDefinitions["Read"];
                    roleAssignment.RoleDefinitionBindings.Add(spRole);
                    list.RoleAssignments.Add(roleAssignment);

                    group = web.SiteGroups[Utilities.FacultyGroupName];
                    roleAssignment = new SPRoleAssignment(group);
                    spRole = web.RoleDefinitions["Full Control"];
                    roleAssignment.RoleDefinitionBindings.Add(spRole);
                    list.RoleAssignments.Add(roleAssignment);

                    list.Update();
                }
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
            }
        }

        private static void AddFieldOnView(SPList list, SPField spField)
        {
            try
            {
                list.DefaultView.ViewFields.Add(spField);
                list.DefaultView.Update();
                for (int i = 0; i < list.Views.Count; i++)
                {
                    SPView view = list.Views[i];
                    view.ViewFields.Add(spField);
                    view.Update();
                }
                list.Update();
            }
            catch (Exception ex)
            {
                SPDiagnosticsService.Local.WriteTrace(0, new SPDiagnosticsCategory("BethesdaSkillLab", TraceSeverity.Monitorable, EventSeverity.Error), TraceSeverity.Monitorable, ex.Message, new object[] { ex.StackTrace });
            }
        }
    }
}