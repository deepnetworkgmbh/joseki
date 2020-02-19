namespace webapp.Audits.Processors.azsk
{
    /// <summary>
    /// Az-sk audit processor.
    /// </summary>
    public class AzskAuditProcessor : IAuditProcessor
    {
        /// <inheritdoc />
        public Task Process(ScannerContainer container, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}