using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MCR_EMApp
{
    
    class MeterModel
    {
        [Key]
        public int Id { get; set; }
        public int MeterID { get; set; }
        public int IB   { get; set; }
        public int IR   { get; set; }
        public int IY   { get; set; }
        public int KWH  { get; set; }
        public int MVAR { get; set; }
        public int MW   { get; set; }
        public int VB   { get; set; }
        public int VR   { get; set; }
        public int VY   { get; set; }
        public DateTime Timestamp { get; set; }
       
       
    }
}