using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    public DeckZone DeckZone { get; private set; }
    public Graveyard Graveyard { get; private set; }
    public ExtraDeckZone ExtraDeckZone { get; private set; }
    public CardZone FieldCardZone { get; private set; }
    public IReadOnlyList<MonsterZone> MonsterCardZones { get => monsterCardZones; }
    public IReadOnlyList<CardZone> SpellTrapCardZones { get => SpellTrapCardZones; }

    private List<MonsterZone> monsterCardZones;
    private List<CardZone> spellTrapCardZones;

    private void Awake()
    {
        DeckZone = transform.Find("Deck Zone").GetComponent<DeckZone>();
        Graveyard = transform.Find("Graveyard").GetComponent<Graveyard>();
        ExtraDeckZone = transform.Find("Fusion Deck Zone").GetComponent<ExtraDeckZone>();
        FieldCardZone = transform.Find("Field Card Zone").GetComponent<CardZone>();
        monsterCardZones = new List<MonsterZone>();
        Transform mZones = transform.Find("Monster Zones");
        for (int i = 0; i < mZones.childCount; i++)
        {
            monsterCardZones.Add(mZones.GetChild(i).GetComponent<MonsterZone>());
        }
        spellTrapCardZones = new List<CardZone>();
        Transform stZones = transform.Find("Spell and Trap Zones");
        for (int i = 0; i < stZones.childCount; i++)
        {
            spellTrapCardZones.Add(stZones.GetChild(i).GetComponent<CardZone>());
        }
    }

    public CardZone GetZoneByName(string name)
    {
        if (name == DeckZone.name) return DeckZone;
        if (name == Graveyard.name) return Graveyard;
        if (name == ExtraDeckZone.name) return ExtraDeckZone;
        if (name == FieldCardZone.name) return FieldCardZone;
        foreach (CardZone zone in monsterCardZones)
        {
            if (name == zone.name) return zone;
        }
        foreach (CardZone zone in spellTrapCardZones)
        {
            if (name == zone.name) return zone;
        }
        return null;
    }

    public IReadOnlyList<CardZone> GetAllZones()
    {
        List<CardZone> allZones = new List<CardZone>();
        allZones.Add(DeckZone);
        allZones.Add(Graveyard);
        allZones.Add(ExtraDeckZone);
        allZones.Add(FieldCardZone);
        allZones.AddRange(monsterCardZones);
        allZones.AddRange(spellTrapCardZones);
        return allZones;
    }

    public void EnableAllZoneEvents()
    {
        foreach (CardZone zone in GetAllZones())
        {
            zone.EventsEnabled = true;
        }
    }

    public void DisableAllZoneEvents()
    {
        foreach (CardZone zone in GetAllZones())
        {
            zone.EventsEnabled = false;
        }
    }
}
