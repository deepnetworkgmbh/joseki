using System;

namespace core.helpers
{
    public static class BlobFolderNameGenerator
    {
        private static readonly Random Randomizer = new Random(DateTime.UtcNow.GetHashCode());

        public static string ForDate(DateTime date)
        {
            var datePart = date.ToString("yyyyMMdd-HHmmss");
            var salt = Randomizer.Next(16777216).ToString("x6");

            return $"{datePart}-{salt}";
        }
    }
}