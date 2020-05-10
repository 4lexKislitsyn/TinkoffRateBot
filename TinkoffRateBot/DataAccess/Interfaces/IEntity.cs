using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TinkoffRateBot.DataAccess.Interfaces
{
    /// <summary>
    /// Interface of any Database Entity.
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Last updated date and time.
        /// </summary>
        DateTime Updated { get; set; }
    }
}
