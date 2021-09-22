namespace HousingSearchListener.V1.Boundary
{
    public enum EventTypes
    {
        public const string PersonCreatedEvent = "PersonCreatedEvent";
        public const string PersonUpdatedEvent = "PersonUpdatedEvent";

        public const string TenureCreatedEvent = "TenureCreatedEvent";
        public const string TenureUpdatedEvent = "TenureUpdatedEvent";
        public const string PersonAddedToTenureEvent = "PersonAddedToTenureEvent";
    }
}