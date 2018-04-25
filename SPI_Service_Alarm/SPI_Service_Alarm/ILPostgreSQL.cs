using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPI_Service_Alarm.Model;
using Npgsql;
using System.Configuration;
using System.Data;
using Dapper;
using System.Reflection;
using log4net;

namespace SPI_Service_Alarm
{
    class ILPostgreSQL
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public IEnumerable<TagIL> SelectDB(List<Tag>tagsList)
        {
            try
            {
                string sSql = "SELECT \"TagValue\",\"ChangedAt\",\"TagName\" ";
                sSql = sSql + " FROM \"SPI_TB_IL_ADDRESS\" ";

                bool firstFilter = true;
                foreach(var tag in tagsList)
                {
                    if (firstFilter)
                    {
                        sSql = sSql + " WHERE Lower(\"TagName\") = '" + tag.physicalTag + "'";
                        firstFilter = false;
                    }
                    else
                        sSql = sSql + " or Lower(\"TagName\") = '" + tag.physicalTag + "'";
                }
                
                IEnumerable<TagIL> tagIlList;
                using (IDbConnection db = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
                {

                    tagIlList = db.Query<TagIL>(sSql);
                }


                return tagIlList;
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return null;
            }
        }
    }
}
