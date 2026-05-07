namespace StilsoftIRS.Models
{
    internal sealed class ResponseResource
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string ResourceType { get; set; }

        public string Responsible { get; set; }

        public bool IsAvailable { get; set; }
    }
}
