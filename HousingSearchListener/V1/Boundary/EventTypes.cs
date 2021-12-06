namespace HousingSearchListener.V1.Boundary
{
    public static class EventTypes
    {
        public const string PersonCreatedEvent = "PersonCreatedEvent";
        public const string PersonUpdatedEvent = "PersonUpdatedEvent";

        public const string TenureCreatedEvent = "TenureCreatedEvent";
        public const string TenureUpdatedEvent = "TenureUpdatedEvent";
        public const string PersonAddedToTenureEvent = "PersonAddedToTenureEvent";
        public const string PersonRemovedFromTenureEvent = "PersonRemovedFromTenureEvent";

        public const string AccountCreatedEvent = "AccountCreatedEvent";
        public const string TransactionCreatedEvent = "TransactionCreatedEvent";
    }
}