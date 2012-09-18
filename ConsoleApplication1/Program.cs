using System;
using System.Collections.Specialized;
using Microsoft.SharePoint;

namespace ConsoleApplication1
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // this console code is for some testing
            using (var site = new SPSite("http://tspsrvr"))
            {
                using (var web = site.OpenWeb())
                {
                    web.AllowUnsafeUpdates = true;

                    /*
                    var uid = web.Lists.Add("test", string.Empty, SPListTemplateType.GenericList);
                    var testList = web.Lists[uid];
                    testList.OnQuickLaunch = true;
                    testList.Update();

                    testList.Fields.Add("test field", SPFieldType.Text, true);
                    var field = (SPFieldText)testList.Fields["test field"];
                    field.StaticName = "test field";
                    field.Update();
                     */

                    var list = web.Lists["SkillLab"];
                    var stringCollection = new StringCollection { "Title" };
                    const string query = @"
                                <Where>
                                    <And>
                                        <Gt>
                                            <FieldRef Name='Schedule_x0020_Date' />
                                            <Value IncludeTimeValue='FALSE' Type='DateTime'>[Today]</Value>
                                        </Gt>
                                        <Eq>
                                            <FieldRef Name='Student' /><Value Type='User'>[Me]</Value>
                                        </Eq>
                                    </And>
                                </Where>";
                    list.Views.Add("test", stringCollection, query, 50, true, true);
                    list.Update();
                    web.AllowUnsafeUpdates = false;
                }
            }
        }
    }
}