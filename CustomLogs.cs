using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CP_FleetDataJob
{
    public class CustomLogs
    {
        FleetDbContext db;

        public async Task LogError(string Message, string FileName = "", string LogType = "Error")
        {
            db = new FleetDbContext();

            Log log = new Log();
            log.Message = Message;
            log.FileName = FileName;
            log.LogType = LogType;
            await db.Logs.AddAsync(log);
            await db.SaveChangesAsync();
        }
    }


}
