using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace GraspService.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class ViewsController : ApiController
    {
        // POST: api/View
        /// <summary>
        /// POST method for creating a new view on DB
        /// </summary>
        /// <param name="view"></param>
        [ResponseType(typeof(void))]
        public void PostView(GraspService.Models.View view)
        {
            if (!ModelState.IsValid)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(string.Format("One or more argument are missing")),
                    ReasonPhrase = "One or more argument are missing"
                };

                throw new HttpResponseException(resp);
            }
            else
            {
                string serverDB = ConfigurationManager.AppSettings["serverDB"];
                string dbName = ConfigurationManager.AppSettings["dbName"];
                string dbUser = ConfigurationManager.AppSettings["dbUser"];
                string dbPassword = ConfigurationManager.AppSettings["dbPassword"];

                Server srv = new Server(new ServerConnection(serverDB, dbUser, dbPassword));

                Database db = srv.Databases[dbName];

                View newView = new View(db, view.Name);
                
                newView.TextHeader = "CREATE VIEW [" + view.Name + "] AS";
                newView.TextBody = view.SqlScript;

                newView.Create();
            }
        }
    }
}
