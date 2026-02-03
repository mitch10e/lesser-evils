using Game.Core.Data;

namespace Game.Core.Events {

    public struct ResourceChangedEvent {

        public ResourceType type;

        public int oldAmount;

        public int newAmount;

    }

}
