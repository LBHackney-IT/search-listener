using System.Collections.Generic;

namespace HousingSearchListener.V1.Boundary
{
    public class EventData
    {
        public Dictionary<string, object> OldData { get; set; }

        public Dictionary<string, object> NewData { get; set; }
    }
}
