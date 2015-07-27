using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace suds
{
    /// <summary>
    /// Runtime: manages state including:
    ///  - loading/unloading assets
    ///  - procedurally generating areas
    ///  - " items/doodads/flavor text
    /// </summary>
    public class Runtime
    {

    }

    public interface IDescribable
    {
        void Describe();
    }
}
