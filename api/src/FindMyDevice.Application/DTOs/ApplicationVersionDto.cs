namespace FindMyDevice.Application.DTOs
{
    public class ApplicationVersionDto
    {
        public required string Application { get; set; }

        public required string Version { get; set; }

        public string? CommitSha { get; set; }

        public string? BuildTimestampUtc { get; set; }

        public required string Environment { get; set; }
    }
}
