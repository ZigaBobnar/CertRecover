using System;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;

namespace RecoveryCore
{
    public class RecoveryEngine
    {
        private readonly IRecoveryContext _context;
        private readonly IWordlistGenerator _wordlistGenerator;

        public RecoveryEngine(IRecoveryContext context, IWordlistGenerator wordlistGenerator)
        {
            _context = context;
            _wordlistGenerator = wordlistGenerator;
        }

        public void StartRecovery()
        {
            while (!_context.IsRecovered)
            {
                string password = _wordlistGenerator.GetNextWord();
                if (_wordlistGenerator.OutOfWords)
                {
                    Console.WriteLine($"Generator ran out of words");
                    _context.Log("Generator ran out of words");

                    return;
                }

                BigInteger attempt = _context.IncreaseAttemptNumber();

                try
                {
                    X509Certificate2 certificate = new X509Certificate2(_context.CertValue, password);

                    Console.WriteLine("Found password...");
                    _context.Log($"Password found: {password}");
                    _context.RecoveredValue = password;
                    _context.IsRecovered = true;
                }
                catch
                {
                    if (attempt % 1000 == 0 || attempt == 1)
                    {
                        Console.WriteLine($"Attempt #{attempt} failed: {password}");
                        _context.LogAttempt(password, attempt);
                        _context.Log($"Next word indexes: {string.Join(',', _wordlistGenerator.NextWordIndexes)}");
                    }
                }
            }
        }
    }
}
