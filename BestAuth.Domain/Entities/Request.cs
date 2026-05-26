namespace BestAuth.Domain.Entities
{
    public class Request
    {
        public Guid Id { get; set; }
        public required string ClientName { get; set; }
        public required string Phone { get; set; }
        public RequestStatus Status { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public static Request Create(string clientName, string phone, string? comment)
        {
            return new Request
            {
                Id = Guid.NewGuid(),
                ClientName = clientName,
                Phone = phone,
                Comment = comment,
                Status = RequestStatus.New,
                CreatedAtUtc = DateTime.UtcNow
            };
        }
    }
}
