using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataManagement
{
    public class Player
    {
        public int PlayerId { get; set; }
        public string LastMoviment { get; set; }
        public bool IsBeingControllable { get; set; }
        public string PlayerMode { get; set; }
        public int InitialX { get; set; }
        public int InitialY { get; set; }
        public int CharacterTypeId { get; set; }
        public int Life { get; set; }
        public int SpeedWalk { get; set; }
        public int SpeedRun { get; set; }
        public int AttackMin { get; set; }
        public int AttackMax { get; set; }
    }
}
