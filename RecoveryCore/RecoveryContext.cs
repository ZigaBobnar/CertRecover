using System;
using System.IO;
using System.Numerics;

namespace RecoveryCore
{
    public class RecoveryContext : IRecoveryContext
    {
        private TextWriter _logger;
        private object _attemptLock = new { };


        public RecoveryContext(string logFilePath, string certPath)
        {
            LogFilePath = logFilePath;
            CertPath = certPath;

            if (!File.Exists(certPath))
            {
                Console.WriteLine("Certificate file does not exist.");
                throw new FileNotFoundException("Certificate file does not exist.");
            }

            CertValue = File.ReadAllBytes(CertPath);

            _logger = File.AppendText(LogFilePath);

            Log("--------------------\r\nStarting new password recovery");
        }

        public string LogFilePath { get; }

        public string CertPath { get; }

        public byte[] CertValue { get; private set; }

        public bool IsRecovered { get; set; } = false;

        public string RecoveredValue { get; set; } = null;

        public BigInteger AttemptNumber { get; private set; } = 0;


        public BigInteger IncreaseAttemptNumber()
        {
            lock (_attemptLock)
            {
                AttemptNumber++;

                return AttemptNumber;
            }
        }

        public void Log(string message)
        {
            _logger.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
            _logger.WriteLine($"    {message}");

            _logger.Flush();
        }

        public void LogAttempt(string value, BigInteger attemptNumber)
        {
            _logger.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
            _logger.WriteLine($"    Attempt #{attemptNumber} - {value}");

            _logger.Flush();
        }

        public void Dispose()
        {
            Log("--------------------\r\nExiting logger\r\n--------------------\r\n");

            _logger.Flush();
            _logger.Close();
            _logger.Dispose();
        }
    }
}
