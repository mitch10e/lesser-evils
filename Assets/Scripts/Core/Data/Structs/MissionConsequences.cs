using System;
using System.Collections.Generic;

namespace Game.Core.Data {

    [Serializable]
    public struct MissionConsequences {

        public string[] storyFlagsOnSuccess;

        public string[] storyFlagsOnFailure;

        public static MissionConsequences Create() {
            return new MissionConsequences {
                storyFlagsOnFailure = new string[0],
                storyFlagsOnSuccess = new string[0]
            };
        }

    }

}
