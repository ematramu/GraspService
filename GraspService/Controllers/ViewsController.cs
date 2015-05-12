using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using log4net;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using GraspService.DBUtility;

namespace GraspService.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class ViewsController : ApiController
    {
        private static ILog log = LogManager.GetLogger(typeof(ViewsController));

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

                newView.TextHeader = "CREATE VIEW [" + view.Name.Trim() + "] AS";
                newView.TextBody = view.SqlScript;

                newView.Create();                
            }
        }

        /// <summary>
        /// GET Method for checking if view exist in the DB
        /// </summary>
        /// <param name="viewName"></param>
        /// <returns>bool that indicates if view exist or not</returns>
        [AcceptVerbs("GET")]
        [ResponseType(typeof(bool))]
        public bool existView(string viewName) {
            bool exist = false;

            log.Debug("INIZIO existView");

            if (string.IsNullOrEmpty(viewName)) {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(string.Format("One or more argument are missing")),
                    ReasonPhrase = "One or more argument are missing"
                };

                throw new HttpResponseException(resp);
            }

            List<SqlParameter> parms = new List<SqlParameter>();
            SqlParameter viewNameParm = new SqlParameter("@NAMEVIEW", SqlDbType.VarChar);
            viewNameParm.Value = viewName.Trim();
            parms.Add(viewNameParm);

            try
            {
                string query = "SELECT * FROM sys.views where name = @NAMEVIEW";
                using (SqlDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnectionString(ConfigurationManager.AppSettings["dbName"]), CommandType.Text, query, parms.ToArray()))
                {
                    if (rdr.Read() && rdr.HasRows)
                    {
                        exist = true;
                    }
                }
            }
            catch (SqlException ex)
            {
                log.Error(ex.Message, ex);

                var resp = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(string.Format(ex.Message)),
                    ReasonPhrase = ex.Message
                };

                throw new HttpResponseException(resp);
            }

            log.Debug("FINE existView");

            return exist;
        }
    }
}
