using System;
using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Maps each physical room to its passive ability by the room's
    /// <see cref="Room.RoomName"/>, and attaches the matching
    /// <see cref="RoomPassive"/> component at runtime.
    ///
    /// Mapping (per team decision):
    ///   ServerRoom  -> GeneratorPassive
    ///   Shop        -> ArmouryPassive
    ///   Air Lock    -> ShieldReactorPassive
    ///
    /// Communication has no passive this round (design doc §2.1).
    /// Already-present passives are left untouched, so the same rooms can be
    /// wired manually in the Inspector without double-installing.
    /// </summary>
    public static class RoomPassiveInstaller
    {
        /// <summary>Called from <see cref="Room"/>'s Awake.</summary>
        public static void Install(Room room)
        {
            if (room == null)
                return;

            Type passiveType = ResolvePassive(Normalize(room.RoomName));
            if (passiveType == null)
                return;

            if (room.GetComponent(passiveType) != null)
                return;

            room.gameObject.AddComponent(passiveType);
        }

        private static string Normalize(string name)
        {
            return name == null
                ? ""
                : name.Replace(" ", "").ToLowerInvariant();
        }

        private static Type ResolvePassive(string key)
        {
            switch (key)
            {
                case "serverroom":
                    return typeof(GeneratorPassive);

                case "shop":
                    return typeof(ArmouryPassive);

                case "airlock":
                    return typeof(ShieldReactorPassive);

                default:
                    return null;
            }
        }
    }
}
