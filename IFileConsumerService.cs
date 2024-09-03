using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CP_FleetDataJob
{
    public interface IFileConsumerService
    {
        public Task<bool> ConsumeFile(string pathToFile); 
    }
}
