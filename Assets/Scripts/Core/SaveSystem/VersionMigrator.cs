using UnityEngine;
using Game.Core.Data;

namespace Game.Core.SaveSystem {

    public static class VersionMigrator {

        public static SaveData migrate(SaveData data) {
            int startVersion = data.saveVersion;

            while (data.saveVersion < SaveConstants.CURRENT_SAVE_VERSION) {
                data = migrateOneVersion(data);
            }

            if (startVersion != data.saveVersion) {
                Debug.Log($"Migrated save from version {startVersion} to {data.saveVersion}");
            }

            return data;
        }

        private static SaveData migrateOneVersion(SaveData data) {
            switch (data.saveVersion) {
                // case 1:
                //     return migrateV1ToV2(data);
                // case 2:
                //     return migrateV2ToV3(data);
                default:
                    Debug.LogWarning($"Unknown save version: {data.saveVersion}");
                    return data;
            }
        }

        private static SaveData migrateV1ToV2(SaveData data) {
            data.saveVersion = 2;
            return data;
        }

        private static SaveData migrateV2toV3(SaveData data) {
            data.saveVersion = 3;
            return data;
        }

    }

}
