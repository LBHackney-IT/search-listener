using System;
using System.Collections.Generic;
using System.Text;

namespace HousingSearchListener.V1.Helper
{
    public static class DateTimeHelpers
    {
        public static DateTime SetStartDate(DateTime date) => date == DateTime.MinValue ? DateTime.UtcNow : date;
    }
}
