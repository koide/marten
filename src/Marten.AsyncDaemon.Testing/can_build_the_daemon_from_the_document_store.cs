using Marten.Testing.Harness;
using Xunit;

namespace Marten.AsyncDaemon.Testing
{
    public class can_build_the_daemon_from_the_document_store : OneOffConfigurationsContext
    {
        [Fact]
        public void can_build_it_out_with_no_custom_logging()
        {
            using var daemon = theStore.BuildProjectionDaemon();
        }
    }
}
