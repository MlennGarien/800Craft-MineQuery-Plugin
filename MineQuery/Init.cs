using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fCraft;

namespace MineQuery
{
    public class Init : Plugin
    {
        private string name = "MineQuery";
        private string version = "1.01";
        public static MineQueryConfig Config = new MineQueryConfig();

        public void Initialize()
        {
            // Load configuration
            Config = (MineQueryConfig)Config.Load(this, "config.xml", typeof(MineQueryConfig));

            // Start server
            MineQueryServer.GetInstance();
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public string Version
        {
            get
            {
                return version;
            }
            set
            {
                version = value;
            }
        }
    }
}
