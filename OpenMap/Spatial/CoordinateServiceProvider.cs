using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenMap
{
    public static class CoordinateServiceProvider
    {
        private static Dictionary<Map, ICoordinateService> coordinateServices = new Dictionary<Map, ICoordinateService>();

        public static ICoordinateService GetService(Map map)
        {
            ICoordinateService coordinateService = null;
            if(coordinateServices.TryGetValue(map, out coordinateService))
            {
                return coordinateService;
            }
            else
            {
                coordinateService = new CoordinateService(map);
                coordinateServices.Add(map, coordinateService);
                return coordinateService;
            }
        }
    }
}
