using Game.Core.Data;

namespace Game.Core.Events {

    public struct UnitStatusChangedEvent {

        public string unitID;

        public UnitStatus oldStatus;

        public UnitStatus newStatus;

    }

}
