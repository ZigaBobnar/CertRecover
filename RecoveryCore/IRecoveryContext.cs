using System;
using System.Numerics;

namespace RecoveryCore
{
    public interface IRecoveryContext : IDisposable
    {
        string LogFilePath { get; }

        string CertPath { get; }

        byte[] CertValue { get; }

        bool IsRecovered { get; set; }

        string RecoveredValue { get; set; }

        BigInteger AttemptNumber { get; }


        public BigInteger IncreaseAttemptNumber();
        public void Log(string message);

        public void LogAttempt(string value, BigInteger attemptNumber);
    }
}
