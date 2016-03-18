using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenMap
{
    public static class CoordinateServiceProvider
    {
        private static CoordinateService coordinateService = null;

        public static ICoordinateService GetService(Map map)
        {
            if (coordinateService != null && coordinateService.Map == map)
                return coordinateService;

            coordinateService = new CoordinateService(map);
            return coordinateService;
        }
    }
}
