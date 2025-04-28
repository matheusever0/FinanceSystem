using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceSystem.Domain.Enums
{
    public enum FinancingInstallmentStatus
    {
        Pending = 1,   
        Paid = 2,      
        PartiallyPaid = 3,
        Overdue = 4,   
        Adjusted = 5  
    }
}
