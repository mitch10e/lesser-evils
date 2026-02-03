using Game.Core.Data;

namespace Game.Core.Events {

    public struct MaterialChangedEvent {

        public MaterialType type;

        public int oldAmount;

        public int newAmount;

    }

}
