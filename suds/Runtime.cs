using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

    public static class Dice
    {
        // Let it be thread-safe
        private static ThreadLocal<Random> s_Gen = new ThreadLocal<Random>(
         () => new Random());

        // Thread-safe non-skewed generator
        public static Random Generator
        {
            get
            {
                return s_Gen.Value;
            }
        }
        
        public static int RollRange(int min, int max)
        {
            return Generator.Next(min, max + 1);
        }

        public static bool CoinFlip()
        {
            return ((Generator.Next(0, 2) == 0) ? false : true);
        }
    }
}
