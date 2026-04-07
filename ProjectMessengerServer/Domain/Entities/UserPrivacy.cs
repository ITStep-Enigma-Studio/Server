namespace ProjectMessengerServer.Domain.Entities
{
    public class UserPrivacy
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public PrivacyLevel ShowEmail { get; set; }
        public PrivacyLevel ShowPhoneNumber { get; set; }
        public PrivacyLevel ShowLastSeen { get; set; }
        public PrivacyLevel Birthday { get; set; }

        public enum PrivacyLevel
        {
            Nobody = 0,
            Contacts = 1,
            Everybody = 2
        }
    }
}
