using Microsoft.EntityFrameworkCore;
using System.Text;

namespace CP_FleetDataJob
{
    public class FileConsumerService : IFileConsumerService
    {
        ILogger<FileConsumerService> _logger;
        FleetDbContext _dbContext;
        public Dictionary<char, char> Cypher { get; set; }

        public FileConsumerService(ILogger<FileConsumerService> logger, FleetDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;

            Cypher = new Dictionary<char, char>();
            Cypher.Add('!', '1');
            Cypher.Add('@', '2');
            Cypher.Add('#', '3');
            Cypher.Add('$', '4');
            Cypher.Add('&', '5');
            Cypher.Add('(', '6');
            Cypher.Add(')', '7');
            Cypher.Add('+', '8');
        }

        public async Task<bool> ConsumeFile(string pathToFile)
        {
            if (!File.Exists(pathToFile))
            {
                _logger.LogWarning($"File {pathToFile} does not exist.");
                return false;
            }

            _logger.LogInformation($"Starting read of {pathToFile}");
            var records = new List<FMSRecords>();

            try
            {
                using (StreamReader sr = new StreamReader(pathToFile))
                {
                    string? line;
                    int counter = 0;
                    while ((line = await sr.ReadLineAsync()) != null)
                    {
                        counter++;
                        if (counter < 3) continue;

                        _logger.LogDebug($"Reading Line {counter} of the file {pathToFile}");
                        string data = line.Substring(line.IndexOf(", ") + 2);
                        records.Add(await DecryptLine(data));
                    }
                }

                await pushToDB(records);
                _logger.LogInformation($"Completed reading and processing {pathToFile}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to consume file {pathToFile}");
                throw new Exception($"Failed to consume file {pathToFile}");
            }
        }


        public async Task<FMSRecords> DecryptLine(string Line)
        {
            StringBuilder sbLine = new StringBuilder();
            sbLine.Append(Line);

            foreach (KeyValuePair<char, char> dick in Cypher)
            {
                sbLine.Replace(dick.Key, dick.Value);
            }

            string TextLine = await HexToString(sbLine.ToString());

            TextLine = TextLine.Replace(';', ',');

            _logger.LogInformation($"Decrypted Line {TextLine}");

            string[] brokenRecord = TextLine.Split(',');

            FMSRecords record = new FMSRecords();
            record.DOC_HEADER = brokenRecord[0];
            record.DOC_NUMBER = brokenRecord[1];
            record.POSTING_DATE = brokenRecord[2];
            record.REFERENCE_NO = brokenRecord[3];
            record.DOCUMENT_DATE = brokenRecord[4];
            record.INVOICE_AMOUNT = brokenRecord[5];
            record.LoadedBy = 131; // Need to change

            return record;

        }
        public async Task<string> HexToString(string hex)
        {
            try
            {
                var bytes = new byte[hex.Length / 2];
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
                }
                return Encoding.UTF8.GetString(bytes);
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Invalid hex string encountered.");
                throw new FormatException("Invalid hex string encountered.");
            }
        }


        public async Task<bool> pushToDB(List<FMSRecords> records)
        {
            try
            {
                foreach (FMSRecords record in records)
                {

                    var existingRecords = await _dbContext.HistoryPersonalVehicleExpensesSAPData.Where(h => h.DOC_HEADER == record.DOC_HEADER).ToListAsync();
                    if (existingRecords.Any())
                    {
                        _dbContext.RemoveRange(existingRecords);
                        await _dbContext.SaveChangesAsync();

                    }
                    await _dbContext.HistoryPersonalVehicleExpensesSAPData.AddAsync(record);
                    await _dbContext.SaveChangesAsync();
                }
                
                //await _dbContext.HistoryPersonalVehicleExpensesSAPData.AddRangeAsync(records);
                //await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving records to the database.");
                throw new Exception($"Error saving records to the database. {ex.Message}");
            }
        }
    }
}
