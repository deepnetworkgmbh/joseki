using System;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using webapp.Audits.Processors.trivy;

namespace tests
{
    [TestClass]
    public class TrivyScanDescriptionNormalizerTests
    {
        [TestMethod]
        public void NotAuthorized()
        {
            var sampleNotAuthorized = @"
\u001b[0m\terror in image scan: failed to analyze image: failed to extract files: failed to create the registry client: Get https://aksrepos.azurecr.io/v2/: http: non-successful response (status=401 body=""{\""errors\"":[{\""code\"":\""UNAUTHORIZED\"",\""message\"":\""authentication required\"",\""detail\"":null}]}\n"")";

            var actualResponse = TrivyScanDescriptionNormalizer.ToHumanReadable(sampleNotAuthorized);

            actualResponse.Should().Be(TrivyScanDescriptionNormalizer.NotAuthorized);
        }

        [TestMethod]
        public void UnknownOS()
        {
            var sampleNotAuthorized = @"\u001b[0m\terror in image scan: failed to scan image: failed to analyze OS: Unknown OS";

            var actualResponse = TrivyScanDescriptionNormalizer.ToHumanReadable(sampleNotAuthorized);

            actualResponse.Should().Be(TrivyScanDescriptionNormalizer.UnknownOS);
        }

        [TestMethod]
        public void FailedToGetPackages()
        {
            var sampleNotAuthorized = @"\u001b[0m\terror in image scan: failed analysis: analyze error: failed to analyze layer: sha256:d6caf8e15a64c3d070d838348e7dc70d065bd7bcab2077d133345aa2c74b60bb : failed to get packages: failed to get packages: failed to parse the pkg info: no rpm command";

            var actualResponse = TrivyScanDescriptionNormalizer.ToHumanReadable(sampleNotAuthorized);

            actualResponse.Should().Be(TrivyScanDescriptionNormalizer.FailedToGetPackages);
        }

        [TestMethod]
        public void UnknownError()
        {
            var sampleNotAuthorized = @"\u001b[0m\terror in image scan: failed to analyze image: failed to extract files: failed to extract files: failed to extract the archive: unexpected EOF";

            var actualResponse = TrivyScanDescriptionNormalizer.ToHumanReadable(sampleNotAuthorized);

            actualResponse.Should().Be(TrivyScanDescriptionNormalizer.UnknownError);
        }

        [TestMethod]
        public void RandomText()
        {
            var sampleNotAuthorized = Guid.NewGuid().ToString();

            var actualResponse = TrivyScanDescriptionNormalizer.ToHumanReadable(sampleNotAuthorized);

            actualResponse.Should().Be(TrivyScanDescriptionNormalizer.UnknownError);
        }
    }
}