using System.Collections.Generic;

namespace ParkyWeb.Models.ViewModel
{
    public class IndexVM
    {
        public IEnumerable<NationalPark> NationalParkList { get; set; }

        public IEnumerable<Trails> TrailList { get; set; }
    }
}