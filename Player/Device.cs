using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GazeInSimSpace.Player
{
    public class Device
    {
        public string Name { get; private set; }
        public uint ID { get; private set; }

        public Device(string aName, uint aID)
        {
            Name = aName;
            ID = aID;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
