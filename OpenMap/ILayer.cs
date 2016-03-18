using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenMap
{
    public interface ILayer
    {
         Map MapControl { get; }
    }
}
