﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class KeyColor : MonoBehaviour
{
    public Color color;

    public List<Door> doors = new List<Door>();
    public List<Key> keys = new List<Key>();

    public HashSet<Unit> openedFor = new HashSet<Unit>();

    void OpenFor(Unit player, bool open = true) {
        if (openedFor.Contains(player) == open) {
            return;
        }
        doors.ForEach(door => door.OpenFor(player, open));
        if (DebugManager.debug) {
            Debug.Log(string.Format("Color {0} open for {1} : {2}", this, player, open));
        }
        if (open) {
            openedFor.Add(player);
        } else {
            openedFor.Remove(player);
        }
    }

    void Start() {
        TimeManager.instance.onUndo += OnUndo;
    }

    public void Recalculate(Unit player) {
        if (player.inventory == null) {
            return;
        }
        if (player.inventory.items.Any(item => {
            var key = item.GetComponent<Key>();
            return key != null && key.keyColor == this;
        })) {
            OpenFor(player, true);
        } else {
            OpenFor(player, false);
        }
    }

    public void OnUndo() {
        Unit.all.ForEach(Recalculate);
    }
}
