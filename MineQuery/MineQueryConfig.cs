using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fCraft;

namespace MineQuery
{
    public class MineQueryConfig : PluginConfig
    {
        private int mineQueryPort;

        public MineQueryConfig()
        {
            mineQueryPort = 25566;
        }

        public int MineQueryPort
        {
            get
            {
                return mineQueryPort;
            }
            set
            {
                mineQueryPort = value;
            }
        }
    }
}
