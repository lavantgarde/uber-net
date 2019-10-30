using System;

namespace uber_net.Models
{
    [Flags]
    enum Scope
    {
        Profile = 1,
        History = 2,
        HistoryLite = 4,
        OfflineAccess = 8,
        Places = 16,
        Request = 32,
        RequestReceipt = 64,
        AllTrips = 128
    }
}
