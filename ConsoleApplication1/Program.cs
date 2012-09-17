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

                    var uid = web.Lists.Add("test", string.Empty, SPListTemplateType.GenericList);
                    var testList = web.Lists[uid];
                    testList.OnQuickLaunch = true;
                    testList.Update();

                    testList.Fields.Add("test field", SPFieldType.Text, true);
                    var field = (SPFieldText)testList.Fields["test field"];
                    field.StaticName = "test field";
                    field.Update();

                    web.AllowUnsafeUpdates = false;
                }
            }
        }
    }
}