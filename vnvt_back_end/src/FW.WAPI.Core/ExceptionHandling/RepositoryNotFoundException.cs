using System;

namespace FW.WAPI.Core.ExceptionHandling
{
    internal class RepositoryNotFoundException : Exception
    {
        public string RepositoryName { get; private set; }

        public RepositoryNotFoundException(string repositoryName, string message) : base(message)
        {
            if (string.IsNullOrWhiteSpace(repositoryName)) throw new ArgumentException($"{nameof(repositoryName)} cannot be null or empty.", nameof(repositoryName));
            RepositoryName = repositoryName;
        }
    }
}