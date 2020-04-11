using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Civilization.Technology
{
    [CreateAssetMenu(fileName = "Technology", menuName = "Civilization/Technology")]
    class Technology : ScriptableObject
    {
        [SerializeField] int travelSpeed;
        [SerializeField] int stackingLimit;
        [SerializeField] int gainCoin;
        [SerializeField] Goverment goverment;
        public int TravelSpeed { get => travelSpeed; }
        public int StackingLimit { get => stackingLimit; }
        public int GainCoin { get => gainCoin; }
        public Goverment Goverment { get => goverment; }
    }
    /// <summary>
    /// 
    /// </summary>
    class Military
    {

    }
    /// <summary>
    /// 
    /// </summary>
    class Building
    {

    }
    /// <summary>
    /// 
    /// </summary>
    class ResourceAbility
    {
        
    }
    /// <summary>
    /// Once per turn, you may spend something to add coin to this card
    /// </summary>
    class PayAbility
    {

    }
    /// <summary>
    /// 
    /// </summary>
    class UltimativeBonus
    {

    }
}