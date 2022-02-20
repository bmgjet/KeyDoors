using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Oxide.Plugins
{
    [Info("KeyDoor", "bmgjet", "1.0.0")]
    [Description("Turns doors by keylocks into security doors")]
    public class KeyDoor : RustPlugin
    {
        private int MaxLockRange = 6;
        private List<Door> ServerDoors = new List<Door>();
        private List<KeyLock> ServerLocks = new List<KeyLock>();
        private List<Door> SecurityDoors = new List<Door>();
        [ChatCommand("doorclippoint")]
        private void showclips(BasePlayer player) { if (player.IsAdmin) { foreach (Door door in ServerDoors) { player.SendConsoleCommand("ddraw.sphere", 8f, Color.red, door.transform.position, MaxLockRange * 2); } } }
        private void OnDoorOpened(Door thisdoor, BasePlayer player) { if (thisdoor == null || player == null || thisdoor.OwnerID != 0) { return; } if (SecurityDoors.Contains(thisdoor)) { thisdoor.CloseRequest(); } }
        private void OnServerInitialized() { PrefabHooks(); }
        private void PrefabHooks()
        {
            foreach(BaseEntity l in BaseEntity.serverEntities.ToList())
            {
                if (l == null) { continue; }
                if (l is Door && l.OwnerID == 0) { ServerDoors.Add(l as Door); continue; }
                if (l is KeyLock && l.OwnerID == 0) { ServerLocks.Add(l as KeyLock); }
            }
            Puts("Found " + ServerDoors.Count + " Server Doors");
            Puts("Found " + ServerLocks.Count + " Server Keylocks");
            int locked = 0;
            foreach (KeyLock kl in ServerLocks)
            {
                kl.SetFlag(BaseEntity.Flags.Locked,true);
                Door ClosestDoor = null;
                foreach (Door doors in ServerDoors)
                {
                    if (doors == null || doors.OwnerID != 0) { continue; }
                    if (ClosestDoor == null) { ClosestDoor = doors; }
                    float distance = kl.Distance(doors);
                    if (distance <= MaxLockRange && distance < Vector3.Distance(doors.transform.position, ClosestDoor.transform.position)) { ClosestDoor = doors; }
                }
                if (ClosestDoor == null) { continue; }
                if (kl.Distance(ClosestDoor.transform.position) > MaxLockRange) { continue; }
                if (!SecurityDoors.Contains(ClosestDoor)) { SecurityDoors.Add(ClosestDoor); }
                locked++;
            }
            Puts(locked + " Doors have been changed to Security Doors");
        }
    }
}