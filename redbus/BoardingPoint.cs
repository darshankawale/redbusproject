//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace redbus
{
    using System;
    using System.Collections.Generic;
    
    public partial class BoardingPoint
    {
        public int PointId { get; set; }
        public string PointName { get; set; }
        public Nullable<int> RouteId { get; set; }
    
        public virtual Route Route { get; set; }
    }
}
