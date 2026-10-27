using UnityEngine;
namespace Bulldozer.Player
{
    // Names describe visible reference colors; exact mineral names were not established by the footage.
    public static class MiningResources
    {
        public const int TypeCount=6;
        public static readonly string[] Names={"Iron ore","Copper ore","Blue mineral","Diamond ore","Gold ore","Amethyst ore"};
        public static readonly Color[] Colors={new Color(.55f,.57f,.53f),new Color(.87f,.39f,.15f),new Color(.16f,.65f,.75f),new Color(.83f,.88f,.9f),new Color(.96f,.73f,.16f),new Color(.61f,.29f,.78f)};
        public static readonly int[] Prices={10,15,20,25,30,35};
        public static bool Valid(int id)=>id>=0 && id<12;
        public static bool Refined(int id)=>id>=6;
        public static Color Color(int id)=>Colors[id%6];
        public static string Name(int id)=>Refined(id)?Names[id%6]+" ingot":Names[id];
    }
}

